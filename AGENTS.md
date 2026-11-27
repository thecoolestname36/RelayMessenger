# AGENTS.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview
RelayMessenger is a .NET 10 Blazor WebAssembly (WASM) PWA acting as a dual-purpose gateway between a Meshtastic mesh network and web clients:
- **Server (`RelayMessenger.Server`)**: Hosts an embedded **MQTT Broker** (`MQTTnet.Server`) for inbound node packets, an **MQTT Client** (`IMqttClient`) for outbound mesh downlinks, and a **SignalR Hub** (`RelayHub`) for real-time client updates.
- **Client (`RelayMessenger.Client`)**: Blazor WASM PWA (`SignalRService`, `NodeService`, `Map.razor`) receiving pushed `NodeUpdated` events and sending outbound commands via SignalR.
- **Shared (`RelayMessenger.Shared`)**: Common models (`NodeStateModel`, Protobuf definitions) and state management (`NodeStateStore`).
- **Tests (`RelayMessenger.Server.UnitTests`)**: xUnit suite for packet processing and hub integration logic.

## Project Plan & Key Files
- **Implementation Plan**: Read `docs/plan.md` when checking phase status, architecture details, or upcoming downlink requirements.
- **Critical Files**: `RelayMessenger.Server/Program.cs`, `RelayMessenger.Server/Services/Processors/MeshtasticPacketProcessor.cs`, `RelayMessenger.Server/Hubs/RelayHub.cs`, `RelayMessenger.Client/Services/NodeService.cs`, `RelayMessenger.Client/Services/SignalRService.cs`, `RelayMessenger.Client/Pages/Map.razor`.

## Development Commands
Always pass `--nologo -v normal` to `dotnet` commands to keep output concise:
```bash
dotnet restore
dotnet build --nologo -v normal
dotnet test RelayMessenger.Server.UnitTests/RelayMessenger.Server.UnitTests.csproj --nologo -v normal
dotnet test RelayMessenger.Server.UnitTests/RelayMessenger.Server.UnitTests.csproj --filter "FullyQualifiedName~<TestName>" --nologo -v normal
```

## Technical Specifications & Guardrails
- **Coordinate Normalization & Validation**: Divide raw Meshtastic `LatitudeI` and `LongitudeI` by `10,000,000` (`1e7`) for decimal degrees. **Always filter out `(0,0)` coordinates or missing telemetry** before updating `NodeStateStore`.
- **Protobuf Reflection (`PortNum`)**: Because `PortNum` may be generated as an Enum or a Message depending on library versions, inspect it via reflection in `MeshtasticPacketProcessor` (checking both `value__` field and `Value` property).
- **State Consistency**: Always update `NodeStateStore` *before* triggering SignalR broadcasts so primary storage matches pushed client state.
- **Concurrency & Ordering**: Use a Last-Write-Wins (LWW) mechanism in `NodeService`. Updates are only applied if the incoming record's timestamp is newer than the existing one to handle out-of-order messages or concurrent updates securely using `ConcurrentDictionary.AddOrUpdate`.
- **SignalR Mocking Standard**: Use manual `FakeHubContext` and `FakeHubClients` implementations in unit/integration tests. **Do NOT use `Moq`** for `IHubContext` (avoids `RuntimeBinder` and duplicate method signature issues).
- **Main Session Only**: Do **NOT** spawn background subagents (`Agent` / `Task` tools) for coding or debugging; keep all work in the main session.
- **Context Hygiene**: NEVER read, search, or list `bin/`, `obj/`, `.git/`, `TestResults/`, or `wwwroot/_framework/`. Exclude build outputs when searching (`rg --glob '!{bin,obj}/**'`).
- **Git Safety**: Do not commit, push, or alter git history unless explicitly asked.
## Current Phase & Handoff Context (added during planning)

- Implementation is tracked in phase-based plan files: `docs/phase-0-cleanup.md`, `docs/phase-1-contract.md`, `docs/phase-2-downlink.md`.
- Phase order: Phase 0 -> Phase 1 (+ Phase 2 as first coding milestone after Phase 0).
- Contract decisions: Shared string-typed DTOs; typed hub methods; strongly-typed SignalR sends; SignalRHubConnectionService pattern; config endpoint via Options pattern.
- Integration decision: in-process MQTT broker acceptable; server/client contract is the subject under test (not HTTP/MQTT itself).
- Canonical UI decision: drop `Components/`, keep `Pages/` + `Shared/`.

## Area Agent Context Files

- `RelayMessenger.Server/AGENTS.md` - server gateway area context.
- `RelayMessenger.Client/AGENTS.md` - client Blazor WASM area context.
- `RelayMessenger.Shared/AGENTS.md` - shared DTO/state area context.

## Handoff Notes For Gemma
- Work directly in the main project directory; no worktrees; no background subagents; no commits unless asked.
- Always pass explicit `.csproj`/`.sln` paths to dotnet commands; use `--nologo -v normal`.
- Never read `bin/`, `obj/`, `.git/`, `TestResults/`, or `wwwroot/_framework/`.

## Custom Tooling
- **File Editor**: A C# CLI tool is available at `~/workspace/OpenCode/edit_tool.sh`.
- **Command Signature**: `./edit_tool.sh <filePath> <oldString> <newString>`
- **Behavior**: 
    - Performs literal string replacement (safe for special characters).
    - Rejects edits if the target string is not found.
    - Rejects edits if there are multiple occurrences of the target string (prevents accidental mass-edits).
- **Usage Note**: Use this tool when performing complex refactors or replacements requiring multi-line precision where standard `sed` might be brittle with escaping.
