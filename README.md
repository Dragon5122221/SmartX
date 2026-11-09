# Smart-X · Signal to action

**PROG7312 · Part 2 — Real-Time Processing and Device Management**

A .NET 10 greenhouse operations system with an ASP.NET Core API and an ASP.NET Core MVC client. Part 1's sensor registry, typed ingestion, live operations, investigation, encrypted attachments, issue journal, power comparison and deployment editor remain available. Part 2 adds a bespoke command desk with priority telemetry processing, reversible actuator overrides, chronological history and explainable suggestions.

## Start on Windows

1. Install the **.NET 10 SDK** and, for Visual Studio, its **ASP.NET and web development** workload.
2. Extract this ZIP to a short path such as `C:\Projects\SmartX-Part2`. Do not run inside the ZIP.
3. Double-click **Start-SmartX.cmd**. It builds the solution and opens two server windows.
4. When both report `Now listening`, open **http://localhost:5080**. Leave both windows open.
5. Open **Command stream**. No hardware, API key, database or account is required.

The launcher uses local HTTP to avoid development-certificate friction. For Visual Studio, open `SmartX.slnx`, configure both web projects as startup projects, use their `https` profiles, and trust the development certificate with `dotnet dev-certs https --trust`. The MVC default API URL remains `https://localhost:7034/`. The API HTTPS profile also binds HTTP port 5267 so the local device emulator can post packets.

### Manual CMD startup

From the solution directory, run:

```bat
dotnet build SmartX.slnx -m:1
set SmartX__AllowHttp=true
dotnet run --project SmartXWebApi --no-launch-profile --urls http://localhost:5267
```

In a second CMD window, from the same directory:

```bat
set SmartX__AllowHttp=true
set SmartXApi__BaseUrl=http://localhost:5267/
dotnet run --project SmartXMVC --no-launch-profile --urls http://localhost:5080
```

The initial registry contains 1,200 devices and 28,800 historical samples. Initial historical seeding is local startup data. **Every subsequent simulated live sample is sent through HTTP to the same telemetry endpoints as an external device.** Normal samples enter a FIFO; critical samples enter a stable priority queue.

## Workspaces

- **Gateway:** three architectural pillars and entry points.
- **Operations:** preserved Part 1 monitoring and simulation scenarios.
- **Command stream:** searchable fleet, observed signal, confirmed valve overrides, undo, insights, command audit, CSV export and controlled experiments.
- **Network topology:** navigable preview of the current deployment hierarchy. Actual mesh routing is deliberately reserved for the final PoE.
- **Sensor registry / Ingestion lab / Deployment / Issue journal:** retained Part 1 workflows.

Main workspace links use asynchronous navigation. Search text, selected devices, pagination, pause state, drafts and scroll positions survive page switching within the open tab. Backend state is owned by singleton services and is independent of navigation. A full browser reload clears unsaved drafts; an API restart clears in-memory telemetry, commands and learned patterns. File uploads must finish or be cancelled before switching views.

## Test the assessed collections

### Queue and priority queue

1. Open **Command stream**, scroll to **Controlled experiment** and select **Send 120 normal packets**. The server registers `DEMO-FIFO` and sends valid integer telemetry through HTTP. Check the accepted count and the cumulative FIFO counter.
2. Select **Inject critical power spike**. `DEMO-POWER` sends 2,200 W; the critical counter increases and an active fault appears.
3. Repeat the spike: the unique active error count does not grow for that same error, while duplicate suppression increases.
4. Select **Recover demo meter**: a normal reading clears the active anomaly.
5. Run the verification executable below for a deterministic backlog test: it enqueues three standard and two urgent packets before starting the consumer and verifies `999, 998, 1, 2, 3` processing order. UI bursts drain too quickly to guarantee a visibly nonzero queue gauge.

Critical means environmental readings above 32 °C/below 10 °C, or power above 1,500 W. Critical packets win at the next dequeue; already executing work cannot be preempted. Equal-priority items use an arrival counter for stability. Capacity is 16,384 waiting packets; excess requests receive HTTP 429. The original monotonic device-sequence rules remain: a later critical packet can supersede earlier queued normal packets **from the same device**; stale packets are explicitly rejected, not used to roll its state backwards. Separate devices are used by the experiments to avoid this effect.

### Dictionary, sorted dictionary and hash set

- Search an exact identifier, such as `SX-0003`, and inspect its signal. Registry access uses the existing case-insensitive `Dictionary<string, SensorProfile>`.
- Filter by category and health. Filter scans are separate from constant-average-time identifier lookups.
- Signal history uses a per-device `SortedDictionary<(timestamp, sequence), StreamReading>`. Equal timestamps remain distinct; each device retains at most 240 readings.
- The `HashSet` stores unique `device:error-kind` lifecycle keys. Recovery removes keys; a later recurrence can become active again.
- Stop the simulation in Operations and wait three device intervals to inspect disconnected nodes; restart it to observe recovery.

### Stack and undo

1. Select actuator `SX-0003`. Open it and confirm the dialog.
2. Close it and confirm. The device emulator honours this manual state on its next HTTP telemetry tick.
3. Undo once: the prior manual-open state returns. Undo again: automatic simulation resumes.
4. The UI separates **requested** override from **observed** telemetry. This is a simulator, not a claim of physical hardware acknowledgement.
5. The undo request includes the expected top command ID. A stale request receives 409 and cannot undo another operator's newer command.

### Recommendations

- **Active fault:** current measured error; explicitly labelled as a fault rather than a prediction.
- **Trend forecast:** click **Generate rising trend**. Six increasing readings below 32 °C support a 30-second linear forecast. Only fits with R² ≥ 0.8 that cross a threshold are shown. Wait six seconds between experiments.
- **Search pattern:** search the same exact device during the same active zone/category anomaly pattern at least twice, with five seconds between identical searches. Exact names also work. Broad searches do not falsely imply a specific device preference.
- **Learned action:** maintain a stable active anomaly (stop the mixed simulator, wait for old anomalies to recover with a Normal run first, then stop and inject the demo spike). Open and close `SX-0003` three times. While closed, the engine can suggest reviewing open, based on three prior choices under that same anomaly context. Undo withdraws the corresponding training example. Clearing the anomaly removes context-dependent suggestions.

Suggestions are reviewable, explain their evidence and never execute an override automatically. This avoids treating historical correlations as operational certainty. See [algorithm notes](docs/PART2_ARCHITECTURE.md) for limitations.

## Verification

```bat
dotnet build SmartX.slnx -m:1
dotnet run --project SmartX.Verification
```

No test NuGet package is required. This executable checks the original typed collections, validation and encrypted attachment handling, plus priority ordering, undo conflicts, error lifecycle, filters, forecasts and learned patterns. Optional standard-library HTTP checks, with both applications running: `python scripts/http_smoke.py` (start with a fresh API instance).

See [current validation evidence](docs/PART2_VALIDATION.md). Older Part 1 reports are retained for traceability and are not evidence of the new implementation.

## Docker (optional)

```sh
docker compose up --build
```

Open http://localhost:5080. Compose binds host ports to loopback. Attachments and API encryption keys use named volumes; preserve both together. Runtime operational state remains in memory. Docker files are provided; consult the validation report for whether Docker was executed in the delivery environment.

## Repository and submission

The supplied Git history and its configured origin are retained. This package adds genuine implementation commits under the coding agent's identity; it does not invent student collaborators. **No remote push has been performed.** See [Git handoff](docs/GIT_HANDOFF.md) before pushing. The lecturer's requirement for a GitHub repository, collaboration evidence and a video/presentation still applies; a ZIP alone does not satisfy those requirements.

## Scope and limits

This is a local academic device simulator. It has no user authentication, physical device transport or durable operational database. Do not expose its command endpoints on the public internet. MVC writes use antiforgery tokens, bounded inputs and an allowlisted proxy. Queues, timelines, searches and commands are intentionally bounded. The undo stack retains 200 active entries, command audit 500 entries (latest 50 shown/exported), search history 200 entries and each learned-pattern map 1,000 keys. Polling is sequential, approximately five seconds per refresh plus request time, and stops updating hidden/paused views. Pausing the view does not pause devices.

## Sources and design rationale

The implementation is original, extending the supplied template. Grafana informed the operator workflow and visible system context; Carbon informed restrained chart presentation and purposeful information hierarchy. These are documented influences, not copied templates or product integrations. Microsoft documentation informed collection semantics and asynchronous hosting. See [Part 2 references](docs/PART2_REFERENCES.md) and the preserved Part 1 reference list.
