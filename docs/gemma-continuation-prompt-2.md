CONTINUATION PROMPT - Gemma (resume: fix integration test compile + continue to completion)

## Validation of the latest halt
- The halt ("compiler cannot find the Program class from the server project within the integration
  test project, even though we added a partial declaration and the correct references ... .NET SDK
  handles implicit namespaces for web projects during testing") is plausible and likely accurate.
- Likely causes: missing <FrameworkReference Include="Microsoft.AspNetCore.App" /> in
  RelayMessenger.Server.IntegrationTests.csproj and/or missing InternalsVisibleTo for the generated
  internal Program class from the server assembly into the integration test assembly.
- Placeholder UnitTest1.cs should be replaced by the real HubIntegrationTests contract test.

## Directive - continue until completion
- Fix integration test project so WebApplicationFactory<RelayMessenger.Server.Program> compiles.
- Replace placeholder UnitTest1.cs with real SendMessage integration coverage; run the test.
- Then proceed Phase 2 (outbound downlink + inbound) to completion per docs/phase-2-downlink.md.
- After Phase 2, return to Phase 1 checklist closure (Step 1.5 integration green).

## Constraints (recap)
- Explicit dotnet paths; --nologo -v normal.
- No bin/obj/.git reads.
- No new NuGet unless required.
- Manual SignalR fakes in unit tests, not Moq.
- WASM/sandbox/JS-interop/async/nullability rules from AGENTS.md.

## Context budget
- ~100k usable tokens (Gemma 4 12B, 8Q); phase-by-phase; do not load later phases early.
