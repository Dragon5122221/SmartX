namespace SmartX.Shared.Telemetry;

/// <summary>DATA STRUCTURE DS-02: immutable watt value supporting meaningful overloaded arithmetic. See Microsoft (2026b).</summary>
public readonly record struct PowerReading(int Watts)
{
    /// <summary>OP-01: aggregate two watt readings with explicit overflow detection.</summary>
    public static PowerReading operator +(PowerReading left, PowerReading right) => new(checked(left.Watts + right.Watts));
    /// <summary>OP-02: calculate a signed watt delta; a negative result is meaningful.</summary>
    public static PowerReading operator -(PowerReading left, PowerReading right) => new(checked(left.Watts - right.Watts));
}
