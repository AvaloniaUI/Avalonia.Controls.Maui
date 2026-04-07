# Configuration and setup

`Avalonia.Controls.Maui` supports two setup modes:

- Full hosting: MAUI controls are rendered through Avalonia (`UseAvaloniaApp`).
- Embedding: only `AvaloniaView` is registered inside a MAUI-native app (`UseAvaloniaEmbedding<TApp>`).

Before your app can use Avalonia-drawn controls, fonts, or assets, register the appropriate services in `MauiProgram.cs` by chaining extension methods on `MauiAppBuilder` inside `CreateMauiApp`.

This document explains how to configure the app builder, register fonts and assets, and add custom control handlers.

## Configure the app builder

### Choose a mode

Use exactly one of the following entry points:

```csharp
// Full hosting mode
builder.UseAvaloniaApp();

// Embedding mode (MAUI-native host)
builder.UseAvaloniaEmbedding<AvaloniaApp>();
```

> [!IMPORTANT]
> `UseAvaloniaApp` and `UseAvaloniaEmbedding<TApp>` are mutually exclusive. Use one or the other in a single app.

### UseAvaloniaApp

`UseAvaloniaApp` is the required entry point for full hosting mode. It replaces MAUI handlers with Avalonia handlers and configures the Avalonia dispatcher, ticker, image services, and font services.

```csharp
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseAvaloniaApp();

        return builder.Build();
    }
}
```

`UseAvaloniaApp` accepts an optional `useSingleViewLifetime` parameter. Set this to `true` for WebAssembly/Browser targets (single-view lifetime). For standard desktop windowed lifetimes, omit it or pass `false`.

```csharp
// WASM / Browser
builder.UseAvaloniaApp(useSingleViewLifetime: true);
```

On current source state, the `net*-windows` WinUI path is not production-ready. For Avalonia on Windows, prefer the base desktop TFM (`net11.0`) with full hosting.

> [!IMPORTANT]
> In full-hosting mode, call `UseAvaloniaApp` before optional package extensions such as `UseAvaloniaCompatibility`, `UseAvaloniaEssentials`, or `UseAvaloniaSkiaSharp`.

### UseAvaloniaEmbedding

`UseAvaloniaEmbedding<TApp>` registers only `AvaloniaView` so you can host Avalonia content inside a MAUI-native UI stack.

```csharp
builder
    .UseMauiApp<App>()
    .UseAvaloniaEmbedding<AvaloniaApp>();
```

Use embedding when incrementally adopting Avalonia in a MAUI-native app. On current source state, Windows embedding is a stub path and should not be treated as production-ready.

### UseAvaloniaCompatibility

The `UseAvaloniaCompatibility` method registers Avalonia-based handlers for .NET MAUI controls that are deprecated but may still be present in existing projects:

| Deprecated control | Recommended replacement |
|---|---|
| `Frame` | `Border` |
| `ListView` | `CollectionView` |
| `TableView` | `CollectionView` with grouping |
| `TextCell`, `ImageCell`, `ViewCell`, `SwitchCell`, `EntryCell` | Custom `DataTemplate` cells |

```csharp
using Avalonia.Controls.Maui.Compatibility;

builder
    .UseMauiApp<App>()
    .UseAvaloniaApp()
    .UseAvaloniaCompatibility();
```

> [!NOTE]
> New projects should use the modern replacements listed above. `UseAvaloniaCompatibility` is intended for migrating existing codebases.

### UseAvaloniaEssentials

The `UseAvaloniaEssentials` method registers Avalonia-based implementations of `Microsoft.Maui.Essentials` APIs. Without this call, those APIs fall back to MAUI defaults, which typically throw platform-specific not-supported/not-implemented exceptions on desktop and browser targets.

```csharp
using Avalonia.Controls.Maui.Essentials;

builder
    .UseMauiApp<App>()
    .UseAvaloniaApp()
    .UseAvaloniaEssentials();
```

The following Essentials APIs are implemented:

| API | Notes |
|---|---|
| `Screenshot` | Always outputs PNG format |
| `FilePicker` | Single and multi-file selection |
| `MediaPicker` | Photo and video picking; camera capture throws `NotSupportedException` |
| `FileSystem` | `CacheDirectory`, `AppDataDirectory`, `avares://` asset loading |
| `Preferences` | Full type support; persisted as JSON |
| `HapticFeedback` | Stub implementation; `IsSupported` returns `false` |
| `VersionTracking` | Registered as an `IVersionTracking` service and auto-initialized at startup; tracks first launch, current version/build, and version/build history |

### UseAvaloniaSkiaSharp

The `UseAvaloniaSkiaSharp` method registers Avalonia-backed handlers for `SKCanvasView` and `SKGLView` from the `SkiaSharp.Views.Maui` package, along with image source services for SkiaSharp image types.

```csharp
using Avalonia.Controls.Maui.SkiaSharp.Views;

builder
    .UseMauiApp<App>()
    .UseAvaloniaApp()
    .UseAvaloniaSkiaSharp();
```

Pass `forceSoftwareRendering: true` on platforms where GPU rendering is unreliable (e.g. Browser/WASM):

```csharp
builder.UseAvaloniaSkiaSharp(forceSoftwareRendering: true);
```

> [!NOTE]
> This method is provided by the `Avalonia.Controls.Maui.SkiaSharp.Views` package, which must be added separately.

### Full desktop example

A typical `MauiProgram.cs` for a desktop app:

```csharp
using Avalonia.Controls.Maui;
using Avalonia.Controls.Maui.Compatibility;
using Avalonia.Controls.Maui.Essentials;
using Avalonia.Controls.Maui.SkiaSharp.Views;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseAvaloniaApp()
            .UseAvaloniaCompatibility()
            .UseAvaloniaEssentials()
            .UseAvaloniaSkiaSharp();

        return builder.Build();
    }
}
```

For a WASM/Browser target, pass `useSingleViewLifetime: true`:

```csharp
builder
    .UseMauiApp<App>()
    .UseAvaloniaApp(useSingleViewLifetime: true)
    .UseAvaloniaEssentials();
```

## Register fonts

True type format (TTF) and open type font (OTF) fonts can be added to your app and referenced by filename or alias. Registration is performed in the `CreateMauiApp` method by invoking the `ConfigureFonts` method on the `MauiAppBuilder` object. Then, on the `IFontCollection` object, call the `AddFont` method to add each required font:

```csharp
builder
    .UseMauiApp<App>()
    .UseAvaloniaApp()
    .ConfigureFonts(fonts =>
    {
        fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
        fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        fonts.AddFont("ionicons.ttf", "Ionicons");
    });
```

The first argument to `AddFont` is the font filename. The second argument is an optional alias by which the font can be referenced when consuming it.

A font can be added to your project by placing it in the `Resources/Fonts` folder and setting its build action to `MauiFont` in the project file:

```xml
<ItemGroup>
  <MauiFont Include="Resources/Fonts/OpenSans-Regular.ttf" />
  <MauiFont Include="Resources/Fonts/OpenSans-Semibold.ttf" />
  <MauiFont Include="Resources/Fonts/ionicons.ttf" />
</ItemGroup>
```

Alternatively, use a wildcard to include all fonts in the folder at once:

```xml
<ItemGroup>
  <MauiFont Include="Resources/Fonts/*" />
</ItemGroup>
```

At build time, `MauiFont` items are automatically converted to Avalonia embedded resources and placed under `Assets/Fonts/` inside the assembly.

### Consume registered fonts

Registered fonts can be consumed by setting `FontFamily` to either:

- the alias passed to `AddFont`, or
- the exact registered filename (including extension).

Prefer aliases for stability.

```xml
<!-- Use the font alias -->
<Label Text="Hello" FontFamily="OpenSansRegular" />

<!-- Use the exact registered filename -->
<Label Text="Hello" FontFamily="OpenSans-Regular.ttf" />
```

The equivalent C# code is:

```csharp
// Use font alias
var label = new Label
{
    Text = "Hello",
    FontFamily = "OpenSansRegular"
};
```

## Register images and raw assets

### Images

Images added to your project with the `MauiImage` build action are automatically resized at build time and embedded as Avalonia resources under `Images/` in the assembly:

```xml
<ItemGroup>
  <MauiImage Include="Resources/Images/logo.png" />
  <MauiImage Include="Resources/Images/icon.png" />
</ItemGroup>
```

They can then be referenced in XAML by filename, the same way as in a standard .NET MAUI app:

```xml
<Image Source="logo.png" />
<Image Source="icon.png" />
```

### Raw assets

Files added with the `MauiAsset` build action are embedded as raw Avalonia resources. Add them to the project file as follows:

```xml
<ItemGroup>
  <MauiAsset Include="Resources/Raw/data.json" />
</ItemGroup>
```

They can be read at runtime using `FileSystem.OpenAppPackageFileAsync` from `Microsoft.Maui.Storage`:

```csharp
using var stream = await FileSystem.OpenAppPackageFileAsync("data.json");
using var reader = new StreamReader(stream);
var content = await reader.ReadToEndAsync();
```

> [!NOTE]
> `FileSystem.OpenAppPackageFileAsync` requires `UseAvaloniaEssentials` to be registered in `MauiProgram`.

Raw assets can also be accessed directly via the Avalonia asset loader using an `avares://` URI:

```csharp
var uri = new Uri("avares://YourAssemblyName/data.json");
using var stream = AssetLoader.Open(uri);
```

### Remote images

Remote images are supported through `UriImageSource` with optional disk caching. The `CacheValidity` property accepts a `TimeSpan` value:

```xml
<Image>
    <Image.Source>
        <UriImageSource Uri="https://example.com/image.png"
                        CachingEnabled="True"
                        CacheValidity="1.00:00:00" />
    </Image.Source>
</Image>
```

## Register custom handlers

Handlers map a .NET MAUI virtual view to a platform view rendered by Avalonia. You can register custom handlers to add support for your own controls or to replace the built-in handlers for existing .NET MAUI controls.

### Create a handler

A handler inherits from `ViewHandler<TVirtualView, TPlatformView>`. A property mapper defines which handler methods are invoked when properties on the virtual view change:

```csharp
using Avalonia.Controls;
using Microsoft.Maui.Handlers;

public class RatingViewHandler : ViewHandler<RatingView, StackPanel>
{
    public static IPropertyMapper<RatingView, RatingViewHandler> Mapper =
        new PropertyMapper<RatingView, RatingViewHandler>(ViewMapper)
        {
            [nameof(RatingView.Value)] = MapValue,
            [nameof(RatingView.Maximum)] = MapMaximum,
        };

    public RatingViewHandler() : base(Mapper) { }

    protected override StackPanel CreatePlatformView() => new StackPanel
    {
        Orientation = Orientation.Horizontal
    };

    private static void MapValue(RatingViewHandler handler, RatingView view)
    {
        // Update handler.PlatformView to reflect view.Value
    }

    private static void MapMaximum(RatingViewHandler handler, RatingView view)
    {
        // Update handler.PlatformView to reflect view.Maximum
    }
}
```

### Register a handler

Handlers are registered by calling `ConfigureMauiHandlers` on the `MauiAppBuilder` object:

```csharp
builder
    .UseMauiApp<App>()
    .UseAvaloniaApp()
    .ConfigureMauiHandlers(handlers =>
    {
        handlers.AddHandler<RatingView, RatingViewHandler>();
    });
```

To replace the Avalonia handler for a built-in .NET MAUI control, register your own handler for that control type. Registrations made via `ConfigureMauiHandlers` after `UseAvaloniaApp` take precedence over the defaults:

```csharp
builder
    .UseMauiApp<App>()
    .UseAvaloniaApp()
    .ConfigureMauiHandlers(handlers =>
    {
        // Replace the default Button handler
        handlers.AddHandler<Microsoft.Maui.Controls.Button, MyButtonHandler>();
    });
```

## Reference context

This document aligns implementation details with:

- https://avaloniaui.net/blog/avalonia-maui-progress-update
- https://avaloniaui.net/blog/net-maui-is-coming-to-linux-and-the-browser-powered-by-avalonia

Blog posts include roadmap and vision context. Source code in this repository is the implementation authority for current behavior.
