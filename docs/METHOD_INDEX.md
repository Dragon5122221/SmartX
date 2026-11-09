# Method and code label index

Search these identifiers in Visual Studio. Public methods, helpers, event handlers, stream overrides and startup sections carry purpose-specific comments; immutable records use type-level documentation for compiler-generated members. Standard unsupported Stream operations intentionally throw NotSupportedException and are not incomplete functionality.

| File | Comment identifier and purpose |
|---|---|
| `SmartX.Core/DataStructures/TelemetryBatchBuffer.cs` | DS-04: custom bounded ring of jagged batches. See Microsoft (2026c) for jagged arrays. |
| `SmartX.Core/DataStructures/TelemetryBatchBuffer.cs` | COL-01: copy an immutable-packet batch; evict oldest batch at capacity. |
| `SmartX.Core/DataStructures/TelemetryBatchBuffer.cs` | COL-02: expose independent jagged arrays in chronological batch order. |
| `SmartX.Core/DataStructures/TelemetryBatchBuffer.cs` | COL-03: explicitly transfer jagged batches into a pre-sized generic list. |
| `SmartX.Core/DataStructures/TelemetryStore.cs` | DS-06: indexed typed sensor histories, serialized by the owning gateway lock. |
| `SmartX.Core/DataStructures/TelemetryStore.cs` | STORE-01: add one sensor's already validated sequential batch. |
| `SmartX.Core/DataStructures/TelemetryStore.cs` | STORE-02: produce a bounded snapshot without exposing mutable internal storage. |
| `SmartX.Core/DataStructures/TelemetryStore.cs` | STORE-03: obtain the latest typed packet for sequence validation or comparisons. |
| `SmartX.Core/Services/PowerAggregationService.cs` | PWR-01: application-level demonstration of overloaded watt arithmetic. |
| `SmartX.Core/Services/PowerAggregationService.cs` | PWR-02: return operands, total and signed delta with original sample timestamps. |
| `SmartX.Core/Services/SensorHealthService.cs` | HEALTH-01: distinguish a silent device from a valid stable or zero-valued signal. |
| `SmartX.Core/Services/SensorHealthService.cs` | HEALTH-02: use server receipt time and three expected reporting intervals. |
| `SmartX.Core/Simulation/TelemetrySeedGenerator.cs` | SEED-01: deterministic Johannesburg greenhouse simulation; no hardware dependency. |
| `SmartX.Core/Simulation/TelemetrySeedGenerator.cs` | SEED-02: generate one facility with four independently identifiable deployment zones. |
| `SmartX.Core/Simulation/TelemetrySeedGenerator.cs` | SEED-03: create 1,200 mixed sensor profiles with explicit types, units and locations. |
| `SmartX.Core/Validation/DeploymentTreeValidator.cs` | ALG-01: bounded recursive depth-first validation of the deployment tree. |
| `SmartX.Core/Validation/DeploymentTreeValidator.cs` | TREE-01: validate root, unique identifiers, safety inheritance, depth and cycles. |
| `SmartX.Core/Validation/DeploymentTreeValidator.cs` | TREE-02: recursive step; each branch terminates on null, depth limit, cycle or leaf. |
| `SmartX.Core/Validation/SensorProfileValidator.cs` | REG-01: validate user-facing registration fields before API state changes. |
| `SmartX.Core/Validation/SensorProfileValidator.cs` | REG-02: accept a MAC or safe unique ID, a valid category and a real deployment node. |
| `SmartX.Core/Validation/SensorProfileValidator.cs` | REG-03: compile the bounded device identifier grammar once. |
| `SmartX.Core/Validation/TelemetryPacketValidator.cs` | PKT-01: generic validation using a typed value predicate, without boxing (Microsoft, 2026a). |
| `SmartX.Core/Validation/TelemetryPacketValidator.cs` | PKT-02: reject stale ordering, future/old timestamps and invalid typed values. |
| `SmartX.Shared/Telemetry/PowerReading.cs` | OP-01: aggregate two watt readings with explicit overflow detection. |
| `SmartX.Shared/Telemetry/PowerReading.cs` | OP-02: calculate a signed watt delta; a negative result is meaningful. |
| `SmartX.Verification/Program.cs` | VERIFY-02: fail the process immediately if a required invariant is violated. |
| `SmartX.Verification/Program.cs` | VERIFY-03: construct deterministic typed packets for collection checks. |
| `SmartXMVC/Controllers/DashboardController.cs` | PAGE-DASHBOARD-01: render the index workspace; data is fetched asynchronously through the MVC gateway. |
| `SmartXMVC/Controllers/DeploymentController.cs` | PAGE-DEPLOYMENT-01: render the index workspace; data is fetched asynchronously through the MVC gateway. |
| `SmartXMVC/Controllers/GatewayController.cs` | CLIENT-HTTP-01: stream JSON/multipart requests and results with cancellation and safe outage feedback. |
| `SmartXMVC/Controllers/GatewayController.cs` | CLIENT-HTTP-02: reject absolute URLs, traversal and arbitrary upstream resource names. |
| `SmartXMVC/Controllers/HomeController.cs` | PAGE-HOME-01: render the index workspace; data is fetched asynchronously through the MVC gateway. |
| `SmartXMVC/Controllers/IssuesController.cs` | PAGE-ISSUES-01: render the index workspace; data is fetched asynchronously through the MVC gateway. |
| `SmartXMVC/Controllers/SensorsController.cs` | PAGE-SENSORS-01: render the index workspace; data is fetched asynchronously through the MVC gateway. |
| `SmartXMVC/Controllers/SensorsController.cs` | PAGE-SENSORS-02: render the create workspace; data is fetched asynchronously through the MVC gateway. |
| `SmartXMVC/Controllers/SensorsController.cs` | PAGE-SENSORS-03: render the edit workspace; data is fetched asynchronously through the MVC gateway. |
| `SmartXMVC/Controllers/SensorsController.cs` | PAGE-SENSORS-04: render the details workspace; data is fetched asynchronously through the MVC gateway. |
| `SmartXMVC/Controllers/TelemetryController.cs` | PAGE-TELEMETRY-01: render the index workspace; data is fetched asynchronously through the MVC gateway. |
| `SmartXMVC/Controllers/TelemetryController.cs` | PAGE-TELEMETRY-02: render the history workspace; data is fetched asynchronously through the MVC gateway. |
| `SmartXMVC/Filters/StreamingRequestAttribute.cs` | STREAM-MVC-01: keep multipart bodies unread until the streaming action takes ownership (Microsoft, 2026d). |
| `SmartXMVC/Filters/StreamingRequestAttribute.cs` | STREAM-MVC-02: remove form value providers that would otherwise consume the request before the action. |
| `SmartXMVC/Filters/StreamingRequestAttribute.cs` | STREAM-MVC-03: leave stream disposal to the request owner after action execution. |
| `SmartXMVC/Services/SmartXApiClient.cs` | CLIENT-01: async streaming API transport managed by IHttpClientFactory (Microsoft, 2026e). |
| `SmartXMVC/Services/SmartXApiClient.cs` | CLIENT-02: forward a validated relative route and stream the response without whole-body buffering. |
| `SmartXMVC/Services/SmartXApiClient.cs` | CLIENT-03: dispose the outbound request after headers arrive; caller owns the returned response. |
| `SmartXMVC/wwwroot/js/smartx.js` | UI-02: HTML-escape all server/user strings before placing them in structured markup. |
| `SmartXMVC/wwwroot/js/smartx.js` | UI-03: show accessible success/error feedback without blocking the workspace. |
| `SmartXMVC/wwwroot/js/smartx.js` | UI-04: async same-origin request with anti-forgery header, timeout, and meaningful API validation errors. |
| `SmartXMVC/wwwroot/js/smartx.js` | UI-05: reflect real API availability in the header. |
| `SmartXMVC/wwwroot/js/smartx.js` | UI-06: format timestamps explicitly in UTC, matching the telemetry table. |
| `SmartXMVC/wwwroot/js/smartx.js` | UI-07: render a non-colour-only device status badge. |
| `SmartXMVC/wwwroot/js/smartx.js` | UI-08: create one labelled numerical measure; every displayed figure comes from the API. |
| `SmartXMVC/wwwroot/js/smartx.js` | UI-09: render an accessible original SVG trend with real timestamp spacing and precise point tooltips. |
| `SmartXMVC/wwwroot/js/smartx.js` | UI-10: update dashboard metrics, trend, actionable alerts and simulation state from one snapshot. |
| `SmartXMVC/wwwroot/js/smartx.js` | UI-11: list/search/paginate the sensor registry, never downloading the whole device fleet into the browser. |
| `SmartXMVC/wwwroot/js/smartx.js` | UI-12: traverse only the bounded, server-validated tree to populate location options. |
| `SmartXMVC/wwwroot/js/smartx.js` | UI-13: populate registration/edit controls; immutable type/identifier fields are locked during editing. |
| `SmartXMVC/wwwroot/js/smartx.js` | UI-14: extract the sensor identifier from the MVC details/edit/history route. |
| `SmartXMVC/wwwroot/js/smartx.js` | UI-15: refresh sensor context, health, signal history and independent attachment metadata. |
| `SmartXMVC/wwwroot/js/smartx.js` | UI-16: render safe download links; the backend enforces sensor association and authentication of ciphertext. |
| `SmartXMVC/wwwroot/js/smartx.js` | UI-17: show explicit array-batch/list evidence alongside a chronological packet table. |
| `SmartXMVC/wwwroot/js/smartx.js` | UI-18: render a hierarchy with native keyboard-accessible expandable details. |
| `SmartXMVC/wwwroot/js/smartx.js` | UI-19: load the saved configuration into the tree inspector and editable JSON area. |
| `SmartXMVC/wwwroot/js/smartx.js` | UI-20: validate or save edited JSON and display all returned node paths. |
| `SmartXMVC/wwwroot/js/smartx.js` | UI-21: populate the issue journal with real report and resolution state. |
| `SmartXMVC/wwwroot/js/smartx.js` | UI-22: wrap events so rejected async operations always become visible feedback. |
| `SmartXMVC/wwwroot/js/smartx.js` | UI-23: bind feature-specific controls once, avoiding duplicate event handlers during refresh. |
| `SmartXMVC/wwwroot/js/smartx.js` | UI-24: refresh sequentially; do not overlap requests or overwrite a user's configuration edits. |
| `SmartXMVC/wwwroot/js/smartx.js` | UI-25: initialise the correct workspace, preserve page-specific forms and establish live availability. |
| `SmartXWebApi/BackgroundServices/SimulationBackgroundService.cs` | SIM-06: one cancellable producer; no overlapping timers or fire-and-forget mutations. |
| `SmartXWebApi/BackgroundServices/SimulationBackgroundService.cs` | SIM-07: tick at the configured simulation cadence until the host shuts down. |
| `SmartXWebApi/Controllers/AttachmentsController.cs` | HTTP-UPLOAD-01: list encrypted attachment metadata for an existing sensor. |
| `SmartXWebApi/Controllers/AttachmentsController.cs` | HTTP-UPLOAD-02: parse one multipart file without IFormFile model-binding buffers. |
| `SmartXWebApi/Controllers/AttachmentsController.cs` | HTTP-UPLOAD-03: decrypt and authenticate before the framework streams an attachment download. |
| `SmartXWebApi/Controllers/DashboardController.cs` | HTTP-DASHBOARD-01: route validated input to the gateway and return its result. |
| `SmartXWebApi/Controllers/DashboardController.cs` | HTTP-DASHBOARD-02: route validated input to the gateway and return its result. |
| `SmartXWebApi/Controllers/DashboardController.cs` | HTTP-DASHBOARD-03: route validated input to the gateway and return its result. |
| `SmartXWebApi/Controllers/DashboardController.cs` | HTTP-DASHBOARD-04: route validated input to the gateway and return its result. |
| `SmartXWebApi/Controllers/DashboardController.cs` | HTTP-DASHBOARD-05: route validated input to the gateway and return its result. |
| `SmartXWebApi/Controllers/DeploymentController.cs` | HTTP-DEPLOYMENT-01: route validated input to the gateway and return its result. |
| `SmartXWebApi/Controllers/DeploymentController.cs` | HTTP-DEPLOYMENT-02: route validated input to the gateway and return its result. |
| `SmartXWebApi/Controllers/DeploymentController.cs` | HTTP-DEPLOYMENT-03: route validated input to the gateway and return its result. |
| `SmartXWebApi/Controllers/HealthController.cs` | HTTP-HEALTH-01: confirm a responsive process without exposing host details. |
| `SmartXWebApi/Controllers/SensorsController.cs` | HTTP-SENSORS-01: route validated input to the gateway and return its result. |
| `SmartXWebApi/Controllers/SensorsController.cs` | HTTP-SENSORS-02: route validated input to the gateway and return its result. |
| `SmartXWebApi/Controllers/SensorsController.cs` | HTTP-SENSORS-03: route validated input to the gateway and return its result. |
| `SmartXWebApi/Controllers/SensorsController.cs` | HTTP-SENSORS-04: route validated input to the gateway and return its result. |
| `SmartXWebApi/Controllers/SimulationController.cs` | HTTP-SIMULATION-01: route validated input to the gateway and return its result. |
| `SmartXWebApi/Controllers/SimulationController.cs` | HTTP-SIMULATION-02: route validated input to the gateway and return its result. |
| `SmartXWebApi/Controllers/SimulationController.cs` | HTTP-SIMULATION-03: route validated input to the gateway and return its result. |
| `SmartXWebApi/Controllers/TelemetryController.cs` | HTTP-TELEMETRY-01: route validated input to the gateway and return its result. |
| `SmartXWebApi/Controllers/TelemetryController.cs` | HTTP-TELEMETRY-02: route validated input to the gateway and return its result. |
| `SmartXWebApi/Controllers/TelemetryController.cs` | HTTP-TELEMETRY-03: route validated input to the gateway and return its result. |
| `SmartXWebApi/Controllers/TelemetryController.cs` | HTTP-TELEMETRY-04: route validated input to the gateway and return its result. |
| `SmartXWebApi/Controllers/TelemetryController.cs` | HTTP-TELEMETRY-05: route validated input to the gateway and return its result. |
| `SmartXWebApi/Controllers/TelemetryController.cs` | HTTP-TELEMETRY-06: route validated input to the gateway and return its result. |
| `SmartXWebApi/Controllers/TelemetryController.cs` | HTTP-TELEMETRY-07: route validated input to the gateway and return its result. |
| `SmartXWebApi/Filters/StreamingRequestAttribute.cs` | STREAM-API-01: keep multipart bodies unread until the streaming action takes ownership (Microsoft, 2026d). |
| `SmartXWebApi/Filters/StreamingRequestAttribute.cs` | STREAM-API-02: remove form value providers that would otherwise consume the request before the action. |
| `SmartXWebApi/Filters/StreamingRequestAttribute.cs` | STREAM-API-03: leave stream disposal to the request owner after action execution. |
| `SmartXWebApi/Services/GatewayException.cs` | ERR-01: expected gateway failure carrying an intentional HTTP status. |
| `SmartXWebApi/Services/GatewayService.cs` | GATE-01: single in-memory owner of coherent sensor, ingestion and investigation state. |
| `SmartXWebApi/Services/GatewayService.cs` | GATE-02: seed profiles and 28,800 packets through the real validation/collection pipeline. |
| `SmartXWebApi/Services/GatewayService.cs` | REG-04: list a bounded searchable snapshot, with health calculated at read time. |
| `SmartXWebApi/Services/GatewayService.cs` | REG-05: retrieve one registered sensor or return a deliberate 404. |
| `SmartXWebApi/Services/GatewayService.cs` | REG-06: validate and create/update a profile while enforcing registry uniqueness. |
| `SmartXWebApi/Services/GatewayService.cs` | TREE-03: return a deep copy so callers cannot mutate the validated internal tree. |
| `SmartXWebApi/Services/GatewayService.cs` | TREE-04: validate recursively and reject removal of locations used by registered sensors. |
| `SmartXWebApi/Services/GatewayService.cs` | ING-01: dispatch concrete float batches, rejecting NaN/infinite sensor values. |
| `SmartXWebApi/Services/GatewayService.cs` | ING-02: dispatch integer watt batches; negative meter load is invalid for this simulation. |
| `SmartXWebApi/Services/GatewayService.cs` | ING-03: dispatch boolean valve batches without conversion into strings or numeric payloads. |
| `SmartXWebApi/Services/GatewayService.cs` | ING-04: validate sequential raw batches, retain typed packets, and update anomaly evidence. |
| `SmartXWebApi/Services/GatewayService.cs` | HIST-01: expose typed float history after custom array-to-list transfer. |
| `SmartXWebApi/Services/GatewayService.cs` | HIST-02: expose typed integer history after custom array-to-list transfer. |
| `SmartXWebApi/Services/GatewayService.cs` | HIST-03: expose typed boolean history after custom array-to-list transfer. |
| `SmartXWebApi/Services/GatewayService.cs` | PWR-03: compare registered power meters using the latest integer packets. |
| `SmartXWebApi/Services/GatewayService.cs` | DASH-01: combine bounded views for live investigation, keeping conversion out of ingestion. |
| `SmartXWebApi/Services/GatewayService.cs` | ISSUE-01: append a validated user-reported issue to the explicitly required List. |
| `SmartXWebApi/Services/GatewayService.cs` | ISSUE-02: snapshot recent reported issues for the investigation journal. |
| `SmartXWebApi/Services/GatewayService.cs` | ISSUE-03: resolve a known issue without deleting its audit context. |
| `SmartXWebApi/Services/GatewayService.cs` | ALERT-01: acknowledge a displayed alert while retaining its evidence. |
| `SmartXWebApi/Services/GatewayService.cs` | SIM-01: read simulation state under the same lock as telemetry mutations. |
| `SmartXWebApi/Services/GatewayService.cs` | SIM-02: change fault scenario and resume the single background producer. |
| `SmartXWebApi/Services/GatewayService.cs` | SIM-03: pause generation; devices then become disconnected according to their cadence. |
| `SmartXWebApi/Services/GatewayService.cs` | SIM-04: produce one deterministic tick then evaluate time-based disconnect alerts. |
| `SmartXWebApi/Services/GatewayService.cs` | SIM-05: create typed jagged batches; seed and live data both use the assessed ingestion path. |
| `SmartXWebApi/Services/GatewayService.cs` | ALERT-02: throttle repeated sensor/kind alerts to one per minute and cap retained evidence. |
| `SmartXWebApi/Services/GatewayService.cs` | REG-07: enforce sensor existence for every dependent workflow. |
| `SmartXWebApi/Services/GatewayService.cs` | PKT-03: reject use of a typed route against an incompatible sensor. |
| `SmartXWebApi/Services/GatewayService.cs` | DASH-02: format display-only values without changing the typed processing stores. |
| `SmartXWebApi/Services/GatewayService.cs` | TREE-05: clone validated nodes recursively so child lists have independent ownership. |
| `SmartXWebApi/Services/GatewayService.cs` | TREE-06: traverse an already validated bounded deployment tree for location lookup. |
| `SmartXWebApi/Storage/AttachmentKeyProvider.cs` | KEY-01: persist the encryption key outside the repository, protected by ASP.NET Data Protection (Anderson, 2025). |
| `SmartXWebApi/Storage/AttachmentKeyProvider.cs` | KEY-02: unwrap an existing 256-bit key or create one on first run; never log key material. |
| `SmartXWebApi/Storage/AuthenticatedAttachmentReadStream.cs` | ENC-STREAM-01: forward-only authenticated decryption with fixed buffers and no plaintext temporary files. |
| `SmartXWebApi/Storage/AuthenticatedAttachmentReadStream.cs` | ENC-STREAM-02: take ownership of the prevalidated encrypted stream and copy the key into AesGcm. |
| `SmartXWebApi/Storage/AuthenticatedAttachmentReadStream.cs` | ENC-STREAM-03: authenticate each record again, then copy only requested plaintext bytes to the caller. |
| `SmartXWebApi/Storage/AuthenticatedAttachmentReadStream.cs` | ENC-STREAM-04: support the legacy byte-array async overload without introducing buffering. |
| `SmartXWebApi/Storage/AuthenticatedAttachmentReadStream.cs` | ENC-STREAM-05: synchronous Stream compatibility; ASP.NET downloads use the asynchronous overload. |
| `SmartXWebApi/Storage/AuthenticatedAttachmentReadStream.cs` | ENC-STREAM-06: clear plaintext and dispose cryptography and the underlying file handle. |
| `SmartXWebApi/Storage/AuthenticatedAttachmentReadStream.cs` | ENC-STREAM-07: forward-only read streams cannot seek. |
| `SmartXWebApi/Storage/AuthenticatedAttachmentReadStream.cs` | ENC-STREAM-08: read-only streams cannot resize. |
| `SmartXWebApi/Storage/AuthenticatedAttachmentReadStream.cs` | ENC-STREAM-09: read-only streams cannot write. |
| `SmartXWebApi/Storage/AuthenticatedAttachmentReadStream.cs` | ENC-STREAM-10: a read-only stream has no pending writes to flush. |
| `SmartXWebApi/Storage/EncryptedAttachmentStorage.cs` | ENC-01: streamed authenticated AES-GCM records, with protected metadata and a mandatory final record. |
| `SmartXWebApi/Storage/EncryptedAttachmentStorage.cs` | ENC-02: load protected sidecars after restart; sensor association is checked by the API. |
| `SmartXWebApi/Storage/EncryptedAttachmentStorage.cs` | ENC-03: return only metadata belonging to the requested sensor. |
| `SmartXWebApi/Storage/EncryptedAttachmentStorage.cs` | ENC-04: stream upload records with bounded buffers; authenticate file identity and record order. |
| `SmartXWebApi/Storage/EncryptedAttachmentStorage.cs` | ENC-05: authenticate the complete file before returning a bounded decryption stream; no plaintext is written to disk. |
| `SmartXWebApi/Storage/EncryptedAttachmentStorage.cs` | ENC-06: fill one chunk even when network reads return short segments. |
| `SmartXWebApi/Storage/EncryptedAttachmentStorage.cs` | ENC-07: nonce counter is unique within each independently derived per-file key. |
| `SmartXWebApi/Storage/EncryptedAttachmentStorage.cs` | ENC-08: authenticate association, ordinal and record size to reject swapping or truncation. |
| `SmartXWebApi/Validation/AttachmentUploadValidator.cs` | UPLOAD-01: bounded configuration, log and deployment-photo file policy. |
| `SmartXWebApi/Validation/AttachmentUploadValidator.cs` | UPLOAD-02: retain a safe display name and derive its permitted response content type. |
| `SmartXWebApi/Validation/AttachmentUploadValidator.cs` | UPLOAD-03: verify photo signatures and reject NUL-bearing text/config binaries. |
