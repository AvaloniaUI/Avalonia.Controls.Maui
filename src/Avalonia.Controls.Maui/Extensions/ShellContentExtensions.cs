using Microsoft.Maui.Controls;
using Avalonia.Controls.Maui.Handlers.Shell;
using MauiPage = Microsoft.Maui.Controls.Page;

namespace Avalonia.Controls.Maui.Extensions;

/// <summary>
/// Extension methods for <see cref="ShellContentHandler"/>.
/// </summary>
public static class ShellContentExtensions
{
    /// <summary>
    /// Updates the content of the <see cref="ShellContentHandler"/>.
    /// </summary>
    /// <param name="handler">The <see cref="ShellContentHandler"/> instance.</param>
    /// <param name="shellContent">The <see cref="ShellContent"/> instance to update from.</param>
    public static void UpdateContent(this ShellContentHandler handler, ShellContent shellContent)
    {
        if (handler._contentContainer == null || shellContent == null || handler.MauiContext == null)
            return;

        // Get the actual page content
        MauiPage? page = null;

        if (shellContent is IShellContentController contentController)
        {
            page = contentController.GetOrCreateContent();
        }
        else if (shellContent.Content is MauiPage contentPage)
        {
            page = contentPage;
        }

        if (page != null)
        {
            // Content placement is managed by ShellSectionHandler to ensure 
            // correct navigation and transitions.
        }
        else
        {
            handler._contentContainer.Content = null;
        }
    }
}
