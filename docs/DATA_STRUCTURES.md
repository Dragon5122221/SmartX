# Data structure labels and exact code locations

Paths are relative to the extracted folder containing `SmartX.slnx`. Search the exact **DS** or **ALG** label in Visual Studio using Ctrl+Shift+F. The code comments explain the structures at their declarations and significant usage sites; this document makes them easy to demonstrate.

| Label | Structure and exact file | Main member / purpose |
|---|---|---|
| DS-01 | `SmartX.Shared/Telemetry/TelemetryPacket.cs` | `TelemetryPacket<T>`: immutable generic reference envelope; float/int/bool values retain their type without object payloads |
| DS-02 | `SmartX.Shared/Telemetry/PowerReading.cs` | `PowerReading`: immutable value record; `operator +` and `operator -` use checked watt arithmetic |
| DS-03 | `SmartX.Shared/Deployment/DeploymentNode.cs` | `Children`: `List<DeploymentNode>` represents the nested facility/zone/node tree |
| DS-04 | `SmartX.Core/DataStructures/TelemetryBatchBuffer.cs` | `_batches`: custom ring owning `TelemetryPacket<T>[][]`; `AppendBatch`, `SnapshotBatches`, `ToList` |
| DS-04 input | `SmartX.Shared/Contracts/Telemetry/TelemetryBatchRequest.cs` | `Batches`: raw, variable-length sequential telemetry arrays |
| DS-05 | `SmartX.Core/DataStructures/TelemetryBatchBuffer.cs` | `ToList`: explicit pre-sized `List<TelemetryPacket<T>>` transfer in oldest-to-newest batch order |
| DS-06 | `SmartX.Core/DataStructures/TelemetryStore.cs` | `_history`: dictionary mapping sensor ID to a typed custom batch ring |
| DS-06 use | `SmartXWebApi/Services/GatewayService.cs` | `_floats`, `_integers`, `_booleans`: separate closed generic stores |
| DS-07 | `SmartXWebApi/Services/GatewayService.cs` | `_issues`: required `List<ReportedIssue>`; `_alerts`: bounded `List<TelemetryAlert>` |
| DS-08 | `SmartX.Shared/Validation/ValidationResult.cs` | `Issues`: ordered validation-error list retaining full field/node paths |
| DS-09 | `SmartX.Core/Validation/DeploymentTreeValidator.cs` | `ids` and `active`: hash sets for duplicate IDs and active-path reference cycles |
| DS-10 | `SmartXWebApi/Services/GatewayService.cs` | `_sensors`: sensor registry dictionary with case-insensitive identity lookup |
| DS-11 | `SmartXWebApi/Services/GatewayService.cs` | `_anomalies`: membership set; `_lastAlert`: sensor/kind timestamp dictionary |
| DS-12 | `SmartXWebApi/Services/GatewayService.cs` | `Ingest<T>`'s `groups`: typed per-sensor lists before entering the custom array buffer |
| DS-13 | `SmartXWebApi/Validation/AttachmentUploadValidator.cs` | `Types`: fixed extension-to-content-type dictionary |
| DS-14 | `SmartXWebApi/Storage/EncryptedAttachmentStorage.cs` | `_metadata`: bounded attachment index list under a dedicated lock |
| DS-15 | `SmartXWebApi/Storage/EncryptedAttachmentStorage.cs` | fixed plaintext/ciphertext byte arrays support streaming encryption and verification |
| DS-15 use | `SmartXWebApi/Storage/AuthenticatedAttachmentReadStream.cs` | fixed 64 KiB buffers support the verified download stream without plaintext files |
| DS-16 | `SmartX.Core/Simulation/TelemetrySeedGenerator.cs` | `Sensors`: pre-sized `List<SensorProfile>` for deterministic seeding |
| DS-17 | `SmartXWebApi/Services/GatewayService.cs` | `Generate`: separate float/int/bool producer lists converted to raw arrays |
| DS-18 | `SmartXWebApi/Services/GatewayService.cs` | `Dashboard`: coherent sensor-row snapshot list for counts and priority ordering |
| UI-DS-01 | `SmartXMVC/wwwroot/js/smartx.js` | only current browser inspection state; it is not the assessed server ingestion store |
| ALG-01 | `SmartX.Core/Validation/DeploymentTreeValidator.cs` | `Validate` and recursive `Visit`: null/leaf base cases, depth/node bounds, cycle detection and inherited enabled-state validation |

## Why these are substantive implementations

The collection is **custom**, with its own jagged backing array, circular head, logical counts, retention, defensive copies and chronological transfer. It is not an empty subclass of `List<T>`. `GatewayService.Ingest<T>` calls the typed store, whose rings are then flattened when history/dashboard consumers request snapshots. The code therefore demonstrates the actual transition from sequential raw arrays to a generic list.

For a batch of k readings, copying into the ring is O(k); finding/evicting its oldest slot is O(1). Flattening n retained readings is O(n) time and O(n) output space. The ring retains at most 32 batches, each limited to 256 packets/sensor, so memory is bounded. Dictionary lookup is O(1) on average, not an unconditional worst-case guarantee. History's current implementation flattens before applying its 120-reading display limit; the retained bound limits that work.

All gateway state mutations and snapshots share `_gate`, including background production. The Core stores are intentionally not internally thread-safe: their documented owner serializes use. Immutable packets and copied arrays prevent callers from modifying retained history. This favors coherent assessment evidence over an unmeasured claim of production-scale parallel throughput.

Recursive tree validation visits at most 4,096 nodes and allows depth at most 32. Expected traversal work is O(v + e), with sets using O(v) space and recursion using O(depth) stack space. Active-path detection handles reference cycles, while a separate identifier set rejects duplicates. JSON depth 100 accommodates the validated nesting while preventing unbounded deserialization.

Power operator evidence is at `SmartX.Core/Services/PowerAggregationService.cs`, `Compare`: `meter1 + meter2` and `meter1 - meter2` execute the overloads and feed the Ingestion lab comparison results. Both sample timestamps are retained so non-simultaneous measurements are not disguised.

Method IDs such as `COL-03`, `ING-04`, `TREE-02` and `ENC-STREAM-03` are indexed in `METHOD_INDEX.md`. Inline source citations and the full IIE Harvard reference list are in `REFERENCES.md`.
