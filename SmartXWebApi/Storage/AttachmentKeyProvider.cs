using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;
namespace SmartXWebApi.Storage;

/// <summary>KEY-01: persist the encryption key outside the repository, protected by ASP.NET Data Protection (Anderson, 2025).</summary>
public sealed class AttachmentKeyProvider
{
    public byte[] Key { get; }
    public string DirectoryPath { get; }
    public IDataProtector MetadataProtector { get; }
    /// <summary>KEY-02: unwrap an existing 256-bit key or create one on first run; never log key material.</summary>
    public AttachmentKeyProvider(IDataProtectionProvider provider, IConfiguration configuration)
    {
        DirectoryPath = configuration["SmartX:StoragePath"] ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SmartX", "Attachments");
        Directory.CreateDirectory(DirectoryPath);
        var protector = provider.CreateProtector("SmartX.AttachmentKey.v1");
        MetadataProtector = provider.CreateProtector("SmartX.AttachmentMetadata.v1");
        var path = Path.Combine(DirectoryPath, "master.protected");
        if (File.Exists(path)) Key = protector.Unprotect(File.ReadAllBytes(path));
        else { Key = RandomNumberGenerator.GetBytes(32); File.WriteAllBytes(path, protector.Protect(Key)); }
        if (Key.Length != 32) throw new CryptographicException("Invalid attachment key length.");
    }
}
