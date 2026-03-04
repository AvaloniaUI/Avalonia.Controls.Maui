using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Maui;
using System;

namespace AlohaAI;

class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<AvaloniaApp>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
