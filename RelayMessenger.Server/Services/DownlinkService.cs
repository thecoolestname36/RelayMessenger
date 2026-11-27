using MQTTnet;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RelayMessenger.Server.Hubs;
using RelayMessenger.Server.Services;
using RelayMessenger.Shared;
using RelayMessenger.Shared.Models;
using Meshtastic.Protobufs;

namespace RelayMessenger.Server.Services;

public class DownlinkService
{
    private readonly IMqttGateway _mqttGateway;

    public DownlinkService(IMqttGateway mqttGateway)
    {
        _mqttGateway = mqttGateway;
    }

    public async Task PublishDownlinkAsync(SendMessageRequest request, bool useProtobuf = false)
    {
        string topic;
        byte[] payload;

        if (useProtobuf)
        {
            topic = "msh/US/2/mqtt/";
            payload = BuildProtobufDownlink(request);
        }
        else
        {
            topic = "msh/US/2/json/mqtt/";
            payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request));
        }

        var messageBuilder = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .Build();

        await _mqttGateway.PublishAsync(messageBuilder, CancellationToken.None);
    }

    private byte[] BuildProtobufDownlink(SendMessageRequest request)
    {
        var data = new Data();
        data.Portnum = (PortNum)1; // TEXTMESSAGE_APP
        
        SetProperty(data, "Payload", request.Message);

        using var stream = new System.IO.MemoryStream();
        using var codedOutput = new Google.Protobuf.CodedOutputStream(stream);
        data.WriteTo(codedOutput);
        codedOutput.Flush();
        return stream.ToArray();
    }

    private static void SetProperty(object obj, string propertyName, object value)
    {
        var prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (prop != null)
        {
            if (prop.PropertyType == typeof(Google.Protobuf.ByteString) && value is string strValue)
            {
                prop.SetValue(obj, Google.Protobuf.ByteString.CopyFromUtf8(strValue));
            }
            else
            {
                prop.SetValue(obj, value);
            }
        }
        else
        {
            var field = obj.GetType().GetField(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (field != null)
            {
                if (field.FieldType == typeof(Google.Protobuf.ByteString) && value is string strValue)
                {
                    field.SetValue(obj, Google.Protobuf.ByteString.CopyFromUtf8(strValue));
                }
                else
                {
                    field.SetValue(obj, value);
                }
            }
        }
    }
}