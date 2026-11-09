namespace SmartXWebApi.Controllers;
/// <summary>HTTP boundary for sensors; typed contracts and centralized error handling.</summary>
[ApiController, Route("api/sensors")]
public sealed class SensorsController(GatewayService gateway) : ControllerBase
{
    /// <summary>HTTP-SENSORS-01: route validated input to the gateway and return its result.</summary>
    [HttpGet] public IActionResult List(string? search, string? category, int page = 1) => Ok(gateway.Sensors(search, category, page));
    /// <summary>HTTP-SENSORS-02: route validated input to the gateway and return its result.</summary>
    [HttpGet("{id}")] public IActionResult Get(string id) => Ok(gateway.Sensor(id));
    /// <summary>HTTP-SENSORS-03: route validated input to the gateway and return its result.</summary>
    [HttpPost] public IActionResult Create(RegisterSensorRequest request) => Ok(gateway.Register(request));
    /// <summary>HTTP-SENSORS-04: route validated input to the gateway and return its result.</summary>
    [HttpPut("{id}")] public IActionResult Update(string id, RegisterSensorRequest request) => Ok(gateway.Register(request, id));
}
