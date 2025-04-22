namespace RelayMessenger.Client.Services.Models;

internal record PrivateIdentity
{
    internal required string UserName { get; init; }
    internal required EcdsaKeyPair KeyPair { get; init; }
    internal Dictionary<string, string> ToDictionary() => new()
    {
        ["UserName"] = UserName,
        ["PublicKey"] = Convert.ToBase64String(KeyPair.PublicKey.Key),
        ["PrivateKey"] = Convert.ToBase64String(KeyPair.PrivateKey.Key)
    };
}
