namespace RelayMessenger.Shared;

public struct Routing {
    public struct RelayApi
    {
        public const string Path = "api";
    }
    public struct RelayHub 
    {
        public const string Path = "hubs/relay";

        public struct Shared
        {
            public const string PublicKeyExchange  = "PublicKeyExchange";
            public const string SecureHandshake = "SecureHandshake";
            public const string RelayCiphertext = "RelayCiphertext";
        }
        public struct Server
        {
            public const string CreateIdentity = "CreateIdentity";
        }
        public struct Client
        {
        }
    }
}
