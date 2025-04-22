using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
namespace RelayMessenger.Client.Interops;

[SupportedOSPlatform("browser")]
public static partial class SubtleCryptoInterop
{
    [JSImport("globalThis.relayMessenger.subtleCrypto.exportKey")]
    internal static partial Task<JSObject> ExportKey(
        string format, 
        JSObject key);
    
    #region RSA
    
    [JSImport("globalThis.relayMessenger.subtleCrypto.generateKey_RsaHashedKeyGen")]
    internal static partial Task<JSObject> GenerateKey_RsaHashedKeyGen(
        string algorithmName,
        int algorithmModulusLength,
        [JSMarshalAs<JSType.Array<JSType.Number>>()]
        byte[] algorithmPublicExponent,
        string algorithmHash,
        bool extractable,
        string[] keyUsages);
    
    [JSImport("globalThis.relayMessenger.subtleCrypto.decrypt_RsaOaep")]
    internal static partial Task<JSObject> Decrypt_RsaOaep(
        string algorithmName,
        JSObject privateKey,
        [JSMarshalAs<JSType.Array<JSType.Number>>()] byte[] payload);
    
    [JSImport("globalThis.relayMessenger.subtleCrypto.encrypt_RsaOaep")]
    internal static partial Task<JSObject> Encrypt_RsaOaep(
        string algorithmName,
        JSObject publicKey,
        [JSMarshalAs<JSType.Array<JSType.Number>>()] byte[] payload);
    
    // RSA
    #endregion
    
    #region ECDH
    
    [JSImport("globalThis.relayMessenger.subtleCrypto.generateKey_EcKeyGen")]
    internal static partial Task<JSObject> GenerateKey_EcKeyGen(
        string algorithmName,
        string algorithmNamedCurve,
        bool extractable,
        string[] keyUsages);

    [JSImport("globalThis.relayMessenger.subtleCrypto.deriveKey_EcdhKeyDerive_AesKeyGen")]
    internal static partial Task<JSObject> DeriveKey_EcdhKeyDerive_AesKeyGen(
        string algorithmEcdhName,
        JSObject algorithmEcdhPublic,
        JSObject baseKey,
        int deriveBitLength,
        string algorithmAesName,
        int algorithmAesLength,
        bool extractable, 
        string[] keyUsages,
        string digestAlgorithm);
    
    [JSImport("globalThis.relayMessenger.subtleCrypto.importKey_EcKeyImport")]
    internal static partial Task<JSObject> ImportKey_EcKeyImport(
        string format,
        [JSMarshalAs<JSType.Array<JSType.Number>>()] byte[] keyData,
        string algorithmName,
        string algorithmNamedCurve,
        bool extractable, 
        string[] keyUsages);

    #endregion
    
    #region AES-GCM

    [JSImport("globalThis.relayMessenger.subtleCrypto.decrypt_AesGcm")]
    internal static partial Task<JSObject> Decrypt_AesGcm(
        string algorithmName,
        [JSMarshalAs<JSType.Array<JSType.Number>>()] byte[] algorithmIv,
        int algorithmTagLength,
        JSObject key,
        [JSMarshalAs<JSType.Array<JSType.Number>>()] byte[] payload);
    
    [JSImport("globalThis.relayMessenger.subtleCrypto.encrypt_AesGcm")]
    internal static partial Task<JSObject> Encrypt_AesGcm(
        string algorithmName,
        [JSMarshalAs<JSType.Array<JSType.Number>>()] byte[] algorithmIv,
        int algorithmTagLength,
        JSObject key,
        [JSMarshalAs<JSType.Array<JSType.Number>>()] byte[] payload);

    #endregion
}
