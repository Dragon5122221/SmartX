using SmartX.Shared.Sensors;
using SmartX.Shared.Dashboard;
namespace SmartX.Core.Services;

/// <summary>HEALTH-01: distinguish a silent device from a valid stable or zero-valued signal.</summary>
public static class SensorHealthService
{
    /// <summary>HEALTH-02: use server receipt time and three expected reporting intervals.</summary>
    public static SensorHealth Evaluate(SensorProfile sensor, bool anomaly, DateTimeOffset now)
        => sensor.LastSeen is null ? SensorHealth.Waiting
         : now - sensor.LastSeen > TimeSpan.FromSeconds(sensor.IntervalSeconds * 3) ? SensorHealth.Disconnected
         : anomaly ? SensorHealth.Anomalous : SensorHealth.Healthy;
}
