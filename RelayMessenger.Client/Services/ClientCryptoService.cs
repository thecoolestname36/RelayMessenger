using System.Runtime.Versioning;
using System.Security.Cryptography;
using RelayMessenger.Shared;
using RelayMessenger.Client.Interops;
using RelayMessenger.Client.Services.Models;

// Notes:
//  - If the connection is interrupted during an ECDH secure handshake, we need to start over for security sake
// TODO
//  - need to have a way to expire, refresh and publish teh AES key in the background so the client does not have to wait
//      and instead the service will just be privided a key to continue with after.. some way to ensure we have all our messages? 
//  - 
//  - 

namespace RelayMessenger.Client.Services;

[SupportedOSPlatform("browser")]
public class ClientCryptoService : IDisposable
{
    private byte[]? _handshakeTranscript;
    
    internal async Task<byte[]> ClientPublicKeyExchange()
    {
        await GenerateEcdhKeyPair();
        var nonce = Guid.NewGuid().ToByteArray();
        var pubKey = await ExportClientEcdhPublicKey();
        _handshakeTranscript = new byte[272];
        Buffer.BlockCopy(nonce, 0, _handshakeTranscript, 0, nonce.Length);
        Buffer.BlockCopy(pubKey, 0, _handshakeTranscript, nonce.Length, pubKey.Length);
        return _handshakeTranscript[..136];
    }

    internal async Task<byte[]> ServerPublicKeyExchange(byte[] payload)
    {
        if (_handshakeTranscript == null) throw new CryptographicException("Handshake transcript is null.");
        // First 16 bytes are the Guid nonce
        await ImportServerEcdhPublicKey(payload[16..]);
        await DeriveAesGcmKeyFromEcdh();
        Buffer.BlockCopy(payload, 0, _handshakeTranscript, 136, payload.Length);
        return _handshakeTranscript;
    }

    internal async Task EnsureSecureHandshake(byte[] payload)
    {
        if (_handshakeTranscript == null) throw new CryptographicException("Handshake transcript is null.");
        var pt = await DecryptAesGcm(payload);
        var equal = _handshakeTranscript.SequenceEqual(pt);
        _handshakeTranscript = null;
        _clientEcdhKeyPair?.Dispose();
        _serverEcdhPublicKey?.Dispose();
        if (equal) return;
        // Not equal, we need to clean up the rest
        _aesGcmKey?.Dispose();
        throw new CryptographicException("Handshake transcript is invalid.");
    }

    #region RSA
    private JsCryptoKeyPair? _rsaKeyPair;
    
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/RsaHashedKeyGenParams"/>
    private static class RsaHashedKeyGenParams
    {
        internal const string Name = "RSA-OAEP";
        internal const int ModulusLength = 8192;
        internal static readonly byte[] PublicExponent = [1, 0, 1];
        internal const string Hash = "SHA-256";
    }
    internal async Task GenerateRsaKeyPair()
    {
        _rsaKeyPair?.Dispose();
        _rsaKeyPair = new(await SubtleCryptoInterop.GenerateKey_RsaHashedKeyGen(
            algorithmName: RsaHashedKeyGenParams.Name,
            algorithmModulusLength: RsaHashedKeyGenParams.ModulusLength,
            algorithmPublicExponent: RsaHashedKeyGenParams.PublicExponent,
            algorithmHash: RsaHashedKeyGenParams.Hash,
            extractable: true,
            keyUsages: ["encrypt", "decrypt"]));
    }
    
    private static class ExportRsaPublicKeyParams
    {
        internal const string Format = "spki";
    }
    internal async Task<byte[]> ExportRsaPublicKey()
    {
        if (_rsaKeyPair == null) throw new CryptographicException("RSA key pair is null");
        return (await SubtleCryptoInterop.ExportKey(
                ExportClientEcdhPublicKeyParams.Format, 
                _rsaKeyPair.PublicKey.Key))
            .GetPropertyAsByteArray("key") ?? throw new CryptographicException("Public key could not be retrieved.");
    }
    
    internal async Task<byte[]> DecryptRsa(byte[] payload)
    {
        if (_rsaKeyPair == null) throw new CryptographicException("RSA key pair is null");
        return (await SubtleCryptoInterop.Decrypt_RsaOaep(
                RsaHashedKeyGenParams.Name, 
                _rsaKeyPair.PrivateKey.Key, 
                payload))
            .GetPropertyAsByteArray("payload") ?? throw new CryptographicException("Failed to retrieve payload from JSInterop.");
    }
    
    internal async Task<byte[]> EncryptRsa(byte[] payload)
    {
        if (_rsaKeyPair == null) throw new CryptographicException("RSA key pair is null");
        return (await SubtleCryptoInterop.Encrypt_RsaOaep(
                RsaHashedKeyGenParams.Name, 
                _rsaKeyPair.PublicKey.Key, 
                payload))
            .GetPropertyAsByteArray("payload") ?? throw new CryptographicException("Failed to retrieve payload from JSInterop.");
    }
    
    // RSA
    #endregion
    
    // TODO: Need to make it so we store the 
    
    #region ECDH
    private JsCryptoKeyPair? _clientEcdhKeyPair;
    private JsCryptoKey? _serverEcdhPublicKey;
    
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/EcKeyGenParams"/>
    private static class EcdhKeyGenParams
    {
        internal const string Name = "ECDH";
        internal const string NamedCurve = "P-384";
    }
    internal async Task GenerateEcdhKeyPair()
    {
        _clientEcdhKeyPair?.Dispose();
        _clientEcdhKeyPair = new(await SubtleCryptoInterop.GenerateKey_EcKeyGen(
            algorithmName: EcdhKeyGenParams.Name,
            algorithmNamedCurve: EcdhKeyGenParams.NamedCurve,
            extractable: true,
            keyUsages: ["deriveKey", "deriveBits"]));
    }

    private static class ExportClientEcdhPublicKeyParams
    {
        internal const string Format = "spki";
    }
    private async Task<byte[]> ExportClientEcdhPublicKey()
    {
        if (_clientEcdhKeyPair == null) throw new CryptographicException("ECDH key pair is null");
        return (await SubtleCryptoInterop.ExportKey(
                ExportClientEcdhPublicKeyParams.Format, 
                _clientEcdhKeyPair.PublicKey.Key))
            .GetPropertyAsByteArray("key") ?? throw new CryptographicException("Public key could not be retrieved.");
    }

    private static class EcKeyImportParams
    {
        internal const string Format = "spki";
        internal const string Name = "ECDH";
    }
    private async Task ImportServerEcdhPublicKey(byte[] payload)
    {
        _serverEcdhPublicKey?.Dispose();
        _serverEcdhPublicKey = new(await SubtleCryptoInterop.ImportKey_EcKeyImport(
            format: EcKeyImportParams.Format,
            keyData: payload,
            algorithmName: EcKeyImportParams.Name,
            algorithmNamedCurve: EcdhKeyGenParams.NamedCurve,
            extractable: false,
            keyUsages: []));
    }
    
    // ECDH
    #endregion
    
    #region ECDSA
    
    private JsCryptoKeyPair? _clientEcdsaKeyPair;
    
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/EcKeyGenParams"/>
    private static class EcdsaKeyGenParams
    {
        internal const string Name = "ECDSA";
        internal const string NamedCurve = "P-384";
    }
    private static class ExportClientEcdsaPublicKeyParams
    {
        internal const string Format = "spki";
    }
    private static class ExportClientEcdsaPrivateKeyParams
    {
        internal const string Format = "pkcs8";
    }
    internal async Task<EcdsaKeyPair> GenerateEcdsaKeyPair()
    {
        _clientEcdsaKeyPair?.Dispose();
        _clientEcdsaKeyPair = new(await SubtleCryptoInterop.GenerateKey_EcKeyGen(
            algorithmName: EcdsaKeyGenParams.Name,
            algorithmNamedCurve: EcdsaKeyGenParams.NamedCurve,
            extractable: true,
            keyUsages: ["sign"]));
        return new EcdsaKeyPair
        {
            PublicKey = new EcdsaKey
            {
                Key = (await SubtleCryptoInterop.ExportKey(ExportClientEcdsaPublicKeyParams.Format, _clientEcdsaKeyPair.PublicKey.Key))
                    .GetPropertyAsByteArray("key") ?? throw new CryptographicException($"Public ECDSA key could not be retrieved as {ExportClientEcdsaPublicKeyParams.Format}.")
            },
            PrivateKey = new EcdsaKey
            {
                Key = (await SubtleCryptoInterop.ExportKey(ExportClientEcdsaPrivateKeyParams.Format, _clientEcdsaKeyPair.PrivateKey.Key))
                    .GetPropertyAsByteArray("key") ?? throw new CryptographicException($"Private ECDSA key could not be retrieved as {ExportClientEcdsaPrivateKeyParams.Format}.")
            }
        };
    }
    
    // ECDSA
    #endregion
    
    #region AES-GCM
    private JsCryptoKey? _aesGcmKey;
    private AesGcmNonce? _aesGcmNonce;
    
    private static class EcdhKeyDeriveParams
    {
        internal const string Name = EcdhKeyGenParams.Name;
    }
    private static class AesKeyGenParams
    {
        internal const int DeriveBitLength = 384;
        internal const string Name = "AES-GCM";
        internal const int Length = 256;
        internal const string DigestAlgorithm = "SHA-256";
    }
    internal async Task DeriveAesGcmKeyFromEcdh()
    {
        _aesGcmKey?.Dispose();
        _aesGcmNonce = null;
        if (_clientEcdhKeyPair == null) throw new CryptographicException("Client ECDH key pair is null");
        if (_serverEcdhPublicKey == null) throw new CryptographicException("Server ECDH public key is null");
        _aesGcmKey = new(await SubtleCryptoInterop.DeriveKey_EcdhKeyDerive_AesKeyGen(
            algorithmEcdhName: EcdhKeyDeriveParams.Name,
            algorithmEcdhPublic: _serverEcdhPublicKey.Key,
            baseKey: _clientEcdhKeyPair.PrivateKey.Key,
            deriveBitLength: AesKeyGenParams.DeriveBitLength,
            algorithmAesName: AesKeyGenParams.Name,
            algorithmAesLength: AesKeyGenParams.Length,
            extractable: false,
            keyUsages: ["encrypt", "decrypt"],
            digestAlgorithm: AesKeyGenParams.DigestAlgorithm));
        _aesGcmNonce = new AesGcmNonce(false);
    }

    private static class AesGcmParams
    {
        internal const string Name = AesKeyGenParams.Name;
        internal const int TagLength = AesGcmTag.Length * 8;
    }

    internal async Task<byte[]> DecryptAesGcm(byte[] payload)
    {
        if (_aesGcmKey == null) throw new CryptographicException("AES-GSM key is null");
        return (await SubtleCryptoInterop.Decrypt_AesGcm(
            AesGcmParams.Name, 
            payload[..AesGcmNonce.Length],
            AesGcmParams.TagLength,
            _aesGcmKey.Key,
            payload[AesGcmNonce.Length..]))
        .GetPropertyAsByteArray("payload") ?? throw new CryptographicException("Failed to retrieve payload from JSInterop.");
    }
    
    internal async Task<byte[]> EncryptAesGcm(byte[] payload)
    {
        if (_aesGcmKey == null) throw new CryptographicException("AES-GSM key is null");
        if (_aesGcmNonce == null) throw new CryptographicException("AES-GSM nonce is null");
        var nonce = _aesGcmNonce.GenerateNonce();
        var ciphertext = (await SubtleCryptoInterop.Encrypt_AesGcm(
                AesGcmParams.Name,
                nonce,
                AesGcmParams.TagLength,
                _aesGcmKey.Key,
                payload))
            .GetPropertyAsByteArray("payload") ?? throw new CryptographicException("Failed to retrieve payload from JSInterop.");
        var buffer = new byte[nonce.Length + ciphertext.Length];
        Buffer.BlockCopy(nonce, 0, buffer, 0, nonce.Length);
        Buffer.BlockCopy(ciphertext, 0, buffer, nonce.Length, ciphertext.Length);
        return buffer;
    }

    // AES-GCM
    #endregion
    
    public void Dispose()
    {
        _rsaKeyPair?.Dispose();
        _clientEcdhKeyPair?.Dispose();
        _aesGcmKey?.Dispose();
    }
}
