using Avalonia.Controls;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Platform;

namespace Avalonia.Controls.Maui.Extensions;

/// <summary>
/// Extension methods for mapping IContentView properties to Avalonia ContentView.
/// </summary>
public static class ContentViewExtensions
{
    /// <summary>
    /// Updates the Content of the ContentView.
    /// </summary>
    /// <param name="platformView">The Avalonia ContentView control to update.</param>
    /// <param name="contentView">The .NET MAUI ContentView providing the content.</param>
    /// <param name="mauiContext">The MauiContext for platform conversion.</param>
    public static void UpdateContent(this Avalonia.Controls.Maui.Platform.ContentView platformView, IContentView contentView, IMauiContext? mauiContext)
    {
        if (mauiContext == null)
            return;

        platformView.Children.Clear();

        if (contentView.PresentedContent is IView view)
        {
            platformView.Children.Add((Control)view.ToPlatform(mauiContext));
        }
    }
}
