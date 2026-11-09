namespace SmartX.Shared.Realtime;

public sealed record StreamReading(string SensorId, DateTimeOffset Timestamp, double Value, long Sequence, bool Critical);
public sealed record StreamDevice(string Id, string Name, string Category, string Zone, string Unit,
    string Health, double? Value, bool? Override, DateTimeOffset? LastSeen);
public sealed record CommandRequest(string SensorId, bool Enabled);
public sealed record SearchRequest(string Query, string Category = "", string Health = "");
public sealed record CommandEntry(Guid Id, string SensorId, bool Enabled, bool? PreviousOverride,
    DateTimeOffset Timestamp, string Context, bool Undone = false);
public sealed record SuggestedAction(string Kind, string SensorId, string Title, string Evidence,
    int Observations, bool? Enabled, string? TriggerId);
public sealed record QueueStatus(int StandardDepth, int CriticalDepth, long StandardProcessed,
    long CriticalProcessed, long Rejected, double LastLatencyMs);
public sealed record StreamSnapshot(DateTimeOffset Timestamp, int TotalDevices, int ActiveErrors,
    int DuplicateAlertsSuppressed, int SearchCount, int UndoDepth, int MatchingDevices,
    IReadOnlyList<StreamDevice> Devices, IReadOnlyList<StreamReading> Timeline,
    IReadOnlyList<CommandEntry> Commands, IReadOnlyList<SuggestedAction> Suggestions,
    string SelectedId, bool? SelectedOverride, Guid? UndoId, QueueStatus Queue);
