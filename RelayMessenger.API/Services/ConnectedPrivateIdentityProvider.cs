using System.Security.Cryptography;
using RelayMessenger.Shared;

namespace RelayMessenger.API.Services;

// TODO: Somehow have the ability to export your account creds so you can open
//  the app with the same user on multiple devices. Also have a view for the user
//  to view the authorized connections and see when they're active/ revoke them

public class ConnectedPrivateIdentityProvider
{
    internal ConnectedPrivateIdentity Build() => new();
}

internal class ConnectedPrivateIdentity: IDisposable
{
    // Create an RSA DH key and then md5 it to store the identity of the user in teh future. the RSA-DH MD5 will be the unique thumbprint of the user, and the user can do the same for the server

    private byte[]? _handshakeTranscript;
    private RSA? _clientRsaPublicKey;
    private ECDiffieHellman? _clientEcdhPublicKey;
    private ECDiffieHellman? _serverEcdhKeyPair;
    private AesGcm? _aesGcm;
    private AesGcmNonce? _aesGcmNonce;
    private PublicIdentity? _publicIdentity;

    internal void SetClientRsaPublicKey(byte[] payload)
    {
        _clientRsaPublicKey?.Dispose();
        _clientRsaPublicKey = RSA.Create();
        _clientRsaPublicKey.ImportSubjectPublicKeyInfo(payload, out _);
    }

    internal byte[] PublicKeyExchange(byte[] payload)
    {
        SetClientEcdhPublicKey(payload[16..]);
        GenerateEcdhKeyPair();
        DeriveAesGcmKeyFromEcdh();
        var nonce = Guid.NewGuid().ToByteArray();
        var pubKey = ExportServerEcdhPublicKey();
        _handshakeTranscript = new byte[272];
        Buffer.BlockCopy(payload, 0, _handshakeTranscript, 0, payload.Length);
        Buffer.BlockCopy(nonce, 0, _handshakeTranscript, payload.Length, nonce.Length);
        Buffer.BlockCopy(pubKey, 0, _handshakeTranscript, payload.Length + nonce.Length, pubKey.Length);
        return _handshakeTranscript[payload.Length..];
    }

    internal byte[] EnsureSecureHandshake(byte[] payload)
    {
        if(_handshakeTranscript == null) throw new CryptographicException("Handshake transcript is null");
        var handshakeTranscript = _handshakeTranscript;
        _handshakeTranscript = null;
        _clientEcdhPublicKey?.Dispose();
        _serverEcdhKeyPair?.Dispose();
        if (handshakeTranscript.SequenceEqual(DecryptAesGcm(payload))) return EncryptAesGcm(handshakeTranscript);
        // Not equal, we need to clean up
        _aesGcm?.Dispose();
        throw new CryptographicException("Handshake transcript failed");
    }

    private void SetClientEcdhPublicKey(byte[] payload)
    {
        _clientEcdhPublicKey?.Dispose();
        _clientEcdhPublicKey = ECDiffieHellman.Create();
        _clientEcdhPublicKey.ImportSubjectPublicKeyInfo(payload, out _);
    }

    private void GenerateEcdhKeyPair()
    {
        _serverEcdhKeyPair?.Dispose();
        _serverEcdhKeyPair = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP384);
    }

    private byte[] ExportServerEcdhPublicKey()
    {
        if (_serverEcdhKeyPair == null) throw new CryptographicException("Server ECDH key is null");
        var result = _serverEcdhKeyPair.ExportSubjectPublicKeyInfo();
        return result;
    }
    
    private void DeriveAesGcmKeyFromEcdh()
    {
        _aesGcm?.Dispose();
        _aesGcmNonce = null;
        if (_clientEcdhPublicKey == null) throw new CryptographicException("Client ECDH public key is null");
        if (_serverEcdhKeyPair == null) throw new CryptographicException("Server ECDH key pair is null");
        
        _aesGcm = new AesGcm(_serverEcdhKeyPair.DeriveKeyMaterial(_clientEcdhPublicKey.PublicKey), AesGcmTag.Length);
        _aesGcmNonce = new AesGcmNonce(true);
    }

    // https://stackoverflow.com/a/78577582/6775340
    internal byte[] DecryptAesGcm(byte[] payload)
    {
        if (_aesGcm == null) throw new CryptographicException("AES-GCM key is null");
        var plaintextBytes = new byte[payload.Length - (AesGcmNonce.Length + AesGcmTag.Length)];
        _aesGcm.Decrypt(
            payload[..AesGcmNonce.Length],
            payload[AesGcmNonce.Length..^AesGcmTag.Length],
            payload[^AesGcmTag.Length..], 
            plaintextBytes);
        return plaintextBytes;
    }

    // https://stackoverflow.com/a/78577582/6775340
    internal byte[] EncryptAesGcm(byte[] payload)
    {
        if (_aesGcm == null) throw new CryptographicException("AES-GCM key is null");
        if (_aesGcmNonce == null) throw new CryptographicException("GCM nonce is null");
        
        var cipherTextBytes = new byte[payload.Length];
        var tag = new byte[AesGcmTag.Length];
        var nonce = _aesGcmNonce.GenerateNonce();
        _aesGcm.Encrypt(
            nonce,
            payload, 
            cipherTextBytes, 
            tag);
        
        var buffer = new byte[nonce.Length + cipherTextBytes.Length + tag.Length];
        Buffer.BlockCopy(nonce, 0, buffer, 0, nonce.Length);
        Buffer.BlockCopy(cipherTextBytes, 0, buffer, nonce.Length, cipherTextBytes.Length);
        Buffer.BlockCopy(tag, 0, buffer, nonce.Length + cipherTextBytes.Length, tag.Length);
        return buffer;
    }

    internal void SetPublicIdentity(PublicIdentity identity)
    {
        _publicIdentity = identity;
    }

    internal string? GetUserName() => _publicIdentity?.UserName;

    public void Dispose()
    {
        _clientRsaPublicKey?.Dispose();
        _clientEcdhPublicKey?.Dispose();
        _serverEcdhKeyPair?.Dispose();
    }
}
