namespace RelayMessenger.Shared;

public record ClientMessageRequest
{
    public required string Message { get; init; }
    public uint? To { get; init; }
}
