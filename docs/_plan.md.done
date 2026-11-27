# RelayMessenger Project Plan

## Architecture Overview
- **Host (`RelayMessenger.Server`)**: Acts as a dual-purpose gateway.
  - **MQTT Broker (Inbound)**: Hosts an MQTT server (via `MQTTnet.Server`) for Meshtastic nodes to connect to and send data.
  - **MQTT Client (Outbound)**: Uses an `IMqttClient` to publish messages back into the Meshtastic mesh.
  - **SignalR Hub**: Provides a SignalR Hub (`RelayHub`) for the Blazor WASM client to receive pushed updates from the server.
- **Client (`RelayMessenger.Client`)**: A Blazor WASM PWA that connects to the server's SignalR Hub. It treats the server as a gateway to the mesh, receiving "pushed" updates for all nodes on the mesh and sending outbound commands to the server.

## Implementation Phases

### Phase 1: Server-Side SignalR & Processing (Completed)
- [x] **Create `RelayHub`**: Create a new SignalR Hub in `RelayMessenger.Server/Hubs/RelayHub.cs`.
- [x] **Update `MeshtasticPacketProcessor`**:
  - Inject `IHubContext<RelayHub>` into `MeshtasticPacketProcessor`.
  - Update `ProcessPacket` to broadcast decoded `NodeStateModel` updates to all clients in the Hub.
  - Update `UpdatePosition`, `UpdateNodeInfo`, and `UpdateTelemetry` to trigger these broadcasts.
- [x] **Register Services**: Update `RelayMessenger.Server/Program.cs` to register the SignalR Hub and any required dependencies.

### Phase 2: Client-Side SignalR & Node Service (Completed)
- [x] **Create `SignalRService`**: Create a new service in `RelayMessenger.Client/Services/SignalRService.cs` to manage the connection to the Server's Hub.
- [x] **Refactor `NodeService`**:
  - Replace the `MqttClientService` dependency with `SignalRService`.
  - Update `NodeService` to listen for `NodeUpdated` events from the SignalR Hub.
- [x] **Verify Server Logic**: Ensure all server updates (Position, NodeInfo, Telemetry) correctly broadcast SignalR payloads and filter invalid data.
- [x] **Update `Map.razor`**: Ensure `Map.razor` continues to work seamlessly with the updated `NodeService`.

### Phase 3: Outbound Downlink (Planned)
- [x] **Client UI**: Update the "Send Message" logic in `Map.razor` to send a message to `RelayHub`.
- [x] **Server Gateway**: Implement logic in the Server to receive this request and use its internal **MQTT Client** to publish a JSON message to the Meshtastic MQTT topic `msh/REGION/2/json/mqtt/`.

## Critical Files
- `RelayMessenger.Server/Program.cs`
- `RelayMessenger.Server/Services/Processors/MeshtasticPacketProcessor.cs`
- `RelayMessenger.Server/Hubs/RelayHub.cs`
- `RelayMessenger.Client/Services/NodeService.cs`
- `RelayMessenger.Client/Services/SignalRService.cs`
- `RelayMessenger.Client/Pages/Map.razor`
