namespace Avalonia.Controls.Maui.Essentials;

/// <summary>
/// Browser/WASM implementation that opens URLs via <c>window.open()</c> JavaScript interop.
/// </summary>
public partial class AvaloniaBrowser
{
    private partial Task<bool> PlatformOpenAsync(Uri uri)
    {
        var window = LauncherInterop.WindowOpen(uri.AbsoluteUri, "_blank");
        return Task.FromResult(window is not null);
    }
}
