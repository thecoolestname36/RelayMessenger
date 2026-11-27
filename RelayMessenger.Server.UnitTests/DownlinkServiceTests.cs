using System;
using System.Buffers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using MQTTnet;
using RelayMessenger.Server.Services;
using RelayMessenger.Shared.Models;

namespace RelayMessenger.Server.UnitTests;

public class MockMqttGateway : IMqttGateway
{
    public MqttApplicationMessage? LastPublishedMessage { get; private set; }

    public Task<MqttClientConnectResult> ConnectAsync(MqttClientOptions options, CancellationToken cancellationToken = default) 
        => Task.FromResult(new MqttClientConnectResult());

    public Task<MqttClientPublishResult> PublishAsync(MqttApplicationMessage message, CancellationToken cancellationToken = default)
    {
        LastPublishedMessage = message;
        return Task.FromResult(new MqttClientPublishResult(null, MqttClientPublishReasonCode.Success, null, null));
    }

    public void Dispose() { }
}

public class DownlinkServiceTests
{
    private readonly MockMqttGateway _mqttGateway;
    private readonly DownlinkService _downlinkService;

    public DownlinkServiceTests()
    {
        _mqttGateway = new MockMqttGateway();
        _downlinkService = new DownlinkService(_mqttGateway);
    }

    [Fact]
    public async Task PublishDownlinkAsync_PublishesCorrectJson()
    {
        // Arrange
        var request = new SendMessageRequest(123, "Hello World!");
        var expectedJson = JsonSerializer.Serialize(request);

        // Act
        await _downlinkService.PublishDownlinkAsync(request, useProtobuf: false);

        // Assert
        Assert.NotNull(_mqttGateway.LastPublishedMessage);
        Assert.Equal("msh/US/2/json/mqtt/", _mqttGateway.LastPublishedMessage!.Topic);
        Assert.Equal(expectedJson, Encoding.UTF8.GetString(_mqttGateway.LastPublishedMessage.Payload));
    }

    [Fact]
    public async Task PublishDownlinkAsync_PublishesCorrectProtobuf()
    {
        // Arrange
        var request = new SendMessageRequest(456, "Protobuf Payload");
        var expectedTopic = "msh/US/2/mqtt/";

        // Act
        await _downlinkService.PublishDownlinkAsync(request, useProtobuf: true);

        // Assert
        Assert.NotNull(_mqttGateway.LastPublishedMessage);
        Assert.Equal(expectedTopic, _mqttGateway.LastPublishedMessage!.Topic);
        var payloadString = Encoding.UTF8.GetString(_mqttGateway.LastPublishedMessage.Payload);
        Assert.DoesNotContain("{", payloadString); // Not JSON
    }

    [Theory]
    [InlineData("Special characters: !@#$%^&*()_+=-{}[]|\\:;\"'<>,.?/")]
    [InlineData("")]
    [InlineData("A very long message that might exceed standard buffer sizes in some protocols...")]
    public async Task PublishDownlinkAsync_HandlesVariousPayloads(string content)
    {
        // Arrange
        var request = new SendMessageRequest(789, content);

        // Act & Assert (Json Path)
        await _downlinkService.PublishDownlinkAsync(request, useProtobuf: false);
        Assert.Equal(JsonSerializer.Serialize(request), Encoding.UTF8.GetString(_mqttGateway.LastPublishedMessage!.Payload));

        // Act & Assert (Protobuf Path)
        var request2 = new SendMessageRequest(999, content);
        await _downlinkService.PublishDownlinkAsync(request2, useProtobuf: true);
        Assert.Equal("msh/US/2/mqtt/", _mqttGateway.LastPublishedMessage!.Topic);
    }
}
