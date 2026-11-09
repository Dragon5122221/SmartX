using SmartX.Shared.Deployment;
using SmartX.Shared.Sensors;
using SmartX.Shared.Telemetry;
namespace SmartX.Core.Simulation;

/// <summary>SEED-01: deterministic Johannesburg greenhouse simulation; no hardware dependency.</summary>
public static class TelemetrySeedGenerator
{
    /// <summary>SEED-02: generate one facility with four independently identifiable deployment zones.</summary>
    public static DeploymentNode Tree() => new() { Id = "facility-a", Name = "Rivonia • Research greenhouse", Children =
        [new() { Id = "zone-a", Name = "Propagation / North" }, new() { Id = "zone-b", Name = "Hydroponics / East" },
         new() { Id = "zone-c", Name = "Utilities / South" }, new() { Id = "zone-d", Name = "Irrigation / West" }] };
    /// <summary>SEED-03: create 1,200 mixed sensor profiles with explicit types, units and locations.</summary>
    public static List<SensorProfile> Sensors(int count = 1200)
    {
        // DATA STRUCTURE DS-16: pre-sized seed registry list; capacity matches the known device count.
        var list = new List<SensorProfile>(count);
        for (int i = 0; i < count; i++)
        {
            var category = (SensorCategory)(i % 3);
            list.Add(new() { Id = $"SX-{i + 1:0000}", Name = $"{(category == SensorCategory.Environmental ? "Canopy temperature" : category == SensorCategory.PowerConsumption ? "Power meter" : "Irrigation valve")} {i + 1:0000}",
                Category = category, ValueKind = (TelemetryValueKind)(i % 3), Unit = i % 3 == 0 ? "°C" : i % 3 == 1 ? "W" : "state",
                LocationId = $"zone-{(char)('a' + i % 4)}", IntervalSeconds = 3, Simulated = true });
        }
        return list;
    }
}
