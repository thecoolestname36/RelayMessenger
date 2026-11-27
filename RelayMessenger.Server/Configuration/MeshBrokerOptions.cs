namespace RelayMessenger.Server.Configuration;

public sealed class MeshBrokerOptions
{
    public const string SectionName = "MeshBroker";

    public MeshBrokerOptions()
    {
        Host = "mesh.example.com";
        Port = 8883;
    }

    public string Host { get; set; } = default!;
    public int Port { get; set; }
}
