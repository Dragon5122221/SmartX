namespace SmartXWebApi.Controllers;
/// <summary>HTTP boundary for deployment; typed contracts and centralized error handling.</summary>
[ApiController, Route("api/deployment")]
public sealed class DeploymentController(GatewayService gateway) : ControllerBase
{
    /// <summary>HTTP-DEPLOYMENT-01: route validated input to the gateway and return its result.</summary>
    [HttpGet] public IActionResult Get() => Ok(gateway.Deployment());
    /// <summary>HTTP-DEPLOYMENT-02: route validated input to the gateway and return its result.</summary>
    [HttpPost("validate")] public IActionResult Validate(SmartX.Shared.Contracts.Deployment.ValidateDeploymentRequest request) => Ok(gateway.SetDeployment(request.Root, false));
    /// <summary>HTTP-DEPLOYMENT-03: route validated input to the gateway and return its result.</summary>
    [HttpPut] public IActionResult Save(DeploymentNode root) => Ok(gateway.SetDeployment(root, true));
}
