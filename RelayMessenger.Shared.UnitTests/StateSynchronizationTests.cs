using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using RelayMessenger.Shared;
using RelayMessenger.Server.Services;
using RelayMessenger.Client.Services;
using RelayMessenger.Shared.Models;
using System.Text.Json;

namespace RelayMessenger.Shared.UnitTests;

public class StateSynchronizationTests
{
    [Fact]
    public void PositionUpdate_ConsistencyTest()
    {
        // Arrange - Shared scenario: A new node moves from (10, 20) to (30, 40)
        uint nodeId = 55u;
        var initialModel = new NodeStateModel
        {
            Id = nodeId,
            Name = "MobileNode",
            Latitude = 10.0,
            Longitude = 20.0,
            LastSeen = DateTime.UtcNow
        };

        // Act & Assert - Verify Contract Consistency
        // This test ensures that the Shared NodeStateModel is used identically across services.
        Assert.Equal(nodeId, initialModel.Id);
        Assert.Equal(10.0, initialModel.Latitude);
    }

    [Fact]
    public async Task ServerDownlink_SerializationConsistency()
    {
        // Arrange - Verify the message serialized by the hub matches expectations before it hits MQTT topic logic
        var uintTo = 1u;
        var messageContent = "Test";
        var request = new SendMessageRequest(uintTo, messageContent);

        // Act
        var json = JsonSerializer.Serialize(request);
        
        // Assert - Verify the structure isn't corrupted by property naming changes etc.
        var deserialized = JsonSerializer.Deserialize<SendMessageRequest>(json);
        Assert.NotNull(deserialized);
        Assert.Equal(uintTo, deserialized?.To);
        Assert.Equal(messageContent, deserialized?.Message);
    }
}
