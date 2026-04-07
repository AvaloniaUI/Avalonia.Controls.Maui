using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace Avalonia.Controls.Maui.Essentials;

/// <summary>
/// JavaScript interop bindings for the Web Speech API via a JS module.
/// </summary>
[SupportedOSPlatform("browser")]
internal static partial class SpeechSynthesisInterop
{
    const string ModuleName = "AvaloniaTextToSpeech";
    static bool _moduleLoaded;

    internal static async Task EnsureModuleLoadedAsync()
    {
        if (_moduleLoaded)
            return;

        await JSHost.ImportAsync(ModuleName,
            "../js/speechSynthesis-interop.js")
            .ConfigureAwait(false);

        _moduleLoaded = true;
    }

    [JSImport("getVoicesJson", ModuleName)]
    internal static partial Task<string> GetVoicesJsonAsync();

    [JSImport("speak", ModuleName)]
    internal static partial Task SpeakAsync(string text, string lang, string voiceId,
        double pitch, double rate, double volume);

    [JSImport("cancelSpeech", ModuleName)]
    internal static partial void CancelSpeech();
}
