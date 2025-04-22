using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using RelayMessenger.Client.Interops;

namespace RelayMessenger.Client.Services.Models;

[SupportedOSPlatform("browser")]
internal record JsCryptoKeyPair : IDisposable
{
    internal JsCryptoKey PrivateKey { get; }
    internal JsCryptoKey PublicKey { get; }
    /// <exception cref="CryptographicException">On failure to get a public/private key from the keyPair <see cref="JSObject"/></exception>
    internal JsCryptoKeyPair(JSObject keyPair) {
        PrivateKey = new(keyPair.GetPropertyAsJSObject("privateKey") ?? throw new CryptographicException("Private key could not be retrieved."));
        PublicKey = new(keyPair.GetPropertyAsJSObject("publicKey") ?? throw new CryptographicException("Public key could not be retrieved."));
    }
    public void Dispose()
    {
        PrivateKey.Dispose();
        PublicKey.Dispose();
    }
}