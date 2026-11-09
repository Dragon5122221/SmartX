namespace SmartX.Shared.Contracts.Telemetry;

/// <summary>Typed transport model; serialized consistently by the API and MVC client.</summary>
public sealed record PowerComparisonRequest(string LeftSensorId, string RightSensorId);
