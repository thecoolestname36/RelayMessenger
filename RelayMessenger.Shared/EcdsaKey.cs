namespace RelayMessenger.Shared;

public record EcdsaKey
{
    public required byte[] Key { get; init; }
}