# Avalonia.Controls.Maui.Essentials

Avalonia.Controls.Maui.Essentials provides Avalonia-based implementations of [Microsoft.Maui.Essentials](https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/) APIs. Internally, it generally uses Avalonia APIs for accessing services, unless they don't exist or are specific to platforms where that would not be applicable.

## Setup

Call `UseAvaloniaEssentials()` on your `MauiAppBuilder`:

```csharp
var builder = MauiApp.CreateBuilder();
builder
    .UseMauiApp<App>()
    .UseAvaloniaApp()
    .UseAvaloniaEssentials();
```

## Implemented APIs

| API | Namespace | Status | Notes |
|-----|-----------|--------|-------|
| Screenshot | `Microsoft.Maui.Media` | Implemented | Full support. **Note**: Always outputs PNG regardless of requested format. |
| FilePicker | `Microsoft.Maui.Storage` | Implemented | Full support for single and multi-file selection. MIME types and UTIs are converted to glob patterns where possible. |
| MediaPicker | `Microsoft.Maui.Media` | Partial | Photo and video **picking** is supported. Camera capture (`CapturePhotoAsync`/`CaptureVideoAsync`) throws `NotSupportedException`. |
| FileSystem | `Microsoft.Maui.Storage` | Implemented | `CacheDirectory`, `AppDataDirectory`, and app package file access via Avalonia's `avares://` asset loader. |
| HapticFeedback | `Microsoft.Maui.Devices` | Stub | `IsSupported` returns `false`. `Perform()` is a no-op. Haptic feedback is not available on desktop platforms. |
| Preferences | `Microsoft.Maui.Storage` | Implemented | Full support for all types (`string`, `int`, `bool`, `long`, `double`, `float`, `DateTime`, `DateTimeOffset`). Persists to a JSON file. Supports shared containers. |

## Not Implemented APIs

For APIs that have not been implemented in Microsoft.Maui.Essentials APIs, they will fall back to the default .NET MAUI standard implementations. These will most likely throw `NotImplementedException` as most of them are platform specific implementations. We are adding more support for the areas we can cover.
