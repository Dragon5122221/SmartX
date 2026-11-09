namespace SmartXWebApi.Controllers;
/// <summary>HTTP boundary for dashboard; typed contracts and centralized error handling.</summary>
[ApiController, Route("api/dashboard")]
public sealed class DashboardController(GatewayService gateway) : ControllerBase
{
    /// <summary>HTTP-DASHBOARD-01: route validated input to the gateway and return its result.</summary>
    [HttpGet] public IActionResult Get(string? sensorId) => Ok(gateway.Dashboard(sensorId));
    /// <summary>HTTP-DASHBOARD-02: route validated input to the gateway and return its result.</summary>
    [HttpGet("issues")] public IActionResult Issues() => Ok(gateway.Issues());
    /// <summary>HTTP-DASHBOARD-03: route validated input to the gateway and return its result.</summary>
    [HttpPost("issues")] public IActionResult Report(ReportIssueRequest request) => Ok(gateway.Report(request));
    /// <summary>HTTP-DASHBOARD-04: route validated input to the gateway and return its result.</summary>
    [HttpPost("issues/{id:guid}/resolve")] public IActionResult Resolve(Guid id) { gateway.Resolve(id); return Ok(new { message = "Issue resolved." }); }
    /// <summary>HTTP-DASHBOARD-05: route validated input to the gateway and return its result.</summary>
    [HttpPost("alerts/{id:guid}/acknowledge")] public IActionResult Acknowledge(Guid id) { gateway.Acknowledge(id); return Ok(new { message = "Alert acknowledged." }); }
}
