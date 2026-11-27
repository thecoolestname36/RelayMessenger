# RelayMessenger Client

Blazor WebAssembly PWA client of RelayMessenger.

## Purpose
- Connect to Server SignalR hub (hosted on `RelayMessenger.Server`).
- Receive real-time "pushed" node updates from the mesh gateway via SignalR.
- Send outbound commands/messages back to the server for delivery into the Meshtastic mesh.

## Interop Service Pattern (JS interop)
- See `RelayMessenger.Client/AGENTS.md` -> JS Interop Service Pattern.
- Google Maps interop: `GoogleMapsInterop` + `GoogleMap` (Interops/Services) with `IJSObjectReference` via JS module; JS module `wwwroot/js/googleMaps.js`; JSInvokable callbacks.

## Architecture
- `SignalRService`, `NodeService`, Map UI, Google Maps, interops.
- Connects to the Server's SignalR Hub; treats the server as a gateway to the mesh network.

## Commands
- `dotnet build RelayMessenger.Client/RelayMessenger.Client.csproj --nologo -v normal`
- `dotnet test RelayMessenger.Client.UnitTests/RelayMessenger.Client.UnitTests.csproj --nologo -v normal`
