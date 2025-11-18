using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;

namespace Avalonia.Controls.Maui;

public static class ScrollViewerExtensions
{
    public static global::Avalonia.Controls.Primitives.ScrollBarVisibility ToAvaloniaScrollBarVisibility(this Microsoft.Maui.ScrollBarVisibility visibility)
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