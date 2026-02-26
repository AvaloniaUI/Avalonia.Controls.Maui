using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;

namespace Avalonia.Controls.Maui;

/// <summary>
/// Provides extension methods for integrating Avalonia <see cref="Avalonia.Controls.Window"/> with .NET MAUI <see cref="IWindow"/>.
/// </summary>
public static class WindowExtensions
{
    /// <summary>
    /// Updates the title of an Avalonia <see cref="Avalonia.Controls.Window"/> from the associated .NET MAUI <see cref="IWindow"/>.
    /// </summary>
    /// <param name="platformWindow">The Avalonia window to update.</param>
    /// <param name="window">The .NET MAUI window providing the title.</param>
    public static void UpdateTitle(this Avalonia.Controls.Window platformWindow, IWindow window) =>
        platformWindow.Title = window.Title;

    /// <summary>
    /// Finds the .NET MAUI <see cref="IWindow"/> associated with the specified Avalonia <see cref="Avalonia.Controls.Window"/>.
    /// </summary>
    /// <param name="platformWindow">The Avalonia window to look up.</param>
    /// <returns>The associated <see cref="IWindow"/>, or <c>null</c> if no match is found.</returns>
    public static IWindow? GetWindow(this Avalonia.Controls.Window platformWindow)
    {
        foreach (var window in MauiAvaloniaApplication.Current.Application.Windows)
        {
            if (window?.Handler?.PlatformView is Avalonia.Controls.Window win && win == platformWindow)
                return window;
        }

        return null;
    }
}