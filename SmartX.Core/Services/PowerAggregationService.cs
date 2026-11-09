using SmartX.Shared.Telemetry;
using SmartX.Shared.Contracts.Telemetry;
namespace SmartX.Core.Services;

/// <summary>PWR-01: application-level demonstration of overloaded watt arithmetic.</summary>
public static class PowerAggregationService
{
    /// <summary>PWR-02: return operands, total and signed delta with original sample timestamps.</summary>
    public static PowerComparisonResponse Compare(TelemetryPacket<int> left, TelemetryPacket<int> right)
    {
        var meter1 = new PowerReading(left.Value);
        var meter2 = new PowerReading(right.Value);
        // DATA STRUCTURE DS-02 USE: these invoke our operators, not ad-hoc arithmetic in the UI.
        var total = meter1 + meter2;
        var delta = meter1 - meter2;
        return new(left.Value, right.Value, total.Watts, delta.Watts, left.Timestamp, right.Timestamp);
    }
}
