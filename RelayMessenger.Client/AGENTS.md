# Client Area - Agent Context

## Purpose
Blazor WebAssembly PWA: SignalR connection, node state management, map UI, Google Maps JS interop.

## Critical Files
- `RelayMessenger.Client/Program.cs`
- `RelayMessenger.Client/Services/SignalRService.cs`
- `RelayMessenger.Client/Services/NodeService.cs`
- `RelayMessenger.Client/Pages/Map.razor` (canonical home: `Pages/` + `Shared/` after Phase 0)
- `RelayMessenger.Client/Interops/GoogleMap.cs`
- `RelayMessenger.Client/Services/GoogleMapsInterop.cs`
- `RelayMessenger.Client/wwwroot/js/googleMaps.js`

## Patterns
- SignalRHubConnectionService-style: `IAsyncDisposable`, WebSockets, status, reconnect.
- Typed SignalR sends; strongly-typed payloads.
- JS interop sourced from BluForTracker `MapView.razor.js`.
- WASM sandbox: no filesystem/OS/TCP; use IJSRuntime.

## JS Interop Service Pattern (Google Maps / browser interop)
- C# interop service wraps `IJSRuntime`; holds cached `IJSObjectReference?`; obtains via dynamic import or JS module global factory; exposes typed operations.
- Map: `GoogleMapsInterop.CreateObject()` -> `IJSObjectReference` via JS module global (`window.relayMessenger.googleMaps.CreateMapModule`); `GoogleMap` service holds the reference and exposes typed methods (`Initialize`, `SyncMarkers`, `SetCenter`, `SetZoom`, `WatchPosition`, `StopPositionWatch`, `UpdateMyInfoMarker`, `UpdateCurrentPosition`, `RemoveInfoMarker`).
- JS -> .NET: JS module holds `dotNetReference`; callbacks via JSInvokable (`OnMapIdle`, `UpdateMyInfoMarker`, `UpdateCurrentPosition`).
- Lifecycle: create on demand; dispose `IJSObjectReference` in `DisposeAsync`; polling via `PeriodicTimer`; wrap refreshes in `InvokeAsync(StateHasChanged)`.
- WASM sandbox: no filesystem/OS/TCP; all browser APIs via IJSRuntime.

## Phase Context
- Contract work in Phase 1; UI cleanup in Phase 0.
