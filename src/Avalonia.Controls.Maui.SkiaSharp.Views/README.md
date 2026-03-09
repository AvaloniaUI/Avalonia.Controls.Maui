# Avalonia.Controls.Maui.SkiaSharp.Views

Avalonia-backed handlers for [SkiaSharp.Views.Maui](https://github.com/mono/SkiaSharp), enabling `SKCanvasView` and `SKGLView` to render when .NET MAUI runs on Avalonia. In short, this can be used for controls that require using `.WithSkiaSharp()`.

- **`SKCanvasView`** — CPU raster SkiaSharp drawing surface
- **`SKGLView`** — GPU-accelerated drawing surface (uses `GRContext` from Avalonia's Skia lease when available, with CPU fallback)
- **Image sources** — `SKBitmapImageSource`, `SKImageImageSource`, `SKPixmapImageSource`, and `SKPictureImageSource` convert to Avalonia bitmaps via direct pixel copy

## Getting Started

### Prerequisites

- [.NET 11 SDK](https://dotnet.microsoft.com/download)
- [Avalonia.Controls.Maui](https://www.nuget.org/packages/Avalonia.Controls.Maui) configured in your project

### Installation

Add the NuGet package to your project:

```xml
<PackageReference Include="Avalonia.Controls.Maui.SkiaSharp.Views" Version="..." />
```

### Usage

Call `UseAvaloniaSkiaSharp()` in your `MauiProgram.cs` after `UseAvaloniaApp()`:

```csharp
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseAvaloniaApp()
            .UseAvaloniaSkiaSharp()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        return builder.Build();
    }
}
```

This registers Avalonia handlers for `SKCanvasView`, `SKGLView`, and all SkiaSharp image source types. Your existing SkiaSharp.Views.Maui code works without changes.

## License

This project is licensed under the [MIT License](https://github.com/AvaloniaUI/Avalonia.Controls.Maui/blob/main/LICENSE).
