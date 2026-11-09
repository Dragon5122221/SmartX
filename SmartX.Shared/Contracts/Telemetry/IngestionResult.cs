namespace SmartX.Shared.Contracts.Telemetry;

/// <summary>Typed transport model; serialized consistently by the API and MVC client.</summary>
public sealed record IngestionResult(int Accepted, int Rejected, double Milliseconds, List<SmartX.Shared.Validation.ValidationIssue> Errors);
