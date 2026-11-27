# Architecture Overview

## System Flow
1. **Meshtastic Mesh**: Nodes transmit UDP packets to the gateway.
2. **RelayMessenger.Server**: 
   - **MQTT Broker/Client**: Receives and manages mesh communication.
   - **SignalR Hub**: Broadcasts real-time updates to connected web clients.
3. **RelayMessenger.Client (Blazor WASM)**:
   - **SignalRService**: Maintains a persistent connection to the server hub.
   - **NodeService**: Manages local state of all nodes, providing thread-safe and order-aware updates using timestamps.
   - **UI/Map**: Displays node positions and states in real-time.

## Data Consistency Model: Last-Write-Wins (LWW)
To ensure correctness against out-of-order network delivery or concurrent SignalR messages, `NodeService` implements a Timestamp-based LWW mechanism:
- Every `NodeStateModel` contains a `Timestamp`.
- Updates are only accepted if the incoming record's timestamp is strictly newer than the existing one in the local dictionary.

## Key Components
- **Shared**: DTOs and state models shared between Server and Client.
- **Server Gateway**: Decouples the raw Meshtastic protocol from the web client contract.
- **Client State Store**: A `ConcurrentDictionary` holding current node statuses, protected by LWW rules.