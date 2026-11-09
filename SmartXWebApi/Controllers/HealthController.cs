namespace SmartXWebApi.Controllers;
/// <summary>HTTP-HEALTH: availability endpoint for the client connection indicator.</summary>
[ApiController, Route("api/health")]
public sealed class HealthController : ControllerBase
{
    /// <summary>HTTP-HEALTH-01: confirm a responsive process without exposing host details.</summary>
    [HttpGet] public IActionResult Get() => Ok(new { status = "online", system = "Smart-X", timestamp = DateTimeOffset.UtcNow });
}
