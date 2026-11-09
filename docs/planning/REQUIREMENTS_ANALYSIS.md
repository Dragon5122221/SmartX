# Complete analysis of the supplied Part 1 transcript

## Source boundary

Read in full: `docs/source/Smart-X_Part_1_OCR_Transcript.txt`. It transcribes source pages 3–6 and 14–20 of 29. These contain the scenario, Part 1 tasks and Part 1 marking rubric. The other pages were not supplied; no requirements from them are assumed. OCR wording is retained, including contradictions. The supplied `SmartX_Initial-Template.7z` contains an MVC project and a controller-based Web API project, both .NET 10, in `SmartX.slnx`.

## Scenario and architectural decision

Smart-X is an IoT gateway simulation for distributed devices such as ESP32 sensors. The brief explicitly rejects a generic unrelated application. All planned data and workflows therefore use identifiable sensor devices, facility/zone/node placement, environmental measurements, integer power readings, boolean actuator states, reporting cadence, anomalies and disconnects. Hardware integration is optional. Heavy mock seeding is required to demonstrate behaviour under load; “thousands” describes the scenario, not a formally specified minimum benchmark or SLA.

Preserve the supplied MVC + Web API architecture. Although the architecture examples mention Minimal APIs with Blazor/React/Vue, the functional API requirement expressly allows Controllers and the architecture wording grants flexibility. Add two .NET class libraries to share contracts and isolate the assessed data-structure code. Do not convert this to WPF, MAUI, a frontend-only prototype or an application that bypasses HTTP.

## Research: 20 marks, pages 4 and 14–15

| Requirement | Marks/evidence | Preparation |
|---|---|---|
| Five well-defined developer engagement/interactivity strategies specific to IoT | 5 | Research planning file; future journal research required |
| 500-word explanation, planning and justification of ONE selected strategy | 10 | Must connect spike/disconnect discovery with easier and faster troubleshooting |
| Proper citations and references using journal articles | 5 | Source ledger plan; no invented citations or finished research claim |
| Word report with diagrams, 1.5 spacing, Arial or Times New Roman 11/12 pt, maximum 2–3 pages | Deliverable constraint | Report remains to be researched and written; balance diagram/reference space within limit |

## Implementation: 80 marks, pages 5–6 and 16–20

| ID | Obligation and highest-band evidence | Marks | Main scaffold locations |
|---|---|---:|---|
| F1 | Three-pillar landing menu; asynchronous responsive and robust API/client communication | 10 | Existing HomeController/Home Index; _GatewayPillars; GatewayViewModel; SmartXApiClient; API/MVC Program |
| F2/F3 | Sensor registration and typed telemetry ingestion; robust client validation and accurate backend parsing | 10 | SensorsController, TelemetryController in both apps; shared contracts; Core validators |
| G1/G2 | Reusable TelemetryPacket<T>, no boxed value pipeline; meaningful overloaded sensor arithmetic | 10 | Shared TelemetryPacket<T>, PowerReading; Core PowerAggregationService; API power-comparison route |
| D1/D2 | Sequential history in jagged/multidimensional arrays; recursive configuration validation including deep hierarchies | 10 | TelemetryBatchBuffer<T>, DeploymentNode, DeploymentTreeValidator |
| U1 | Per-sensor configuration/photo/log upload; efficient multipart streams; rubric explicitly adds full encryption | 10 | AttachmentsController in both apps; EncryptedAttachmentStorage, AttachmentKeyProvider, upload validator |
| E1/E2 | Chosen research strategy integrated dynamically, understandable and interactive | 10 | Dashboard services, health worker, dashboard views/JS, issue repository |
| UI | Intuitive, visually consistent, responsive interface, readable typography, labels and crisp feedback | 10 | ViewModels, Views, smartx.css and feature JS placeholders |
| C1 | Efficient arrays-to-List<T> ingestion and a custom collection for highest band | 5 | Custom TelemetryBatchBuffer<T>, TelemetryStore<T> |
| DOC | Excellent Markdown README with precise restore/build/API/client launch and live/video demo evidence | 5 | README, implementation/acceptance plan and submission checklist |

The marks above sum to 80; research sums to 20. Data structures must be used in real working flows, not merely declared or shown in comments.

## Exact functional scope

- Startup names: **Sensor Data Ingestion and Telemetry** enabled in Part 1; **Real-Time Command Stream and History** disabled until Part 2; **Network Topology and Mesh Routing** disabled until final PoE. Do not build command queues or routing algorithms now.
- Historical *telemetry batches* are required now. Their history view is distinct from the disabled future command history pillar.
- Registration captures a device MAC address **or** unique identifier, deployment location (room, zone or node), and category (Environmental, Power Consumption or Actuator). Do not incorrectly reject every non-MAC unique identifier.
- Typed payloads support float, int and bool with device/type consistency. Typed HTTP routes are a design choice to avoid object-based heterogeneous packet storage, not an expressly prescribed endpoint scheme.
- File association is to one existing sensor. Configurations, photos and logs all need demonstrable support. Local files are uploaded through the MVC client and API; paths alone are insufficient.
- Engagement must be dynamic and based on the researched chosen strategy. A static graph or progress bar alone does not satisfy this.
- The entire frontend and API must compile and run. The brief warns that otherwise no application-functionality marks are awarded.

## Important rubric nuances and resolutions

1. **Custom collection:** page 19 explicitly says the collection is a custom data structure. Plan an owned jagged-array batch buffer with real capacity, order, retention and list-transfer behaviour. Merely wrapping List<T> without meaningful custom behaviour would provide weak evidence.
2. **User-reported issues:** page 18's lowest band unexpectedly says a list is not used to store user-reported issues, while the rest concerns engagement. This appears inconsistent with the main task. Conservatively scaffold a small IoT issue-reporting flow backed by List<ReportedIssue>; flag the wording for lecturer clarification without allowing it to block preparation.
3. **Encryption:** the main upload bullet does not mention it, but the highest rubric band does. Plan both HTTPS in transit and authenticated encryption at rest, protected persistent keys and bounded-memory transfer. “Full encryption” does not specify algorithm, authentication system or key-management product. No login system is invented as a requirement.
4. **Deep recursion:** support realistic deep trees, cycle detection and a documented maximum depth. A controlled validation error beyond that limit is preferable to stack overflow. Explain the limit and measure accepted-depth behaviour during implementation.
5. **GitHub:** main instructions say over 20 commits per project and a 5% penalty; the rubric shows up to 5 marks deducted and 25+ well-structured commits plus multiple contributors for no deduction. Genuine ongoing history is required; clarify collaboration expectations if working individually. Do not invent commits or contributors.
6. **Report references:** the rubric mentions “the given article”, but no such article is included in this transcript. Obtain it if the lecturer provided one elsewhere. Otherwise do not pretend it was supplied.
7. **Demo:** YouTube demo or presentation is subject to lecturer advice. Prepare an evidence checklist; do not assume which submission format applies.
8. **Docker:** suggested rather than required. Deliberately deferred; no nonfunctional placeholder Dockerfiles are presented as deployment support.

## Chosen preparation decisions versus requirements

Proposed engagement is a live anomaly investigation dashboard: typed trends, health indicators, spike/disconnect alerts and drill-down to sensor context. This is provisional until justified with journal research. Polling through the MVC/API boundary is an initial implementation choice, not a mandated transport. SignalR, MQTT, physical ESP32 devices, database persistence, login, cloud deployment and external chart packages are not required by the supplied pages. In-memory repositories keep data structures visible for assessment. Retention, concurrency, cancellation, streaming, input limits and safe error responses support the stated performance and robustness criteria.

## Template preservation

Both original project names, launch ports, framework settings, OpenAPI package version, static libraries, layouts and sample source files were retained. Added project references establish the new dependency graph. New classes have no business methods or fields yet; their comments specify the future work. Program files only gain a scaffold note; the Home view gains a TODO. MVC settings gain an as-yet-unbound API configuration section. Build caches, .vs state and personal .csproj.user settings were excluded from the deliverable. The original uploaded archive is not overwritten.
