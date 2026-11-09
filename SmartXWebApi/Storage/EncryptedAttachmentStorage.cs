using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using SmartX.Shared.Attachments;
using SmartXWebApi.Validation;
namespace SmartXWebApi.Storage;

/// <summary>ENC-01: streamed authenticated AES-GCM records, with protected metadata and a mandatory final record.</summary>
public sealed class EncryptedAttachmentStorage(AttachmentKeyProvider keys)
{
    private const int ChunkSize = 65536;
    private readonly SemaphoreSlim _uploads = new(4, 4);
    // DATA STRUCTURE DS-14: bounded metadata index, guarded separately from slow file I/O.
    private readonly object _gate = new();
    private readonly List<SensorAttachment> _metadata = LoadMetadata(keys);

    /// <summary>ENC-02: load protected sidecars after restart; sensor association is checked by the API.</summary>
    private static List<SensorAttachment> LoadMetadata(AttachmentKeyProvider keys)
    {
        var list = new List<SensorAttachment>();
        foreach (var file in Directory.EnumerateFiles(keys.DirectoryPath, "*.meta"))
        {
            var item = JsonSerializer.Deserialize<SensorAttachment>(keys.MetadataProtector.Unprotect(File.ReadAllBytes(file)));
            if (item is not null) list.Add(item);
        }
        return list;
    }
    /// <summary>ENC-03: return only metadata belonging to the requested sensor.</summary>
    public List<SensorAttachment> List(string sensorId) { lock (_gate) return _metadata.Where(x => x.SensorId.Equals(sensorId, StringComparison.OrdinalIgnoreCase)).OrderByDescending(x => x.UploadedAt).ToList(); }
    /// <summary>ENC-04: stream upload records with bounded buffers; authenticate file identity and record order.</summary>
    public async Task<SensorAttachment> SaveAsync(string sensorId, string name, Stream source, CancellationToken token)
    {
        var validated = AttachmentUploadValidator.ValidateName(name);
        await _uploads.WaitAsync(token);
        var id = Guid.NewGuid(); var path = Path.Combine(keys.DirectoryPath, id + ".sxenc");
        try
        {
            lock (_gate) if (_metadata.Count >= 1000 || _metadata.Count(x => x.SensorId.Equals(sensorId, StringComparison.OrdinalIgnoreCase)) >= 20) throw new GatewayException("Attachment limit reached (20 per sensor, 1,000 total).", 409);
            // DATA STRUCTURE DS-15: fixed byte buffers cap per-transfer memory; no whole-file MemoryStream.
            var plain = new byte[ChunkSize]; var cipher = new byte[ChunkSize]; var tag = new byte[16];
            var salt = RandomNumberGenerator.GetBytes(32);
            var key = HKDF.DeriveKey(HashAlgorithmName.SHA256, keys.Key, 32, salt, Encoding.UTF8.GetBytes("SmartX:" + id));
            using var aes = new AesGcm(key, 16); // Microsoft ([s.a.]): explicit authenticated tag length.
            await using var file = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, ChunkSize, true);
            await file.WriteAsync("SX01"u8.ToArray(), token); await file.WriteAsync(salt, token);
            long total = 0; uint index = 0;
            try
            {
                while (true)
                {
                    int count = await ReadChunkAsync(source, plain, token);
                    if (count == 0 && total == 0) throw new GatewayException("Empty files cannot be attached.");
                    total += count;
                    if (total > AttachmentUploadValidator.MaximumBytes) throw new GatewayException("File exceeds the 8 MiB limit.", 413);
                    if (index == 0) AttachmentUploadValidator.ValidateContent(validated.Type, plain.AsSpan(0, count));
                    else if (!validated.Type.StartsWith("image/")) AttachmentUploadValidator.ValidateContent(validated.Type, plain.AsSpan(0, count));
                    var nonce = Nonce(index); var aad = AssociatedData(sensorId, id, index, count);
                    aes.Encrypt(nonce, plain.AsSpan(0, count), cipher.AsSpan(0, count), tag, aad);
                    var length = new byte[4]; BinaryPrimitives.WriteInt32LittleEndian(length, count);
                    await file.WriteAsync(length, token); await file.WriteAsync(cipher.AsMemory(0, count), token); await file.WriteAsync(tag, token);
                    index++;
                    // ENC FINAL RECORD: authenticated zero length proves completeness; EOF alone is never accepted.
                    if (count == 0) break;
                }
            }
            finally { CryptographicOperations.ZeroMemory(key); CryptographicOperations.ZeroMemory(plain); }
            await file.FlushAsync(token);
            var metadata = new SensorAttachment(id, sensorId, validated.Name, validated.Type, total, DateTimeOffset.UtcNow);
            await File.WriteAllBytesAsync(Path.Combine(keys.DirectoryPath, id + ".meta"), keys.MetadataProtector.Protect(JsonSerializer.SerializeToUtf8Bytes(metadata)), token);
            lock (_gate)
            {
                // ENC-09: recheck quotas at commit time because other upload streams may have completed meanwhile.
                if (_metadata.Count >= 1000 || _metadata.Count(x => x.SensorId.Equals(sensorId, StringComparison.OrdinalIgnoreCase)) >= 20)
                    throw new GatewayException("Attachment limit reached while uploading.", 409);
                _metadata.Add(metadata);
            }
            return metadata;
        }
        catch { File.Delete(path); File.Delete(Path.Combine(keys.DirectoryPath, id + ".meta")); throw; }
        finally { _uploads.Release(); }
    }
    /// <summary>ENC-05: authenticate the complete file before returning a bounded decryption stream; no plaintext is written to disk.</summary>
    public async Task<(SensorAttachment Metadata, Stream Stream)> OpenAsync(string sensorId, Guid id, CancellationToken token)
    {
        SensorAttachment meta;
        lock (_gate) meta = _metadata.FirstOrDefault(x => x.Id == id && x.SensorId.Equals(sensorId, StringComparison.OrdinalIgnoreCase)) ?? throw new GatewayException("Attachment not found for this sensor.", 404);
        var file = new FileStream(Path.Combine(keys.DirectoryPath, id + ".sxenc"), FileMode.Open, FileAccess.Read, FileShare.Read, ChunkSize, true);
        var header = new byte[36];
        try {
            await file.ReadExactlyAsync(header, token);
            if (!header.AsSpan(0, 4).SequenceEqual("SX01"u8)) throw new CryptographicException("Unrecognised encrypted file header.");
        } catch { await file.DisposeAsync(); throw; }
        var key = HKDF.DeriveKey(HashAlgorithmName.SHA256, keys.Key, 32, header.AsSpan(4).ToArray(), Encoding.UTF8.GetBytes("SmartX:" + id));
        using var aes = new AesGcm(key, 16);
        var plain = new byte[ChunkSize]; var cipher = new byte[ChunkSize]; var tag = new byte[16];
        try
        {
            long total = 0; uint index = 0;
            while (true)
            {
                var size = new byte[4]; await file.ReadExactlyAsync(size, token); int count = BinaryPrimitives.ReadInt32LittleEndian(size);
                if (count is < 0 or > ChunkSize || total + count > AttachmentUploadValidator.MaximumBytes) throw new CryptographicException("Invalid encrypted record length.");
                await file.ReadExactlyAsync(cipher.AsMemory(0, count), token); await file.ReadExactlyAsync(tag, token);
                aes.Decrypt(Nonce(index), cipher.AsSpan(0, count), tag, plain.AsSpan(0, count), AssociatedData(meta.SensorId, id, index, count));
                index++;
                if (count == 0) { if (file.Position != file.Length || total != meta.Length) throw new CryptographicException("Encrypted file is incomplete or has trailing data."); break; }
                total += count; // Verification pass discards plaintext rather than persisting it.
            }
            file.Position = 36; return (meta, new AuthenticatedAttachmentReadStream(file, key, meta));
        }
        catch { await file.DisposeAsync(); throw; }
        finally { CryptographicOperations.ZeroMemory(key); CryptographicOperations.ZeroMemory(plain); }
    }
    /// <summary>ENC-06: fill one chunk even when network reads return short segments.</summary>
    private static async Task<int> ReadChunkAsync(Stream source, byte[] buffer, CancellationToken token)
    { int count = 0; while (count < buffer.Length) { int read = await source.ReadAsync(buffer.AsMemory(count), token); if (read == 0) break; count += read; } return count; }
    /// <summary>ENC-07: nonce counter is unique within each independently derived per-file key.</summary>
    internal static byte[] Nonce(uint index) { var nonce = new byte[12]; BinaryPrimitives.WriteUInt32BigEndian(nonce.AsSpan(8), index); return nonce; }
    /// <summary>ENC-08: authenticate association, ordinal and record size to reject swapping or truncation.</summary>
    internal static byte[] AssociatedData(string sensorId, Guid id, uint index, int count) => Encoding.UTF8.GetBytes($"SX01|{sensorId}|{id}|{index}|{count}");
}
