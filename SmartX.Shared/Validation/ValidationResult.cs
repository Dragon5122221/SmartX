namespace SmartX.Shared.Validation;

/// <summary>Typed transport model; serialized consistently by the API and MVC client.</summary>
public sealed record ValidationResult
{
    // DATA STRUCTURE DS-08: ordered error list preserves all actionable field/node paths.
    public List<ValidationIssue> Issues { get; init; } = [];
    public bool IsValid => Issues.Count == 0;
    public int NodesVisited { get; set; }
}
