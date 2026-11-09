namespace SmartX.Shared.Contracts.Deployment;

/// <summary>Typed transport model; serialized consistently by the API and MVC client.</summary>
public sealed record ValidateDeploymentRequest(SmartX.Shared.Deployment.DeploymentNode Root);
