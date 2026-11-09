# Part 2 — architecture and algorithm notes

## End-to-end processing

The live device emulator prepares typed packets every three seconds and posts JSON over HTTP. `TelemetryController` submits individual packets to `TelemetryDispatcher`. A bounded FIFO serves normal data, and a priority heap serves critical readings first. The consumer applies the original `GatewayService` validation and typed stores. Accepted readings also update per-device ordered history and current error membership. The asynchronous MVC proxy returns coherent snapshots to the client.

No device browser calls the in-memory service directly. The only direct data seeding is the startup historical fixture and the isolated verification suite. The simulator uses the configured `SmartX:SimulationApiUrl`, or the server's bound HTTP address. For HTTPS-only hosting, explicitly configure a reachable, trusted URL.

## Collection ownership and costs

| Structure | Owner and purpose | Cost / bound |
|---|---|---|
| `Queue<Work>` | Dispatcher standard telemetry | Amortised O(1) enqueue/dequeue |
| `PriorityQueue<Work,(int,long)>` | Critical telemetry, stable arrival tie-breaker | O(log n); shares 16,384 pending capacity |
| `Stack<CommandEntry>` | Previous manual states | O(1) push/pop; 200 active entries |
| `Dictionary<string,SensorProfile>` | Canonical live registry inherited from Part 1 | Expected O(1) lookup; 5,000 devices |
| `Dictionary<string,SortedDictionary<(DateTimeOffset,long),StreamReading>>` | Device-local timeline index | Expected O(1) device lookup, O(log h) insert; h ≤ 240 |
| `HashSet<string>` | Active unique anomaly/disconnect keys | Expected O(1) add/remove/membership |
| `Queue<(time,query,context)>` | Recent operator searches | 200 entries |
| Pattern dictionaries | Context → action/queried target support counts | 1,000 keys per map |

Dictionary performance is expected/average rather than a promise of collision-free worst-case O(1). Category/name filtering necessarily scans the bounded registry. A sorted timeline avoids sorting its samples again on every snapshot. Forecasting examines six samples per eligible sensor; its fleet pass is O(d) with fixed bounded sample size, although obtaining tails from tree enumeration still traverses each bounded history. It is deliberately transparent rather than presented as machine learning infrastructure.

## Synchronisation and lifecycle

The gateway's existing lock owns device profiles, telemetry, overrides, histories and learned evidence together. The dispatcher uses a separate queue lock and releases it before invoking gateway work; no reverse nested lock acquisition exists. Snapshot data is copied into transport lists, not exposed as mutable internal collections. Request completion sources run continuations asynchronously.

Normal traffic is FIFO within its queue. Urgent work wins the next available consumer slot, with stable ordering among equal priorities. This is non-preemptive scheduling; it does not make a zero-latency guarantee. Persistent critical traffic can delay normal traffic. Backpressure rejects excess waiting work with 429. Shutdown faults pending completions. Cancelling an HTTP wait does not silently remove an accepted device packet.

Original validation rejects backwards timestamps and non-increasing device sequences. An urgent later packet can overtake pending older samples for the same device; those stale samples are rejected explicitly to protect current state. It is a deliberate latest-state policy, not a claim of lossless archival ingestion. A future durable ingestion version could separate event-time archival acceptance from latest-state advancement.

## Command semantics

Manual commands apply only to actuators. They modify a requested override, not the last observed telemetry sample. Simulated devices honour the override in the next packet. The stack stores the exact prior override (`true`, `false` or absent/automatic). Undo checks the caller's expected top ID before popping, preventing stale double-clicks or concurrent tabs from undoing an unintended command. UI confirmation is required before applying an override; undo itself is immediately reversible by issuing another command.

The prototype has one shared operator context, matching its single-user local scope. For multi-user deployment, identities, per-operator histories and authorisation must be added. An undo removes that command's training example; an undone action must not teach the engine that the operator still prefers it.

## Suggestion algorithm

1. Build a context signature from unique active anomaly zone/category pairs, sorted consistently (maximum eight pairs). Device IDs remain in the underlying alert set. This generalises repeat choices within an operational zone without pretending to know physical causality.
2. Exact device-ID/name searches create `(context,target)` counts. At least two matching interactions are required; identical searches within five seconds are deduplicated.
3. Commands create `(context,target,desired-state)` counts. At least three retained examples are required. An action already active is not suggested. Undo decreases support.
4. For currently healthy numeric devices, regress six timestamped readings against elapsed seconds. Extrapolate 30 seconds. Require R² ≥ 0.8 and an impending upper/lower threshold crossing.
5. Include measured faults separately, with clear labels and evidence. Rank learned actions first, followed by query patterns, qualifying trends and active faults; bound the displayed list to eight.

Counts are **support**, not calibrated probabilities. A high R² describes fit to six points, not certainty of future behaviour. Pattern counts accumulate for this API session until restart; the recent search queue is bounded independently. The system neither learns physical wiring nor automatically operates valves. Recommendations lead to inspection or a confirmation dialog.

## Client state and design

The command workspace uses a continuous three-part structure: searchable fleet, signal detail with adjacent control, and evidence-led suggestions. Command audit and experiments follow in reading order. The palette extends the supplied sage/charcoal identity with amber for attention; text labels accompany status colours. A custom SVG chart preserves real timestamp spacing and includes point tooltips. Small screens stack the panes and contain horizontal audit-table scrolling.

Workspace navigation fetches MVC markup asynchronously and changes only the main content. Page-local inputs and choices are retained by URL. A generation token prevents stale API responses from rendering into a different view. Live requests are sequential, avoiding overlapping refresh storms. Native links still support opening a new tab. Reduced-motion preferences, focus outlines, a skip link, labelled fields and a native confirmation dialog support accessibility.
