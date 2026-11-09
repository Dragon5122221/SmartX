namespace SmartX.Shared.Validation;

/// <summary>Typed transport model; serialized consistently by the API and MVC client.</summary>
public sealed record ValidationIssue(string Path, string Message);
