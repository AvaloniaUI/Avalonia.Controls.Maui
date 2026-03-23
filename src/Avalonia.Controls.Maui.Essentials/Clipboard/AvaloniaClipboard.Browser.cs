namespace Avalonia.Controls.Maui.Essentials;

/// <summary>
/// Browser-specific clipboard implementation using direct <c>navigator.clipboard</c> JavaScript interop
/// for reliable clipboard access on WebAssembly targets.
/// </summary>
public partial class AvaloniaClipboard
{
    private partial Task<string?> PlatformGetTextAsync()
    {
        return ClipboardInterop.ReadTextAsync();
    }

    private partial Task PlatformSetTextAsync(string? text)
    {
        if (text is null)
            return Task.CompletedTask;

        return ClipboardInterop.WriteTextAsync(text);
    }
}
