using MQTTnet;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Meshtastic.Protobufs;
using Google.Protobuf;
using RelayMessenger.Shared;
using RelayMessenger.Shared.State;
using Microsoft.Extensions.Options;
using RelayMessenger.Server.Configuration;

namespace RelayMessenger.Server.Services;

public interface IMeshClientService
{
    IMqttClient GetClient();
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}

public class MeshClientService : IMeshClientService, IHostedService
{
    private readonly IMqttClient _meshClient;
    private readonly ILogger<MeshClientService> _logger;
    private readonly MeshBrokerOptions _options;

    public MeshClientService(IMqttClient meshClient, ILogger<MeshClientService> logger, IOptions<MeshBrokerOptions> options)
    {
        _meshClient = meshClient;
        _logger = logger;
        _options = options.Value;
    }

    public IMqttClient GetClient() => _meshClient;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting MeshClientService to {Host}:{Port}...", _options.Host, _options.Port);

        var mqttOptions = new MQTTnet.MqttClientOptionsBuilder()
            .WithTcpServer(_options.Host, _options.Port)
            .WithCredentials("admin", "password")
            .Build();

        try
        {
            await _meshClient.ConnectAsync(mqttOptions, CancellationToken.None);
            _logger.LogInformation("MeshClientService connected to mesh.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to mesh at {Host}:{Port}", _options.Host, _options.Port);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping MeshClientService...");
        if (_meshClient != null && _meshClient.IsConnected)
        {
            await _meshClient.DisconnectAsync();
        }
    }
}