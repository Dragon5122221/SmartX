# Part 2 validation — 16 October 2026

## Executed checks

| Layer | Result |
|---|---|
| Complete .NET solution | Build succeeded: **0 warnings, 0 errors** using .NET SDK 10.0.103 |
| Behavioural verification | **43 checks passed**: 24 retained Part 1 checks plus 19 Part 2 checks |
| Real HTTP services | Emulator ingress, all eight MVC entry pages, same-origin API proxy, CSRF rejection, command application and undo passed |
| Headless Chromium | Confirmed command/undo; asynchronous route changes; browser back/forward; search and ingestion-draft preservation passed |
| UI experiments | HTTP critical spike and six-sample rising trend produced accepted results and visible forecast |
| Responsive layout | Inspected desktop at 1440 × 1100 and mobile at 390 × 844; mobile page width did not overflow |
| Browser runtime | No uncaught JavaScript errors in exercised journeys |

Raw outputs are retained under `docs/part2-evidence/`. Browser checks are reproducible in `scripts/browser_smoke.cjs`; install Playwright and its browser separately if using them. They are development-only tools, not application runtime requirements. Set `SMARTX_BROWSER` only if using an explicit browser binary; the normal Playwright installation requires no such variable. Run against a fresh API and both local services. `scripts/http_smoke.py` uses only Python's standard library.

The browser scenario deliberately checks the experiment reading after it is rendered, rather than assuming request completion and UI rendering happen simultaneously. The demo-device registration workflow may make an expected 404 lookup before registering a new ID; this is handled and is not an uncaught browser error.

## Defects found and corrected during validation

- The original global `history()` renderer shadowed the browser History API. It was renamed to `telemetryHistory()`, and routing explicitly uses `window.history`.
- Long device lists stretched the command desk. The fleet now scrolls within a bounded desktop pane.
- Repeated trend demonstrations could submit backwards timestamps. A six-second repeat guard now provides clear feedback.
- Overlapping selection/refresh requests could delay rendering the latest selected device. Refreshes now coalesce and discard obsolete device selections.
- Docker's attachment directory was initially different from the mounted volume. `SmartX__StoragePath` now points to the persistent mount.

## Not executed / not claimed

Docker is not installed in the validation environment; the Dockerfiles and Compose configuration were reviewed but no container build/run is claimed. Windows Visual Studio launch, Windows certificate trust and the Windows launcher require local confirmation. The solution itself compiled and both application layers ran under .NET 10 on Linux. No physical IoT hardware, external GitHub push, production deployment, multi-user authorisation, durability across API restarts or lecturer video upload is claimed.

## Acceptance boundaries

Queue timing depends on workload and scheduling. The deterministic verification checks priority order, not an absolute latency guarantee. Forecasts and contextual recommendations are explanatory heuristics with explicit evidence; they are not assurances of future faults or safe physical actuator behaviour. The UI requests a human confirmation for every suggested/manual override.
