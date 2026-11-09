using System.Buffers.Binary;
using System.Security.Cryptography;
using SmartX.Shared.Attachments;
namespace SmartXWebApi.Storage;

/// <summary>ENC-STREAM-01: forward-only authenticated decryption with fixed buffers and no plaintext temporary files.</summary>
public sealed class AuthenticatedAttachmentReadStream : Stream
{
    private readonly FileStream _source;
    private readonly AesGcm _aes;
    private readonly SensorAttachment _metadata;
    // DATA STRUCTURE DS-15 USE: two fixed 64 KiB buffers cap working memory regardless of attachment length.
    private readonly byte[] _plain = new byte[65536], _cipher = new byte[65536], _tag = new byte[16];
    private int _offset, _available;
    private uint _index;
    private long _position;
    private bool _complete;
    /// <summary>ENC-STREAM-02: take ownership of the prevalidated encrypted stream and copy the key into AesGcm.</summary>
    public AuthenticatedAttachmentReadStream(FileStream source, byte[] key, SensorAttachment metadata)
    { _source = source; _aes = new AesGcm(key, 16); _metadata = metadata; }
    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => _metadata.Length;
    public override long Position { get => _position; set => throw new NotSupportedException(); }
    /// <summary>ENC-STREAM-03: authenticate each record again, then copy only requested plaintext bytes to the caller.</summary>
    public override async ValueTask<int> ReadAsync(Memory<byte> destination, CancellationToken cancellationToken = default)
    {
        if (destination.Length == 0 || _complete) return 0;
        if (_offset == _available)
        {
            var size = new byte[4]; await _source.ReadExactlyAsync(size, cancellationToken);
            int count = BinaryPrimitives.ReadInt32LittleEndian(size);
            if (count is < 0 or > 65536 || _position + count > Length) throw new CryptographicException("Invalid encrypted record length.");
            await _source.ReadExactlyAsync(_cipher.AsMemory(0, count), cancellationToken);
            await _source.ReadExactlyAsync(_tag, cancellationToken);
            _aes.Decrypt(EncryptedAttachmentStorage.Nonce(_index++), _cipher.AsSpan(0,count), _tag, _plain.AsSpan(0,count),
                EncryptedAttachmentStorage.AssociatedData(_metadata.SensorId, _metadata.Id, _index-1, count));
            _offset=0; _available=count;
            if (count == 0)
            {
                if (_position != Length || _source.Position != _source.Length) throw new CryptographicException("Invalid final encrypted record.");
                _complete=true; return 0;
            }
        }
        int copied=Math.Min(destination.Length,_available-_offset);
        _plain.AsMemory(_offset,copied).CopyTo(destination); _offset+=copied; _position+=copied; return copied;
    }
    /// <summary>ENC-STREAM-04: support the legacy byte-array async overload without introducing buffering.</summary>
    public override Task<int> ReadAsync(byte[] buffer,int offset,int count,CancellationToken cancellationToken) => ReadAsync(buffer.AsMemory(offset,count),cancellationToken).AsTask();
    /// <summary>ENC-STREAM-05: synchronous Stream compatibility; ASP.NET downloads use the asynchronous overload.</summary>
    public override int Read(byte[] buffer,int offset,int count) => ReadAsync(buffer.AsMemory(offset,count)).AsTask().GetAwaiter().GetResult();
    /// <summary>ENC-STREAM-06: clear plaintext and dispose cryptography and the underlying file handle.</summary>
    protected override void Dispose(bool disposing)
    { if(disposing){CryptographicOperations.ZeroMemory(_plain);_aes.Dispose();_source.Dispose();}base.Dispose(disposing); }
    /// <summary>ENC-STREAM-07: forward-only read streams cannot seek.</summary>
    public override long Seek(long offset,SeekOrigin origin) => throw new NotSupportedException();
    /// <summary>ENC-STREAM-08: read-only streams cannot resize.</summary>
    public override void SetLength(long value) => throw new NotSupportedException();
    /// <summary>ENC-STREAM-09: read-only streams cannot write.</summary>
    public override void Write(byte[] buffer,int offset,int count) => throw new NotSupportedException();
    /// <summary>ENC-STREAM-10: a read-only stream has no pending writes to flush.</summary>
    public override void Flush() { }
}
