using RelayMessenger.Shared;

namespace RelayMessenger.Client.Services.Models;

internal record EcdsaKeyPair()
{
    internal required EcdsaKey PublicKey { get; init; }
    internal required EcdsaKey PrivateKey { get; init; }
}