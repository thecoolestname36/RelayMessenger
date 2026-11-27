using System;
using System.Collections.Generic;
using System.Threading;
using MQTTnet;

namespace RelayMessenger.Server.Services.Processors;

public class InterceptingPublishEventArgs
{
    public MqttApplicationMessage ApplicationMessage { get; }
    public string ClientId { get; }
    public string Topic { get; }
    public bool ProcessPublish { get; set; } = true;

    public InterceptingPublishEventArgs(
        MqttApplicationMessage applicationMessage,
        CancellationToken cancellationToken,
        string clientId,
        string topic,
        Dictionary<string, object>? properties = null)
    {
        ApplicationMessage = applicationMessage;
        ClientId = clientId;
        Topic = topic;
    }
}