using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Components.Web;
using RelayMessenger.Client.Services;
using RelayMessenger.Shared;
using RelayMessenger.Client;
using RelayMessenger.Client.Interops;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Add services to the container.
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:5001") });

builder.Services.AddScoped<SignalRService>(sp => 
    new SignalRService("https://localhost:5001/relayhub", sp.GetRequiredService<ILogger<SignalRService>>()));
builder.Services.AddScoped<NodeService>();
builder.Services.AddScoped<GoogleMapsInterop>();

await builder.Build().RunAsync();
