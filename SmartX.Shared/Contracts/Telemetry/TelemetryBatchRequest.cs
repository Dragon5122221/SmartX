namespace SmartX.Shared.Contracts.Telemetry;

/// <summary>Typed transport model; serialized consistently by the API and MVC client.</summary>
public sealed record TelemetryBatchRequest<T>
{
    // DATA STRUCTURE DS-04 input: variable-length sequential batches, never object[][] (Microsoft, 2026c).
    public required SmartX.Shared.Telemetry.TelemetryPacket<T>[][] Batches { get; init; }
}
