# Smart-X — IoT Data Ingestion and Validation Gateway

Smart-X is a .NET 10 application for monitoring simulated IoT devices at a Rivonia greenhouse facility. It combines an ASP.NET Core Web API with an ASP.NET Core MVC interface for sensor registration, typed telemetry ingestion, anomaly investigation, deployment validation and encrypted device attachments.

The interface uses a custom dark theme, responsive layouts and SVG telemetry charts.

**Module:** Programming 3B — PROG7312  
**Assessment:** Portfolio of Evidence, Part 1  
**Solution:** `SmartX.slnx`

## 1. Assessment scope

Part 1 implements the Sensor Data Ingestion and Telemetry pillar.

The landing page displays all three architectural pillars:

| Pillar | Status |
|---|---|
| Sensor Data Ingestion and Telemetry | Implemented |
| Real-Time Command Stream and History | Disabled — reserved for Part 2 |
| Network Topology and Mesh Routing | Disabled — reserved for the final PoE |

The deployment tree validates configuration for Part 1. It does not implement mesh-routing algorithms. Likewise, telemetry history contains sensor readings, rather than the command-stream history reserved for Part 2.

The application is a local simulation. Physical microcontrollers are not required.

## 2. Requirements

- Windows with a Visual Studio version supporting .NET 10.
- Visual Studio's **ASP.NET and web development** workload.
- The .NET 10 SDK.
- A modern browser.
- Internet access for initial NuGet restoration.
- Git for repository operations.

Check the available SDKs:

```cmd
dotnet --list-sdks
```

A `10.0.x` SDK must be available.

## 3. Clone and open the project

For a fresh checkout:

```cmd
git clone https://github.com/EMGPSD/emgpsd-prog7312-group-2-2026-poe-part1-st10438564.git
cd emgpsd-prog7312-group-2-2026-poe-part1-st10438564
```

Open the folder containing `SmartX.slnx`. If the solution is inside a subfolder, change into that folder before running the commands below.

When using a downloaded archive, extract the complete archive before opening the solution.

## 4. Restore and build

Run these commands from the directory containing `SmartX.slnx`:

```cmd
dotnet restore SmartX.slnx --disable-parallel
dotnet build SmartX.slnx --no-restore -m:1
dotnet dev-certs https --trust
```

Accept the development-certificate trust prompt when requested.

The `-m:1` option performs a serial build. It avoids the parallel MSBuild issue observed in the original isolated verification environment; normal Visual Studio builds can use their default settings.

## 5. NuGet dependencies

The delivered API project declares this direct package dependency:

| Project | Package | Version |
|---|---|---|
| SmartXWebApi | Microsoft.AspNetCore.OpenApi | 10.0.11 |

Normal restore installs the declared dependency and its transitive dependencies.

If the reference needs to be added manually, use Visual Studio's NuGet Package Manager Console:

```powershell
Install-Package Microsoft.AspNetCore.OpenApi -Version 10.0.11 -ProjectName SmartXWebApi
```

No separate MVC, JSON, HttpClient or cryptography package is required by this implementation. These capabilities are supplied by the targeted .NET and ASP.NET Core frameworks.

The frontend uses local CSS, JavaScript and SVG. It does not require Node.js, npm, a charting package, or an external font/icon CDN.

## 6. Run in Visual Studio

1. Open `SmartX.slnx`.
2. Restore packages and build the solution.
3. Open the solution's startup-project configuration.
4. Configure **SmartXWebApi** and **SmartXMVC** to start.
5. Select their **https** launch profiles.
6. Leave the class libraries and verification project disabled as startup projects.
7. Start debugging.

Open the application at:

**https://localhost:7174/**

| Application | Address | Purpose |
|---|---|---|
| SmartXMVC | `https://localhost:7174/` | Browser interface |
| SmartXWebApi | `https://localhost:7034/` | Backend API |

Both web projects must run. The API address is not the dashboard address.

## 7. Run from terminals

First restore and build as described above.

Start the API in one terminal:

```cmd
dotnet run --project SmartXWebApi/SmartXWebApi.csproj --no-build --launch-profile https
```

Start the MVC application in a second terminal:

```cmd
dotnet run --project SmartXMVC/SmartXMVC.csproj --no-build --launch-profile https
```

Keep both terminals open while using the application.

If the configured ports differ in a local copy, inspect:

- `SmartXWebApi/Properties/launchSettings.json`
- `SmartXMVC/Properties/launchSettings.json`
- `SmartXMVC/appsettings.json`

The MVC setting `SmartXApi:BaseUrl` must match the running API address.

## 8. Solution architecture

| Project | Responsibility |
|---|---|
| `SmartX.Shared` | Generic telemetry packets, sensor records, deployment models, enums and transport contracts |
| `SmartX.Core` | Custom collections, typed history storage, validation, seeding and power aggregation |
| `SmartXWebApi` | HTTP endpoints, gateway state, simulation background service and encrypted attachment storage |
| `SmartXMVC` | Razor interface, browser interactions and asynchronous API transport |
| `SmartX.Verification` | Executable regression checks without an additional test-framework dependency |

The browser sends requests to the MVC application's `/gateway/...` routes. MVC forwards permitted requests to the Web API. The API owns the processing state.

The MVC transport uses HttpClient with response streaming. Framework-managed client creation follows the IHttpClientFactory approach described by Microsoft (2026e).

The Core collections do not provide their own internal synchronization. The gateway serializes access through its state lock.

## 9. Main workflows

### Gateway

The landing page introduces the greenhouse scenario and presents the three assessment pillars. Select **Enter operations** to open the dashboard.

### Operations

The dashboard provides:

- Live readings for the selected sensor.
- Anomaly markers and sensor-health indicators.
- Recent alerts with investigation links.
- Alert acknowledgement.
- Simulation controls.
- A pause-view control for inspecting the display.

Pausing the view does not stop telemetry production. Simulation controls manage the producer separately.

### Sensor registry

Users can register, edit, search and filter sensors.

Registration records include:

- Device MAC address or another supported unique identifier.
- Display name.
- Deployment location.
- Sensor category.
- Expected reporting interval.

Supported categories are:

| Category | Typed payload | Simulated measurement |
|---|---|---|
| Environmental | `float` | Temperature in °C |
| Power consumption | `int` | Power in watts |
| Actuator | `bool` | Valve state |

The identifier and category cannot be changed after registration. Register a new sensor when a different identity or payload type is needed.

### Ingestion lab

Users can submit typed telemetry and inspect acceptance or rejection feedback.

Validation includes registered identity, matching payload type, sequence ordering, timestamp constraints and channel-specific value checks.

For a predictable manual demonstration, register a custom sensor. The simulator also writes to seeded sensors, so their sequence numbers continue advancing.

### Power comparison

The comparison feature uses overloaded `PowerReading` operators to calculate combined wattage and signed differences.

The calculations call the actual C# overloads through `PowerAggregationService.Compare`; they are not merely arithmetic performed in browser JavaScript.

Original sample timestamps are returned with the comparison.

### Sensor history

History displays retained readings with timestamps and sequence numbers.

The custom collection transfers a requested recent window from its jagged backing array into a separate `List<TelemetryPacket<T>>`. The API therefore exposes chronological snapshots without exposing internal arrays.

### Sensor attachments

A sensor profile supports uploading configuration files, hardware logs and deployment photographs.

Supported extensions are:

`.json`, `.txt`, `.log`, `.csv`, `.yaml`, `.yml`, `.ini`, `.cfg`, `.png`, `.jpg` and `.jpeg`.

Uploads are associated with a particular sensor. Downloads return the authenticated, decrypted original content.

### Issue journal

Users can report an issue from a sensor profile and resolve it through the issue journal.

Reported issues are stored in a `List<ReportedIssue>`, explicitly addressing the list requirement in the rubric.

### Deployment validation

Users can inspect, validate and save a nested deployment configuration.

Recursive validation detects:

- Null nodes.
- Missing or duplicate identifiers.
- Invalid node names.
- Enabled descendants beneath disabled ancestors.
- Excessive depth or node count.
- Reference cycles supplied through the Core model.

JSON cannot represent object-reference cycles directly. The verification project constructs a cycle in memory to exercise that guard.

Saving also prevents removal or disabling of a location assigned to a registered sensor.

## 10. Simulation and retention

The delivered simulation creates:

- **1,200 sensor profiles.**
- **28,800 initial telemetry packets.**
- Additional simulated readings on a three-second timer.

Initial readings pass through the actual ingestion pipeline.

| Scenario | Behaviour |
|---|---|
| Normal | Routine simulated measurements |
| Spikes | Out-of-range simulated readings |
| Disconnects | Selected devices stop reporting |
| Mixed | Both spike and disconnect conditions |

Disconnection is calculated from receipt timing and the expected reporting interval. Allow approximately 10–15 seconds after startup for the default Mixed scenario to show disconnected devices.

The custom history buffer retains whole batches. Its default capacity is 32 batches per sensor, with at most 256 packets per sensor batch.

As new batches arrive at capacity, the oldest whole batch is evicted.

## 11. Data structures and algorithms

Search the labels below using **Ctrl+Shift+F** in Visual Studio.

Generic types preserve concrete payload types in the ingestion pipeline rather than storing sensor values as `object` (Microsoft, 2026a). Operator overloading gives the watt value type meaningful arithmetic behaviour (Microsoft, 2026b). Jagged arrays support variable-length historical batches (Microsoft, 2026c).

| Label | File location | Structure or algorithm |
|---|---|---|
| DS-01 | `SmartX.Shared/Telemetry/TelemetryPacket.cs` | Generic immutable `TelemetryPacket<T>` record |
| DS-02 | `SmartX.Shared/Telemetry/PowerReading.cs` | Immutable watt value with checked `+` and `-` operators |
| DS-03 | `SmartX.Shared/Deployment/DeploymentNode.cs` | `List<DeploymentNode>` child collection |
| DS-04 | `SmartX.Core/DataStructures/TelemetryBatchBuffer.cs` | Custom circular collection backed by `TelemetryPacket<T>[][]` |
| DS-04 input | `SmartX.Shared/Contracts/Telemetry/TelemetryBatchRequest.cs` | Raw jagged telemetry batches |
| DS-05 | `SmartX.Core/DataStructures/TelemetryBatchBuffer.cs` | Pre-sized lists returned by `ToList()` and `Recent()` |
| DS-06 | `SmartX.Core/DataStructures/TelemetryStore.cs` | Case-insensitive dictionary of typed sensor-history buffers |
| DS-07 | `SmartXWebApi/Services/GatewayService.cs` | User-reported issue list and alert list |
| DS-08 | `SmartX.Shared/Validation/ValidationResult.cs` | Ordered validation-issue list |
| DS-09 | `SmartX.Core/Validation/DeploymentTreeValidator.cs` | Hash sets for duplicate IDs and active-path cycle detection |
| DS-10 | `SmartXWebApi/Services/GatewayService.cs` | Sensor registry dictionary |
| DS-11 | `SmartXWebApi/Services/GatewayService.cs` | Anomaly membership set and alert-throttling dictionary |
| DS-12 | `SmartXWebApi/Services/GatewayService.cs` | Temporary typed packet-grouping lists in `Ingest<T>()` |
| DS-13 | `SmartXWebApi/Validation/AttachmentUploadValidator.cs` | Permitted extension/content-type dictionary |
| DS-14 | `SmartXWebApi/Storage/EncryptedAttachmentStorage.cs` | Attachment metadata index |
| DS-15 | `SmartXWebApi/Storage/EncryptedAttachmentStorage.cs` | Bounded byte buffers for encryption and verification |
| DS-15 use | `SmartXWebApi/Storage/AuthenticatedAttachmentReadStream.cs` | Bounded buffers for authenticated download streaming |
| DS-16 | `SmartX.Core/Simulation/TelemetrySeedGenerator.cs` | Pre-sized seed-profile list |
| DS-17 | `SmartXWebApi/Services/GatewayService.cs` | Separate typed simulation lists |
| DS-18 | `SmartXWebApi/Services/GatewayService.cs` | Dashboard snapshot list |
| UI-DS-01 | `SmartXMVC/wwwroot/js/smartx.js` | Browser inspection state |
| ALG-01 | `SmartX.Core/Validation/DeploymentTreeValidator.cs` | Recursive deployment validation |

The assessed collection is a custom implementation, rather than an empty subclass of `List<T>`. It owns its circular indexing, jagged storage, retention rules and snapshot behaviour.

### Collection refinements

The refinement sequence adds the following behaviours:

- `Latest()` reads the newest packet directly without creating a history list.
- `Recent(maximumReadings)` copies only the requested recent window.
- Capacities outside 1–128 throw a descriptive exception.
- Null batch arrays and null packets are rejected.
- Incoming arrays are copied and checked before retained state changes.
- Empty batches remain a no-op.

Method comments identify `COL-01` through `COL-06`. `STORE-02` and `STORE-03` identify the corresponding history and latest-packet access methods.

### Complexity

| Operation | Complexity |
|---|---|
| Append a batch of k packets | O(k) time and copied-array space |
| Select the oldest slot for eviction | O(1) |
| Retrieve the latest packet from a buffer | O(1), without a snapshot allocation |
| Copy all n retained packets | O(n) time and output space |
| Copy k recent packets across b retained batches | O(b + k) time and O(k) output space |
| Dictionary lookup | O(1) average time |
| Validate a deployment tree | Expected O(v + e) traversal work, within configured bounds |

The collection owner must synchronize concurrent access. Returned arrays and lists are independent containers; telemetry packets are immutable records.

Dashboard chart values are display projections. The server's raw ingestion and history stores retain their concrete `float`, `int` or `bool` payload types.

## 12. Dashboard engagement and research

The chosen strategy is a live anomaly-investigation dashboard: notice an unusual signal, identify its sensor, inspect the context and record an action.

Iancu, Ciolofan and Drăgoicea (2024) describe an IoT monitoring architecture combining sensor visualisation and alerting. Harum, Emran, Md Fauadi, Hamid, Khambari, Ridzuan and Kchouri (2024) describe industrial IoT logging, dashboard integration and anomaly notification.

These studies informed Smart-X's monitoring approach. Smart-X adapts the ideas to its greenhouse simulation; it does not claim to reproduce the studies' implementations or measured results.

The research considers five strategies:

1. Live trends with anomaly markers.
2. Actionable alerts.
3. Contextual sensor drill-down.
4. Historical comparison.
5. Guided configuration feedback.

The interface combines status text with colour, sensor-specific charts, profile links and an issue journal. Faster troubleshooting is a design objective, not a measured human-usability result.

The separate Word report is:

`docs/SmartX_Research_Report.docx`

It contains the five strategies, a 500-word justification, a diagram and journal references. The original report was rendered as three pages with Arial 11 body text and 1.5-line spacing.

## 13. Attachment storage and protection

The upload implementation uses multipart streaming rather than requiring a complete file to be buffered through standard form binding. Microsoft's upload guidance distinguishes buffered and streaming approaches (Microsoft, 2026d).

Payload files use AES-256-GCM authenticated encryption in 64 KiB records. The .NET `AesGcm` API provides authenticated encryption operations (Microsoft, [s.a.]).

The implementation:

- Derives a separate file key using HKDF-SHA256 and a random salt.
- Authenticates the sensor association, attachment identity and record position.
- Requires an authenticated terminal record.
- Checks the complete encrypted file before returning download content.
- Streams the decrypted content in a second bounded pass.
- Avoids plaintext temporary files.

ASP.NET Core Data Protection protects the master key and attachment metadata. Under the documented Windows user-profile defaults, the Data Protection key ring is protected at rest with DPAPI (Anderson, 2025). Hosting conditions and explicit key-storage overrides can change that behaviour.

The default Windows attachment directory is:

```text
%LOCALAPPDATA%\SmartX\Attachments
```

The `SmartX:StoragePath` setting can override attachment storage.

Preserve both attachment storage and the Data Protection keys needed to decrypt it. Do not place runtime keys or uploaded files in `wwwroot` or commit them to GitHub.

Photo signatures and text NUL-byte checks provide format screening. They are not a malware scanner.

## 14. Configuration limits

| Item | Delivered limit or default |
|---|---|
| Registered sensors | 5,000 maximum |
| Reporting interval | 1–3,600 seconds |
| Raw batches per ingestion request | 1–64 |
| Packets per raw batch | 4,096 maximum |
| Packets per request | 8,192 maximum |
| Packets per sensor within one raw batch | 256 maximum |
| Retained batches per sensor | 32 by default |
| Supported custom buffer capacity | 1–128 batches |
| Store history request | Clamped to 1–512 readings; default 120 |
| Deployment depth | Maximum depth 32 |
| Deployment nodes | Maximum 4,096 visited nodes |
| Attachment size | Non-empty; maximum 8 MiB |
| Upload HTTP request | Maximum 9 MiB, allowing multipart overhead |
| Attachments per sensor | 20 maximum |
| Total attachments | 1,000 maximum |
| Concurrent uploads | 4 maximum |
| Retained alerts | 500 maximum |
| Retained reported issues | 500 maximum |

## 15. Verification

Run the executable checks after building:

```cmd
dotnet run --project SmartX.Verification/SmartX.Verification.csproj --no-build
```

The process returns a non-zero exit code if a check fails.

The original delivered implementation was verified with:

- A five-project build reporting zero warnings and errors.
- 24 executable backend checks.
- 22 live HTTP assertions.
- Browser checks covering registration, editing, ingestion, history, power comparison, attachments, issues, deployment and simulation controls.
- Desktop and mobile layout inspection.
- HTTPS communication between the MVC application and API.

These results describe the original verification run. They do not certify every later local edit.

The existing suite does not exhaustively cover each subsequent refinement. Rebuild and repeat relevant checks after modifying the application. Record new results separately instead of treating earlier evidence files as a new test run.

Windows-specific DPAPI behaviour was not executed in the original Linux verification environment.

Evidence and supporting notes are in:

- `docs/VERIFICATION.md`
- `docs/BUILD_RESULTS.txt`
- `docs/CORE_TEST_RESULTS.txt`
- `docs/HTTP_TEST_RESULTS.txt`
- `docs/HTTPS_TEST_RESULTS.txt`
- `docs/BROWSER_TEST_RESULTS.txt`
- `docs/screenshots/`

## 16. Troubleshooting

### The browser does not open

Start both web projects and open:

`https://localhost:7174/`

Check the console output for the actual listening addresses.

### The dashboard reports that the API is unavailable

Confirm that:

1. SmartXWebApi is running.
2. Its HTTPS address matches `SmartXApi:BaseUrl`.
3. The development certificate is trusted.
4. Another process is not occupying the configured port.

### HTTPS certificate errors

Run:

```cmd
dotnet dev-certs https --trust
```

Restart the applications after trusting the certificate. Do not disable certificate validation to hide the problem.

### Build files are locked

Stop debugging and close any terminals running the API, MVC application or verification executable. Then rebuild.

### Upload rejected

Check the file extension, content, non-empty size and attachment quotas. Also verify that the running account can access its storage directory and protected keys.

### Manual telemetry rejected

Check the sensor ID, selected channel, sequence number and timestamp. Seeded devices continue receiving simulated readings; use a custom sensor for controlled manual submissions.

### Data disappears after restarting the API

Telemetry, registration changes, deployment edits and issues are held in memory. Their reset on restart is expected in this simulation.

## 17. Known limitations

- There is no durable telemetry database.
- Custom registrations, issue records and deployment changes reset when the API restarts.
- Encrypted attachment files, metadata and keys persist separately.
- After restart, attachments for a custom sensor become accessible again when the same sensor ID is registered.
- The application has no user-account or authentication system.
- It is configured for local assessment use, not public internet deployment.
- Simulation results are not a production throughput guarantee.
- No human-user study has established reduced troubleshooting time.
- Docker configuration is not included.
- Part 2 command processing and final-PoE mesh routing remain outside this implementation.

## 18. Documentation

| File | Purpose |
|---|---|
| `README.md` | Setup, functionality, current collection behaviour, limitations and consolidated references |
| `docs/SmartX_Research_Report.docx` | Part 1 research report |
| `docs/REFERENCES.md` | Original ten-source project reference list and attribution context |
| `docs/DATA_STRUCTURES.md` | Detailed data-structure locations and explanations |
| `docs/METHOD_INDEX.md` | Original method/comment navigation index |
| `docs/FILE_MAP.md` | Source-file responsibilities |
| `docs/REQUIREMENTS_COVERAGE.md` | Implementation-to-rubric mapping |
| `docs/DEMO_WALKTHROUGH.md` | Suggested demonstration sequence |
| `docs/SUBMISSION_CHECKLIST.md` | Submission preparation checklist |
| `docs/NUGET_PMC.txt` | Package Manager Console command |
| `docs/planning/` | Historical scaffold planning documents |
| `docs/source/Smart-X_Part_1_OCR_Transcript.txt` | Supplied assessment transcript |

The original indexes and test evidence are snapshots of the delivered version. For the subsequent collection refinements, use the current source comments and Section 11 of this README.



## 19. Referencing conventions

The references below follow the IIE-adapted Harvard conventions in the verified 2022 guide (The Independent Institute of Education, 2022). Any newer guide specifically prescribed by the lecturer takes precedence.

The list retains all ten original project sources and includes three Git manuals consulted during repository setup.

The Microsoft year-letter labels remain consistent with the existing inline code references:

| Citation | Source |
|---|---|
| Microsoft, 2026a | Generic types and methods |
| Microsoft, 2026b | Operator overloading |
| Microsoft, 2026c | The array reference type |
| Microsoft, 2026d | Upload files in ASP.NET Core |
| Microsoft, 2026e | Use the IHttpClientFactory |
| Microsoft, [s.a.] | AesGcm class |

`[s.a.]` indicates that no reliable publication/update year was established for the cited source. Access dates refer to the source-verification date.

## 20. Reference list

Anderson, R. 2025. *Data Protection key management and lifetime in ASP.NET Core*. [Online]. Available at: https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/default-settings?view=aspnetcore-10.0 [Accessed 14 September 2026].

Git. 2026a. *git — the stupid content tracker*. [Online]. Available at: https://git-scm.com/docs/git [Accessed 14 September 2026].

Git. 2026b. *git-add — add file contents to the index*. [Online]. Available at: https://git-scm.com/docs/git-add [Accessed 14 September 2026].

Git. 2026c. *git-merge — join two or more development histories together*. [Online]. Available at: https://git-scm.com/docs/git-merge [Accessed 14 September 2026].

Harum, N., Emran, N.A., Md Fauadi, M.H.F., Hamid, E., Khambari, M.N.M., Ridzuan, M.M. and Kchouri, M. 2024. An automated data logger system for real-time monitoring and anomaly detection in industrial IoT environment. *Journal of Advanced Manufacturing Technology*, 18(3):219–236. [Online]. Available at: https://jamt.utem.edu.my/jamt/article/view/6798 [Accessed 14 September 2026].

Iancu, G., Ciolofan, S.N. and Drăgoicea, M. 2024. Real-time IoT architecture for water management in smart cities. *Discover Applied Sciences*, 6, article 191. [Online]. Available at: https://doi.org/10.1007/s42452-024-05855-9 [Accessed 14 September 2026].

Microsoft. [s.a.]. *AesGcm class*. [Online]. Available at: https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.aesgcm?view=net-10.0 [Accessed 14 September 2026].

Microsoft. 2026a. *Generic types and methods*. [Online]. Available at: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/generics [Accessed 14 September 2026].

Microsoft. 2026b. *Operator overloading — define unary, arithmetic, equality, and comparison operators*. [Online]. Available at: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/operator-overloading [Accessed 14 September 2026].

Microsoft. 2026c. *The array reference type*. [Online]. Available at: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/arrays [Accessed 14 September 2026].

Microsoft. 2026d. *Upload files in ASP.NET Core*. [Online]. Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-10.0 [Accessed 14 September 2026].

Microsoft. 2026e. *Use the IHttpClientFactory*. [Online]. Available at: https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory [Accessed 14 September 2026].

The Independent Institute of Education. 2022. *Harvard style reference guide – adapted for The IIE*. [Online]. Available at: https://irp.cdn-website.com/271d35b6/files/uploaded/The%20IIE%20Harvard%20Style%20Reference%20Guide%20%E2%80%93%20Adapted%20for%20The%20IIE%202022.pdf [Accessed 14 September 2026].