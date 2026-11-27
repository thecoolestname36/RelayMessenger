using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Xunit;
using RelayMessenger.Client.Services;
using RelayMessenger.Shared;
using RelayMessenger.Shared.Models;
using Microsoft.Extensions.Logging;

namespace RelayMessenger.Client.UnitTests;

public class NodeServiceTests_v2
{
    private readonly Mock<ISignalRService> _mockSignalR;
    private readonly Mock<ILogger<NodeService>> _mockLogger;
    
    public NodeServiceTests_v2()
    {
        _mockSignalR = new Mock<ISignalRService>();
        _mockLogger = new Mock<ILogger<NodeService>>();
    }


    [Fact]
    public async Task SendDownlink_SendsCorrectContract_ViaSignalR()
    {
        // Arrange
        var service = new NodeService(_mockSignalR.Object, _mockLogger.Object);
        uint nodeId = 200;
        string message = "Test Message";

        // Act
        await service.SendDownlink(nodeId, message);

        // Assert - Verify that the call to SignalR uses the expected strongly-typed SendMessageRequest
        _mockSignalR.Verify(s => s.SendMessageAsync(It.Is<SendMessageRequest>(r =>
            r.To == nodeId && 
            r.Message == message)), Times.Once);
    }

    [Fact]
    public async Task SendClientMessage_WhenToIsNull_SendsToZero()
    {
        // Arrange
        var service = new NodeService(_mockSignalR.Object, _mockLogger.Object);
        string message = "Null Target Message";

        // Act
        await service.SendClientMessage(null, message);

        // Assert
        _mockSignalR.Verify(s => s.SendMessageAsync(It.Is<SendMessageRequest>(r =>
            r.To == 0 && r.Message == message)), Times.Once);
    }

    [Fact]
    public void OnNodeUpdated_UpdatesLocalStateAndTriggersEvent()
    {
        // Arrange
        var service = new NodeService(_mockSignalR.Object, _mockLogger.Object);
        bool eventFired = false;
        service.OnNodesUpdated += () => eventFired = true;

        var updatedNode = new NodeStateModel 
        { 
            Id = 100, 
            Name = "Test node", 
            Latitude = 1.0d, 
            Longitude = 2.0d,
            AltitudeMeters = 0.0,
            Snr = 0,
            Rssi = 0,
            LastSeen = DateTime.UtcNow,
            Battery = 100f,
            Temperature = 25.5f,
            Region = "US",
            GatewayId = "test-gw"
        };

        // Act - Simulate SignalR event trigger by calling the internal method directly
        service.UpdateLocalNodeList(updatedNode);

        // Assert
        Assert.True(eventFired);
        var nodes = service.GetNodes();
        Assert.Single(nodes);
        Xunit.Assert.Equal((uint)100, nodes[0].Id);



    }

    [Fact]
    public async Task InitializeAsync_WhenSignalRFails_ReturnsEmptyList()
    {
        // Arrange
        var mockSignalR = new Mock<ISignalRService>();
        mockSignalR.Setup(s => s.GetInitialNodesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("SignalR Error"));

        var service = new NodeService(mockSignalR.Object, _mockLogger.Object);

        // Act
        await service.InitializeAsync();

        // Assert
        var nodes = service.GetNodes();
        Assert.Empty(nodes);
    }
}