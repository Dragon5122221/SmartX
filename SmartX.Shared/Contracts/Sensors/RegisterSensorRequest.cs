namespace SmartX.Shared.Contracts.Sensors;

/// <summary>Typed transport model; serialized consistently by the API and MVC client.</summary>
public sealed record RegisterSensorRequest
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string LocationId { get; init; } = "";
    public SmartX.Shared.Sensors.SensorCategory Category { get; init; }
    public int IntervalSeconds { get; init; } = 3;
}
