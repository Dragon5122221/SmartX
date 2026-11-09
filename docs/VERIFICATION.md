# Verification of the delivered implementation

## Build and runtime

- Built all five projects with .NET SDK 10.0.100 on Linux and the supplied/restored Microsoft.AspNetCore.OpenApi 10.0.11 package.
- Final build completed with **0 warnings and 0 errors**, including Razor compilation.
- Both real web processes booted and communicated through the MVC-to-API boundary.
- A separate **HTTPS → MVC → HTTPS API** check passed with certificate chain and hostname validation enabled. No certificate-validation bypass was used. Browser automation used explicit loopback HTTP as an isolated test configuration; shipped settings use HTTPS.

## Regression evidence

- **24 executable backend checks passed**, including unequal batches, order, defensive snapshots, retention, operator arithmetic/overflow, valid/null/duplicate/cyclic/deep/unsafe trees, 1,200-device/28,800-packet seeding, typed values, wrong-type and duplicate rejection, issue resolution, encrypted round-trip, restart metadata/key recovery, ciphertext-at-rest, tamper/truncation rejection, association enforcement and empty-file rejection.
- **22 live HTTP assertions passed**, including all Razor workspaces, the API proxy, anti-forgery rejection, registry validation, typed binding, missing records, 24 concurrent dashboard requests, background ticks and Mixed-scenario disconnection/alert generation.
- Browser workflow checks passed for registration, editing, searching, manual typed ingestion/history, meter comparison, encrypted file upload/download (byte equality), issue reporting/resolution, recursive validation/save, simulation controls and pause-view interaction.
- Desktop 1440×1000 and mobile 390×844 were inspected. Tested mobile pages had no document-level horizontal overflow. No JavaScript page exceptions occurred.
- The research DOCX rendered to **three pages** and every page was visually inspected. Body text is Arial 11 with 1.5 spacing. Its chosen-strategy section has exactly **500 whitespace-delimited words**; Word's punctuation/hyphen counting can differ slightly.

## One measured seed run

MEASURE: seed duration 32.84 ms; managed memory 9,787,600 bytes

This is one local observation, not a throughput SLA or a comparison benchmark. Managed-memory reporting is process-level at that moment, not peak allocation attributable exclusively to the data structures. Concurrency checks establish successful responses in the tested case; they are not a full production load test.

## Evidence files

`BUILD_RESULTS.txt`, `CORE_TEST_RESULTS.txt`, `HTTP_TEST_RESULTS.txt`, `HTTPS_TEST_RESULTS.txt`, `BROWSER_TEST_RESULTS.txt` and the `screenshots/` directory accompany the source. Run `dotnet run --project SmartX.Verification --no-build` after building to repeat the backend checks.

## Remaining platform and assessment checks

The user-specific Windows Visual Studio installation, Windows DPAPI encryption of the Data Protection key ring, and any external hosting environment were not executed here. Run both HTTPS profiles locally before submission. No external penetration test, independent cryptographic review or human usability study is claimed. GitHub history, contributors, final video/presentation and submission remain the student's external deliverables.
