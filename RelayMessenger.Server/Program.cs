using RelayMessenger.Server.Services;
using RelayMessenger.Shared.State;
using RelayMessenger.Server.Services.Processors;
using RelayMessenger.Shared;
using RelayMessenger.Server;
using MQTTnet;
using MQTTnet.Server;
using RelayMessenger.Server.Hubs;
using RelayMessenger.Server.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Register Options
builder.Services.Configure<MeshBrokerOptions>(
    builder.Configuration.GetSection(MeshBrokerOptions.SectionName));
builder.Services.Configure<GatewayOptions>(
    builder.Configuration.GetSection(GatewayOptions.SectionName));
builder.Services.AddSingleton(sp =>
{
    var options = new GatewayOptions();
    sp.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>()
        .GetSection(GatewayOptions.SectionName)
        .Bind(options);
    return options;
});

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR();
builder.Services.AddAntiforgery();

// Register Mesh services
builder.Services.AddSingleton<NodeStateStore>();
builder.Services.AddSingleton<IMeshClientService, MeshClientService>(); // Added interface mapping
builder.Services.AddSingleton<IMessageBroadcaster, SignalRMessageBroadcaster>();
builder.Services.AddSingleton<MeshtasticPacketProcessor>();

// Register MQTT Server
builder.Services.AddSingleton<MqttServer>(sp =>
{
    var options = new MqttServerOptionsBuilder()
        .WithoutDefaultEndpoint()
        .WithEncryptedEndpoint()
        .WithEncryptedEndpointPort(8883)
        .Build();

    var server = new MqttServerFactory().CreateMqttServer(options);

    // Hook up our processor
    var processor = sp.GetRequiredService<MeshtasticPacketProcessor>();
    server.InterceptingPublishAsync += async (mqttArgs) =>
    {
        var properties = new Dictionary<string, object>();
        foreach (var p in mqttArgs.ApplicationMessage.UserProperties)
        {
            properties[p.Name] = p.Value;
        }

        var relayArgs = new RelayMessenger.Server.Services.Processors.InterceptingPublishEventArgs(
            mqttArgs.ApplicationMessage,
            mqttArgs.CancellationToken,
            mqttArgs.ClientId,
            mqttArgs.ApplicationMessage.Topic,
            properties);
        await processor.HandleInterceptingPublish(relayArgs);
    };

    return server;
});

// Register MQTT Client (for outbound messages to the mesh)
builder.Services.AddSingleton<IMqttClient>(sp =>
{
    return new MQTTnet.MqttClientFactory().CreateMqttClient();
});

builder.Services.AddSingleton<IMqttGateway, MqttGateway>();
builder.Services.AddSingleton<DownlinkService>();
builder.Services.AddHostedService<MeshClientService>();

var app = builder.Build();
var gatewayOptions = app.Services.GetRequiredService<GatewayOptions>();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.UseAntiforgery();

// SignalR Hub mapping
app.MapHub<RelayHub>(gatewayOptions.HubUrl);

// Minimal API Endpoints


app.Run();

public partial class Program { }