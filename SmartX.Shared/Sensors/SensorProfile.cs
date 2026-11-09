namespace SmartX.Shared.Sensors;

/// <summary>Typed transport model; serialized consistently by the API and MVC client.</summary>
public sealed record SensorProfile
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string LocationId { get; init; } = "";
    public SensorCategory Category { get; init; }
    public SmartX.Shared.Telemetry.TelemetryValueKind ValueKind { get; init; }
    public string Unit { get; init; } = "";
    public int IntervalSeconds { get; init; } = 3;
    public DateTimeOffset RegisteredAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastSeen { get; init; }
    public bool Simulated { get; init; }
}
