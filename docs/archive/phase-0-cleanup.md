# Phase 0 - Cleanup & Architecture Alignment

## Objective
Remove leftover garbage / dead-end code and lock final architecture decisions BEFORE contract implementation (Phase 1) and downlink (Phase 2).

## Context for the Agent
- Solution: `RelayMessenger.sln` (.NET 10).
- Server: `RelayMessenger.Server` - MQTT broker (TLS 8883) + MQTT client outbound + SignalR hub `RelayHub` at `/relayhub` + `GET /Nodes`.
- Client: `RelayMessenger.Client` - Blazor WASM, `SignalRService`, `NodeService`, `Map.razor`, `wwwroot/js/googleMaps.js`.
- Shared: `RelayMessenger.Shared` - `NodeStateModel`, `NodeStateStore`, `SendMessageRequest`, `ClientMessageRequest`, `Routing`, crypto (`EcdsaKey`, `AesGcmNonce`, `PublicIdentity`).
- Tests: Server/Client/Shared unit projects; stale `RelayMessenger.Server.IntegrationTests` referenced only by deleted state (`test_output.txt`).

## Final Decisions (user-approved)
1. Dead code: delete candidates if unused / dead-end / not valuable.
   - Delete: `RelayMessenger.Client/Services/MqttClientService.cs`, `RelayMessenger.Client/Interops/PromptInterop.cs`, `RelayMessenger.Client/Interops/MessageNotificationsInterop.cs`, `RelayMessenger.Client/Interops/LocalStorageInterop.cs`, empty `RelayMessenger.Client/Services/Models/`, root `test_output.txt`, `data_properties.txt`, `data_properties_v2.txt`.
   - Delete: `RelayMessenger.Server/Services/MeshClientService.PublishToMesh` + `IMeshClientService.PublishToMesh`.
2. Canonical UI: drop `Components/`, keep `Pages/` + `Shared/`.
   - Move `Components/Pages/Home.razor` -> `Pages/Home.razor`; update `@using RelayMessenger.Client.Components.Views` -> `@using RelayMessenger.Client.Pages`.
   - Move `Components/Views/Map.razor` -> `Pages/Map.razor`; move colocated `Map.razor.js` -> `Pages/Map.razor.js`; `Map.razor.css` -> `Pages/Map.razor.css`.
   - Delete `Components/Layout/MainLayout.razor`; keep `Shared/MainLayout.razor`; in `RelayMessenger.Client/_Imports.razor` replace `@using RelayMessenger.Client.Components.Layout` -> `@using RelayMessenger.Client.Shared`.
   - Keep `RelayMessenger.Client/Pages/Index.razor`.
   - Remove empty `Components/` tree.
3. Security: keep `Routing` + crypto types; plan client-side Meshtastic decryption later (Subtle Crypto JS).
4. URL: Option C - configurable hub/API endpoint via Options pattern; defaults from Options A/B in appsettings.

## Steps (atomic; 1-3 related files per step)
- [ ] **Step 0.1 - Dead code removal** (files: `RelayMessenger.Client/Services/MqttClientService.cs`, `RelayMessenger.Client/Interops/*.cs`, `RelayMessenger.Client/Services/Models/`, root junk, `RelayMessenger.Server/Services/MeshClientService.cs`).
  - Grep for references first; delete only confirmed-unused candidates.
- [ ] **Step 0.2 - Canonical UI consolidation** (files: `Components/Pages/Home.razor`, `Components/Views/Map.razor`, `Components/Views/Map.razor.js`, `Components/Views/Map.razor.css`, `Components/Layout/MainLayout.razor`, `Shared/MainLayout.razor`, `_Imports.razor`, `App.razor`).
  - Move Home -> `Pages/Home.razor`; fix using. Move Map (+colocated) -> `Pages/`. Delete Components layout; update `_Imports.razor` using to `RelayMessenger.Client.Shared`. Remove `Components/`.
- [ ] **Step 0.3 - Replace `Console.WriteLine` with `ILogger`** (files: `RelayMessenger.Server/Services/Processors/MeshtasticPacketProcessor.cs`, `RelayMessenger.Shared/State/NodeStateStore.cs`, `RelayMessenger.Client/Services/SignalRService.cs`, `RelayMessenger.Client/Services/NodeService.cs`).
- [ ] **Step 0.4 - Endpoint options pattern** (files: `RelayMessenger.Client/Program.cs`, `RelayMessenger.Server/Program.cs`, `RelayMessenger.Server/Configuration/MeshBrokerOptions.cs`, appsettings).
- [ ] **Step 0.5 - Test cleanup** (files: `RelayMessenger.Server.UnitTests/*`, `RelayMessenger.Client.UnitTests/*`, `RelayMessenger.Shared.UnitTests/*`).

## Build / Test Commands
- `dotnet build RelayMessenger.Client/RelayMessenger.Client.csproj --nologo -v normal`
- `dotnet build RelayMessenger.Server/RelayMessenger.Server.csproj --nologo -v normal`
- `dotnet test RelayMessenger.Server.UnitTests/RelayMessenger.Server.UnitTests.csproj --nologo -v normal`

## Definition of Done (Phase 0)
- No dead candidates remain (per decision 1). Canonical UI in `Pages/` + `Shared/`. ILogger used. Endpoint configurable. Tests aligned.

## Running Checklist (work item by item)
- [ ] Step 0.1 complete
- [ ] Step 0.2 complete
- [ ] Step 0.3 complete
- [ ] Step 0.4 complete
- [ ] Step 0.5 complete
- [ ] Build passes: `dotnet build RelayMessenger.sln --nologo -v normal`
