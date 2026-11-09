# Planned API and client contract — not implemented

All MVC interactions use ISmartXApiClient/SmartXApiClient over HTTP. Razor/browser refresh requests go through MVC actions, so the proposed approach does not require browser-to-API cross-origin access. Core services own validation and algorithms; API repositories own shared state. Decide exact member signatures during implementation before filling the service shells.

| Endpoint | Method | Input / result |
|---|---|---|
| /api/health | GET | Backend availability |
| /api/sensors | GET / POST | Bounded profiles / RegisterSensorRequest |
| /api/sensors/{id} | GET / PUT | Profile / UpdateSensorRequest |
| /api/telemetry/float | POST | TelemetryBatchRequest<float> → IngestionResult |
| /api/telemetry/integer | POST | TelemetryBatchRequest<int> → IngestionResult |
| /api/telemetry/boolean | POST | TelemetryBatchRequest<bool> → IngestionResult |
| /api/telemetry/float/{sensorId}/history | GET | Bounded TelemetryHistoryResponse<float> |
| /api/telemetry/integer/{sensorId}/history | GET | Bounded TelemetryHistoryResponse<int> |
| /api/telemetry/boolean/{sensorId}/history | GET | Bounded TelemetryHistoryResponse<bool> |
| /api/telemetry/power-comparison | POST | PowerComparisonRequest → PowerComparisonResponse |
| /api/deployment | GET / PUT | Read / validate and replace root DeploymentNode |
| /api/deployment/validate | POST | ValidateDeploymentRequest → ValidationResult |
| /api/sensors/{sensorId}/attachments | GET / POST | Metadata / streaming multipart upload |
| /api/sensors/{sensorId}/attachments/{attachmentId}/download | GET | Decrypted download stream over HTTPS |
| /api/dashboard | GET | Bounded DashboardSnapshot |
| /api/dashboard/issues | POST | ReportIssueRequest → ReportedIssue |
| /api/dashboard/issues/{id}/resolve | POST | Update issue state |
| /api/dashboard/alerts/{id}/acknowledge | POST | Update alert state |
| /api/simulation/start | POST | StartSimulationRequest |
| /api/simulation/stop | POST | Cancellation acknowledgement |
| /api/simulation/status | GET | Active scenario and bounded metrics |

Define safe ProblemDetails responses and distinguish malformed input, missing sensors, duplicate IDs, payload/type mismatch, oversized/unsupported uploads and internal failures. Use CancellationToken through API, service, stream and HttpClient calls. Never retry mutation requests blindly. Forward API validation errors into MVC ModelState; anti-forgery validation applies to MVC form posts.

The server must validate identifier/location/category, packet shape and declared type before ingestion. JSON numbers must bind to concrete float or int contracts; do not parse everything as strings or double. Input bounds and JSON hierarchy depth must agree with processing limits. Every response list/history query must have a documented maximum size.
