using System.Runtime.Versioning;
using System.Text.Json;
using RelayMessenger.Shared;
using RelayMessenger.Client.Interops;
using RelayMessenger.Client.Services.Models;

namespace RelayMessenger.Client.Services;

[SupportedOSPlatform("browser")]
public class ClientIdentityService
{
    internal void SetIdentity(PrivateIdentity identity)
    {
        LocalStorageInterop.SetItem("identity", JsonSerializer.Serialize(identity.ToDictionary()));
    }

    internal PublicIdentity GetPublicIdentity()
    {
        var identityString = LocalStorageInterop.GetItem("identity");
        if (string.IsNullOrEmpty(identityString)) throw new NullReferenceException("Could not get identity from local storage.");
        var identity = JsonSerializer.Deserialize<Dictionary<string, string>>(identityString) ?? throw new NullReferenceException("Could not deserialize identity from local storage.");
        return new PublicIdentity
        {
            UserName = identity["UserName"],
            PublicKey = new EcdsaKey
            {
                Key = Convert.FromBase64String(identity["PublicKey"]),
            }
        };
    }
}