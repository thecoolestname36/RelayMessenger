using System;
using System.Text;
using System.Text.Json;
using Xunit;
using RelayMessenger.Shared.Models;

namespace RelayMessenger.Client.UnitTests;

public class ClientContractTests
{
    [Fact]
    public void SendMessageRequest_JsonSerialization_MatchesExpectedSchema()
    {
        // Arrange
        var request = new SendMessageRequest(42, "Test Message");
        
        // Act
        var json = JsonSerializer.Serialize(request);

        // Assert - Verification that key fields are present as expected in JSON output
        Assert.Contains("\"To\":42", json);
        Assert.Contains("\"Message\":\"Test Message\"", json);
    }
}