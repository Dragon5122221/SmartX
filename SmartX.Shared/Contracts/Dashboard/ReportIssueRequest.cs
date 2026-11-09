namespace SmartX.Shared.Contracts.Dashboard;

/// <summary>Typed transport model; serialized consistently by the API and MVC client.</summary>
public sealed record ReportIssueRequest(string SensorId, string Description);
