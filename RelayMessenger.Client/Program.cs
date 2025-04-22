using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using RelayMessenger.Shared;
using RelayMessenger.Client.Clients;
using RelayMessenger.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

if (OperatingSystem.IsBrowser())
{
    builder.Services.AddHttpClient<RelayApiClient>("RelayMessenger.Server", client => client.BaseAddress = new UriBuilder(new Uri(builder.HostEnvironment.BaseAddress))
    {
        Path = Routing.RelayApi.Path
    }.Uri);
    builder.Services.AddScoped(provider => provider.GetRequiredService<IHttpClientFactory>().CreateClient("RelayMessenger.Server"));
    builder.Services.AddSingleton<ClientCryptoService>();
    builder.Services.AddSingleton<ClientIdentityService>();
    builder.Services.AddScoped<RelayHubClient>(provider =>
    {
        var hub =  new HubConnectionBuilder().WithUrl(new UriBuilder(new Uri(builder.HostEnvironment.BaseAddress))
        {
            Path = Routing.RelayHub.Path,
        }.Uri)
            //.WithStatefulReconnect() TODO: Only if we need to, figure out a authentication process so a user doesn't have to renegotiate certs every connection
            .Build();
        hub.KeepAliveInterval = TimeSpan.FromSeconds(15);
        hub.ServerTimeout = TimeSpan.FromSeconds(45);
        return new RelayHubClient(hub);
    });
    builder.Services.AddScoped<ClientRelayService>();
}

await builder.Build().RunAsync();
