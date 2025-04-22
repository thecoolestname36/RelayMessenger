using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using RelayMessenger.Shared;

namespace RelayMessenger.Client.Interops;

[SupportedOSPlatform("browser")]
public static partial class MessageNotificationsInterop
{
    [JSImport("globalThis.relayMessenger.messageNotifications.requestPermission")]
    public static partial void RequestPermission();
}
