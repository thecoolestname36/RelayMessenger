You are implementing the RelayMessenger project at /home/brad/git/thecoolestname36/RelayMessenger
(branch: feat/meshtastic-mqtt-tracker; .NET 10; Blazor WASM + SignalR + MQTTnet + Meshtastic).

MISSION
Complete the remaining implementation milestone: Phase 1 (client/server contract alignment +
integration tests, TDD) followed by Phase 2 (outbound downlink + inbound message flow). Treat
Phase 0 (cleanup) as context/decisions source only; do not re-litigate it.

CONTEXT TO LOAD (in order)
1. /home/brad/git/thecoolestname36/RelayMessenger/AGENTS.md
2. /home/brad/git/thecoolestname36/RelayMessenger/README.MD
3. /home/brad/git/thecoolestname36/RelayMessenger/docs/phase-0-cleanup.md
4. /home/brad/git/thecoolestname36/RelayMessenger/docs/phase-1-contract.md
5. /home/brad/git/thecoolestname36/RelayMessenger/docs/phase-2-downlink.md
6. /home/brad/git/thecoolestname36/RelayMessenger/RelayMessenger.Server/AGENTS.md
7. /home/brad/git/thecoolestname36/RelayMessenger/RelayMessenger.Client/AGENTS.md
8. /home/brad/git/thecoolestname36/RelayMessenger/RelayMessenger.Shared/AGENTS.md

HARD CONSTRAINTS
- dotnet: always pass explicit .csproj/.sln paths; run with `--nologo -v normal`.
- Never read/list bin/, obj/, .git/, TestResults/, wwwroot/_framework/.
- No worktrees; main session only; no background subagents for coding/debugging.
- No new NuGet packages unless required (explain any addition).
- SignalR unit tests: manual IHubContext/IHubClients fakes, NOT Moq.
- Protobuf: inspect PropertyInfo AND FieldInfo for PortNum.
- WASM client: no filesystem/OS/TCP; browser APIs via IJSRuntime.
- JS interop: dispose IJSObjectReference in DisposeAsync; polling via PeriodicTimer; UI refresh via InvokeAsync(StateHasChanged).
- Async: propagate CancellationToken; avoid .Result/.Wait().
- Nullable: enable; file-scoped namespaces; sealed where appropriate; guard clauses.
- Coordinates: divide Meshtastic Lat/Lng by 1e7; filter (0,0). Update NodeStateStore before broadcast.

TDD WORKFLOW (every code step)
- Red: write failing unit test -> run -> confirm fail.
- Green: implement minimal -> run -> pass.
- Refactor: keep behavior.

PHASE 1 (contract) TASKS
- Shared: string-typed DTO + serialization round-trip tests.
- Server: DownlinkService publishes serialized Shared DTO JSON to `msh/US/2/json/mqtt/`.
- Client: NodeService sends Shared DTO JSON via SignalR; SignalRService typed send.
- Integration: recreate RelayMessenger.Server.IntegrationTests; in-process broker OK; contract is subject under test; no Moq for hub.

PHASE 2 (downlink + inbound) TASKS
- Outbound: build proper Meshtastic protobuf downlink (TextMessageFactory pattern).
- Inbound: text mesh packet -> hub `ReceiveMessage` broadcast.

QUALITY
- Small scoped edits (1-3 files/step); build/test commands per phase docs.
- No commits unless asked.
- Context: ~100k usable tokens (Gemma 4 12B, 8Q) - work phase-by-phase; do not load later phases early.

READY STATE
- Phase plans finalized; cleanup decisions locked; contract/downlink decisions locked; docs updated.

