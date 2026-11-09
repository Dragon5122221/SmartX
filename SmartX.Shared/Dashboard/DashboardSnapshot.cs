namespace SmartX.Shared.Dashboard;

/// <summary>Typed transport model; serialized consistently by the API and MVC client.</summary>
public sealed record SensorRow(SmartX.Shared.Sensors.SensorProfile Profile, SensorHealth Health, string DisplayValue);
/// <summary>A bounded chart projection; source telemetry remains strongly typed.</summary>
public sealed record ChartPoint(DateTimeOffset Timestamp, double Value);
/// <summary>Operational snapshot with typed metadata; no raw values boxed in the ingestion path.</summary>
public sealed record DashboardSnapshot(int Sensors, int Healthy, int Anomalous, int Disconnected,
    long Accepted, long Rejected, int Retained, double LastBatchMilliseconds, DateTimeOffset Timestamp,
    List<TelemetryAlert> Alerts, List<SensorRow> Watchlist, List<ChartPoint> Trend, string TrendSensorId,
    SmartX.Shared.Simulation.SimulationStatus Simulation);
