namespace SmartX.Shared.Attachments;

/// <summary>Typed transport model; serialized consistently by the API and MVC client.</summary>
public sealed record SensorAttachment(Guid Id, string SensorId, string FileName, string ContentType, long Length, DateTimeOffset UploadedAt);
