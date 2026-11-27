using System;
using System.Reflection;
using Xunit;
using Meshtastic.Protobufs;

namespace RelayMessenger.Server.UnitTests;

public class ProtobufDiscoveryTests
{
    [Fact]
    public void ListProperties()
    {
        var dataProps = typeof(Data).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in dataProps)
        {
            Console.WriteLine($"Data: {prop.Name} ({prop.PropertyType})");
        }

        var packetProps = typeof(MeshPacket).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in packetProps)
        {
            Console.WriteLine($"MeshPacket: {prop.Name} ({prop.PropertyType})");
        }
    }
}