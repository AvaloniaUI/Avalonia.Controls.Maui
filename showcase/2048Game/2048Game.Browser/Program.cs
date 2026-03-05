using Avalonia;
using Avalonia.Browser;
using Avalonia.Controls;
using Avalonia.Controls.Maui;
using System;
using System.Threading.Tasks;

namespace _2048Game;

internal sealed partial class Program
{
    private static Task Main(string[] args) => BuildAvaloniaApp()
            .WithInterFont()
            .StartBrowserAppAsync("out");

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<AvaloniaApp>();
}
