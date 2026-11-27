using System.Collections.Concurrent;
using System.Collections.Generic;
using System;

namespace RelayMessenger.Shared.State;

using RelayMessenger.Shared;

public class NodeStateStore
{
    private readonly ConcurrentDictionary<uint, NodeStateModel> _nodes = new();

    // Event to notify subscribers (like the Blazor UI) when a node's state changes
    public event Action<NodeStateModel>? OnNodeUpdated;

    public void UpdateNode(NodeStateModel state)
    {
        _nodes[state.Id] = state;
        OnNodeUpdated?.Invoke(state);
    }


    public NodeStateModel? GetNode(uint nodeId)
    {
        _nodes.TryGetValue(nodeId, out var state);
        return state;
    }

    public IEnumerable<NodeStateModel> GetAllNodes()
    {
        return _nodes.Values;
    }

    public void RemoveNode(uint nodeId)
    {
        _nodes.TryRemove(nodeId, out _);
    }
}
