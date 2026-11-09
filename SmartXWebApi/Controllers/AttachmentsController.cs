using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using SmartXWebApi.Storage;
namespace SmartXWebApi.Controllers;

/// <summary>HTTP-UPLOAD: sensor-bound streaming multipart uploads (Microsoft, 2026d).</summary>
[ApiController, Route("api/sensors/{sensorId}/attachments")]
public sealed class AttachmentsController(GatewayService gateway, EncryptedAttachmentStorage storage) : ControllerBase
{
    /// <summary>HTTP-UPLOAD-01: list encrypted attachment metadata for an existing sensor.</summary>
    [HttpGet] public IActionResult List(string sensorId) { gateway.Sensor(sensorId); return Ok(storage.List(sensorId)); }
    /// <summary>HTTP-UPLOAD-02: parse one multipart file without IFormFile model-binding buffers.</summary>
    [SmartXWebApi.Filters.StreamingRequest]
    [HttpPost, RequestSizeLimit(9 * 1024 * 1024)]
    public async Task<IActionResult> Upload(string sensorId, CancellationToken token)
    {
        gateway.Sensor(sensorId);
        if (!MediaTypeHeaderValue.TryParse(Request.ContentType, out var type) || !type.MediaType.Equals("multipart/form-data", StringComparison.OrdinalIgnoreCase))
            throw new GatewayException("A multipart/form-data request is required.", 415);
        var boundary = HeaderUtilities.RemoveQuotes(type.Boundary).Value;
        if (string.IsNullOrEmpty(boundary) || boundary.Length > 128) throw new GatewayException("Invalid multipart boundary.");
        var reader = new MultipartReader(boundary, Request.Body) { BodyLengthLimit = 9 * 1024 * 1024 };
        var section = await reader.ReadNextSectionAsync(token) ?? throw new GatewayException("Select a file to upload.");
        if (!ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var disposition) || (!disposition.FileName.HasValue && !disposition.FileNameStar.HasValue))
            throw new GatewayException("The first multipart section must be a file.");
        string name = HeaderUtilities.RemoveQuotes(disposition.FileNameStar.HasValue ? disposition.FileNameStar : disposition.FileName).Value ?? "";
        var result = await storage.SaveAsync(sensorId, name, section.Body, token);
        return Ok(result);
    }
    /// <summary>HTTP-UPLOAD-03: decrypt and authenticate before the framework streams an attachment download.</summary>
    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(string sensorId, Guid id, CancellationToken token)
    {
        gateway.Sensor(sensorId); var result = await storage.OpenAsync(sensorId, id, token);
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(result.Stream, result.Metadata.ContentType, result.Metadata.FileName);
    }
}
