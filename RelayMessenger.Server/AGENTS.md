# Server Area - Agent Context

## Purpose
Gateway host: 
- **Inbound**: Embedded MQTT broker (`MQTTnet.Server`) for Meshtastic nodes to connect to and send data.
- **Outbound**: MQTT client specifically via `IMqttGateway` (using `DownlinkService`) to publish messages back into the mesh.
- **Client Updates**: SignalR hub (`RelayHub` at `/relayhub`) and `GET /Nodes`.

## Critical Files
- `RelayMessenger.Server/Program.cs`
- `RelayMessenger.Server/Hubs/RelayHub.cs`
- `RelayMessenger.Server/Services/Processors/MeshtasticPacketProcessor.cs`
- `RelayMessenger.Server/Services/DownlinkService.cs` (Handles JSON and Protobuf outbound payloads)
- `RelayMessenger.Server/Services/MeshClientService.cs`
- `RelayMessenger.Server/Configuration/MeshBrokerOptions.cs`
- `RelayMessenger.Server/Services/IMqttGateway.cs`

## Business Logic Notes
- Coordinates: divide Meshtastic `LatitudeI` / `LongitudeI` by 10,000,000; filter `(0,0)`.
- Protobuf: use reflection for `PortNum` (check `value__` / `Value`).
- State: update `NodeStateStore` before SignalR broadcast.

## Patterns
- MQTTnet patterns from `/home/brad/git/dotnet/MQTTnet`.
- Meshtastic protobuf patterns from `/home/brad/git/meshtastic`.

## Phase Context
- Part of Phase 0/1/2 plan flow; server contract tests in Phase 1; downlink completed in Phase 3.
