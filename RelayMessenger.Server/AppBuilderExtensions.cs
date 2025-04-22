using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using RelayMessenger.Server.Services;
using RelayMessenger.Server.Hubs;

namespace RelayMessenger.Server;

public static class AppBuilderExtensions
{
    public static IServiceCollection AddRelayMessengerServerServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddSignalR(options =>
        {
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(45);
        });
        services.AddSingleton<ServerCryptoService>();
        services.AddTransient<ConnectedPrivateIdentityProvider>();
        return services;
    }

    public static IEndpointRouteBuilder UseRelayMessengerServer(this IEndpointRouteBuilder routeBuilder, string apiBasePath, string hubBasePath)
    {
        routeBuilder.MapHub<RelayHub>(hubBasePath, options =>
        {
            //options.AllowStatefulReconnects = true; // TODO: Only enable this if we need to save time on connection 
        });
        return routeBuilder;
    }
}