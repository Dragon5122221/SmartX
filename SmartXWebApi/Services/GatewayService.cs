using System.Diagnostics;
using System.Globalization;
using SmartX.Core.Simulation;
namespace SmartXWebApi.Services;

/// <summary>GATE-01: single in-memory owner of coherent sensor, ingestion and investigation state.</summary>
public sealed partial class GatewayService
{
    private readonly object _gate = new();
    // DATA STRUCTURE DS-10: registry dictionary; O(1)-average identifier lookup, snapshots under _gate.
    private readonly Dictionary<string, SensorProfile> _sensors = new(StringComparer.OrdinalIgnoreCase);
    // DATA STRUCTURE DS-06 USE: three closed generic stores; value payloads are never boxed.
    private readonly TelemetryStore<float> _floats = new();
    private readonly TelemetryStore<int> _integers = new();
    private readonly TelemetryStore<bool> _booleans = new();
    // DATA STRUCTURE DS-07: required user-reported issue list and bounded chronological alerts.
    private readonly List<ReportedIssue> _issues = [];
    private readonly List<TelemetryAlert> _alerts = [];
    // DATA STRUCTURE DS-11: anomaly membership and alert throttling indexes.
    private readonly HashSet<string> _anomalies = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, DateTimeOffset> _lastAlert = new();
    private DeploymentNode _root = TelemetrySeedGenerator.Tree();
    private long _accepted, _rejected, _ticks;
    private double _lastMilliseconds;
    private bool _running = true;
    private SimulationScenario _scenario = SimulationScenario.Mixed;
    private const int Seed = 7312;

    /// <summary>GATE-02: seed profiles and 28,800 packets through the real validation/collection pipeline.</summary>
    public GatewayService()
    {
        foreach (var sensor in TelemetrySeedGenerator.Sensors()) _sensors.Add(sensor.Id, sensor);
        for (int b = 0; b < 3; b++) Generate(b * 8, 8, DateTimeOffset.UtcNow.AddSeconds(-72 + b * 24), false);
    }

    /// <summary>REG-04: list a bounded searchable snapshot, with health calculated at read time.</summary>
    public List<SensorRow> Sensors(string? search = null, string? category = null, int page = 1)
    {
        lock (_gate) return _sensors.Values.Where(s =>
            (string.IsNullOrWhiteSpace(search) || s.Id.Contains(search, StringComparison.OrdinalIgnoreCase) || s.Name.Contains(search, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrWhiteSpace(category) || s.Category.ToString().Equals(category, StringComparison.OrdinalIgnoreCase)))
            .Skip((Math.Clamp(page, 1, 1000) - 1) * 30).Take(30).Select(Row).ToList();
    }
    /// <summary>REG-05: retrieve one registered sensor or return a deliberate 404.</summary>
    public SensorRow Sensor(string id) { lock (_gate) return Row(RequireSensor(id)); }
    /// <summary>REG-06: validate and create/update a profile while enforcing registry uniqueness.</summary>
    public SensorProfile Register(RegisterSensorRequest request, string? updateId = null)
    {
        lock (_gate)
        {
            var result = SensorProfileValidator.Validate(request, Locations(_root).Any(x => x.Id == request.LocationId && x.Enabled));
            if (!result.IsValid) throw new GatewayException(string.Join(" ", result.Issues.Select(x => $"{x.Path}: {x.Message}")));
            SensorProfile? existing = updateId is null ? null : RequireSensor(updateId);
            if (existing is null && _sensors.ContainsKey(request.Id)) throw new GatewayException("Device identifier already exists.", 409);
            if (existing is null && _sensors.Count >= 5000) throw new GatewayException("The simulation gateway is limited to 5,000 devices.", 409);
            if (existing is not null && (!existing.Id.Equals(request.Id, StringComparison.OrdinalIgnoreCase) || existing.Category != request.Category))
                throw new GatewayException("Identifier and category are immutable after registration; register a new sensor to change its data type.");
            var profile = new SensorProfile { Id = request.Id, Name = request.Name.Trim(), LocationId = request.LocationId,
                Category = request.Category, ValueKind = (TelemetryValueKind)(int)request.Category,
                Unit = request.Category == SensorCategory.Environmental ? "°C" : request.Category == SensorCategory.PowerConsumption ? "W" : "state",
                IntervalSeconds = request.IntervalSeconds, LastSeen = existing?.LastSeen,
                RegisteredAt = existing?.RegisteredAt ?? DateTimeOffset.UtcNow, Simulated = existing?.Simulated ?? false };
            _sensors[profile.Id] = profile; return profile;
        }
    }
    /// <summary>TREE-03: return a deep copy so callers cannot mutate the validated internal tree.</summary>
    public DeploymentNode Deployment() { lock (_gate) return Clone(_root); }
    /// <summary>TREE-04: validate recursively and reject removal of locations used by registered sensors.</summary>
    public ValidationResult SetDeployment(DeploymentNode root, bool save)
    {
        lock (_gate)
        {
            var result = new DeploymentTreeValidator().Validate(root);
            if (save && result.IsValid)
            {
                var enabled = Locations(root).Where(n => n.Enabled).Select(n => n.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
                if (_sensors.Values.Any(s => !enabled.Contains(s.LocationId))) result.Issues.Add(new("Root", "Cannot remove or disable a location assigned to registered sensors."));
                else _root = Clone(root);
            }
            return result;
        }
    }
    /// <summary>ING-01: dispatch concrete float batches, rejecting NaN/infinite sensor values.</summary>
    public IngestionResult IngestFloat(TelemetryBatchRequest<float> input) { lock (_gate) return Ingest(input, _floats, TelemetryValueKind.Float, float.IsFinite, v => v is > 32 or < 10); }
    /// <summary>ING-02: dispatch integer watt batches; negative meter load is invalid for this simulation.</summary>
    public IngestionResult IngestInteger(TelemetryBatchRequest<int> input) { lock (_gate) return Ingest(input, _integers, TelemetryValueKind.Integer, v => v >= 0, v => v > 1500); }
    /// <summary>ING-03: dispatch boolean valve batches without conversion into strings or numeric payloads.</summary>
    public IngestionResult IngestBoolean(TelemetryBatchRequest<bool> input) { lock (_gate) return Ingest(input, _booleans, TelemetryValueKind.Boolean, _ => true, _ => false); }

    /// <summary>ING-04: validate sequential raw batches, retain typed packets, and update anomaly evidence.</summary>
    private IngestionResult Ingest<T>(TelemetryBatchRequest<T> input, TelemetryStore<T> store, TelemetryValueKind kind, Func<T, bool> valid, Func<T, bool> anomaly)
    {
        if (input.Batches is null || input.Batches.Length is < 1 or > 64 || input.Batches.Any(b => b is null || b.Length > 4096) || input.Batches.Sum(b => b.Length) > 8192)
            throw new GatewayException("Use 1–64 batches, at most 4,096 packets each and 8,192 packets per request.");
        var watch = Stopwatch.StartNew(); var errors = new List<ValidationIssue>(); int accepted = 0, rejected = 0;
        for (int b = 0; b < input.Batches.Length; b++)
        {
            // DATA STRUCTURE DS-12: temporary typed grouping lists preserve packet order per sensor.
            var groups = new Dictionary<string, List<TelemetryPacket<T>>>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < input.Batches[b].Length; i++)
            {
                var packet = input.Batches[b][i]; string? error = null;
                if (packet is null || string.IsNullOrWhiteSpace(packet.SensorId)) error = "A sensor identifier is required.";
                else if (!_sensors.TryGetValue(packet.SensorId, out var sensor)) error = "Sensor is not registered.";
                else if (sensor.ValueKind != kind) error = "Payload type does not match sensor category.";
                else
                {
                    groups.TryGetValue(packet.SensorId, out var group);
                    error = TelemetryPacketValidator<T>.Validate(packet, group?.LastOrDefault() ?? store.Latest(packet.SensorId), valid, DateTimeOffset.UtcNow);
                    if (group?.Count >= 256) error = "A sensor is limited to 256 readings in one raw batch.";
                    if (error is null)
                    {
                        if (group is null) groups[packet.SensorId] = group = [];
                        group.Add(packet); accepted++;
                        Observe(sensor, packet, anomaly(packet.Value));
                        _sensors[sensor.Id] = sensor with { LastSeen = DateTimeOffset.UtcNow };
                        if (anomaly(packet.Value)) { if (_anomalies.Add(sensor.Id)) Alert(sensor.Id, "Spike", "Reading is outside the configured operating envelope. Inspect the trend and deployment context."); }
                        else _anomalies.Remove(sensor.Id);
                    }
                }
                if (error is not null) { rejected++; if (errors.Count < 100) errors.Add(new($"Batches[{b}][{i}]", error)); }
            }
            // DATA STRUCTURE DS-04 USE: each sensor's raw sequence enters the custom jagged ring.
            foreach (var (id, values) in groups) store.Append(id, values.ToArray());
        }
        watch.Stop(); _accepted += accepted; _rejected += rejected; _lastMilliseconds = watch.Elapsed.TotalMilliseconds;
        return new(accepted, rejected, _lastMilliseconds, errors);
    }
    /// <summary>HIST-01: expose typed float history after custom array-to-list transfer.</summary>
    public TelemetryHistoryResponse<float> FloatHistory(string id) { lock (_gate) { RequireKind(id, TelemetryValueKind.Float); return _floats.History(id); } }
    /// <summary>HIST-02: expose typed integer history after custom array-to-list transfer.</summary>
    public TelemetryHistoryResponse<int> IntegerHistory(string id) { lock (_gate) { RequireKind(id, TelemetryValueKind.Integer); return _integers.History(id); } }
    /// <summary>HIST-03: expose typed boolean history after custom array-to-list transfer.</summary>
    public TelemetryHistoryResponse<bool> BooleanHistory(string id) { lock (_gate) { RequireKind(id, TelemetryValueKind.Boolean); return _booleans.History(id); } }
    /// <summary>PWR-03: compare registered power meters using the latest integer packets.</summary>
    public PowerComparisonResponse Compare(PowerComparisonRequest request)
    {
        lock (_gate)
        {
            RequireKind(request.LeftSensorId, TelemetryValueKind.Integer); RequireKind(request.RightSensorId, TelemetryValueKind.Integer);
            return PowerAggregationService.Compare(_integers.Latest(request.LeftSensorId) ?? throw new GatewayException("Left meter has no readings."),
                _integers.Latest(request.RightSensorId) ?? throw new GatewayException("Right meter has no readings."));
        }
    }
    /// <summary>DASH-01: combine bounded views for live investigation, keeping conversion out of ingestion.</summary>
    public DashboardSnapshot Dashboard(string? sensorId = null)
    {
        lock (_gate)
        {
            // DATA STRUCTURE DS-18: snapshot list supports counts and priority ordering under the gateway lock.
            var rows = _sensors.Values.Select(Row).ToList();
            var id = string.IsNullOrWhiteSpace(sensorId) ? "SX-0001" : RequireSensor(sensorId).Id;
            var sensor = RequireSensor(id);
            var trend = sensor.ValueKind switch {
                TelemetryValueKind.Float => _floats.History(id).Readings.Select(p => new ChartPoint(p.Timestamp, p.Value)).ToList(),
                TelemetryValueKind.Integer => _integers.History(id).Readings.Select(p => new ChartPoint(p.Timestamp, p.Value)).ToList(),
                _ => _booleans.History(id).Readings.Select(p => new ChartPoint(p.Timestamp, p.Value ? 1 : 0)).ToList() };
            return new(_sensors.Count, rows.Count(x => x.Health == SensorHealth.Healthy), rows.Count(x => x.Health == SensorHealth.Anomalous),
                rows.Count(x => x.Health == SensorHealth.Disconnected), _accepted, _rejected, _floats.Count + _integers.Count + _booleans.Count,
                _lastMilliseconds, DateTimeOffset.UtcNow, _alerts.TakeLast(12).Reverse().ToList(),
                rows.OrderByDescending(x => x.Health == SensorHealth.Disconnected || x.Health == SensorHealth.Anomalous).Take(6).ToList(), trend, id, Status());
        }
    }
    /// <summary>ISSUE-01: append a validated user-reported issue to the explicitly required List.</summary>
    public ReportedIssue Report(ReportIssueRequest request)
    {
        lock (_gate)
        {
            RequireSensor(request.SensorId);
            if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Length > 1000) throw new GatewayException("Issue description must be 1–1,000 characters.");
            if (_issues.Count == 500) _issues.RemoveAt(0);
            var issue = new ReportedIssue(Guid.NewGuid(), request.SensorId, request.Description.Trim(), DateTimeOffset.UtcNow);
            _issues.Add(issue); return issue;
        }
    }
    /// <summary>ISSUE-02: snapshot recent reported issues for the investigation journal.</summary>
    public List<ReportedIssue> Issues() { lock (_gate) return _issues.AsEnumerable().Reverse().Take(100).ToList(); }
    /// <summary>ISSUE-03: resolve a known issue without deleting its audit context.</summary>
    public void Resolve(Guid id) { lock (_gate) { int i = _issues.FindIndex(x => x.Id == id); if (i < 0) throw new GatewayException("Issue not found.", 404); _issues[i] = _issues[i] with { Resolved = true }; } }
    /// <summary>ALERT-01: acknowledge a displayed alert while retaining its evidence.</summary>
    public void Acknowledge(Guid id) { lock (_gate) { int i = _alerts.FindIndex(x => x.Id == id); if (i < 0) throw new GatewayException("Alert not found.", 404); _alerts[i] = _alerts[i] with { Acknowledged = true }; } }
    /// <summary>SIM-01: read simulation state under the same lock as telemetry mutations.</summary>
    public SimulationStatus Status() { lock (_gate) return new(_running, _scenario, _sensors.Values.Count(s => s.Simulated), _ticks, Seed); }
    /// <summary>SIM-02: change fault scenario and resume the single background producer.</summary>
    public void Start(SimulationScenario scenario) { lock (_gate) { if (!Enum.IsDefined(scenario)) throw new GatewayException("Unknown scenario."); _scenario = scenario; _running = true; } }
    /// <summary>SIM-03: pause generation; devices then become disconnected according to their cadence.</summary>
    public void Stop() { lock (_gate) _running = false; }
    /// <summary>SIM-04: produce one deterministic tick then evaluate time-based disconnect alerts.</summary>
    public void Tick(Action<object>? emit = null)
    {
        lock (_gate)
        {
            if (_running) { Generate(24 + _ticks, 1, DateTimeOffset.UtcNow, true, emit); _ticks++; }
            RefreshDisconnects();
        }
    }
    /// <summary>SIM-05: create typed jagged batches; seed and live data both use the assessed ingestion path.</summary>
    private void Generate(long start, int count, DateTimeOffset timestamp, bool faults, Action<object>? emit = null)
    {
        // DATA STRUCTURE DS-17: separate typed producer lists become raw jagged input batches.
        var floats = new List<TelemetryPacket<float>>(); var integers = new List<TelemetryPacket<int>>(); var booleans = new List<TelemetryPacket<bool>>();
        int index = 0;
        foreach (var s in _sensors.Values.Where(s => s.Simulated).ToArray())
        {
            int n = index++;
            if (faults && (_scenario is SimulationScenario.Mixed or SimulationScenario.Disconnects) && n % 41 == 0) continue;
            for (int k = 0; k < count; k++)
            {
                long sequence = start + k;
                bool spike = faults && (_scenario is SimulationScenario.Mixed or SimulationScenario.Spikes) && n % 29 == 0 && _ticks % 8 < 3;
                var time = timestamp.AddSeconds(k * 3);
                if (s.ValueKind == TelemetryValueKind.Float) floats.Add(new() { SensorId = s.Id, Sequence = sequence, Timestamp = time, Value = (float)(23 + 2 * Math.Sin((sequence + n + Seed) * .24) + (spike ? 15 : 0)) });
                else if (s.ValueKind == TelemetryValueKind.Integer) integers.Add(new() { SensorId = s.Id, Sequence = sequence, Timestamp = time, Value = 420 + (int)(80 * Math.Sin((sequence + n) * .18)) + (spike ? 1600 : 0) });
                else booleans.Add(new() { SensorId = s.Id, Sequence = sequence, Timestamp = time, Value = _overrides.TryGetValue(s.Id, out var forced) ? forced : (sequence / 5 + n) % 2 == 0 });
            }
        }
        if (floats.Count > 0) { var batch = new TelemetryBatchRequest<float> { Batches = [floats.ToArray()] }; if (emit is null) IngestFloat(batch); else emit(batch); }
        if (integers.Count > 0) { var batch = new TelemetryBatchRequest<int> { Batches = [integers.ToArray()] }; if (emit is null) IngestInteger(batch); else emit(batch); }
        if (booleans.Count > 0) { var batch = new TelemetryBatchRequest<bool> { Batches = [booleans.ToArray()] }; if (emit is null) IngestBoolean(batch); else emit(batch); }
    }
    /// <summary>ALERT-02: throttle repeated sensor/kind alerts to one per minute and cap retained evidence.</summary>
    private void Alert(string id, string kind, string message)
    {
        string key = id + ":" + kind; var now = DateTimeOffset.UtcNow;
        if (_lastAlert.TryGetValue(key, out var last) && now - last < TimeSpan.FromMinutes(1)) return;
        _lastAlert[key] = now;
        if (_alerts.Count == 500) _alerts.RemoveAt(0);
        _alerts.Add(new(Guid.NewGuid(), id, kind, message, now));
    }
    /// <summary>REG-07: enforce sensor existence for every dependent workflow.</summary>
    private SensorProfile RequireSensor(string id) => !string.IsNullOrWhiteSpace(id) && _sensors.TryGetValue(id, out var s) ? s : throw new GatewayException("Sensor not found.", 404);
    /// <summary>PKT-03: reject use of a typed route against an incompatible sensor.</summary>
    private void RequireKind(string id, TelemetryValueKind kind) { if (RequireSensor(id).ValueKind != kind) throw new GatewayException("Sensor type does not match this route."); }
    /// <summary>DASH-02: format display-only values without changing the typed processing stores.</summary>
    private SensorRow Row(SensorProfile s)
    {
        string value = s.ValueKind switch {
            TelemetryValueKind.Float => _floats.Latest(s.Id)?.Value.ToString("F1", CultureInfo.InvariantCulture) ?? "—",
            TelemetryValueKind.Integer => _integers.Latest(s.Id)?.Value.ToString(CultureInfo.InvariantCulture) ?? "—",
            _ => _booleans.Latest(s.Id) is { } p ? (p.Value ? "OPEN" : "CLOSED") : "—" };
        return new(s, SensorHealthService.Evaluate(s, _anomalies.Contains(s.Id), DateTimeOffset.UtcNow), value);
    }
    /// <summary>TREE-05: clone validated nodes recursively so child lists have independent ownership.</summary>
    private static DeploymentNode Clone(DeploymentNode node) => node with { Children = node.Children.Select(Clone).ToList() };
    /// <summary>TREE-06: traverse an already validated bounded deployment tree for location lookup.</summary>
    private static IEnumerable<DeploymentNode> Locations(DeploymentNode node)
    { yield return node; foreach (var child in node.Children) foreach (var item in Locations(child)) yield return item; }
}
