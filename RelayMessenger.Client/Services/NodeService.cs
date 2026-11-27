using System.Net.Http.Json;
using RelayMessenger.Shared;
using RelayMessenger.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Collections.Concurrent;
using System.Text.Json;
using RelayMessenger.Client.Services;

namespace RelayMessenger.Client.Services;

public class NodeService
{
    private readonly ISignalRService _signalRService;
    private readonly ILogger<NodeService> _logger;
    private ConcurrentDictionary<uint, NodeStateModel> _nodes = new();
    
    public NodeService(ISignalRService signalRService, ILogger<NodeService> logger)
    {
        _signalRService = signalRService;
        _logger = logger;

        _signalRService.OnNodeUpdated += (node) =>
        {
            UpdateLocalNodeList(node);
            OnNodesUpdated?.Invoke();
        };
    }

    public void UpdateLocalNodeList(NodeStateModel? updatedNode)
    {
        if (updatedNode == null) return;

        // Coordinate Validation: 
        // Per project guardrails, filter out (0,0) coordinates.
        if (updatedNode.Latitude == 0 && updatedNode.Longitude == 0)
        {
            return;
        }

        _nodes.AddOrUpdate(updatedNode.Id, updatedNode, (id, existing) =>
        {
            return updatedNode.Timestamp > existing.Timestamp ? updatedNode : existing;
        });
        OnNodesUpdated?.Invoke();
    }

    public async Task InitializeAsync()
    {
        // Connect to the SignalR hub first.
        await _signalRService.ConnectAsync();

        // Then fetch initial nodes from the Hub.
        await FetchInitialNodesAsync();
    }

    private async Task FetchInitialNodesAsync()
    {
        try
        {
            var nodes = await _signalRService.GetInitialNodesAsync();
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    _nodes[node.Id] = node;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching initial nodes via SignalR");
        }
    }

    public async Task<List<NodeStateModel>> GetInitialNodesAsync()
    {
        await FetchInitialNodesAsync();
        return GetNodes().ToList();
    }

    public List<NodeStateModel> GetNodes()
    {
        return _nodes.Values.ToList();
    }

    public async Task SendDownlink(uint to, string message)
    {
        var request = new SendMessageRequest(to, message);
        await _signalRService.SendMessageAsync(request);
    }
 
    public async Task SendClientMessage(uint? to, string message)
    {
        var request = new SendMessageRequest(to ?? 0, message);
        await _signalRService.SendMessageAsync(request);
    }

    public event Action? OnNodesUpdated;
}
