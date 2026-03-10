# Embedding Avalonia Controls in a MAUI Application

This guide explains how to use Avalonia controls inside a .NET MAUI application. There are two approaches:

1. **AvaloniaView**: host any existing Avalonia control directly in a MAUI page.
2. **Custom Handler**: create a MAUI `View` backed by an Avalonia control, with full property mapping.

Both approaches use the `UseAvaloniaEmbedding<TApp>()` extension method to initialize Avalonia within a native MAUI app.

---

## Prerequisites

- A .NET MAUI application targeting iOS, Android, Mac Catalyst, or Windows.
- The `Avalonia.Controls.Maui` NuGet package.

## Setup

### 1. Register Avalonia embedding in MauiProgram.cs

Call `UseAvaloniaEmbedding<TApp>()` in your MAUI app builder. This initializes the Avalonia runtime in embedding mode and registers the built-in `AvaloniaView` handler. If you are using the source-generated Avalonia Application, use `AvaloniaApp`.

```csharp
using Microsoft.Maui.Hosting;

namespace MyApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseAvaloniaEmbedding<AvaloniaApp>();

        return builder.Build();
    }
}
```

`UseAvaloniaEmbedding<TApp>()` accepts an optional `Action<Avalonia.AppBuilder>` callback if you need to configure Avalonia further (e.g., adding custom fonts or Skia options):

```csharp
.UseAvaloniaEmbedding<AvaloniaApp>(avaloniaBuilder =>
{
    avaloniaBuilder.WithInterFont();
})
```

---

## Approach 1: AvaloniaView (Direct Hosting)

Use `AvaloniaView` when you want to embed an existing Avalonia control directly in a MAUI page with minimal setup.

### Create an Avalonia control

```csharp
using Avalonia.Controls;

namespace MyApp.Controls;

public class EmbedDemoControl : UserControl
{
    public EmbedDemoControl()
    {
        Content = new StackPanel
        {
            Spacing = 16,
            Children =
            {
                new TextBlock
                {
                    Text = "Hello from Avalonia!",
                    FontSize = 28,
                },
                new Button
                {
                    Content = "Click Me - Avalonia Button",
                },
                new TextBox
                {
                    Watermark = "Type something here...",
                },
                new CheckBox
                {
                    Content = "An Avalonia CheckBox",
                },
                new Slider
                {
                    Minimum = 0,
                    Maximum = 100,
                    Value = 50,
                },
            },
        };
    }
}
```

### Use it in MAUI XAML

Add the `Avalonia.Controls.Maui.Controls` namespace and your control's namespace, then place the control inside an `<AvaloniaView>`:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="MyApp.Pages.DemoPage"
             xmlns:maui="clr-namespace:Avalonia.Controls.Maui.Controls;assembly=Avalonia.Controls.Maui"
             xmlns:controls="clr-namespace:MyApp.Controls"
             Title="Avalonia Embed Demo">
    <Grid Margin="10" RowDefinitions="Auto, *">
        <Label Text="MAUI label above an embedded Avalonia control:" />
        <maui:AvaloniaView Grid.Row="1"
            HorizontalOptions="Fill"
            VerticalOptions="Fill">
            <controls:EmbedDemoControl />
        </maui:AvaloniaView>
    </Grid>
</ContentPage>
```

`AvaloniaView` is a MAUI `View`, so it participates in MAUI layout and supports standard layout properties like `HorizontalOptions`, `VerticalOptions`, `HeightRequest`, `Margin`, etc.

---

## Approach 2: Custom Handler (MAUI View Backed by Avalonia)

Use this approach when you want to create a reusable MAUI control that exposes bindable properties, participates in MAUI data binding, and is rendered by an Avalonia control under the hood. This follows MAUI's standard handler architecture.

There are three pieces:

1. **MAUI View**: defines the public API (bindable properties).
2. **Avalonia Control**: implements the visual rendering.
3. **Handler**: bridges the two, mapping MAUI properties to the Avalonia control.

### Step 1: Define the MAUI View

Create a class that inherits from `Microsoft.Maui.Controls.View` with `BindableProperty` declarations:

```csharp
namespace MyApp.Views;

public class CounterView : Microsoft.Maui.Controls.View
{
    public static readonly BindableProperty CountProperty =
        BindableProperty.Create(nameof(Count), typeof(int), typeof(CounterView), 0);

    public int Count
    {
        get => (int)GetValue(CountProperty);
        set => SetValue(CountProperty, value);
    }
}
```

This view can be used in MAUI XAML and supports data binding like any other MAUI control.

### Step 2: Create the Avalonia Control

Create an Avalonia `UserControl` (or any `Control` subclass) with matching styled properties:

```csharp
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace MyApp.Controls;

public class CounterControl : UserControl
{
    public static readonly StyledProperty<int> CountProperty =
        AvaloniaProperty.Register<CounterControl, int>(nameof(Count));

    public int Count
    {
        get => GetValue(CountProperty);
        set => SetValue(CountProperty, value);
    }

    private readonly TextBlock _countText;

    public CounterControl()
    {
        _countText = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 24,
        };

        var incrementButton = new Avalonia.Controls.Button
        {
            Content = "Increment",
            HorizontalAlignment = HorizontalAlignment.Center,
        };

        incrementButton.Click += (_, _) => Count++;

        Content = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Spacing = 16,
            Children =
            {
                _countText,
                incrementButton,
            },
        };

        UpdateText();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == CountProperty)
        {
            UpdateText();
        }
    }

    private void UpdateText()
    {
        _countText.Text = $"Count: {Count}";
    }
}
```

### Step 3: Create the Handler

Inherit from `AvaloniaControlHandler<TVirtualView, TControl>` and set up a `PropertyMapper` that bridges MAUI properties to the Avalonia control:

```csharp
using Avalonia.Controls.Maui.Handlers;
using Microsoft.Maui;
using MyApp.Controls;
using MyApp.Views;

namespace MyApp.Handlers;

public class CounterViewHandler : AvaloniaControlHandler<CounterView, CounterControl>
{
    public static new readonly IPropertyMapper<CounterView, CounterViewHandler> Mapper =
        new PropertyMapper<CounterView, CounterViewHandler>(
            AvaloniaControlHandler<CounterView, CounterControl>.Mapper)
        {
            [nameof(CounterView.Count)] = MapCount,
        };

    public CounterViewHandler() : base(Mapper)
    {
    }

    public static void MapCount(CounterViewHandler handler, CounterView view)
    {
        if (handler.AvaloniaControl is not null)
        {
            handler.AvaloniaControl.Count = view.Count;
        }
    }
}
```

Notes:

- The `Mapper` chains from the base `AvaloniaControlHandler` mapper, which handles common view properties.
- Each `MapXxx` method reads from the MAUI view and writes to the Avalonia control.
- `handler.AvaloniaControl` gives you the Avalonia control instance.
- You can override `CreateAvaloniaControl()` if you need custom initialization beyond the default `new TControl()`.
- You can override `OnAvaloniaControlCreated(TControl)` and `OnAvaloniaControlDestroying(TControl?)` for setup/teardown logic.

### Step 4: Register the Handler

In `MauiProgram.cs`, register the handler alongside the embedding setup:

```csharp
public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    builder
        .UseMauiApp<App>()
        .UseAvaloniaEmbedding<AvaloniaApp>()
        .ConfigureMauiHandlers(handlers =>
        {
            handlers.AddHandler<CounterView, CounterViewHandler>();
        });

    return builder.Build();
}
```

### Step 5: Use in MAUI XAML

The custom view is now usable like any MAUI control, with full data binding support:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="MyApp.Pages.CounterPage"
             xmlns:views="clr-namespace:MyApp.Views"
             Title="Counter Page">
    <Grid Margin="10" RowDefinitions="Auto, *">
        <Label Text="A MAUI control rendered by Avalonia:" />
        <views:CounterView Grid.Row="1" Count="5" HeightRequest="200" />
    </Grid>
</ContentPage>
```

---

## Platform Considerations

`UseAvaloniaEmbedding<TApp>()` configures the correct Avalonia platform backend automatically based on the target:

| Platform | Avalonia Backend |
|---|---|
| Android | `Avalonia.Android` |
| iOS / Mac Catalyst | `Avalonia.iOS` |
| WinUI | Not supported yet for Embedding. |

For projects that target both native MAUI platforms (iOS, Android) and desktop platforms where Avalonia is the full backend, use conditional compilation:

```csharp
builder
    .UseMauiApp<App>()
// This allows for the standard target framework to operate with full Avalonia drawing.
#if !IOS && !MACCATALYST && !ANDROID && !WINDOWS
    .UseAvaloniaApp()
#else
// This uses .NET MAUI Native handlers, and adds Avalonia embedding support.
    .UseAvaloniaEmbedding<AvaloniaApp>()
#endif
```

---

## Which Approach to Use

| | AvaloniaView | Custom Handler |
|---|---|---|
| **Use when** | Embedding an existing Avalonia control as-is | Creating a reusable MAUI control backed by Avalonia |
| **MAUI data binding** | Not directly on the Avalonia control | Full support via BindableProperty |
| **MAUI XAML usage** | Wrapped in `<AvaloniaView>` | Used directly like any MAUI control |
| **Property mapping** | None needed | Explicit mapping in the handler |
| **Setup complexity** | Minimal | Requires View + Control + Handler |
