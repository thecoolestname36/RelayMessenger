using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace RelayMessenger.Client.Services.Models;

[SupportedOSPlatform("browser")]
internal record JsCryptoKey(JSObject Key) : IDisposable
{
    public void Dispose() => Key.Dispose();
}