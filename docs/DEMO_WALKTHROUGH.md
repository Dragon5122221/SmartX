# Suggested assessment demonstration

1. Open the solution, show .NET 10 projects and a successful build. Start both HTTPS profiles.
2. Show the exact three pillars. Explain why command history and mesh routing are disabled.
3. Enter Operations. Explain the 1,200 seeded devices, three data types, recent readings and receipt-time health. Run Mixed and wait for spike/disconnect evidence. Inspect an affected sensor.
4. In Visual Studio, open `TelemetryPacket.cs` (DS-01), `TelemetryBatchBuffer.cs` (DS-04/05) and `TelemetryStore.cs` (DS-06). Trace `GatewayService.Ingest<T>` into the custom jagged ring and `ToList` back into the history response.
5. Register `DEMO-TEMP-01` in an enabled zone. Send float 24.5 with sequence 1 in the Ingestion lab. Send sequence 1 again to demonstrate rejection. Submit a wrong type to demonstrate type/category validation.
6. Inspect the profile and history. Upload a small hardware.log or deployment.png; download it and show byte-preserving encryption/decryption. Show `EncryptedAttachmentStorage.cs` and the external runtime storage directory without exposing keys.
7. Compare SX-0002 and SX-0005 in the Ingestion lab. Open `PowerReading.cs` and `PowerAggregationService.Compare` to show actual operator calls.
8. Open Deployment, validate the tree, introduce a duplicate ID, observe path-specific errors, then restore the valid configuration. Show `DeploymentTreeValidator.Visit`, including recursive call and guards. Run the verification program to demonstrate the in-memory reference-cycle case.
9. Report an observation from a sensor profile and resolve it in Issue journal. Show `_issues` (DS-07), which is an actual List<ReportedIssue>.
10. Show the research report, REFERENCES.md and DATA_STRUCTURES.md. Explain that improved human troubleshooting time is a design objective, not a measured usability result. Confirm the lecturer's video/presentation instructions before recording the final submission.
