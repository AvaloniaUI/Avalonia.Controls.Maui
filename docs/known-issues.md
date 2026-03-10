# Known Issues

## 11.x-preview

- The following features are not implemented yet and will be addressed in future versions.
 - WebView and Blazor Hybrid support are currently not available.
 - Some Microsoft.Maui.Essentials APIs, such as [MainThread](https://github.com/dotnet/maui/blob/6c123d72970865ccb1312e118f5098ef6c44e892/src/Essentials/src/MainThread/MainThread.netstandard.cs), can not be directly overridden. User code or libraries that uses them will throw `NotSupportedOrImplementedException` exceptions, since it will flow to the .NET Standard implementation. To work around this while we implement new APIs, in user code, you can use alternative APIs (such as the .NET MAUI Dispatcher) that offer the same functions.
 - Avalonia components can be embedded into .NET MAUI Native UI apps, but native UI elements cannot be directly added to Avalonia.Controls.Maui applications.
 - WinUI support for embedding Avalonia and Avalonia.Controls.Maui views.
 - Accessibility support between the .NET MAUI and Avalonia is currently limited.
 - `VerticalTextAlignment` is not supported in Avalonia, so it is a no-op for Avalonia.Controls.Maui. Text using this property will appear aligned at the top.
 - `IWindowOverlay` is not supported.
 - Deploying Avalonia.Controls.Maui Applications to existing .NET MAUI UI platforms (iOS/Catalyst, Android, WinUI) is not supported. Support for these platforms are planned.
- `Microsoft.Maui.Maps` is not supported. Instead, you can use other .NET MAUI libraries such as [Mapsui.Maui](https://www.nuget.org/packages/Mapsui.Maui/) configured through the `Avalonia.Controls.Maui.SkiaSharp.Views` library. See the `MapApp` sample for more details.