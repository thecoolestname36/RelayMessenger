namespace RelayMessenger.Shared;

public record PublicIdentity
{
    public required string UserName { get; init; }
    public required EcdsaKey PublicKey { get; init; }
    public Dictionary<string, string> ToDictionary() => new()
    {
        ["UserName"] = UserName,
        ["PublicKey"] = Convert.ToBase64String(PublicKey.Key),
    };
}