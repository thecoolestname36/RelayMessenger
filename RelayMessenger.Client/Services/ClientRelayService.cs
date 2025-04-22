using System.Text;
using System.Runtime.Versioning;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR.Client;
using RelayMessenger.Client.Clients;
using RelayMessenger.Client.Services.Models;
using RelayMessenger.Shared;

namespace RelayMessenger.Client.Services;

[SupportedOSPlatform("browser")]
public class ClientRelayService : IAsyncDisposable
{
    private readonly RelayHubClient _relayHubClient;
    private readonly ClientCryptoService _clientCryptoService;
    private readonly ClientIdentityService _clientIdentityService;

    public bool IsReady { get; private set; } = false;
    // Events
    public event Action? OnSecureHandshakeSuccess;
    public event Action? OnSecureHandshakeFailure;
    internal readonly ReceivedCiphertextEvent RelayCiphertextPayloadReceived;
    
    public ClientRelayService(RelayHubClient relayHubClient, ClientCryptoService clientCryptoService, ClientIdentityService clientIdentityService)
    {
        _relayHubClient = relayHubClient;
        _clientCryptoService = clientCryptoService;
        _clientIdentityService = clientIdentityService;
        
        // Events
        RelayCiphertextPayloadReceived = new(CiphertextReceived, _relayHubClient, Routing.RelayHub.Shared.RelayCiphertext);
    }

    private async Task<byte[]> CiphertextReceived(byte[] payload)
    {
        var plaintext = await _clientCryptoService.DecryptAesGcm(payload);
        // TODO Need to refresh the keys, probably need double ratchet if this is what we want :/
        //Task.Run(PerformPublicKeyExchange);
        return plaintext;
    }

    public async Task PerformPublicKeyExchange()
    {
        _relayHubClient.Connection.On(Routing.RelayHub.Shared.PublicKeyExchange, new Func<byte[], Task>(OnPublicKeyExchange));
        await _relayHubClient.Connection.SendAsync(
            Routing.RelayHub.Shared.PublicKeyExchange, 
            await _clientCryptoService.ClientPublicKeyExchange());
    }

    private async Task OnPublicKeyExchange(byte[] payload)
    {
        _relayHubClient.Connection.Remove(Routing.RelayHub.Shared.PublicKeyExchange);
        _relayHubClient.Connection.On(Routing.RelayHub.Shared.SecureHandshake, new Func<byte[], Task>(OnSecureHandshake));
        await SendCiphertext(
            Routing.RelayHub.Shared.SecureHandshake,
            await _clientCryptoService.ServerPublicKeyExchange(payload));
    }
    
    private async Task OnSecureHandshake(byte[] payload)
    {  
        _relayHubClient.Connection.Remove(Routing.RelayHub.Shared.SecureHandshake);
        try
        {
            await _clientCryptoService.EnsureSecureHandshake(payload); // throws on validation failure
            IsReady = true;
            OnSecureHandshakeSuccess?.Invoke();
        }
        catch (Exception e)
        {
            Console.WriteLine($"{nameof(OnSecureHandshake)}: {e.Message}");
            OnSecureHandshakeFailure?.Invoke();
        }
    }

    public async Task CreateIdentity(string userName)
    {
        _clientIdentityService.SetIdentity(new PrivateIdentity
        {
            UserName = userName,
            KeyPair = await _clientCryptoService.GenerateEcdsaKeyPair()
        });
        var identity = _clientIdentityService.GetPublicIdentity();
        var publicIdentity = identity.ToDictionary();
        await SendCiphertext(Routing.RelayHub.Server.CreateIdentity, publicIdentity);
    }

    public async Task RelayCiphertext(string payload) => await SendCiphertext(
        Routing.RelayHub.Shared.RelayCiphertext, 
        Encoding.UTF8.GetBytes(payload));
    
    #region Helpers

    public async Task EnsureConnectionAsync()
    {
        if (_relayHubClient.Connection.State == HubConnectionState.Disconnected) await _relayHubClient.BeginConnectionAsync();
    }
    
    private async Task SendCiphertext(string methodName, byte[] payload)
    {
        if (_relayHubClient.Connection.State == HubConnectionState.Connected)
        {
            try
            {
                await (_relayHubClient.Connection.SendAsync(methodName, await _clientCryptoService.EncryptAesGcm(payload)));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        else
        {
            Console.WriteLine($"{nameof(SendCiphertext)} error client is not connected! {methodName}");
        }
    }

    private Task SendCiphertext(string methodName, Dictionary<string, string> payload) =>
        SendCiphertext(methodName, JsonSerializer.SerializeToUtf8Bytes(payload));
    
    #endregion

    public async ValueTask DisposeAsync()
    {
        await _relayHubClient.DisposeAsync();
        _clientCryptoService.Dispose();
    }
}

[SupportedOSPlatform("browser")]
internal class ReceivedCiphertextEvent
{
    private readonly Func<byte[], Task<byte[]>> _decipher;
    public delegate Task EventHandlerAsync(byte[] payload);
    public event EventHandlerAsync? OnReceivedAsync;

    internal ReceivedCiphertextEvent(Func<byte[], Task<byte[]>> decipher, RelayHubClient relayHubClient, string methodName)
    {
        _decipher = decipher;
        relayHubClient.Connection.On<byte[]>(methodName, RaiseOnReceivedAsync);
    }

    protected async Task RaiseOnReceivedAsync(byte[] payload) => 
        await (OnReceivedAsync?.Invoke(await _decipher(payload)) ?? Task.CompletedTask);
}
