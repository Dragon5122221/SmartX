# Part 2 rubric coverage

This is an implementation map, not a prediction of a lecturer's mark.

| Criterion | Marks | Implementation | Evidence |
|---|---:|---|---|
| Main menu and active pillar state | 30 | Existing Part 1 screens retained; command stream active; topology preview navigable with final-PoE status; asynchronous navigation with page-local state | `navigation.js`, `_Layout.cshtml`, gateway home; browser navigation checks |
| Stacks and queues | 15 | Real HTTP ingress FIFO, critical priority heap, stable tie-breaking, bounded capacity; stack restores prior override or automatic state | `TelemetryDispatcher.cs`, `GatewayService.Realtime.cs`, deterministic verification cases |
| Dictionaries and ordered maps | 15 | Existing canonical registry dictionary; per-device sorted timestamp/sequence history | `GatewayService.cs`, `GatewayService.Realtime.cs`, timeline test |
| Sets | 10 | Unique error lifecycle keys, recovery removal, recurrence and duplicate count | `Observe`, `RefreshDisconnects`; recovery/recurrence tests |
| Predictive engine | 30 | Contextual search/action support maps, undo feedback, six-sample regression forecast, visible evidence and review actions | `RecordSearch`, `Command`, `Undo`, `Insights`; learning and forecast tests |
| GitHub | Deduction risk | Supplied history retained with new real commits; push still required by student | `docs/GIT_HANDOFF.md` |

## Preserved Part 1 behaviours

Typed float/integer/boolean payloads, generic jagged-ring collection, power operators, immutable registration type/ID, recursive deployment validation, simulation scenarios, health monitoring, reported issues, encrypted streaming attachments, download integrity checks, ingestion feedback, chronological history and custom dashboard charts remain in the solution. Their existing verification cases are retained.

## File map for the new work

- `SmartX.Shared/Realtime/StreamModels.cs`: typed snapshots, command/search payloads and suggestion contracts.
- `SmartXWebApi/Services/TelemetryDispatcher.cs`: bounded queue worker and processing metrics.
- `SmartXWebApi/Services/GatewayService.Realtime.cs`: history, alert set, command stack and insights.
- `SmartXWebApi/Controllers/StreamController.cs`: API read/actions.
- `SmartXWebApi/Controllers/TelemetryController.cs`: typed asynchronous queue entry points.
- `SmartXWebApi/BackgroundServices/SimulationBackgroundService.cs`: live HTTP device emulator.
- `SmartXMVC/Views/Stream/Index.cshtml`, `wwwroot/css/stream.css`, `wwwroot/js/stream.js`: command desk.
- `SmartXMVC/wwwroot/js/navigation.js`: asynchronous workspace navigation.
- `SmartXMVC/Views/Topology/Index.cshtml`: honest final-PoE preview with live deployment tree.
- `SmartX.Verification/Part2Checks.cs`: deterministic behavioural checks.
- `scripts/http_smoke.py`: real API/client boundary checks.

## Submission items that require student action

Push to the correct GitHub repository, confirm each student's genuine contribution, and record the video/presentation according to the campus lecturer's direction. Mesh routing belongs to the final PoE and is not represented as implemented here. Review the code and be able to explain the collection choices, rejection policy and suggestion limitations.
