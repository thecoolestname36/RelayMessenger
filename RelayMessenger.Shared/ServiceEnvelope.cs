namespace RelayMessenger.Shared;

public record ServiceEnvelope(byte[] Data)
{
    public bool HasData => Data != null && Data.Length > 0;
}
