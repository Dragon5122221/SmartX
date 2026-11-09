namespace SmartXWebApi.Controllers;
/// <summary>HTTP boundary for simulation; typed contracts and centralized error handling.</summary>
[ApiController, Route("api/simulation")]
public sealed class SimulationController(GatewayService gateway) : ControllerBase
{
    /// <summary>HTTP-SIMULATION-01: route validated input to the gateway and return its result.</summary>
    [HttpGet("status")] public IActionResult Get() => Ok(gateway.Status());
    /// <summary>HTTP-SIMULATION-02: route validated input to the gateway and return its result.</summary>
    [HttpPost("start")] public IActionResult Start(StartSimulationRequest request) { gateway.Start(request.Scenario); return Ok(gateway.Status()); }
    /// <summary>HTTP-SIMULATION-03: route validated input to the gateway and return its result.</summary>
    [HttpPost("stop")] public IActionResult Stop() { gateway.Stop(); return Ok(gateway.Status()); }
}
