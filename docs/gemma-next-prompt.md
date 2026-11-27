NEXT PROMPT - Gemma (execute the approved next action)

## Response to Gemma's last reply
- Acknowledged. Continuing.

## Decided next action
- Proceed immediately with the pending directive from docs/gemma-continuation-prompt-2.md:
  1. Fix integration test compile: add <FrameworkReference Include="Microsoft.AspNetCore.App" /> to
     RelayMessenger.Server.IntegrationTests.csproj; add InternalsVisibleTo (server -> RelayMessenger.Server.IntegrationTests)
     if missing; replace UnitTest1.cs placeholder with real SendMessage coverage.
  2. Run integration tests; get SendMessage contract test green.
  3. Proceed Phase 2 (outbound downlink + inbound) to completion; close Phase 1 checklist.

## Prompt for next turn
"You are resuming. Proceed with the next action above without further stops. Do not re-litigate Phase 0.
Keep edits small (1-3 files/step). Follow TDD red/green. Continue until Phase 2 is complete, then close
the Phase 1 checklist. No commits unless asked. Context budget: ~100k usable tokens (Gemma 4 12B, 8Q);
phase-by-phase."
