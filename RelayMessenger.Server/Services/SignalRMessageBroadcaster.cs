using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using RelayMessenger.Server.Hubs;

namespace RelayMessenger.Server.Services;

public class SignalRMessageBroadcaster : IMessageBroadcaster
{
    private readonly IHubContext<RelayHub> _hubContext;

    public SignalRMessageBroadcaster(IHubContext<RelayHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task BroadcastTextAsync(string message)
    {
        // Map to the actual signalr method name defined in RelayHub
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", message);
    }

    public async Task BroadcastNodeUpdatedAsync(RelayMessenger.Shared.NodeUpdateDto update)
    {
        await _hubContext.Clients.All.SendAsync("NodeUpdated", update);
    }
}