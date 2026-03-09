# Avalonia.Controls.Maui.Compatibility

Provides Avalonia-based handlers for deprecated and legacy .NET MAUI controls. This package is intended for apps migrating from Xamarin.Forms or older MAUI versions that still use controls like `ListView`, `Frame`, and `TableView`.

## Getting Started

Install this package alongside `Avalonia.Controls.Maui` and register the compatibility handlers in your `MauiProgram.cs`. Call `UseAvaloniaCompatibility()` **after** `UseAvaloniaApp()`:

```csharp
using Avalonia.Controls.Maui;
using Avalonia.Controls.Maui.Compatibility;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseAvaloniaApp()
            .UseAvaloniaCompatibility();

        return builder.Build();
    }
}
```

## Supported Controls

| Control | Description |
|---------|-------------|
| **Frame** | Legacy container with border, shadow, and corner radius |
| **ListView** | Legacy list control with item templates, grouping, headers/footers, and pull-to-refresh |
| **TableView** | Legacy table control for displaying data rows and settings-style forms |
| **TextCell** | Cell displaying primary and secondary text |
| **ImageCell** | Cell displaying an image alongside primary and secondary text |
| **SwitchCell** | Cell with a label and toggle switch |
| **EntryCell** | Cell with a label and text input |
| **ViewCell** | Cell with a custom view template |

For new development, prefer the modern MAUI equivalents (`Border`, `CollectionView`, etc.) which are handled by the core `Avalonia.Controls.Maui` package.

## License

This project is licensed under the [MIT License](https://github.com/AvaloniaUI/Avalonia.Controls.Maui/blob/main/LICENSE).
