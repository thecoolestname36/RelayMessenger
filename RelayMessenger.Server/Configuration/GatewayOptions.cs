namespace RelayMessenger.Server.Configuration;

public sealed class GatewayOptions
{
    public const string SectionName = "Gateway";

    public GatewayOptions()
    {
        HubUrl = "/relayhub";
        NodesEndpoint = "/Nodes";
    }

    public string HubUrl { get; set; } = default!;
    public string NodesEndpoint { get; set; } = default!;
    public string Region { get; set; } = "US";
}