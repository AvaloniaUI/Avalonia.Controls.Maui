using Microsoft.Maui.Accessibility;

namespace Avalonia.Controls.Maui.Services;

/// <summary>
/// Avalonia implementation of the <see cref="ISemanticScreenReader"/> interface.
/// Provides a no-op implementation that prevents NotImplementedInReferenceAssemblyException
/// when calling <see cref="SemanticScreenReader.Announce(string)"/> in Avalonia-based MAUI applications.
/// </summary>
/// <remarks>
/// A real screen reader announcement implementation requires upstream Avalonia changes
/// to support live region property change notifications or direct UIA notification events.
/// See docs/avalonia-screen-reader-requirements.md for details.
/// </remarks>
internal class AvaloniaSemanticScreenReaderImplementation : ISemanticScreenReader
{
    /// <summary>
    /// Announces the specified text to the screen reader.
    /// Currently a no-op as Avalonia does not yet support programmatic screen reader announcements.
    /// </summary>
    /// <param name="text">The text to announce.</param>
    public void Announce(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;
    }
}
