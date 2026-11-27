namespace RelayMessenger.Shared;

public record NodeStateModel
{
    public uint Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public double AltitudeMeters { get; init; }
    public int Snr { get; init; }
    public int Rssi { get; init; }
    public DateTime LastSeen { get; init; }
    public float Battery { get; init; }
    public float Temperature { get; init; }
    public string Region { get; init; } = "US";
    public string GatewayId { get; init; } = "default-gw";
    public DateTime Timestamp { get; init; }

    public string GetFormattedId() => $"!{Id:x8}";
}