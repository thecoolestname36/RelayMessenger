# RelayMessenger

RelayMessenger is a .NET 10 Blazor WebAssembly (WASM) PWA acting as a dual-purpose gateway between a Meshtastic mesh network and web clients.

## Goal
- Real-time web-based monitoring of Meshtastic mesh nodes (position, telemetry, info) and support outbound text downlink + inbound message flow through an embedded MQTT broker / SignalR gateway.

## Architecture
- **Server (`RelayMessenger.Server`)**: Hosts embedded MQTT broker (`MQTTnet.Server`) for inbound node packets, MQTT client (`IMqttClient`) for outbound mesh downlinks, SignalR hub (`RelayHub` at `/relayhub`), and `GET /Nodes`.
- **Client (`RelayMessenger.Client`)**: Blazor WASM PWA using `SignalRService` / `NodeService`; receives pushed `NodeUpdated` events; sends downlink via SignalR; Google Maps via JS interop.

- **Shared (`RelayMessenger.Shared`)**: Common models/state: `NodeStateModel`, `NodeStateStore`, `SendMessageRequest`, `ClientMessageRequest`, `Routing`, crypto helpers.

## Current Phase Status
- Implementation is delivered via phase-based plan files:
  - `docs/phase-0-cleanup.md` — cleanup & architecture alignment.
  - `docs/phase-1-contract.md` — client/server contract + integration tests (TDD).
  - `docs/phase-2-downlink.md` — outbound downlink + inbound message flow.
- After Phase 0, the first coding milestone = Phase 1 + Phase 2.

## Key Technical Notes
- Coordinates: divide Meshtastic `LatitudeI` / `LongitudeI` by 10,000,000; filter `(0,0)`.
- Protobuf: use reflection for `PortNum` (check `value__` / `Value`).
- State: update `NodeStateStore` before SignalR broadcast.
- Downlink: publish serialized `SendMessageRequest` JSON to MQTT topic `msh/US/2/json/mqtt/`.

## Development Commands
```bash
dotnet build RelayMessenger.Server/RelayMessenger.Server.csproj --nologo -v minimal
dotnet build RelayMessenger.Client/RelayMessenger.Client.csproj --nologo -v minimal
dotnet test RelayMessenger.Server.UnitTests/RelayMessenger.Server.UnitTests.csproj --nologo -v minimal
dotnet test RelayMessenger.Server.UnitTests/RelayMessenger.Server.UnitTests.csproj --filter "FullyQualifiedName~<TestName>" --nologo -v minimal
```

## AI Agent Context
- Read `AGENTS.md` and the phase docs in `docs/phase-*.md` before coding.
- TDD: red/green per step; integration tests probe contract (in-process broker acceptable).
- Context hygiene: never read `bin/`, `obj/`, `.git/`, `TestResults/`, `wwwroot/_framework/`.

## Reference Examples
- MQTTnet: `/home/brad/git/dotnet/MQTTnet`
- Meshtastic: `/home/brad/git/meshtastic`
