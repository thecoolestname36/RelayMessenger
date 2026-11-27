namespace RelayMessenger.Shared;

/// <summary>
/// A thin Data Transfer Object for telemetry updates, optimized for SignalR transmission 
/// and UI responsiveness. This avoids recursive serialization issues from full Protobuf models.
/// </summary>
public record NodeUpdateDto
{
    public uint Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public double AltitudeMeters { get; init; }
    public int Snr { get; init; }
    public int Rssi { get; init; }
    public float Battery { get; init; }
    public DateTime LastSeen { get; init; }
}
