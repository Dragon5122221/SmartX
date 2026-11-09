namespace SmartX.Shared.Telemetry;

/// <summary>DATA STRUCTURE DS-01: generic envelope keeps T strongly typed; no object/dynamic payload. See Microsoft (2026a).</summary>
public sealed record TelemetryPacket<T>
{
    public required string SensorId { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public required long Sequence { get; init; }
    public required T Value { get; init; }
}
