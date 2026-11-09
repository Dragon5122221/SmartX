using SmartX.Shared.Telemetry;
namespace SmartX.Core.Validation;

/// <summary>PKT-01: generic validation using a typed value predicate, without boxing (Microsoft, 2026a).</summary>
public static class TelemetryPacketValidator<T>
{
    /// <summary>PKT-02: reject stale ordering, future/old timestamps and invalid typed values.</summary>
    public static string? Validate(TelemetryPacket<T> packet, TelemetryPacket<T>? previous, Func<T, bool> validValue, DateTimeOffset now)
    {
        if (packet.Sequence < 0 || (previous is not null && packet.Sequence <= previous.Sequence)) return "Sequence must increase for this sensor.";
        if (packet.Timestamp > now.AddMinutes(1) || packet.Timestamp < now.AddDays(-7)) return "Timestamp must be within the last 7 days and at most 1 minute ahead.";
        if (previous is not null && packet.Timestamp < previous.Timestamp) return "Timestamp cannot move backwards.";
        if (!validValue(packet.Value)) return "Value is invalid for this channel.";
        return null;
    }
}
