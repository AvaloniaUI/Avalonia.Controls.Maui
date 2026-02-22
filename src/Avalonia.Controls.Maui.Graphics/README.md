# Avalonia.Controls.Maui.Graphics

Bridges `Microsoft.Maui.Graphics` drawing to Avalonia using Skia. This package provides an Avalonia-based handler for MAUI's `GraphicsView` control, enabling custom drawing, touch/pointer interactions, and graphics-intensive rendering in your MAUI app through Avalonia.

## Getting Started

Install this package alongside `Avalonia.Controls.Maui` and register the graphics handler in your `MauiProgram.cs`:

```csharp
using Avalonia.Controls.Maui;
using Avalonia.Controls.Maui.Graphics;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseAvaloniaApp()
            .UseAvaloniaGraphics();

        return builder.Build();
    }
}
```

## What's Included

- **GraphicsViewHandler** - Avalonia handler for MAUI's `GraphicsView` control
- **Skia rendering backend** - `IDrawable` implementations render through Avalonia's Skia pipeline
- **Touch and pointer interaction** - Full support for touch, hover, and pointer events on the graphics surface

## License

This project is licensed under the [MIT License](https://github.com/AvaloniaUI/Avalonia.Controls.Maui/blob/main/LICENSE).
