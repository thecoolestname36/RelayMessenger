# Phase 1 - Client/Server Contract Alignment + Integration Tests (TDD)

## Objective
Finalize the client/server wire contract using a Shared string-typed DTO; lock it with TDD unit tests; add integration tests probing real server endpoints (in-process MQTT broker acceptable; the server/client CONTRACT is the subject under test, not HTTP/MQTT itself).

## Context for the Agent
- Hub: `RelayHub.SendMessage(object)` deserializes `SendMessageRequest` -> `DownlinkService.PublishDownlinkAsync` -> MQTT topic `msh/US/2/json/mqtt/` -> broadcast `ReceiveMessage`. `UpdatePosition(uint,double,double)` -> broadcast `NodeUpdated`.
- Client: `SignalRService.SendMessageAsync(string)`, `NodeService.SendDownlink/SendClientMessage` (anonymous-object JSON).
- Shared: `SendMessageRequest(uint To, string Message)`, `ClientMessageRequest`.
- Decision: Shared string-typed DTO; typed hub methods; strongly-typed SignalR sends; `SignalRHubConnectionService` pattern (IAsyncDisposable, WebSockets, config endpoint, status, reconnect); JS interop sourced from `/home/brad/git/thecoolestname36/bluForTrackerWasmHosted/Client/Components/MapView.razor.js`; integration broker: in-process OK.

## Steps (atomic; TDD per step)
- [ ] **Step 1.1 - Red: Shared string DTO contract tests** (files: `RelayMessenger.Shared.UnitTests/MessageSynchronizationTests.cs`, `RelayMessenger.Shared.UnitTests/StateSynchronizationTests.cs`).
  - Write failing tests first for string-typed `SendMessageRequest` serialization round-trip.
- [ ] **Step 1.2 - Red: Server downlink contract tests** (files: `RelayMessenger.Server.UnitTests/DownlinkServiceTests.cs`).
  - Failing test: publish serialized Shared DTO JSON to topic `msh/US/2/json/mqtt/`.
- [ ] **Step 1.3 - Red: Client contract tests** (files: `RelayMessenger.Client.UnitTests/NodeServiceTests_v2.cs`).
  - Failing tests: NodeService sends Shared DTO JSON via SignalR; SignalRService typed send.
- [ ] **Step 1.4 - Green: Implement contract** (files: `RelayMessenger.Shared/Models/SendMessageRequest.cs`, `RelayMessenger.Server/Hubs/RelayHub.cs`, `RelayMessenger.Server/Services/DownlinkService.cs`, `RelayMessenger.Client/Services/SignalRService.cs`, `RelayMessenger.Client/Services/NodeService.cs`).
  - Minimal implementation to satisfy tests; typed hub methods; strongly-typed sends; keep `UpdatePosition`.
- [ ] **Step 1.5 - Integration tests** (files: new `RelayMessenger.Server.IntegrationTests/*`).
  - Recreate project; in-process broker OK; contract under test; no Moq for `IHubContext`.
  - Tests: `GET /Nodes` -> 200 JSON array; SignalR connect -> invoke `UpdatePosition` -> receive `NodeUpdated`; invoke `SendMessage` -> downlink published with expected JSON.

## TDD Flow (per step)
- Red: write failing test -> run test -> confirm fail.
- Green: implement minimal code -> run test -> confirm pass.
- Refactor: keep behavior.

## Build / Test Commands
- `dotnet test RelayMessenger.Shared.UnitTests/RelayMessenger.Shared.UnitTests.csproj --nologo -v normal`
- `dotnet test RelayMessenger.Server.UnitTests/RelayMessenger.Server.UnitTests.csproj --nologo -v normal`
- `dotnet test RelayMessenger.Client.UnitTests/RelayMessenger.Client.UnitTests.csproj --nologo -v normal`
- `dotnet test RelayMessenger.Server.IntegrationTests/RelayMessenger.Server.IntegrationTests.csproj --nologo -v normal`

## Definition of Done (Phase 1)
- Shared string DTO + typed server/client contract; unit tests green; integration tests pass probing contract.

## Running Checklist
- [ ] Step 1.1 red/green
- [ ] Step 1.2 red/green
- [ ] Step 1.3 red/green
- [ ] Step 1.4 green
- [ ] Step 1.5 integration green

## Note
- After Phase 0, the first coding milestone is Phase 1 + Phase 2 (user decision).
- No new NuGet packages unless required; explain any addition clearly.
