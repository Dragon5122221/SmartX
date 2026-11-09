namespace SmartXWebApi.Controllers;
/// <summary>HTTP boundary for telemetry; typed contracts and centralized error handling.</summary>
[ApiController, Route("api/telemetry")]
public sealed class TelemetryController(GatewayService gateway, TelemetryDispatcher dispatcher) : ControllerBase
{
    /// <summary>HTTP-TELEMETRY-01: route validated input to the gateway and return its result.</summary>
    [HttpPost("float")] public async Task<IActionResult> Float(TelemetryBatchRequest<float> request, CancellationToken token) => Ok(await dispatcher.Submit(request, v => v is > 32 or < 10, gateway.IngestFloat, token));
    /// <summary>HTTP-TELEMETRY-02: route validated input to the gateway and return its result.</summary>
    [HttpPost("integer")] public async Task<IActionResult> Integer(TelemetryBatchRequest<int> request, CancellationToken token) => Ok(await dispatcher.Submit(request, v => v > 1500, gateway.IngestInteger, token));
    /// <summary>HTTP-TELEMETRY-03: route validated input to the gateway and return its result.</summary>
    [HttpPost("boolean")] public async Task<IActionResult> Boolean(TelemetryBatchRequest<bool> request, CancellationToken token) => Ok(await dispatcher.Submit(request, _ => false, gateway.IngestBoolean, token));
    /// <summary>HTTP-TELEMETRY-04: route validated input to the gateway and return its result.</summary>
    [HttpGet("float/{id}/history")] public IActionResult Floats(string id) => Ok(gateway.FloatHistory(id));
    /// <summary>HTTP-TELEMETRY-05: route validated input to the gateway and return its result.</summary>
    [HttpGet("integer/{id}/history")] public IActionResult Integers(string id) => Ok(gateway.IntegerHistory(id));
    /// <summary>HTTP-TELEMETRY-06: route validated input to the gateway and return its result.</summary>
    [HttpGet("boolean/{id}/history")] public IActionResult Booleans(string id) => Ok(gateway.BooleanHistory(id));
    /// <summary>HTTP-TELEMETRY-07: route validated input to the gateway and return its result.</summary>
    [HttpPost("power-comparison")] public IActionResult Compare(PowerComparisonRequest request) => Ok(gateway.Compare(request));
}
