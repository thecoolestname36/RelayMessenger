CONTINUATION PROMPT - Gemma (resume from halt)

## Resume Context
- Gemma halted mid-implementation of Phase 1 (client/server contract + integration tests).
- Halted reply (now verified consistent with current code): server uses
  `app.MapHub<RelayHub>(gatewayOptions.HubUrl);` with default `/relayhub`;
  integration tests should use reliable connection setup; simplify HubIntegrationTests to
  assert `SendMessage` hub invocation completes without exception before broker-level checks.

## Verified Current State (just re-read)
- `RelayMessenger.Server/Program.cs`: registers MeshBrokerOptions + GatewayOptions; maps hub at
  `gatewayOptions.HubUrl`; nodes endpoint at `gatewayOptions.NodesEndpoint`.
- `RelayMessenger.Server/Configuration/GatewayOptions.cs`: HubUrl = "/relayhub"; NodesEndpoint = "/Nodes".
- `RelayMessenger.Server.IntegrationTests/HubIntegrationTests.cs`: WebApplicationFactory +
  HubConnectionBuilder; probing `SendMessage` with `SendMessageRequest(123, "...")`; asserts no exception.

## Directive - Keep Going, Do Not Stop
- Proceed immediately with the next incomplete step in docs/phase-1-contract.md (Step 1.5 integration
  tests) without pausing to ask the user.
- Complete HubIntegrationTests: run the simplified no-exception SendMessage test; then add remaining
  integration probes (GET /Nodes, UpdatePosition -> NodeUpdated, SendMessage -> downlink).
- After Phase 1 unit + integration green, move to Phase 2 (downlink + inbound).
- Follow TDD red/green where applicable; keep edits small (1-3 files/step).

## Constraints (recap)
- Explicit dotnet paths; `--nologo -v normal`.
- No bin/obj/.git reads.
- No new NuGet unless required.
- Manual SignalR fakes in unit tests, not Moq.
- WASM/sandbox/JS-interop/async/nullability rules from AGENTS.md.

## Context budget
- ~100k usable tokens (Gemma 4 12B, 8Q); work phase-by-phase; do not load later phases early.
