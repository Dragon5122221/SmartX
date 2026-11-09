namespace SmartX.Shared.Dashboard;

/// <summary>Typed transport model; serialized consistently by the API and MVC client.</summary>
public sealed record TelemetryAlert(Guid Id, string SensorId, string Kind, string Message, DateTimeOffset Timestamp, bool Acknowledged = false);
