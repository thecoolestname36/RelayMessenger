using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using RelayMessenger.API.Services;
using RelayMessenger.Shared;

namespace RelayMessenger.API.Hubs;

/// For autnentication when we want to make the server pasworded somehow, we can use
/// some sort of 2fa mechanism using something like Yubikey
/// https://docs.yubico.com/yesdk/users-manual/application-otp/otp-overview.html
///
/// TODO -- After handshake and ECDH, we need to get a next public rsa key for storing queued messages while disconnected.
/// TODO -- Figure out how to continue to use the derived key for at least a little bit to handle less than ideal connections
///     - This might require a state service for users
/// TODO allow to have an external api service manage users for load balancing and handling different workloads
/// TODO Need a message processing queue instead of directly working on the hub. This way we can "send a message" and
///     read our unread messages as they arrive from teh initial RSA response without blocking the UI.
/// TODO: If you dont have a uuid link for a new connection then you're dumped for DDOS sakes. These one time pad paths can easily be timestamp + nonces, authorized by a a 2FA

public class RelayHub(ConnectedPrivateIdentityProvider connectedPrivateIdentityProvider) : Hub
{
    private ConnectedPrivateIdentity CallerPrivateIdentity
    {
        get => Context.Items["PrivateIdentity"] as ConnectedPrivateIdentity ?? throw new NullReferenceException();
        set => Context.Items["PrivateIdentity"] = value; 
    }
 
    /// <summary>
    /// Static property is shared amongst all connections
    /// </summary>
    private static readonly ConcurrentDictionary<string, ConnectedPrivateIdentity> Shared = new();

    public override async Task OnConnectedAsync()
    {
        if (Shared.ContainsKey(Context.ConnectionId))
        {
            Context.Abort();
            throw new Exception($"Connection {Context.ConnectionId} has already been connected.");
        }
        await base.OnConnectedAsync();

        Console.WriteLine($"[{DateTime.UtcNow:h:mm:ss tt zz}] Connected({Context.ConnectionId})");
        CallerPrivateIdentity = connectedPrivateIdentityProvider.Build();
        Shared[Context.ConnectionId] = CallerPrivateIdentity;
    }
    
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"[{DateTime.UtcNow:h:mm:ss tt zz}]" + (exception == null ? $" Disconnected({Context.ConnectionId})" : $" Exception({Context.ConnectionId}) {exception.Message}"));
        Shared.TryRemove(Context.ConnectionId, out var publicIdentity);
        publicIdentity?.Dispose();
        CallerPrivateIdentity.Dispose();
        return base.OnDisconnectedAsync(exception);
    }

    /// Note: we should only build a key on request so not to do extra work.
    [HubMethodName(Routing.RelayHub.Shared.PublicKeyExchange)]
    public async Task OnPublicKeyExchange(byte[] payload)
    {
        await Clients.Caller.SendAsync(
            Routing.RelayHub.Shared.PublicKeyExchange, 
            CallerPrivateIdentity.PublicKeyExchange(payload));
    }
    
    [HubMethodName(Routing.RelayHub.Shared.SecureHandshake)]
    public async Task OnSecureHandshake(byte[] payload)
    {
        await Clients.Caller.SendAsync(
            Routing.RelayHub.Shared.SecureHandshake, 
            CallerPrivateIdentity.EnsureSecureHandshake(payload));
    }

    [HubMethodName(Routing.RelayHub.Shared.RelayCiphertext)]
    public async Task OnRelayCipherText(byte[] payload)
    {
        var plaintext = CallerPrivateIdentity.DecryptAesGcm(payload);
        var username = Encoding.UTF8.GetBytes(CallerPrivateIdentity.GetUserName() ?? "Unknown");
        var sendPayload = new byte[username.Length + plaintext.Length];
        Buffer.BlockCopy(username, 0, sendPayload, 0, username.Length);
        Buffer.BlockCopy(plaintext, 0, sendPayload, username.Length, plaintext.Length);
        
        await Parallel.ForEachAsync(Shared, new ParallelOptions(),
            async (client, ct) =>
            {
                // Early exit conditionals
                if (ct.IsCancellationRequested) return;
                if(Context.ConnectionId == client.Key) return;
                
                await Clients.Client(client.Key).SendAsync(
                    Routing.RelayHub.Shared.RelayCiphertext, 
                    client.Value.EncryptAesGcm(sendPayload), 
                    ct);
            });
    }

    [HubMethodName(Routing.RelayHub.Server.CreateIdentity)]
    public void OnCreateIdentity(byte[] payload)
    {
        var plaintext = CallerPrivateIdentity.DecryptAesGcm(payload);
        var identity = JsonSerializer.Deserialize<Dictionary<string, string>>(plaintext)
            ?? throw new NullReferenceException("Could not deserialize identity.");
        CallerPrivateIdentity.SetPublicIdentity(new PublicIdentity
        {
            UserName = identity["UserName"],
            PublicKey = new EcdsaKey
            {
                Key = Convert.FromBase64String(identity["PublicKey"]),
            },
        });
    }
}
