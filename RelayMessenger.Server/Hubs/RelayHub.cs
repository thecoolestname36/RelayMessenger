using Microsoft.AspNetCore.SignalR;
using RelayMessenger.Shared;
using RelayMessenger.Server.Services;
using RelayMessenger.Shared.State;
using RelayMessenger.Shared.Models;
using System.Threading.Tasks;
using System;

namespace RelayMessenger.Server.Hubs;

public class RelayHub : Hub
{
    private readonly DownlinkService _downlinkService;
    private readonly NodeStateStore _stateStore;

    public RelayHub(DownlinkService downlinkService, NodeStateStore stateStore)
    {
        _downlinkService = downlinkService;
        _stateStore = stateStore;
    }

    public async Task SendMessage(SendMessageRequest request)
    {
        if (request != null)
        {
            await _downlinkService.PublishDownlinkAsync(request);
            await Clients.All.SendAsync("ReceiveMessage", request.Message);
        }
    }

    public async Task<List<NodeStateModel>> GetInitialNodesAsync()
    {
        return _stateStore.GetAllNodes().ToList();
    }

    public async Task UpdatePosition(uint nodeId, double latitude, double longitude)
    {
        var state = _stateStore.GetNode(nodeId) ?? new NodeStateModel
        {
            Id = nodeId,
            Name = $"Node {nodeId:x8}",
            Latitude = latitude,
            Longitude = longitude,
            AltitudeMeters = 0,
            Snr = 0,
            Rssi = 0,
            LastSeen = DateTime.UtcNow
        };
        
        var updatedState = state with
        {
            Latitude = latitude,
            Longitude = longitude,
            LastSeen = DateTime.UtcNow
        };
        
        _stateStore.UpdateNode(updatedState);

        await Clients.All.SendAsync("NodeUpdated", new RelayMessenger.Shared.NodeUpdateDto
        {
            Id = updatedState.Id,
            Name = updatedState.Name,
            Latitude = updatedState.Latitude,
            Longitude = updatedState.Longitude,
            AltitudeMeters = updatedState.AltitudeMeters,
            Snr = updatedState.Snr,
            Rssi = updatedState.Rssi,
            Battery = updatedState.Battery,
            LastSeen = updatedState.LastSeen
        });
    }
}