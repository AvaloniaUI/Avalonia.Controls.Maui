using Avalonia.Controls;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Platform;

namespace Avalonia.Controls.Maui.Extensions;

/// <summary>
///     Extension methods for <see cref="MauiContentPresenter" />.
/// </summary>
public static class ContentPresenterExtensions
{
    /// <summary>
    ///     Updates the content of the <see cref="MauiContentPresenter" /> based on the <see cref="IContentView" />.
    /// </summary>
    /// <param name="platformView">The native View.</param>
    /// <param name="contentView">The Cross-platform View.</param>
    /// <param name="mauiContext">The MauiContext.</param>
    public static void UpdateContent(this MauiContentPresenter platformView, IContentView contentView, IMauiContext? mauiContext)
    {
        if (mauiContext == null)
            return;

        platformView.Children.Clear();

        if (contentView.PresentedContent is IView view)
            platformView.Children.Add((Control)view.ToPlatform(mauiContext));
    }
}
