using System;
using System.Reflection;
using Meshtastic.Protobufs;
using Xunit;

namespace RelayMessenger.Server.UnitTests;

public class PortNumInspectionTests
{
    [Fact]
    public void InspectPortNum()
    {
        var type = typeof(PortNum);
        Console.WriteLine($"Type: {type.Name}");
        var members = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var m in members)
        {
            Console.WriteLine($"{m.MemberType} {m.Name}");
        }
    }
}