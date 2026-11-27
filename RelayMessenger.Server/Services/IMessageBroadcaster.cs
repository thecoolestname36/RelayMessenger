using System;
using System.Threading.Tasks;

namespace RelayMessenger.Server.Services;

public interface IMessageBroadcaster
{
    Task BroadcastTextAsync(string message);
    Task BroadcastNodeUpdatedAsync(RelayMessenger.Shared.NodeUpdateDto update);
}
