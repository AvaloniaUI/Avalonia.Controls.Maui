using Avalonia.Controls;
using System;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// A content container for MAUI applications running in single-view mode (browser, mobile, etc.)
/// This class doesn't inherit from Window to avoid windowing platform dependencies.
/// </summary>
public class MauiAvaloniaContent : ContentControl, IDisposable
{
    public MauiAvaloniaContent()
    {
    }

    public void Dispose()
    {
    }

    /// <summary>
    /// Sets the main content.
    /// </summary>
    public void SetMainContent(object? content)
    {
        Content = content;
    }
}
