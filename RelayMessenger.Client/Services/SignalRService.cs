using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using RelayMessenger.Shared;
using RelayMessenger.Shared.Models;
using RelayMessenger.Shared.State;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace RelayMessenger.Client.Services;

public interface ISignalRService
{
    event Action<NodeStateModel> OnNodeUpdated;
    event Action<bool> OnConnectionStatusChanged;
    event Action<string> OnMessageReceived;
    bool IsConnected { get; }
    Task ConnectAsync(CancellationToken cancellationToken = default);
    Task SendMessageAsync(SendMessageRequest request, CancellationToken cancellationToken = default);



    Task<List<NodeStateModel>> GetInitialNodesAsync(CancellationToken cancellationToken = default);
}

public class SignalRService : ISignalRService
{
    private readonly HubConnection _hubConnection;
    private readonly ILogger<SignalRService> _logger;
    public event Action<NodeStateModel>? OnNodeUpdated;
    public event Action<bool>? OnConnectionStatusChanged;
    public event Action<string>? OnMessageReceived;
    public bool IsConnected => _hubConnection.State == HubConnectionState.Connected;




    public SignalRService(string hubUrl, ILogger<SignalRService> logger)
    {
        _logger = logger;
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<NodeUpdateDto>("NodeUpdated", (dto) =>
        {
            var node = new NodeStateModel
            {
                Id = dto.Id,
                Name = dto.Name,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                AltitudeMeters = dto.AltitudeMeters,
                Snr = dto.Snr,
                Rssi = dto.Rssi,
                Battery = (float)dto.Battery,
                LastSeen = dto.LastSeen
            };
            OnNodeUpdated?.Invoke(node);
        });

        _hubConnection.On<string>("ReceiveMessage", (message) =>
        {
            RaiseMessageReceived(message);
        });


        _hubConnection.Closed += (error) =>
        {
            OnConnectionStatusChanged?.Invoke(false);
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += (connectionId) =>
        {
            OnConnectionStatusChanged?.Invoke(true);
            return Task.CompletedTask;
        };
    }

    public void RaiseMessageReceived(string message)
    {
        OnMessageReceived?.Invoke(message);
    }

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _hubConnection.StartAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to SignalR");
            OnConnectionStatusChanged?.Invoke(false);
        }
    }

    public async Task SendMessageAsync(SendMessageRequest request, CancellationToken cancellationToken = default)
    {
        if (_hubConnection.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("SendMessage", request, cancellationToken);
        }
    }

    public async Task<List<NodeStateModel>> GetInitialNodesAsync(CancellationToken cancellationToken = default)
    {
        if (_hubConnection.State == HubConnectionState.Connected)
        {
            return await _hubConnection.InvokeAsync<List<NodeStateModel>>("GetInitialNodesAsync", cancellationToken);
        }
        return new List<NodeStateModel>();
    }
}