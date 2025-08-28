using System.Security.Cryptography;

namespace RelayMessenger.API.Services;

// TODO: Hub crypto - When doing pure end to end encryption, we can use a ephemeral Ecdh key that is updated whenever
//  a new user enters the group. This way all users are using the same encryption aes key generation when they connect
//  for E2EE. Note: the synchronous key will not expire ever unless a user requests a new generation exchange, i.e. users
//  joining or leaving. A user should be rate-limited able to request a new key exchange at any time.

public class ServerCryptoService
{
}
