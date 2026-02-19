# Avalonia.Controls.Maui

Avalonia.Controls.Maui replaces .NET MAUI's native platform controls with Avalonia-drawn controls. This enables MAUI apps to run on platforms that MAUI doesn't natively support, such as Linux and WASM, while maintaining a consistent look across all platforms.

## Getting Started

Register the Avalonia handlers in your `MauiProgram.cs`:

```csharp
using Avalonia.Controls.Maui;

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

If your application uses a single-view lifetime (e.g. for WASM), pass `useSingleViewLifetime: true`:

```csharp
builder.UseAvaloniaApp(useSingleViewLifetime: true);
```

## Application Setup

Currently, Avalonia.Controls.Maui does not support Single Project MAUI applications; Your MAUI page/view code lives in a shared project, while each platform head configures Avalonia and bootstraps the MAUI app.

### Shared MAUI Project

Your MAUI `App`, pages, and views live in a shared class library. This library does not have to reference platform specific cod (Although it can support multiple target frameworks), nor does it need to reference Avalonia.Controls.Maui, it only needs to reference the core Microsoft.Maui.Controls libraries.

### Avalonia Projects

To create an Avalonia project and set up the templates, refer to our [Getting Started](https://docs.avaloniaui.net/docs/get-started/) docs.

**App.axaml.cs** — Inherit from `MauiAvaloniaApplication` and override `CreateMauiApp`:

**NOTE**: The Avalonia Application is not required to be an `axaml` document, you can create this from code-behind if you wish, but the templates default to it.

```csharp
using Avalonia.Markup.Xaml;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui.Hosting;

public class App : MauiAvaloniaApplication
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
```

## Supported Controls

We are working on implementing more Avalonia-based controls for .NET MAUI. Currently, we have such controls as:

- **Input**: Button, ImageButton, Entry, Editor, SearchBar, Picker, DatePicker, TimePicker, Stepper, Slider, CheckBox, RadioButton, Switch
- **Display**: Label, Image, ProgressBar, ActivityIndicator, Border, BoxView
- **Layout**: ContentView, ContentPresenter, ScrollView, Layout
- **Collections**: CollectionView, CarouselView, IndicatorView, RefreshView, SwipeView
- **Navigation**: NavigationPage, TabbedPage, FlyoutPage, Shell
- **Shapes**: Line, Rectangle, Ellipse, Polygon, Polyline, Path, RoundRectangle
- **Menus**: MenuBar, MenuFlyout, MenuFlyoutItem, MenuFlyoutSubItem, Toolbar

Also check out [Avalonia.Controls.Maui.Compatibility](https://www.nuget.org/packages/Avalonia.Controls.Maui.Compatibility) and [Avalonia.Controls.Maui.Graphics](https://www.nuget.org/packages/Avalonia.Controls.Maui.Graphics).

## License

This project is licensed under the [MIT License](https://github.com/AvaloniaUI/Avalonia.Controls.Maui/blob/main/LICENSE).
