using System;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;

namespace RelayMessenger.Server.Services;

public interface IMqttGateway : IDisposable
{
    Task<MqttClientConnectResult> ConnectAsync(MqttClientOptions options, CancellationToken cancellationToken = default);
    Task<MqttClientPublishResult> PublishAsync(MqttApplicationMessage message, CancellationToken cancellationToken = default);
}

public class MqttGateway : IMqttGateway
{
    private readonly IMqttClient _client;

    public MqttGateway(IMqttClient client)
    {
        _client = client;
    }

    public async Task<MqttClientConnectResult> ConnectAsync(MqttClientOptions options, CancellationToken cancellationToken = default)
    {
        return await _client.ConnectAsync(options, cancellationToken);
    }

    public async Task<MqttClientPublishResult> PublishAsync(MqttApplicationMessage message, CancellationToken cancellationToken = default)
    {
        return await _client.PublishAsync(message, cancellationToken);
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}
