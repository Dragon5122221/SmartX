using System.Globalization;
using SmartX.Shared.Realtime;
namespace SmartXWebApi.Services;

public sealed partial class GatewayService
{
    // DS-P2-03: exact prior override (including automatic/null) is retained for LIFO undo.
    private readonly Stack<CommandEntry> _undo = new();
    private readonly List<CommandEntry> _commandAudit = [];
    private readonly Dictionary<string, bool> _overrides = new(StringComparer.OrdinalIgnoreCase);
    // DS-P2-04: composite key prevents equal timestamps from replacing another reading.
    private readonly Dictionary<string, SortedDictionary<(DateTimeOffset Time, long Sequence), StreamReading>> _timelines = new(StringComparer.OrdinalIgnoreCase);
    // DS-P2-05: unique active lifecycle keys, cleared on recovery, not a permanent suppression list.
    private readonly HashSet<string> _activeErrors = new(StringComparer.OrdinalIgnoreCase);
    private int _duplicatesSuppressed;
    private readonly Queue<(DateTimeOffset Time, string Query, string Context)> _searches = new();
    private readonly Dictionary<(string Context, string Target, bool Enabled), int> _patterns = new();
    private readonly Dictionary<(string Context, string Target), int> _queryPatterns = new();

    private void Observe<T>(SensorProfile sensor, TelemetryPacket<T> packet, bool critical)
    {
        double value = Convert.ToDouble(packet.Value, CultureInfo.InvariantCulture);
        if (!_timelines.TryGetValue(sensor.Id, out var timeline)) _timelines[sensor.Id] = timeline = new();
        timeline[(packet.Timestamp, packet.Sequence)] = new(sensor.Id, packet.Timestamp, value, packet.Sequence, critical);
        if (timeline.Count > 240) timeline.Remove(timeline.First().Key);
        var key = sensor.Id + ":Anomalous";
        if (critical) { if (!_activeErrors.Add(key)) _duplicatesSuppressed++; }
        else _activeErrors.Remove(key);
        _activeErrors.Remove(sensor.Id + ":Disconnected");
    }

    private void RefreshDisconnects()
    {
        foreach (var sensor in _sensors.Values)
        {
            string key = sensor.Id + ":Disconnected";
            if (SensorHealthService.Evaluate(sensor, false, DateTimeOffset.UtcNow) == SensorHealth.Disconnected)
            {
                if (_activeErrors.Add(key)) Alert(sensor.Id,"Disconnect",$"No packet received for more than {sensor.IntervalSeconds * 3} seconds. Check power and uplink.");
            }
            else _activeErrors.Remove(key);
        }
    }

    private string ContextKey() => string.Join("|", _activeErrors.Where(k => k.EndsWith(":Anomalous", StringComparison.Ordinal))
        .Select(k => { var id = k[..k.LastIndexOf(':')]; var s = _sensors[id]; return s.LocationId + ":" + s.Category; }).Distinct().Order().Take(8));

    public void RecordSearch(SearchRequest request)
    {
        lock (_gate)
        {
            var query = (request.Query ?? "").Trim();
            if (query.Length > 100) throw new GatewayException("Search must be at most 100 characters.");
            if (query.Length == 0) return;
            var context = ContextKey(); var now = DateTimeOffset.UtcNow;
            if (_searches.LastOrDefault() is var last && last.Query == query && now - last.Time < TimeSpan.FromSeconds(5)) return;
            if (_searches.Count == 200) _searches.Dequeue();
            _searches.Enqueue((now, query, context));
            // Learn only an unambiguous exact identifier/name, never infer intent from a broad query.
            var matches = _sensors.Values.Where(s => s.Id.Equals(query, StringComparison.OrdinalIgnoreCase) || s.Name.Equals(query, StringComparison.OrdinalIgnoreCase)).Take(2).ToArray();
            if (context.Length > 0 && matches.Length == 1)
            {
                var key = (context, matches[0].Id);
                if (_queryPatterns.Count < 1000 || _queryPatterns.ContainsKey(key)) _queryPatterns[key] = _queryPatterns.GetValueOrDefault(key) + 1;
            }
        }
    }

    public CommandEntry Command(CommandRequest request)
    {
        lock (_gate)
        {
            var sensor = RequireSensor(request.SensorId);
            if (sensor.Category != SensorCategory.Actuator) throw new GatewayException("Manual overrides are available only for actuator devices.");
            bool? previous = _overrides.TryGetValue(sensor.Id, out var old) ? old : null;
            if (previous == request.Enabled) throw new GatewayException("This override is already active.",409);
            if (_undo.Count >= 200) throw new GatewayException("Undo history is full. Undo an action before issuing another override.",409);
            var context = ContextKey();
            var command = new CommandEntry(Guid.NewGuid(), sensor.Id, request.Enabled, previous, DateTimeOffset.UtcNow, context);
            _overrides[sensor.Id] = request.Enabled; _undo.Push(command);
            if (_commandAudit.Count == 500) _commandAudit.RemoveAt(0);
            _commandAudit.Add(command);
            if (context.Length > 0)
            {
                var key = (context, sensor.Id, request.Enabled);
                if (_patterns.Count < 1000 || _patterns.ContainsKey(key)) _patterns[key] = _patterns.GetValueOrDefault(key) + 1;
            }
            return command;
        }
    }

    public CommandEntry Undo(Guid expectedId)
    {
        lock (_gate)
        {
            if (!_undo.TryPeek(out var last)) throw new GatewayException("There are no commands to undo.",409);
            if (last.Id != expectedId) throw new GatewayException("Another operator changed the stack. Refresh before undoing.",409);
            _undo.Pop();
            if (last.PreviousOverride is { } previous) _overrides[last.SensorId] = previous;
            else _overrides.Remove(last.SensorId);
            int index = _commandAudit.FindIndex(c => c.Id == last.Id);
            if (index >= 0) _commandAudit[index] = last with { Undone = true };
            // Undo is negative feedback: immediately withdraw this example from the learned model.
            var key = (last.Context, last.SensorId, last.Enabled);
            if (_patterns.TryGetValue(key, out int count)) { if (count <= 1) _patterns.Remove(key); else _patterns[key] = count - 1; }
            return last with { Undone = true };
        }
    }

    private List<SuggestedAction> Insights()
    {
        var result = new List<SuggestedAction>(); var context = ContextKey();
        foreach (var pattern in _patterns.Where(p => p.Key.Context == context && p.Value >= 3).OrderByDescending(p => p.Value).Take(3))
        {
            if (_overrides.TryGetValue(pattern.Key.Target, out var state) && state == pattern.Key.Enabled) continue;
            result.Add(new("Learned action",pattern.Key.Target,$"Review {(pattern.Key.Enabled ? "opening" : "closing")} {pattern.Key.Target}",
                $"You chose this action {pattern.Value} times during the same zone/category anomaly pattern. Confirm site conditions before applying.",pattern.Value,pattern.Key.Enabled,null));
        }
        foreach (var pattern in _queryPatterns.Where(p => p.Key.Context == context && p.Value >= 2).OrderByDescending(p => p.Value).Take(2))
            result.Add(new("Search pattern",pattern.Key.Target,$"Inspect {pattern.Key.Target}",$"This exact device was searched {pattern.Value} times while the same anomaly pattern was active.",pattern.Value,null,null));
        foreach (var (id, history) in _timelines)
        {
            if (result.Count >= 8) break;
            var s = _sensors[id];
            if (s.Category == SensorCategory.Actuator || history.Count < 6 || Row(s).Health != SensorHealth.Healthy) continue;
            var values = history.Values.TakeLast(6).ToArray();
            double[] x = values.Select(p => (p.Timestamp - values[0].Timestamp).TotalSeconds).ToArray();
            double avgX=x.Average(), avgY=values.Average(p=>p.Value), denominator=x.Sum(t=>(t-avgX)*(t-avgX));
            if (denominator <= 0) continue;
            double slope=values.Select((p,i)=>(x[i]-avgX)*(p.Value-avgY)).Sum()/denominator;
            double predicted=values[^1].Value+slope*30;
            double limit=s.Category==SensorCategory.Environmental?32:1500;
            double total=values.Sum(p=>Math.Pow(p.Value-avgY,2));
            double residual=values.Select((p,i)=>Math.Pow(p.Value-(avgY+slope*(x[i]-avgX)),2)).Sum();
            double fit=total>0 ? 1-residual/total : 0;
            bool crossing = slope > 0 && predicted >= limit || s.Category==SensorCategory.Environmental && slope<0 && predicted<=10;
            if (crossing && fit>=.8)
                result.Add(new("Trend forecast",id,$"Inspect {id} before a threshold crossing",
                    $"Six samples, linear fit R²={fit:F2}; estimated {predicted:F1} {s.Unit} in 30 seconds. A trend estimate, not a certainty.",6,null,id));
        }
        foreach (var key in _activeErrors.Order().Take(3))
        {
            if (result.Count >= 8) break;
            var id = key[..key.LastIndexOf(':')];
            result.Add(new("Active fault",id,$"Investigate {id}", key.EndsWith("Disconnected",StringComparison.Ordinal) ? "Three reporting intervals missed. Check power and uplink before issuing an override." : "Reading is outside the operating envelope. Inspect its timeline and nearby actuators.",1,null,id));
        }
        return result;
    }

    public StreamSnapshot Stream(QueueStatus queue, string? search, string? category, string? health, string? selected, int page = 1)
    {
        lock (_gate)
        {
            RefreshDisconnects();
            var matches = _sensors.Values.Where(s => (string.IsNullOrWhiteSpace(search) || s.Id.Contains(search,StringComparison.OrdinalIgnoreCase) || s.Name.Contains(search,StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrWhiteSpace(category) || s.Category.ToString().Equals(category,StringComparison.OrdinalIgnoreCase))).Select(s =>
                new StreamDevice(s.Id,s.Name,s.Category.ToString(),s.LocationId,s.Unit,Row(s).Health.ToString(),
                    _timelines.TryGetValue(s.Id,out var h) && h.Count>0 ? h.Last().Value.Value : null,
                    _overrides.TryGetValue(s.Id,out var value)?value:null,s.LastSeen))
                .Where(s => string.IsNullOrWhiteSpace(health) || s.Health.Equals(health,StringComparison.OrdinalIgnoreCase)).ToList();
            string id = string.IsNullOrWhiteSpace(selected) ? "SX-0001" : RequireSensor(selected).Id;
            return new(DateTimeOffset.UtcNow,_sensors.Count,_activeErrors.Count,_duplicatesSuppressed,_searches.Count,_undo.Count,matches.Count,
                matches.Skip((Math.Clamp(page,1,200)-1)*12).Take(12).ToList(),
                _timelines.TryGetValue(id,out var timeline)?timeline.Values.ToList():[],_commandAudit.TakeLast(50).Reverse().ToList(),Insights(),id,_overrides.TryGetValue(id,out var requested)?requested:null,_undo.TryPeek(out var top)?top.Id:null,queue);
        }
    }
}
