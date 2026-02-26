# Avalonia.Controls.Maui.Desktop

This package provides Avalonia.Controls.Maui support for .NET MAUI Single Project apps, adding Avalonia.Desktop support to apps that implement the single project build system.

If you wish to build for Avalonia.Browser (WASM), or the other Avalonia target types, refer to the Avalonia.Controls.Maui documentation.

## Usage

Reference this package in your .NET MAUI project:

```xml
<PackageReference Include="Avalonia.Controls.Maui.Desktop" Version="..." />
```

And update the target frameworks to include the standard .NET version.

```xml
<PropertyGroup>
		<!-- Add net11.0... -->
		<TargetFrameworks>net11.0;net11.0-android</TargetFrameworks>
		<TargetFrameworks Condition="!$([MSBuild]::IsOSPlatform('linux'))">
...
```

The source generator will automatically create a `Program.Main` entry point for your desktop app. You can control its behavior with MSBuild properties:

| Property | Default | Description |
|----------|---------|-------------|
| `AvaloniaControlsMauiGenerateBootstrap` | `true` | Generate the desktop bootstrap entry point |
| `AvaloniaControlsMauiTheme` | `Fluent` | Theme to use |
| `AvaloniaControlsMauiThemeVariant` | `Default` | Theme variant (Default, Light, Dark) |
| `AvaloniaControlsMauiIncludeInterFont` | `true` | Include Inter font family |

## License

This project is licensed under the [MIT License](https://github.com/AvaloniaUI/Avalonia.Controls.Maui/blob/main/LICENSE).
