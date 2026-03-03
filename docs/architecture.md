# Architecture

`Avalonia.Controls.Maui` replaces the .NET MAUI native rendering pipeline with Avalonia. Rather than drawing controls through platform-native APIs, every control is drawn by Avalonia using its own rendering engine, which runs consistently across all supported platforms.

This document describes the design principles, package structure, integration model, handler system, rendering pipeline, lifecycle integration, font and image services, and build-time asset processing.

## Design principles

The library is built on a small set of explicit constraints that shape every implementation decision.

**No fork.** The library works against the official .NET MAUI NuGet packages. It does not maintain a fork of .NET MAUI.

**No new target frameworks.** Apps do not add a new `<TargetFramework>` to use Avalonia rendering. The existing .NET MAUI target frameworks (`net-ios`, `net-android`, `net-maccatalyst`, `net-windows`) are used as-is, and Linux and browser targets use the same mechanism.

**No type swizzling or runtime patching.** Integration happens exclusively through .NET MAUI's public handler registration and dependency injection APIs.

**Single target.** Because all platform views are Avalonia controls, the library maintains one handler implementation for all platforms rather than separate implementations for Android, iOS, Windows, macOS, Linux, and WebAssembly. This makes the codebase significantly easier to maintain and new platform support straightforward to add.

**Upstream collaboration.** The team works closely with Microsoft's .NET MAUI engineers to identify and contribute the upstream changes that make this integration possible.

## Package structure

The library is distributed as a set of NuGet packages, each covering a distinct area of functionality:

| Package | Purpose |
|---|---|
| `Avalonia.Controls.Maui` | Core handlers for all built-in .NET MAUI controls |
| `Avalonia.Controls.Maui.Graphics` | Handler for `GraphicsView` and `Microsoft.Maui.Graphics` |
| `Avalonia.Controls.Maui.Essentials` | Avalonia-based implementations of `Microsoft.Maui.Essentials` APIs |
| `Avalonia.Controls.Maui.Compatibility` | Handlers for deprecated .NET MAUI controls (`Frame`, `ListView`, `TableView`) |
| `Avalonia.Controls.Maui.Maps.Mapsui` | Map control powered by Mapsui |
| `Avalonia.Controls.Maui.Desktop` | Metapackage that references the core package and runs source generation for desktop bootstrapping |

## Platform support

.NET MAUI natively targets Android, iOS, Mac Catalyst, and Windows. `Avalonia.Controls.Maui` extends this to Linux and WebAssembly, because Avalonia already runs on those platforms and all handler implementations are platform-agnostic.

| Platform | .NET MAUI built-in | Avalonia.Controls.Maui |
|---|---|---|
| Android | Yes | Yes |
| iOS | Yes | Yes |
| Mac Catalyst | Yes | Yes |
| Windows | Yes | Yes |
| Linux | No | Yes |
| WebAssembly | No | Yes |

## Integration model

.NET MAUI separates the definition of a control from its rendering. The virtual view (for example, `IButton`) defines the control's properties and behavior. The handler (for example, `ButtonHandler`) maps those properties onto a platform view, which is the actual object that gets rendered.

`Avalonia.Controls.Maui` provides its own handler implementations. Instead of creating a native `UIButton` on iOS or an `android.widget.Button` on Android, these handlers create Avalonia controls rendered through the Avalonia pipeline. The .NET MAUI binding system, XAML, styles, navigation, and Shell architecture are not replaced.

```
┌─────────────────────────────────────┐
│         .NET MAUI layer             │
│  XAML · Bindings · Navigation       │
│  Virtual views (IButton, ILabel...) │
└──────────────┬──────────────────────┘
               │ handler abstraction
┌──────────────▼──────────────────────┐
│    Avalonia.Controls.Maui layer     │
│  Handler implementations            │
│  Font · Image · Asset services      │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│         Avalonia layer              │
│  Controls · Layout · Rendering      │
│  Skia / Impeller                    │
└─────────────────────────────────────┘
```

## Handler system

### Property mapper and command mapper

Every handler exposes two static mappers that define how the virtual view is translated to the platform view.

The `PropertyMapper` maps property names to static methods. When a property on the virtual view changes, the corresponding method is called with the handler and the virtual view as arguments. The method reads from the virtual view and writes to `handler.PlatformView`.

The `CommandMapper` maps command names to static methods that are invoked when the virtual view dispatches a command, such as a focus request or a scroll operation.

```csharp
public partial class ButtonHandler : ViewHandler<IButton, MauiButton>
{
    public static IPropertyMapper<IButton, ButtonHandler> Mapper =
        new PropertyMapper<IButton, ButtonHandler>(ViewHandler.ViewMapper)
        {
            [nameof(IText.Text)]         = MapText,
            [nameof(ITextStyle.Font)]    = MapFont,
            [nameof(IButton.Background)] = MapBackground,
        };

    public static CommandMapper<IButton, ButtonHandler> CommandMapper =
        new(ViewCommandMapper);
}
```

Property mapper methods are static and follow a consistent signature:

```csharp
public static void MapText(ButtonHandler handler, IButton button)
{
    handler.PlatformView.UpdateText(button);
}
```

### Generic base handler

Most handlers inherit from `AvaloniaControlHandler<TVirtualView, TControl>`, a generic base class that manages the lifecycle of the Avalonia control: creation, attachment to the host view, and cleanup. Platform-specific behavior is provided via partial class files (`.Android.cs`, `.iOS.cs`, `.Windows.cs`) that override how the host view is created and how the Avalonia control is attached to it.

```csharp
public partial class AvaloniaControlHandler<TVirtualView, TControl>
    where TVirtualView : class, IView
    where TControl : Avalonia.Controls.Control, new()
```

Handlers that require more control, such as `ButtonHandler` or `EntryHandler`, inherit directly from .NET MAUI's `ViewHandler<TVirtualView, TPlatformView>` and manage the Avalonia control themselves.

### Handler registration

All built-in handlers are registered in `UseAvaloniaApp` via `ConfigureMauiHandlers`. Handlers registered after `UseAvaloniaApp` take precedence, which is how custom handlers and overrides work:

```csharp
builder
    .UseAvaloniaApp()
    .ConfigureMauiHandlers(handlers =>
    {
        handlers.AddHandler<MyControl, MyControlHandler>();
    });
```

## Rendering pipeline

### Application bootstrap

`MauiAvaloniaApplication` is the Avalonia `Application` subclass that hosts the .NET MAUI app. It implements `IPlatformApplication` to bridge .NET MAUI's dependency injection container with Avalonia's application lifetime.

On startup, `OnFrameworkInitializationCompleted` calls the app's `CreateMauiApp` method, registers the Avalonia application instance with the .NET MAUI service container, and then asks the .NET MAUI `IApplication` to create its first window.

### Window structure

`MauiAvaloniaWindow` is the root Avalonia window. Its content is structured as a `Grid` that allows modal overlays to sit above the main content:

```
Grid (root)
├── DockPanel
│   ├── TitleBar (docked top, optional)
│   └── Main content (fills remaining space)
└── Modal overlay stack
```

The methods `PresentModal` and `DismissModal` manage the overlay stack, including animated transitions. `SetTitleBar` configures the title bar with drag-region support.

### Rendering engine

Avalonia currently renders using Skia. The team is working on a migration to Impeller, the GPU-first renderer developed by Google for Flutter. A .NET binding layer called NImpeller wraps Impeller's C API for use from .NET. When complete, this transition is expected to deliver smoother frame rates, lower VRAM usage, and improved performance on macOS compared to the Mac Catalyst approach.

> [!NOTE]
> The Impeller migration is in progress. Current builds use Skia. No changes to app code are required when the renderer transitions.

## Embedding mode

In addition to the full hosting mode, `Avalonia.Controls.Maui` supports an embedding mode where Avalonia content is placed inside a .NET MAUI app. In this mode, `UseAvaloniaEmbedding<TApp>` is called instead of `UseAvaloniaApp`, and only the `AvaloniaView` control and its handler are registered. This is useful for incrementally adopting Avalonia rendering in an existing .NET MAUI project.

```xml
<!-- Inside a .NET MAUI page -->
<AvaloniaView>
    <local:MyAvaloniaControl />
</AvaloniaView>
```

> [!IMPORTANT]
> Embedding mode and full hosting mode are mutually exclusive. A single app uses one or the other.

## Lifecycle integration

### Dispatcher

.NET MAUI schedules work on the UI thread through `IDispatcherProvider`. `Avalonia.Controls.Maui` replaces this with `AvaloniaDispatcherProvider`, which wraps `Avalonia.Threading.Dispatcher.UIThread` in .NET MAUI's `IDispatcher` interface. This ensures that all UI work dispatched by .NET MAUI flows through Avalonia's threading model.

### Animation ticker

.NET MAUI drives animations through `ITicker`. The default implementation uses platform timers. `Avalonia.Controls.Maui` replaces it with `AvaloniaTicker`, which hooks into Avalonia's rendering loop. This synchronizes .NET MAUI animations with Avalonia's frame pipeline.

### Theme changes

`MauiAvaloniaApplication` listens to Avalonia's `ActualThemeVariantChanged` event and calls `Application.ThemeChanged()` on the .NET MAUI application when it fires. This keeps .NET MAUI's theme system in sync with the Avalonia theme.

### Lifecycle events

`Avalonia.Controls.Maui` defines a set of custom lifecycle events through `AvaloniaLifecycle` that can be subscribed to via `ConfigureLifecycleEvents`:

| Event | When it fires |
|---|---|
| `OnLaunching` | Before the .NET MAUI app is created |
| `OnLaunched` | After the .NET MAUI app is created |
| `OnMauiContextCreated` | After the window-scoped DI container is created |
| `OnWindowCreated` | After the window handler is created |

```csharp
builder.ConfigureLifecycleEvents(events =>
{
    events.AddAvalonia(avalonia =>
    {
        avalonia.OnLaunched((app, args) => Console.WriteLine("Launched"));
    });
});
```

## Font system

.NET MAUI's font APIs flow through `IFontRegistrar` and `IFontManager`. `Avalonia.Controls.Maui` replaces both with Avalonia-aware implementations that translate .NET MAUI font registrations into URIs that Avalonia's font pipeline understands.

`AvaloniaMauiFontRegistrar` stores each font by filename and alias. When a control requests a font family by name, the registrar opens the font file from the embedded Avalonia resource, parses the `name` table in the TTF/OTF binary to extract the font family name, and returns an `avares://` URI in the format Avalonia expects:

```
avares://AssemblyName/Assets/Fonts/OpenSans-Regular.ttf#Open Sans
```

Because the URI includes the font family name extracted from the file itself, the font renders correctly regardless of the filename or alias used at registration.

## Image source services

.NET MAUI resolves images through `IImageSourceServiceProvider`, which selects the appropriate `IImageSourceService` implementation based on the type of `IImageSource`. `Avalonia.Controls.Maui` registers four services that each return an Avalonia `Bitmap` rather than a platform-native image type:

| Image source type | Service |
|---|---|
| `IFileImageSource` | `AvaloniaFileImageSourceService` |
| `IUriImageSource` | `AvaloniaUriImageSourceService` |
| `IFontImageSource` | `AvaloniaFontImageSourceService` |
| `IStreamImageSource` | `AvaloniaStreamImageSourceService` |

### File image resolution

`AvaloniaFileImageSourceService` first attempts to resolve the filename as an Avalonia embedded resource using the `avares://` URI scheme. If that fails, it falls back to a direct filesystem read. This two-step approach means that images declared as `MauiImage` in the project file, which are embedded as Avalonia resources at build time, are resolved the same way as images loaded from disk.

### URI image caching

`AvaloniaUriImageSourceService` downloads remote images and caches them to disk using an SHA-256 hash of the URI as the cache key. In-flight requests for the same URI are coalesced so that only one download occurs even if multiple controls request the same image simultaneously. Cache lifetime is controlled by the `CacheValidity` property on `UriImageSource`.

## Build-time asset processing

The `Avalonia.Controls.Maui.targets` file runs three MSBuild targets that convert .NET MAUI resource items into Avalonia embedded resources before compilation.

### Fonts

`ConvertMauiFontsToAvaloniaResources` converts each `MauiFont` item into an `AvaloniaResource` item with a link path of `Assets/Fonts/<filename>`. After this target runs, fonts are accessible at `avares://AssemblyName/Assets/Fonts/<filename>`.

### Raw assets

`ConvertMauiAssetsToAvaloniaResources` converts each `MauiAsset` item into an `AvaloniaResource` item, preserving the logical name. The assets are then accessible via `FileSystem.OpenAppPackageFileAsync` or directly via the `avares://` URI scheme.

### Images

`ConvertMauiImagesToAvaloniaResources` runs after Resizetizer has processed the declared `MauiImage` items. It selects the appropriate resolution output for each platform (for example, `drawable-mdpi` on Android and `scale-100` on Windows) and converts those files into `AvaloniaResource` items with a link path of `Images/<filename>`. The result is a single embedded image per platform at the base resolution, resolved at runtime by `AvaloniaFileImageSourceService`.

The build order is:

```
ResizetizeImages
    ↓
ConvertMauiImagesToAvaloniaResources
    ↓
AddAvaloniaResources
    ↓
CoreCompile
```
