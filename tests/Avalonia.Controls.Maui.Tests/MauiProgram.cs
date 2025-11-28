using Avalonia.Controls.Maui.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace Avalonia.Controls.Maui.Tests;

/// <summary>
/// Creates and configures the MauiApp for testing.
/// </summary>
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<MauiAppStub>()
            .UseAvaloniaApp()
            .UseAvaloniaGraphics();

        // Register a minimal font manager for testing
        builder.Services.AddSingleton<IFontManager>(sp =>
            new FontManager(new TestFontRegistrar(), sp));

        return builder.Build();
    }
}

/// <summary>
/// Minimal font registrar for testing that does nothing.
/// </summary>
public class TestFontRegistrar : IFontRegistrar
{
    public void Register(string filename, string? alias, System.Reflection.Assembly assembly)
    {
        // No-op for tests
    }

    public void Register(string filename, string? alias)
    {
        // No-op for tests
    }

    public string? GetFont(string font)
    {
        return font;
    }
}
