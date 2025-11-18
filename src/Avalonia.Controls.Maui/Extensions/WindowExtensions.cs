using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;

namespace Avalonia.Controls.Maui;

public static class WindowExtensions
{
    public static void UpdateTitle(this Avalonia.Controls.Window platformWindow, IWindow window) =>
        platformWindow.Title = window.Title;

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