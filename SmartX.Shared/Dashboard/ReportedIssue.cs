namespace SmartX.Shared.Dashboard;

/// <summary>Typed transport model; serialized consistently by the API and MVC client.</summary>
public sealed record ReportedIssue(Guid Id, string SensorId, string Description, DateTimeOffset CreatedAt, bool Resolved = false);
