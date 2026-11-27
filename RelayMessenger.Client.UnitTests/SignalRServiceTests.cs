using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using RelayMessenger.Client.Services;
using Microsoft.Extensions.Logging;

namespace RelayMessenger.Client.UnitTests;

public class SignalRServiceTests
{
    [Fact]
    public void OnMessageReceived_FiresEvent_WhenSignalRCallbackTriggered()
    {
        // Arrange
        var logger = new Mock<ILogger<SignalRService>>();
        // We can't easily mock the internal HubConnection instance of SignalRService directly.
        // Instead, we test that if the event is invoked, any listeners are notified. 
        // This validates our service layer contract for messages.
        var service = new SignalRService("http://localhost", logger.Object);

        string? receivedMessage = null;
        service.OnMessageReceived += (msg) => receivedMessage = msg;

        // Act
        service.RaiseMessageReceived("Test Meshtastic message");

        // Assert
        Assert.Equal("Test Meshtastic message", receivedMessage);
    }
}