# Current implementation file map

This index describes the implemented source. Planned empty scaffolding was consolidated into a single synchronized gateway service where that made state ownership clearer; obsolete shells were removed rather than left as fake implementations. The original planning record is retained under `planning/`.

| File | Role |
|---|---|
| `SmartX.Core/DataStructures/TelemetryBatchBuffer.cs` | DS-04: custom bounded ring of jagged batches. See Microsoft (2026c) for jagged arrays. |
| `SmartX.Core/DataStructures/TelemetryStore.cs` | DS-06: indexed typed sensor histories, serialized by the owning gateway lock. |
| `SmartX.Core/Services/PowerAggregationService.cs` | PWR-01: application-level demonstration of overloaded watt arithmetic. |
| `SmartX.Core/Services/SensorHealthService.cs` | HEALTH-01: distinguish a silent device from a valid stable or zero-valued signal. |
| `SmartX.Core/Simulation/TelemetrySeedGenerator.cs` | SEED-01: deterministic Johannesburg greenhouse simulation; no hardware dependency. |
| `SmartX.Core/SmartX.Core.csproj` | Project or runtime configuration |
| `SmartX.Core/Validation/DeploymentTreeValidator.cs` | ALG-01: bounded recursive depth-first validation of the deployment tree. |
| `SmartX.Core/Validation/SensorProfileValidator.cs` | REG-01: validate user-facing registration fields before API state changes. |
| `SmartX.Core/Validation/TelemetryPacketValidator.cs` | PKT-01: generic validation using a typed value predicate, without boxing (Microsoft, 2026a). |
| `SmartX.Shared/Attachments/SensorAttachment.cs` | Typed transport model; serialized consistently by the API and MVC client. |
| `SmartX.Shared/Contracts/Dashboard/ReportIssueRequest.cs` | Typed transport model; serialized consistently by the API and MVC client. |
| `SmartX.Shared/Contracts/Deployment/ValidateDeploymentRequest.cs` | Typed transport model; serialized consistently by the API and MVC client. |
| `SmartX.Shared/Contracts/Sensors/RegisterSensorRequest.cs` | Typed transport model; serialized consistently by the API and MVC client. |
| `SmartX.Shared/Contracts/Simulation/StartSimulationRequest.cs` | Typed transport model; serialized consistently by the API and MVC client. |
| `SmartX.Shared/Contracts/Telemetry/IngestionResult.cs` | Typed transport model; serialized consistently by the API and MVC client. |
| `SmartX.Shared/Contracts/Telemetry/PowerComparisonRequest.cs` | Typed transport model; serialized consistently by the API and MVC client. |
| `SmartX.Shared/Contracts/Telemetry/PowerComparisonResponse.cs` | Typed transport model; serialized consistently by the API and MVC client. |
| `SmartX.Shared/Contracts/Telemetry/TelemetryBatchRequest.cs` | Typed transport model; serialized consistently by the API and MVC client. |
| `SmartX.Shared/Contracts/Telemetry/TelemetryHistoryResponse.cs` | DATA STRUCTURE DS-05: typed list returned after explicit jagged-array flattening. |
| `SmartX.Shared/Dashboard/DashboardSnapshot.cs` | Typed transport model; serialized consistently by the API and MVC client. |
| `SmartX.Shared/Dashboard/ReportedIssue.cs` | Typed transport model; serialized consistently by the API and MVC client. |
| `SmartX.Shared/Dashboard/SensorHealth.cs` | Typed transport model; serialized consistently by the API and MVC client. |
| `SmartX.Shared/Dashboard/TelemetryAlert.cs` | Typed transport model; serialized consistently by the API and MVC client. |
| `SmartX.Shared/Deployment/DeploymentNode.cs` | DATA STRUCTURE DS-03: deployment tree with typed child lists; validated recursively before storage. |
| `SmartX.Shared/Sensors/SensorCategory.cs` | Typed transport model; serialized consistently by the API and MVC client. |
| `SmartX.Shared/Sensors/SensorProfile.cs` | Typed transport model; serialized consistently by the API and MVC client. |
| `SmartX.Shared/Simulation/SimulationScenario.cs` | Typed transport model; serialized consistently by the API and MVC client. |
| `SmartX.Shared/Simulation/SimulationStatus.cs` | Typed transport model; serialized consistently by the API and MVC client. |
| `SmartX.Shared/SmartX.Shared.csproj` | Project or runtime configuration |
| `SmartX.Shared/Telemetry/PowerReading.cs` | DATA STRUCTURE DS-02: immutable watt value supporting meaningful overloaded arithmetic. See Microsoft (2026b). |
| `SmartX.Shared/Telemetry/TelemetryPacket.cs` | DATA STRUCTURE DS-01: generic envelope keeps T strongly typed; no object/dynamic payload. See Microsoft (2026a). |
| `SmartX.Shared/Telemetry/TelemetryValueKind.cs` | Typed transport model; serialized consistently by the API and MVC client. |
| `SmartX.Shared/Validation/ValidationIssue.cs` | Typed transport model; serialized consistently by the API and MVC client. |
| `SmartX.Shared/Validation/ValidationResult.cs` | Typed transport model; serialized consistently by the API and MVC client. |
| `SmartX.Verification/Program.cs` | VERIFY-02: fail the process immediately if a required invariant is violated. |
| `SmartX.Verification/SmartX.Verification.csproj` | Project or runtime configuration |
| `SmartX.slnx` | Project or runtime configuration |
| `SmartXMVC/Controllers/DashboardController.cs` | PAGE-DASHBOARD: navigation routes for the Smart-X dashboard workspace. |
| `SmartXMVC/Controllers/DeploymentController.cs` | PAGE-DEPLOYMENT: navigation routes for the Smart-X deployment workspace. |
| `SmartXMVC/Controllers/GatewayController.cs` | CLIENT-HTTP: same-origin MVC gateway; only fixed backend routes are forwarded. |
| `SmartXMVC/Controllers/HomeController.cs` | PAGE-HOME: navigation routes for the Smart-X home workspace. |
| `SmartXMVC/Controllers/IssuesController.cs` | PAGE-ISSUES: navigation routes for the Smart-X issues workspace. |
| `SmartXMVC/Controllers/SensorsController.cs` | PAGE-SENSORS: navigation routes for the Smart-X sensors workspace. |
| `SmartXMVC/Controllers/TelemetryController.cs` | PAGE-TELEMETRY: navigation routes for the Smart-X telemetry workspace. |
| `SmartXMVC/Filters/StreamingRequestAttribute.cs` | STREAM-MVC-01: keep multipart bodies unread until the streaming action takes ownership (Microsoft, 2026d). |
| `SmartXMVC/Program.cs` | Project or runtime configuration |
| `SmartXMVC/Properties/launchSettings.json` | Project or runtime configuration |
| `SmartXMVC/Services/SmartXApiClient.cs` | CLIENT-01: async streaming API transport managed by IHttpClientFactory (Microsoft, 2026e). |
| `SmartXMVC/SmartXMVC.csproj` | Project or runtime configuration |
| `SmartXMVC/Views/Dashboard/Index.cshtml` | Razor workspace/layout |
| `SmartXMVC/Views/Deployment/Index.cshtml` | Razor workspace/layout |
| `SmartXMVC/Views/Home/Index.cshtml` | Razor workspace/layout |
| `SmartXMVC/Views/Issues/Index.cshtml` | Razor workspace/layout |
| `SmartXMVC/Views/Sensors/Create.cshtml` | Razor workspace/layout |
| `SmartXMVC/Views/Sensors/Details.cshtml` | Razor workspace/layout |
| `SmartXMVC/Views/Sensors/Edit.cshtml` | Razor workspace/layout |
| `SmartXMVC/Views/Sensors/Index.cshtml` | Razor workspace/layout |
| `SmartXMVC/Views/Shared/_Layout.cshtml` | Razor workspace/layout |
| `SmartXMVC/Views/Telemetry/History.cshtml` | Razor workspace/layout |
| `SmartXMVC/Views/Telemetry/Index.cshtml` | Razor workspace/layout |
| `SmartXMVC/Views/_ViewImports.cshtml` | Razor workspace/layout |
| `SmartXMVC/Views/_ViewStart.cshtml` | Razor workspace/layout |
| `SmartXMVC/appsettings.Development.json` | Project or runtime configuration |
| `SmartXMVC/appsettings.json` | Project or runtime configuration |
| `SmartXMVC/wwwroot/css/smartx.css` | Custom visual styling |
| `SmartXMVC/wwwroot/js/smartx.js` | Browser interactions |
| `SmartXWebApi/BackgroundServices/SimulationBackgroundService.cs` | SIM-06: one cancellable producer; no overlapping timers or fire-and-forget mutations. |
| `SmartXWebApi/Controllers/AttachmentsController.cs` | HTTP-UPLOAD: sensor-bound streaming multipart uploads (Microsoft, 2026d). |
| `SmartXWebApi/Controllers/DashboardController.cs` | HTTP boundary for dashboard; typed contracts and centralized error handling. |
| `SmartXWebApi/Controllers/DeploymentController.cs` | HTTP boundary for deployment; typed contracts and centralized error handling. |
| `SmartXWebApi/Controllers/HealthController.cs` | HTTP-HEALTH: availability endpoint for the client connection indicator. |
| `SmartXWebApi/Controllers/SensorsController.cs` | HTTP boundary for sensors; typed contracts and centralized error handling. |
| `SmartXWebApi/Controllers/SimulationController.cs` | HTTP boundary for simulation; typed contracts and centralized error handling. |
| `SmartXWebApi/Controllers/TelemetryController.cs` | HTTP boundary for telemetry; typed contracts and centralized error handling. |
| `SmartXWebApi/Filters/StreamingRequestAttribute.cs` | STREAM-API-01: keep multipart bodies unread until the streaming action takes ownership (Microsoft, 2026d). |
| `SmartXWebApi/GlobalUsings.cs` | Project or runtime configuration |
| `SmartXWebApi/Program.cs` | Project or runtime configuration |
| `SmartXWebApi/Properties/launchSettings.json` | Project or runtime configuration |
| `SmartXWebApi/Services/GatewayException.cs` | ERR-01: expected gateway failure carrying an intentional HTTP status. |
| `SmartXWebApi/Services/GatewayService.cs` | GATE-01: single in-memory owner of coherent sensor, ingestion and investigation state. |
| `SmartXWebApi/SmartXWebApi.csproj` | Project or runtime configuration |
| `SmartXWebApi/Storage/AttachmentKeyProvider.cs` | KEY-01: persist the encryption key outside the repository, protected by ASP.NET Data Protection (Anderson, 2025). |
| `SmartXWebApi/Storage/AuthenticatedAttachmentReadStream.cs` | ENC-STREAM-01: forward-only authenticated decryption with fixed buffers and no plaintext temporary files. |
| `SmartXWebApi/Storage/EncryptedAttachmentStorage.cs` | ENC-01: streamed authenticated AES-GCM records, with protected metadata and a mandatory final record. |
| `SmartXWebApi/Validation/AttachmentUploadValidator.cs` | UPLOAD-01: bounded configuration, log and deployment-photo file policy. |
| `SmartXWebApi/appsettings.Development.json` | Project or runtime configuration |
| `SmartXWebApi/appsettings.json` | Project or runtime configuration |
