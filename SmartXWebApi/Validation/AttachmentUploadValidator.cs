namespace SmartXWebApi.Validation;
/// <summary>UPLOAD-01: bounded configuration, log and deployment-photo file policy.</summary>
public static class AttachmentUploadValidator
{
    public const long MaximumBytes = 8 * 1024 * 1024;
    // DATA STRUCTURE DS-13: fixed extension/type lookup prevents executable uploads and MIME guessing.
    private static readonly Dictionary<string, string> Types = new(StringComparer.OrdinalIgnoreCase)
    { [".json"] = "application/json", [".txt"] = "text/plain", [".log"] = "text/plain", [".csv"] = "text/csv",
      [".yaml"] = "text/plain", [".yml"] = "text/plain", [".ini"] = "text/plain", [".cfg"] = "text/plain",
      [".png"] = "image/png", [".jpg"] = "image/jpeg", [".jpeg"] = "image/jpeg" };
    /// <summary>UPLOAD-02: retain a safe display name and derive its permitted response content type.</summary>
    public static (string Name, string Type) ValidateName(string fileName)
    {
        string name = Path.GetFileName(fileName.Replace('\\', '/'));
        if (name.Length is < 1 or > 120 || name.Any(char.IsControl) || !Types.TryGetValue(Path.GetExtension(name), out var type))
            throw new GatewayException("Use a configuration/log file (.json, .txt, .log, .csv, .yaml, .yml, .ini, .cfg) or PNG/JPEG photo; filename limit is 120 characters.");
        return (name, type);
    }
    /// <summary>UPLOAD-03: verify photo signatures and reject NUL-bearing text/config binaries.</summary>
    public static void ValidateContent(string type, ReadOnlySpan<byte> bytes)
    {
        if (type == "image/png" && !bytes.StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })) throw new GatewayException("Invalid PNG signature.");
        if (type == "image/jpeg" && !bytes.StartsWith(new byte[] { 255, 216, 255 })) throw new GatewayException("Invalid JPEG signature.");
        if (!type.StartsWith("image/") && bytes.Contains((byte)0)) throw new GatewayException("Text and configuration uploads cannot contain binary NUL bytes.");
    }
}
