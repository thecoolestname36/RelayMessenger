using Microsoft.AspNetCore.SignalR.Client;

namespace RelayMessenger.Client.Clients;

public class RelayHubClient : IAsyncDisposable
{
    internal HubConnection Connection { get; }

    public RelayHubClient(HubConnection connection)
    {
        Connection = connection;
    }

    public async Task BeginConnectionAsync()
    {
        Console.WriteLine("Connecting to the Relay Hub...");
        await Connection.StartAsync();
        Connection.Closed += OnClosed;
        Console.WriteLine("Connection established.");
    }

    public async Task EndConnectionAsync()
    {
        Console.WriteLine("Disconnecting from the Relay Hub...");
        Connection.Closed -= OnClosed;
        await Connection.StopAsync();
        Console.WriteLine("Connection terminated.");
    }
    
    private async Task OnClosed(Exception? error)
    {
        Console.WriteLine("Connection closed.");
        await Task.Delay(new Random().Next(0,5) * 1000);
        await Connection.StartAsync();
        Console.WriteLine("Connection established.");
    }

    public async ValueTask DisposeAsync()
    {
        await Connection.DisposeAsync();
    }
}
