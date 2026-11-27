using System;
using System.Buffers;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Meshtastic.Protobufs;
using RelayMessenger.Shared;
using RelayMessenger.Shared.State;
using RelayMessenger.Server.Services;

namespace RelayMessenger.Server.Services.Processors;

public class MeshtasticPacketProcessor
{
    private readonly IMessageBroadcaster _broadcaster;
    private readonly NodeStateStore _stateStore;

    public MeshtasticPacketProcessor(IMessageBroadcaster broadcaster, NodeStateStore stateStore)
    {
        _broadcaster = broadcaster;
        _stateStore = stateStore;
    }

    public async Task ProcessPacketAsync(RelayMessenger.Shared.ServiceEnvelope envelope)
    {
        if (envelope.Data == null) return;
        await ProcessPacket(envelope, envelope.Data);
    }

    public async Task ProcessPacket(RelayMessenger.Shared.ServiceEnvelope envelope, byte[] encryptedData, uint portNum = 0)
    {
        using var ms = new MemoryStream(encryptedData);
        var packet = Meshtastic.Protobufs.MeshPacket.Parser.ParseFrom(ms);
        
        uint actualPortNum = portNum != 0 ? portNum : (GetNestedValue_Internal(packet, "Portnum") ?? GetNestedValue_Internal(packet, "PortNum") ?? 0);
        
        await ProcessPacketRawAsync(packet, actualPortNum);
    }

    internal async Task ProcessPacketRawAsync(Meshtastic.Protobufs.MeshPacket packet, uint portNum)
    {
        switch (portNum)
        {
            case 3: // POSITION_APP
                var posUpdate = UpdateNodeState(packet, portNum);
                if (posUpdate != null) await _broadcaster.BroadcastNodeUpdatedAsync(posUpdate);
                break;
            case 1: // TEXTMESSAGE_APP
                var decodedTxt = FindRecursive(packet, "Decoded");
                object? bytesObj = null;

                if (decodedTxt is byte[] b) 
                {
                    bytesObj = b;
                }
                else if (decodedTxt is Google.Protobuf.ByteString bs) 
                {
                    bytesObj = bs.ToByteArray();
                }
                else if (decodedTxt != null && !(decodedTxt is string))
                {
                    // Port 1 "Decoded" can be a 'Data' message containing the text in its 'Bytes' field.
                    var bytesResult = FindRecursive(decodedTxt, "Payload");
                    if (bytesResult is byte[] bArray) 
                    {
                        bytesObj = bArray;
                    }
                    else if (bytesResult is Google.Protobuf.ByteString bStr) 
                    {
                        bytesObj = bStr.ToByteArray();
                    }
                }
                
                if (bytesObj != null)
                {
                    var message = Encoding.UTF8.GetString((byte[])bytesObj);
                    await _broadcaster.BroadcastTextAsync($"[{message}]");
                }
                break;
            case 4: // NODEINFO_APP
                var infoUpdate = UpdateNodeState(packet, portNum);
                if (infoUpdate != null) await _broadcaster.BroadcastNodeUpdatedAsync(infoUpdate);
                break;
            default:
                throw new Exception($"Unsupported Portnum: {portNum}");
        }
    }

    public RelayMessenger.Shared.NodeUpdateDto? UpdateNodeState(Meshtastic.Protobufs.MeshPacket packet, uint portNum)
    {
        var node = _stateStore.GetNode(packet.From);
        if (node == null)
        {
            node = new NodeStateModel { Id = packet.From, Name = "Unknown", LastSeen = DateTime.UtcNow };
            _stateStore.UpdateNode(node);
        }
    
        switch (portNum)
        {
            case 3: // PositionApp
                var posData = FindRecursive(packet, "Decoded");
                if (posData != null)
                {
                    var position = GetPositionFromData(posData);
                    double latRawVal = position != null ? position.LatitudeI : ConvertToDouble(posData, "Latitude");
                    double lonRawVal = position != null ? position.LongitudeI : ConvertToDouble(posData, "Longitude");
                    if (latRawVal != 0 || lonRawVal != 0)
                    {
                        _stateStore.UpdateNode(node with 
                        { 
                            Latitude = latRawVal / 1e7,
                            Longitude = lonRawVal / 1e7,
                            LastSeen = DateTime.UtcNow
                        });
                    
                        var rawSnr = GetNestedValue_Internal(posData, "Snr");
                        var rawRssi = GetNestedValue_Internal(posData, "Rssi");
                        var rawBatt = GetNestedValue_Internal(posData, "Battery");
                        double altValRaw = ConvertToDouble(posData, "Altitude");
                    
                        int snr = (rawSnr.HasValue) ? (int)rawSnr.Value : node.Snr;
                        int rssi = (rawRssi.HasValue) ? (int)rawRssi.Value : node.Rssi;
                        float battery = (rawBatt.HasValue) ? (float)rawBatt.Value : node.Battery;
                    
                        return new RelayMessenger.Shared.NodeUpdateDto
                        {
                            Id = node.Id,
                            Name = node.Name,
                            Latitude = latRawVal / 1e7,
                            Longitude = lonRawVal / 1e7,
                            AltitudeMeters = altValRaw > 0 ? altValRaw : node.AltitudeMeters,
                            Snr = snr,
                            Rssi = rssi,
                            Battery = battery,
                            LastSeen = DateTime.UtcNow
                        };
                    }
                }
                break;
            case 4: // NodeInfoApp
                var infoData = FindRecursive(packet, "Decoded");
                if (infoData != null)
                {
                    string nameResultRaw = GetStringValue(infoData, "Name");
                    if (!string.IsNullOrEmpty(nameResultRaw))
                    {
                        _stateStore.UpdateNode(node with 
                        { 
                            Name = nameResultRaw,
                            LastSeen = DateTime.UtcNow
                        });
                    
                        var rawSnr = GetNestedValue_Internal(infoData, "Snr");
                        var rawRssi = GetNestedValue_Internal(infoData, "Rssi");
                        var rawBatt = GetNestedValue_Internal(infoData, "Battery");
                    
                        int snr = (rawSnr.HasValue) ? (int)rawSnr.Value : node.Snr;
                        int rssi = (rawRssi.HasValue) ? (int)rawRssi.Value : node.Rssi;
                        float battery = (rawBatt.HasValue) ? (float)rawBatt.Value : node.Battery;
                    
                        return new RelayMessenger.Shared.NodeUpdateDto
                        {
                            Id = node.Id,
                            Name = nameResultRaw,
                            Latitude = node.Latitude,
                            Longitude = node.Longitude,
                            AltitudeMeters = node.AltitudeMeters,
                            Snr = snr,
                            Rssi = rssi,
                            Battery = battery,
                            LastSeen = DateTime.UtcNow
                        };
                    }
                }
                break;
        }
        return null;
    }

    private uint? GetNestedValue_Internal(object obj, string name)
    {
        var val = FindRecursive(obj, name);
        if (val is int i) return (uint)i;
        if (val is uint u) return u;
        return null;
    }

    private object? FindRecursive(object obj, string name)
    {
        return FindRecursiveInternal(obj, name, new HashSet<object>(), 0);
    }

    private object? FindRecursiveInternal(object obj, string name, HashSet<object> visited, int depth)
    {
        if (obj == null || obj is string) return obj;
        if (!visited.Add(obj)) return null;
        if (depth > 5) return null;

        var type = obj.GetType();

        try
        {
            // Priority 1: Direct match on properties/fields
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop != null)
            {
                var val = prop.GetValue(obj);
                if (CheckIsSimple(val) || val is Google.Protobuf.IMessage) return val;
            }

            var fld = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (fld != null)
            {
                var val = fld.GetValue(obj);
                if (CheckIsSimple(val) || val is Google.Protobuf.IMessage) return val;
            }

            // Priority 2: Recurse into nested containers
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
            foreach (var p in properties)
            {
                try
                {
                    if (p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) continue;

                    var val = p.GetValue(obj);
                    if (val != null && !CheckIsSimple(val))
                    {
                        // Narrowing recursion to only Protobuf messages or lists/collections of them to avoid getting stuck in internal engine types
                        if (val is Google.Protobuf.IMessage || val is System.Collections.IList || val is System.Collections.IEnumerable)
                        {
                            var result = FindRecursiveInternal(val, name, visited, depth + 1);
                            if (result != null) return result;
                        }
                    }
                } catch { }
            }

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
            foreach (var f in fields)
            {
                try
                {
                    if (f.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) continue;

                    var val = f.GetValue(obj);
                    if (val != null && !CheckIsSimple(val))
                    {
                        // Narrowing recursion to only Protobuf messages or lists/collections of them to avoid getting stuck in internal engine types
                        if (val is Google.Protobuf.IMessage || val is System.Collections.IList || val is System.Collections.IEnumerable)
                        {
                            var result = FindRecursiveInternal(val, name, visited, depth + 1);
                            if (result != null) return result;
                        }
                    }
                } catch { }
            }
        }
        catch { }

        return null;
    }

    private bool CheckIsSimple(object? val)
    {
        if (val == null) return false;
        return val is string || val is int || val is long || val is float || val is double || 
               val is bool || val is byte || val is short || val is uint || val is ulong || val is Google.Protobuf.ByteString;
    }

    private double ConvertToDouble(object obj, string propName)
    {
        var val = FindRecursive(obj, propName);
        if (val == null) return 0.0;
        if (val is int i) return (double)i;
        if (val is long l) return (double)l;
        if (val is float f) return (double)f;
        if (val is double d) return d;
        try { 
            return Convert.ToDouble(val); 
        } catch { 
            return 0.0; 
        }
    }

    private Position? GetPositionFromData(object data)
    {
        if (data is Data dataMsg)
        {
            try
            {
                if (dataMsg.Payload != null && dataMsg.Payload.Length > 0)
                {
                    return Position.Parser.ParseFrom(dataMsg.Payload.ToByteArray());
                }
            }
            catch { }
        }
        return null;
    }

    private string GetStringValue(object obj, string propName)
    {
        var val = FindRecursive(obj, propName);
        if (val != null && !(val is string)) return string.Empty;
        return val?.ToString() ?? string.Empty;
    }

    public void UpdateNodeStateFromBytes(byte[] data, uint fromId)
    {
        using var ms = new MemoryStream(data);
        var packet = Meshtastic.Protobufs.MeshPacket.Parser.ParseFrom(ms);
        UpdateNodeState(packet, GetNestedValue_Internal(packet, "Portnum") ?? GetNestedValue_Internal(packet, "PortNum") ?? 0);
    }

    public async Task HandleInterceptingPublish(RelayMessenger.Server.Services.Processors.InterceptingPublishEventArgs args)
    {
        var sequence = args.ApplicationMessage.Payload;
        byte[] data = new byte[sequence.Length];
        int offset = 0;
        foreach (var segment in sequence)
        {
            segment.CopyTo(data.AsMemory(offset));
            offset += segment.Length;
        }
    
        await ProcessPacketAsync(new RelayMessenger.Shared.ServiceEnvelope(data));
    }

    internal async Task ProcessPacketInternalAsync(MeshPacket packet, uint portNum)
    {
        // For testing purposes, we bypass the WireFormat here because 'Decoded' is a custom test field 
        // that won't survive Protobuf serialization/deserialization roundtrips.
        await ProcessPacketRawAsync(packet, portNum);
    }
}
