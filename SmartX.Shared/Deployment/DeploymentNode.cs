namespace SmartX.Shared.Deployment;

/// <summary>DATA STRUCTURE DS-03: deployment tree with typed child lists; validated recursively before storage.</summary>
public sealed record DeploymentNode
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public bool Enabled { get; init; } = true;
    public List<DeploymentNode> Children { get; init; } = [];
}
