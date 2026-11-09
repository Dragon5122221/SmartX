namespace SmartX.Shared.Contracts.Telemetry;

/// <summary>DATA STRUCTURE DS-05: typed list returned after explicit jagged-array flattening.</summary>
public sealed record TelemetryHistoryResponse<T>(string SensorId, int BatchCount, List<SmartX.Shared.Telemetry.TelemetryPacket<T>> Readings);
