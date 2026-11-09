# Next coding phase and acceptance evidence

No algorithms, business methods, controller actions, service wiring, UI interactions or automated tests were implemented in this preparation phase. Use the file map to work through complete functional slices.

1. Confirm the local .NET restore/build baseline and complete journal research to settle engagement design.
2. Fill Shared models/contracts/enums; implement typed packets and PowerReading operators. Define validation/HTTP error contracts.
3. Implement the custom batch collection and typed store. Document DS labels at declarations and usage. Verify array-to-list order and retention.
4. Implement sensor and recursive deployment validation; define API JSON depth and Core recursion limits together.
5. Implement repositories, ingestion and API dependency wiring; register float/int/bool service instances. Replace the weather sample once real endpoints exist.
6. Add heavy deterministic seeding and cancellable simulation through the same ingestion pipeline; record actual load evidence.
7. Implement typed HttpClient and registration/ingestion MVC screens with async feedback. Replace the Home template with the exact three pillars and disable the two future entries.
8. Implement secure streamed attachment round trips with sensor metadata association, encryption and cleanup. Decide reviewed encryption format/key persistence before wiring it.
9. Implement the researched dashboard strategy, typed trends, spike/dropout detection, drill-down and issue reporting using List<ReportedIssue>.
10. Finish responsive layout, accessibility and meaningful errors. Complete tests/checks below, report, README and demo evidence.

## Meaningful acceptance checks for the implementation phase

| Area | Required evidence |
|---|---|
| Build / startup | Fresh restore/build; API and MVC boot independently and together; no functionality marks claimed before both run |
| Navigation | Exact pillar labels; Part 1 enabled; future pillars disabled; all implemented links/forms work |
| Registration | MAC and non-MAC unique IDs; valid location/category; duplicate, missing and incompatible inputs fail clearly on client/API |
| Generic pipeline | Float, int, bool round trips; no boxed value storage; mismatched and malformed packets rejected |
| Operators | Actual + total and - delta displayed; incompatible units/missing readings/overflow handled |
| Custom collection | Unequal and empty batches, batch order, list transfer, growth, retention and snapshot ownership; state remains typed |
| Recursion | Valid multi-tier hierarchy, leaf/null, invalid descendant, duplicate ID, cycle and depth boundary; no uncontrolled stack overflow |
| Concurrency | Simultaneous ingest, refresh and snapshot requests preserve order/counts and avoid collection-modified exceptions |
| Simulation | Reproducible thousands-device scenario; all three types, variable batches, spikes and dropouts; record count, duration and memory, no invented speed claims |
| Uploads | Config/photo/log round trips for correct sensor, actual bytes equal after decrypt; ciphertext at rest; HTTPS; tamper rejection, missing key, limit, cancellation and partial cleanup |
| Engagement | Seeded spike highlights correct device/time; dropout follows cadence timeout; investigation and issue reporting actually work |
| API resilience | Backend down, cancellation, timeout, validation errors and large requests give usable UI feedback without synchronous blocking |
| UI | Narrow/wide viewport, keyboard/focus, readable labels, loading/empty/error/success states; health not communicated only by colour |
| Submission | Actual report format/citations checked, source excludes runtime/secrets/cache, genuine GitHub history and chosen demo included |

Do not add fake passing tests around empty classes. Testing should verify real observable requirements once functionality is implemented.
