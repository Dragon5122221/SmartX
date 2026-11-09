# References and source attribution

## How the sources are used

`TelemetryPacket<T>` and typed validators retain concrete payload types in line with the generic-type guidance (Microsoft, 2026a). `PowerReading` overloads watt addition and subtraction using user-defined operators (Microsoft, 2026b). The custom ring owns jagged arrays and explicitly transfers their contents into typed lists (Microsoft, 2026c). The custom ring algorithm, simulated greenhouse scenarios and visual design are original application work, not copied Microsoft samples.

The MVC transport uses a factory-managed HttpClient (Microsoft, 2026e). Multipart actions prevent eager form-model binding and consume file streams directly, following the documented distinction between buffered and streamed uploads (Microsoft, 2026d). File encryption uses the framework AesGcm primitive (Microsoft, [s.a.]); the bounded record format, identity binding and terminal-record checks are application design. A reference to the primitive does not claim external review of the complete application protocol. Data Protection's environment-dependent key storage is documented by Anderson (2025).

The engagement research draws on two journal articles: a water-monitoring architecture connecting anomaly information with sensor history (Iancu, Ciolofan & Drăgoicea, 2024), and industrial logging with dashboard visualisation and alerts (Harum, Emran, Md Fauadi, Hamid, Khambari, Ridzuan & Kchouri, 2024). The Word report separates their findings from Smart-X design objectives. No usability or troubleshooting-speed results are attributed to Smart-X without a user study.

The reference format uses the publicly available IIE guide consulted for author-date citations, full author lists on first use, alphabetised references, same-year letter suffixes, online access details and missing dates (The Independent Institute of Education, 2022). This is the **2022 guide actually verified**, not a claimed verification of a newer campus-specific edition. Check any later lecturer-supplied variant before submission. The AesGcm reference page did not expose a reliable publication/update date, so `[s.a.]` is used instead of inventing one.

## Full reference list

Anderson, R. 2025. *Data Protection key management and lifetime in ASP.NET Core*. [Online]. Available at: https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/default-settings?view=aspnetcore-10.0 [Accessed 14 September 2026].

Harum, N., Emran, N.A., Md Fauadi, M.H.F., Hamid, E., Khambari, M.N.M., Ridzuan, M.M. and Kchouri, M. 2024. An automated data logger system for real-time monitoring and anomaly detection in industrial IoT environment. *Journal of Advanced Manufacturing Technology*, 18(3):219–236. [Online]. Available at: https://jamt.utem.edu.my/jamt/article/view/6798 [Accessed 14 September 2026].

Iancu, G., Ciolofan, S.N. and Drăgoicea, M. 2024. Real-time IoT architecture for water management in smart cities. *Discover Applied Sciences*, 6:191. [Online]. Available at: https://doi.org/10.1007/s42452-024-05855-9 [Accessed 14 September 2026].

Microsoft. 2026a. *Generic types and methods*. [Online]. Available at: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/generics [Accessed 14 September 2026].

Microsoft. 2026b. *Operator overloading*. [Online]. Available at: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/operator-overloading [Accessed 14 September 2026].

Microsoft. 2026c. *The array reference type*. [Online]. Available at: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/arrays [Accessed 14 September 2026].

Microsoft. 2026d. *Upload files in ASP.NET Core*. [Online]. Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-10.0 [Accessed 14 September 2026].

Microsoft. 2026e. *Use the IHttpClientFactory*. [Online]. Available at: https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory [Accessed 14 September 2026].

Microsoft. [s.a.]. *AesGcm class*. [Online]. Available at: https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.aesgcm?view=net-10.0 [Accessed 14 September 2026].

The Independent Institute of Education. 2022. *Harvard style reference guide adapted for The IIE*. [Online]. Available at: https://irp.cdn-website.com/271d35b6/files/uploaded/The%20IIE%20Harvard%20Style%20Reference%20Guide%20%E2%80%93%20Adapted%20for%20The%20IIE%202022.pdf [Accessed 14 September 2026].

## Inline citation locations

| Source | Application/reference location |
|---|---|
| Anderson (2025) | `SmartXWebApi/Storage/AttachmentKeyProvider.cs`, README encryption section |
| Harum et al. (2024) | `docs/SmartX_Research_Report.docx`, `docs/RESEARCH_REPORT_TEXT.md` |
| Iancu et al. (2024) | `docs/SmartX_Research_Report.docx`, `docs/RESEARCH_REPORT_TEXT.md` |
| Microsoft (2026a) | `SmartX.Shared/Telemetry/TelemetryPacket.cs`, `SmartX.Core/Validation/TelemetryPacketValidator.cs` |
| Microsoft (2026b) | `SmartX.Shared/Telemetry/PowerReading.cs` |
| Microsoft (2026c) | `SmartX.Core/DataStructures/TelemetryBatchBuffer.cs`, `SmartX.Shared/Contracts/Telemetry/TelemetryBatchRequest.cs` |
| Microsoft (2026d) | `SmartXWebApi/Controllers/AttachmentsController.cs`, both projects' `Filters/StreamingRequestAttribute.cs` |
| Microsoft (2026e) | `SmartXMVC/Services/SmartXApiClient.cs` |
| Microsoft ([s.a.]) | `SmartXWebApi/Storage/EncryptedAttachmentStorage.cs` |
| The IIE (2022) | This file's referencing-method explanation |

The runtime package was additionally checked at the official NuGet feed and restored successfully as version 10.0.11. Package installation instructions are in `NUGET_PMC.txt`; this operational check is not an additional academic source in the ten-entry list.
