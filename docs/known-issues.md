# Known Issues

## Current Limitations

- Blazor Hybrid is not yet implemented.
- Some `Microsoft.Maui.Essentials` APIs, such as [`MainThread`](https://github.com/dotnet/maui/blob/6c123d72970865ccb1312e118f5098ef6c44e892/src/Essentials/src/MainThread/MainThread.netstandard.cs), cannot be directly overridden. Code that calls them will fall through to the .NET Standard stub and throw `NotSupportedOrImplementedException`. Use alternative APIs such as the .NET MAUI `Dispatcher` as a workaround while support is being added.
- Avalonia controls can be embedded into .NET MAUI native UI apps, but native UI elements cannot be embedded directly into `Avalonia.Controls.Maui` apps.
- WinUI embedding of Avalonia and `Avalonia.Controls.Maui` views is not yet supported.
- Accessibility bridging between .NET MAUI and Avalonia is currently limited.
- `VerticalTextAlignment` is not supported in Avalonia and is a no-op. Text using this property will appear top-aligned.
- `IWindowOverlay` is not supported.
- Deploying to existing .NET MAUI native targets (iOS, Mac Catalyst, Android, WinUI) in full-hosting mode is not yet supported. These platforms are planned.
- `Microsoft.Maui.Maps` is not supported. As an alternative, use [Mapsui.Maui](https://www.nuget.org/packages/Mapsui.Maui/) configured through the `Avalonia.Controls.Maui.SkiaSharp.Views` package. See the `MapApp` sample for details.
