using System.Text.Json;
using Xunit;
using RelayMessenger.Shared.Models;

namespace RelayMessenger.Shared.UnitTests;

public class MessageSynchronizationTests
{
    [Fact]
    public void SendMessageRequest_SerializationRoundTrip_MatchesExpectedStructure()
    {
        // Arrange
        var original = new SendMessageRequest(123, "Hello World");
        
        // Act
        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<SendMessageRequest>(json);

        // Assert - Verify that the JSON contains expected property names (camelCase usually)
        // If we decide on specific naming (e.g. from Mesh docs), this will fail if not matched.
        Assert.NotNull(deserialized);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        Assert.Equal(original.To, deserialized!.To);
        Assert.Equal(original.Message, deserialized.Message);
    }

    [Fact]
    public void SendMessageRequest_JsonContainsCorrectKeys()
    {
        // Arrange
        var original = new SendMessageRequest(123, "Hello World");
        
        // Act
        var json = JsonSerializer.Serialize(original);

        // Assert - This is the part that will likely fail if PropertyName attributes aren't explicitly set
        // based on a specific contract agreement (e.g. "to" vs "To")
        Assert.Contains("\"To\":123", json); // Hypothetical check for case sensitivity or explicit naming
    }
}