using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace RelayMessenger.Client.Interops;

[SupportedOSPlatform("browser")]
public static partial class PromptInterop
{
    [JSImport("globalThis.prompt")]
    public static partial string Prompt(string? text = "", string? defaultText = "");
}