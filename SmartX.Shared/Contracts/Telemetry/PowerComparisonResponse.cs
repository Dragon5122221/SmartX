namespace SmartX.Shared.Contracts.Telemetry;

/// <summary>Typed transport model; serialized consistently by the API and MVC client.</summary>
public sealed record PowerComparisonResponse(int LeftWatts, int RightWatts, int TotalWatts, int DeltaWatts, DateTimeOffset LeftTime, DateTimeOffset RightTime);
