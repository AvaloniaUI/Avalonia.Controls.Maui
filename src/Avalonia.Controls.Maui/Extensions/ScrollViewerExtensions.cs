using Microsoft.Maui;

namespace Avalonia.Controls.Maui;

public static class ScrollViewerExtensions
{
    /// <summary>
    /// Converts a .NET MAUI <see cref="Microsoft.Maui.ScrollBarVisibility"/> value to the corresponding
    /// Avalonia <see cref="global::Avalonia.Controls.Primitives.ScrollBarVisibility"/> value.
    /// </summary>
    /// <param name="visibility">MAUI scroll bar visibility.</param>
    /// <returns>Avalonia scroll bar visibility.</returns>
    public static global::Avalonia.Controls.Primitives.ScrollBarVisibility ToAvaloniaScrollBarVisibility(this ScrollBarVisibility visibility)
    {
        return visibility switch
        {
            ScrollBarVisibility.Default => global::Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            ScrollBarVisibility.Never => global::Avalonia.Controls.Primitives.ScrollBarVisibility.Hidden,
            ScrollBarVisibility.Always => global::Avalonia.Controls.Primitives.ScrollBarVisibility.Visible,
            _ => global::Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
        };
    }
}
