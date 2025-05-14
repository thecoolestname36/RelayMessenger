using System.Runtime.Versioning;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using RelayMessenger.Client.Clients;
using RelayMessenger.Client.Interops;
using RelayMessenger.Client.Services;
using RelayMessenger.Shared;

namespace RelayMessenger.Client.Components;

[SupportedOSPlatform("browser")]
public partial class Messenger : ComponentBase, IAsyncDisposable
{
    private record Message(Guid Id, string Content, string? Class = "");
    private ConcurrentBag<Message> _messageQueue = [];
    private CancellationTokenSource _cancellationTokenSource = new();
    private string Input { get; set; } = "";
    private bool IsLoading { get; set; } = true;
    private bool HasIdentity { get; set; } = false;
    
    [Inject] public required ClientRelayService RelayService { get; set; }
    
    [Inject] public required RelayApiClient ApiClient { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        
        MessageNotificationsInterop.RequestPermission();
        
        // Events
        RelayService.RelayCiphertextPayloadReceived.OnReceivedAsync += AddCipherTextMessage;
        RelayService.OnSecureHandshakeSuccess += Ready;
        RelayService.OnSecureHandshakeFailure += Failed;
        
        await RelayService.EnsureConnectionAsync();
        await InitiateSecureHandshake();
    }
    
    Task AddCipherTextMessage(byte[] payload)
    {   
        var message = Encoding.UTF8.GetString(payload);
        
        // Add to queue and notify view to update
        _messageQueue.Add(new(Guid.NewGuid(), message));
        StateHasChanged();
        return Task.CompletedTask;
    }

    void AddPlainTextMessage(object? sender, string? input, string? cssClass = "")
    {
        // Add to queue and notify view to update
        _messageQueue.Add(new(Guid.NewGuid(), input ?? string.Empty, cssClass));
        StateHasChanged();
    }

    async Task InitiateSecureHandshake()
    {
        AddPlainTextMessage(this, $"[{DateTime.UtcNow:h:mm:ss tt zz}] Started secure handshake...");
        await RelayService.PerformPublicKeyExchange();
    }

    void Ready()
    {
        AddPlainTextMessage(this, $"[{DateTime.UtcNow:h:mm:ss tt zz}] Ready.");
        IsLoading = false;
        StateHasChanged();
    }

    void Failed()
    {
        AddPlainTextMessage(this, $"[{DateTime.UtcNow:h:mm:ss tt zz}] Failed! Retrying...");
        Task.Delay(TimeSpan.FromSeconds(3)).Wait();
        Task.Run(InitiateSecureHandshake);
    }

    async Task RelayCiphertext()
    {
        var prefix = $"[{DateTime.UtcNow:h:mm:ss tt zz} RelayCipherText]";
        await RelayService.RelayCiphertext($"{prefix} {Input}");
        AddPlainTextMessage(this, $"{Input} {prefix}", "textAlignRight");
    }

    async Task CreateIdentity()
    {
        await RelayService.CreateIdentity(PromptInterop.Prompt("Enter your username"));
    }

    public async ValueTask DisposeAsync()
    {
        await _cancellationTokenSource.CancelAsync();
        await RelayService.DisposeAsync();
    }
}