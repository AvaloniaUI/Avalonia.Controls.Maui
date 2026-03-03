# Avalonia.Controls.Maui

Avalonia.Controls.Maui replaces .NET MAUI's native platform controls with Avalonia-drawn controls. This enables MAUI apps to run on platforms that MAUI doesn't natively support, such as Linux and WASM, as well as platforms Avalonia supports, such as macOS AppKit. All while maintaining a consistent look across all platforms.

## Getting Started

### Prerequisites

- [.NET 11 SDK](https://dotnet.microsoft.com/download)
- A .NET MAUI workload installed (`dotnet workload install maui`)
- If using WASM, install `wasm-tools` (`dotnet workload install wasm-tools`)

### Quick Start

The fastest way to get started is with a MAUI single-project app using the `Avalonia.Controls.Maui.Desktop` package.

For other cases, refer to our [docs](https://docs.avaloniaui.net).

**1. Include the standard TFM and NuGet packages to your `.csproj`:**

```xml
<PropertyGroup>
    <!-- Include the net11.0 TFM. -->
    <TargetFrameworks>net11.0;net11.0-android;net11.0-ios;net11.0-maccatalyst</TargetFrameworks>
    <TargetFrameworks Condition="$([MSBuild]::IsOSPlatform('windows'))">
        $(TargetFrameworks);net11.0-windows10.0.19041.0</TargetFrameworks>
    <OutputType>Exe</OutputType>
    <UseMaui>true</UseMaui>
    <SingleProject>true</SingleProject>
</PropertyGroup>

<ItemGroup>
    <PackageReference Include="Microsoft.Maui.Controls" Version="$(MauiVersion)" />
    <PackageReference Include="Avalonia.Controls.Maui" Version="..." />
    <!-- 
     Avalonia.Controls.Maui.Desktop includes Avalonia.Desktop, 
     and will automatically set up the required Avalonia Application
     and themes.
     -->
    <PackageReference Include="Avalonia.Controls.Maui.Desktop"
                      Condition="'$(TargetFramework)' == 'net11.0'" Version="..." />
</ItemGroup>
```

If you include the `Avalonia.Controls.Maui.Desktop` package, it will automatically create the Avalonia bootstrap code through its source generator. No manual Avalonia `Program.cs` or `AppBuilder` setup is needed for desktop.

**2. Call `UseAvaloniaApp()` in your `MauiProgram.cs`:**

```csharp
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
// `UseAvaloniaApp()` replaces all MAUI controls with Avalonia-drawn equivalents.
#if !IOS && !MACCATALYST && !ANDROID && !WINDOWS
            .UseAvaloniaApp()
#else
// `UseAvaloniaEmbedding<AvaloniaApp>()` lets you embed Avalonia views inside the native MAUI shell.
            .UseAvaloniaEmbedding<AvaloniaApp>()
#endif
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        return builder.Build();
    }
}
```

That's it — run with `dotnet run` and your MAUI app renders with Avalonia.

### WASM / Browser

Because the WASM target framework requires a different SDK, we can't reference it inside of the .NET MAUI Single Project application. Instead, we can create a separate browser project that references your MAUI library.

**1. Create a browser `.csproj`:**

```xml
<Project Sdk="Microsoft.NET.Sdk.WebAssembly">
    <PropertyGroup>
        <TargetFramework>net11.0-browser</TargetFramework>
        <OutputType>Exe</OutputType>
        <UseMaui>true</UseMaui>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="Avalonia.Controls.Maui" Version="..." />
        <PackageReference Include="Avalonia.Browser" Version="..." />
        <PackageReference Include="Avalonia.Themes.Fluent" Version="..." />
        <PackageReference Include="Avalonia.Fonts.Inter" Version="..." />
        <PackageReference Include="Microsoft.Maui.Controls" Version="$(MauiVersion)" />
    </ItemGroup>

    <ItemGroup>
        <ProjectReference Include="../YourMauiApp/YourMauiApp.csproj" />
    </ItemGroup>
</Project>
```

Your MAUI code can live in a class library referenced by both the single-project app and the browser project. Alternatively, you can keep a single-project app and disable `OutputType Exe` for the `net11.0-browser` TFM.

**2. Add a `MauiProgram.cs` with `useSingleViewLifetime: true`:**

```csharp
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<MauiAppStub>()
// Hint: useSingleViewLifetime also applies to Mobile platforms, like iOS/Android, and Mac Catalyst.
#if BROWSER
            .UseAvaloniaApp(useSingleViewLifetime: true)
#endif
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                ...
            });

        return builder.Build();
    }
}
```

Browser apps use `useSingleViewLifetime: true` since WebAssembly runs in a single-view context.

## License

This project is licensed under the [MIT License](https://github.com/AvaloniaUI/Avalonia.Controls.Maui/blob/main/LICENSE).
