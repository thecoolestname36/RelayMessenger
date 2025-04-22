namespace RelayMessenger.Client.Clients;

public class RelayApiClient(HttpClient httpClient)
{
    public readonly HttpClient Client = httpClient;
    
}