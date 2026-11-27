using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using MQTTnet;
using RelayMessenger.Server.Services;
using RelayMessenger.Server.Hubs;
using RelayMessenger.Shared;
using RelayMessenger.Shared.Models;
using Microsoft.AspNetCore.SignalR;
using Meshtastic.Protobufs;
using RelayMessenger.Shared.State;
using RelayMessenger.Server.Services.Processors;
using Google.Protobuf;
using System.Reflection;

namespace RelayMessenger.Server.UnitTests;

public class FakeMessageBroadcaster : IMessageBroadcaster
{
    public List<string> RecordedMessages { get; } = new List<string>();
    public List<NodeUpdateDto> RecordedNodeUpdates { get; } = new List<NodeUpdateDto>();

    public Task BroadcastTextAsync(string message)
    {
        RecordedMessages.Add(message);
        return Task.CompletedTask;
    }

    public Task BroadcastNodeUpdatedAsync(NodeUpdateDto update)
    {
        RecordedNodeUpdates.Add(update);
        return Task.CompletedTask;
    }
}

public class MeshtasticPacketProcessorTests
{
    private readonly NodeStateStore _stateStore;
    private readonly FakeMessageBroadcaster _broadcaster;
    private readonly MeshtasticPacketProcessor _processor;

    public MeshtasticPacketProcessorTests()
    {
        _stateStore = new NodeStateStore();
        _broadcaster = new FakeMessageBroadcaster();
        _processor = new MeshtasticPacketProcessor(_broadcaster, _stateStore);
    }

    private MeshPacket CreateTestPacket(uint portNum, uint fromId)
    {
        var packet = new MeshPacket { From = fromId };
        SetPortNum(packet, (int)portNum);

        switch (portNum)
        {
            case 1: // TEXTMESSAGE_APP
                packet.Decoded = new Data
                {
                    Portnum = PortNum.TextMessageApp,
                    Payload = ByteString.CopyFromUtf8("Hello Mesh")
                };
                break;
            case 3: // POSITION_APP
                packet.Decoded = new Data
                {
                    Portnum = PortNum.PositionApp,
                    Payload = new Position { LatitudeI = 123456700, LongitudeI = 765432100 }.ToByteString()
                };
                break;
            case 4: // NODEINFO_APP
                packet.Decoded = new Data
                {
                    Portnum = PortNum.NodeinfoApp,
                    Payload = ByteString.CopyFromUtf8("TestNode")
                };
                break;
        }

        return packet;
    }

    private void SetPortNum(MeshPacket packet, int portNum)
    {
        var vField = typeof(MeshPacket).GetField("value__", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (vField != null) 
        { 
            vField.SetValue(packet, portNum); 
            return; 
        }

        var propNames = new[] { "Portnum", "PortNum" };
        foreach (var name in propNames)
        {
            var p = typeof(MeshPacket).GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (p != null && p.CanWrite) { p.SetValue(packet, (dynamic)portNum); return; }

            var f = typeof(MeshPacket).GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (f != null) { f.SetValue(packet, (dynamic)portNum); return; }
        }
    }

    private void SetProperty(object obj, string name, object value)
    {
        var type = obj.GetType();
        
        if (name == "Name")
        {
            var prop = type.GetProperty("Name", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop != null && prop.CanWrite) { prop.SetValue(obj, value); return; }
        }

        var directProp = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (directProp != null && directProp.CanWrite)
        {
            try { directProp.SetValue(obj, value); return; } catch {}
        }

        var directFld = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (directFld != null)
        {
            try { directFld.SetValue(obj, value); return; } catch {}
        }

        if (name.Contains("PortNum") || name.Contains("Portnum"))
        {
            var variants = new[] { "PortNum", "Portnum", "value__" };
            foreach (var v in variants)
            {
                var pCheck = type.GetProperty(v, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (pCheck != null && pCheck.CanWrite) { try { pCheck.SetValue(obj, value); return; } catch {} }

                var fCheck = type.GetField(v, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (fCheck != null) { try { fCheck.SetValue(obj, value); return; } catch {} }
            }
        }

        if (value is int iVal && type.GetProperty("Portnum", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase) != null)
        {
            var pFinal = type.GetProperty("Portnum", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (pFinal != null && pFinal.CanWrite)
            {
                try { pFinal.SetValue(obj, Convert.ChangeType(iVal, pFinal.PropertyType)); return; } catch {}
            }
        }
    }

    [Fact]
    public async Task ProcessPacketRawAsync_Port1_BroadcastsFormattedText()
    {
        // Arrange
        var packet = CreateTestPacket(1, 10);

        // Act
        await _processor.ProcessPacketRawAsync(packet, 1);

        // Assert
        Assert.Single(_broadcaster.RecordedMessages);
        Assert.Equal("[Hello Mesh]", _broadcaster.RecordedMessages[0]);
    }

    [Fact]
    public async Task ProcessPacketRawAsync_Port3_UpdatesStateAndBroadcasts()
    {
        // Arrange
        uint nodeId = 20u;
        var packet = CreateTestPacket(3, nodeId);

    // Act
        await _processor.ProcessPacketRawAsync(packet, 3);

    // Assert - Verify State Update
        var node = _stateStore.GetNode(nodeId);
        Assert.NotNull(node);
        Assert.Equal(12.34567, node.Latitude); // 123456700 / 1e7
        Assert.Equal(76.54321, node.Longitude); // 765432100 / 1e7

    // Assert - Verify Broadcast
        Assert.Single(_broadcaster.RecordedNodeUpdates);
        var update = _broadcaster.RecordedNodeUpdates[0];
        Assert.Equal(nodeId, update.Id);
        Assert.Equal(12.34567, update.Latitude);
        Assert.Equal(76.54321, update.Longitude);
    }

    [Fact]
    public async Task ProcessPacket_UnsupportedPortNum_ThrowsException()
    {
        // Arrange
        uint nodeId = 4u;
        var packet = CreateTestPacket(99, nodeId);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(async () =>
        {
            await _processor.ProcessPacketRawAsync(packet, 99);
        });
    }
}
