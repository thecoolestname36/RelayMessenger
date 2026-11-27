using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using RelayMessenger.Client.Services;
using RelayMessenger.Shared;
using RelayMessenger.Shared.Models;

namespace RelayMessenger.Client.UnitTests;

public class NodeService_EdgeCases
{
    private readonly Mock<ISignalRService> _mockSignalR = new();
    private readonly Mock<HttpMessageHandler> _mockHttpHandler = new();
    private readonly Mock<ILogger<NodeService>> _mockLogger = new();

    [Fact]
    public async Task SendDownlink_WithEmptyMessage_CallsSignalR()
    {
        // Arrange
        var service = new NodeService(_mockSignalR.Object, _mockLogger.Object);
        
        // Act
        await service.SendDownlink(100, "");
        
        // Assert - Verify that even an empty message is passed through the contract correctly
        _mockSignalR.Verify(s => s.SendMessageAsync(It.Is<SendMessageRequest>(r => r.To == 100 && r.Message == ""), default), Times.Once);
    }

    [Fact]
    public async Task SendClientMessage_WithLargePayload_CallsSignalR()
    {
        // Arrange
        var service = new NodeService(_mockSignalR.Object, _mockLogger.Object);
        string largeMessage = new string('A', 10000);

        // Act
        await service.SendClientMessage(200, largeMessage);

        // Assert
        _mockSignalR.Verify(s => s.SendMessageAsync(It.Is<SendMessageRequest>(r => r.To == 200 && r.Message == largeMessage), default), Times.Once);
    }

    [Fact]
    public async Task UpdateLocalNodeList_WithZeroCoordinates_DoesNotUpdateState()
    {
        // Arrange
        var service = new NodeService(_mockSignalR.Object, _mockLogger.Object);
        bool eventFired = false;
        service.OnNodesUpdated += () => eventFired = true;

        var invalidNode = new NodeStateModel 
        { 
            Id = 1, 
            Latitude = 0, 
            Longitude = 0, 
            Name = "Invalid"
        };

        // Act
        service.UpdateLocalNodeList(invalidNode);

        // Assert
        Assert.Empty(service.GetNodes());
        Assert.False(eventFired);
    }

    [Fact]
    public async Task UpdateLocalNodeList_WithNullPayload_DoesNotCrash()
    {
        // Arrange
        var service = new NodeService(_mockSignalR.Object, _mockLogger.Object);

        // Act & Assert - Should not throw exception
        try
        {
            service.UpdateLocalNodeList(null!);
        }
        catch (Exception ex)
        {
            Assert.Null(ex);
        }
    }

    [Fact]
    public async Task UpdateLocalNodeList_ConcurrentUpdates_MaintainsConsistency()
    {
        // Arrange
        var service = new NodeService(_mockSignalR.Object, _mockLogger.Object);
        uint nodeId = 1;
        int count = 100;

        // Act
        Parallel.For(0, count, i => 
        {
            var node = new NodeStateModel { Id = nodeId, Latitude = (float)i, Longitude = (float)i, Timestamp = DateTime.UtcNow };
            service.UpdateLocalNodeList(node);
        });

        // Assert - The final state should be one of the nodes updated during the parallel loop, 
        // and it must remain stable even though multiple threads were writing to it.
        var result = service.GetNodes();
        Assert.Single(result);
        Assert.Equal(nodeId, result[0].Id);
    }

    [Fact]
    public async Task UpdateLocalNodeList_MaxId_HandledCorrectly()
    {
        // Arrange
        var service = new NodeService(_mockSignalR.Object, _mockLogger.Object);
        uint maxId = uint.MaxValue;
        var node = new NodeStateModel { Id = maxId, Latitude = 10, Longitude = 20 };

        // Act
        service.UpdateLocalNodeList(node);

        // Assert
        Assert.Single(service.GetNodes());
        Assert.Equal(maxId, service.GetNodes()[0].Id);
    }

    [Fact]
    public async Task UpdateLocalNodeList_OutOfOrderUpdates_RetainsNewestState()
    {
        // Arrange
        var service = new NodeService(_mockSignalR.Object, _mockLogger.Object);
        uint nodeId = 50;
        DateTime earlyTime = DateTime.UtcNow.AddMinutes(-10);
        DateTime lateTime = DateTime.UtcNow;

        var oldUpdate = new NodeStateModel 
        { 
            Id = nodeId, 
            Latitude = 1.1, 
            Longitude = 2.2, 
            Timestamp = earlyTime,
            Name = "Old Name"
        };
        var newUpdate = new NodeStateModel 
        { 
            Id = nodeId, 
            Latitude = 3.3, 
            Longitude = 4.4, 
            Timestamp = lateTime,
            Name = "New Name"
        };

        // Act - Simulate out-of-order arrival: Newer one arrives first
        service.UpdateLocalNodeList(newUpdate);
        service.UpdateLocalNodeList(oldUpdate);

        // Assert - The state should still be the newer record (lateTime)
        var result = service.GetNodes();
        Assert.Single(result);
        Assert.Equal("New Name", result[0].Name);
        Assert.Equal(lateTime, result[0].Timestamp);
    }
}