using MauiClipboard = Microsoft.Maui.ApplicationModel.DataTransfer.IClipboard;

namespace Avalonia.Controls.Maui.Essentials;

/// <summary>
/// Avalonia implementation of <see cref="MauiClipboard"/> that provides cross-platform clipboard support
/// for both desktop (Linux) and browser (WebAssembly) targets.
/// </summary>
/// <remarks>
/// On desktop, delegates to Avalonia's <c>TopLevel.Clipboard</c> API and monitors window activation
/// to detect external clipboard changes.
/// On browser, uses direct <c>navigator.clipboard</c> JavaScript interop for reliable clipboard access.
/// </remarks>
public partial class AvaloniaClipboard : MauiClipboard
{
    readonly IAvaloniaEssentialsPlatformProvider _platformProvider;
    string? _lastKnownText;
    bool _hasText;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaloniaClipboard"/> class with the specified platform provider.
    /// </summary>
    /// <param name="platformProvider">The platform provider used to retrieve the Avalonia TopLevel for clipboard access.</param>
    public AvaloniaClipboard(IAvaloniaEssentialsPlatformProvider platformProvider)
    {
        _platformProvider = platformProvider;
    }

    /// <inheritdoc/>
    public bool HasText => _hasText;

    /// <inheritdoc/>
    public event EventHandler<EventArgs>? ClipboardContentChanged;

    /// <inheritdoc/>
    public async Task<string?> GetTextAsync()
    {
        try
        {
            var text = await PlatformGetTextAsync();
            UpdateState(text);
            return text;
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task SetTextAsync(string? text)
    {
        bool success;
        try
        {
            success = await PlatformSetTextAsync(text);
        }
        catch
        {
            return;
        }

        if (!success)
            return;

        UpdateState(text);
        ClipboardContentChanged?.Invoke(this, EventArgs.Empty);
    }

    void UpdateState(string? text)
    {
        _lastKnownText = text;
        _hasText = !string.IsNullOrEmpty(text);
    }

    private partial Task<string?> PlatformGetTextAsync();

    private partial Task<bool> PlatformSetTextAsync(string? text);
}
