using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace RelayMessenger.Server.IntegrationTests;

public class DiSmokeTests
{
    private readonly WebApplicationFactory<Program> _factory = new();

    [Fact]
    public async Task Server_StartsCorrectly()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/");
        Assert.True(response.IsSuccessStatusCode);
    }
}