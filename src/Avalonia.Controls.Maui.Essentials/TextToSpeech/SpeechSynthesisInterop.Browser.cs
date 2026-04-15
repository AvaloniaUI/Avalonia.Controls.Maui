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
    const string ModulePath = "../_content/Avalonia.Controls.Maui.Essentials/js/speechSynthesis-interop.js";
    static readonly object ModuleLoadLock = new();
    static bool _moduleLoaded;
    static Task? _moduleLoadTask;

    internal static Task EnsureModuleLoadedAsync()
    {
        lock (ModuleLoadLock)
        {
            if (_moduleLoaded)
                return Task.CompletedTask;

            _moduleLoadTask ??= LoadModuleAsync();
            return _moduleLoadTask;
        }
    }

    static async Task LoadModuleAsync()
    {
        try
        {
            await JSHost.ImportAsync(ModuleName, ModulePath).ConfigureAwait(false);

            lock (ModuleLoadLock)
            {
                _moduleLoaded = true;
            }
        }
        catch
        {
            lock (ModuleLoadLock)
            {
                _moduleLoadTask = null;
            }

            throw;
        }
    }

    [JSImport("getVoicesJson", ModuleName)]
    internal static partial Task<string> GetVoicesJsonAsync();

    [JSImport("speak", ModuleName)]
    internal static partial Task SpeakAsync(string text, string lang, string voiceId,
        double pitch, double rate, double volume);

    [JSImport("cancelSpeech", ModuleName)]
    internal static partial void CancelSpeech();
}
