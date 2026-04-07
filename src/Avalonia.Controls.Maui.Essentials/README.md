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
| TextToSpeech | `Microsoft.Maui.Media` | Implemented | Desktop/Linux uses `espeak-ng` (primary) or `spd-say` (fallback). Browser uses the Web Speech API. Supports locale selection, pitch, rate, and volume. |

## Usage Examples

### Screenshot

```csharp
using Microsoft.Maui.Media;

if (Screenshot.Default.IsCaptureSupported)
{
    var result = await Screenshot.Default.CaptureAsync();
    using var stream = await result.OpenReadAsync(ScreenshotFormat.Png);
    // Use the stream (e.g., save to file, display in Image control)
}
```

### FilePicker

```csharp
using Microsoft.Maui.Storage;

var result = await FilePicker.Default.PickAsync(new PickOptions
{
    PickerTitle = "Select a file",
    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
    {
        { DevicePlatform.WinUI, new[] { ".png", ".jpg" } }
    })
});

if (result != null)
{
    var filePath = result.FullPath;
}
```

### MediaPicker

```csharp
using Microsoft.Maui.Media;

// Pick photos (capture is not supported on desktop)
var photo = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
{
    Title = "Select a photo"
});

if (photo != null)
{
    using var stream = await photo.OpenReadAsync();
}
```

### FileSystem

```csharp
using Microsoft.Maui.Storage;

// Access app directories
var cacheDir = FileSystem.Current.CacheDirectory;
var dataDir = FileSystem.Current.AppDataDirectory;

// Read an embedded app package file
if (await FileSystem.Current.AppPackageFileExistsAsync("assets/data.json"))
{
    using var stream = await FileSystem.Current.OpenAppPackageFileAsync("assets/data.json");
    // Read from stream
}
```

### HapticFeedback

```csharp
using Microsoft.Maui.Devices;

// Always check IsSupported first (returns false on desktop)
if (HapticFeedback.Default.IsSupported)
{
    HapticFeedback.Default.Perform(HapticFeedbackType.Click);
}
```

### Preferences

```csharp
using Microsoft.Maui.Storage;

// Set and get typed values
Preferences.Default.Set("username", "John");
Preferences.Default.Set("login_count", 42);
Preferences.Default.Set("last_login", DateTime.UtcNow);

var name = Preferences.Default.Get("username", "Unknown");
var count = Preferences.Default.Get("login_count", 0);

// Use shared containers for namespacing
Preferences.Default.Set("theme", "dark", "ui_settings");
var theme = Preferences.Default.Get("theme", "light", "ui_settings");
```

### TextToSpeech

```csharp
using Microsoft.Maui.Media;

// Get available locales
var locales = await TextToSpeech.Default.GetLocalesAsync();

// Speak with options
await TextToSpeech.Default.SpeakAsync("Hello, world!", new SpeechOptions
{
    Pitch = 1.0f,
    Rate = 1.0f,
    Volume = 0.75f,
    Locale = locales.FirstOrDefault()
});

// Cancel speech
var cts = new CancellationTokenSource();
var speakTask = TextToSpeech.Default.SpeakAsync("Long text...", cancelToken: cts.Token);
cts.Cancel(); // Stops speech immediately
```

> **Note**: On Linux, requires `espeak-ng` or `speech-dispatcher` installed. Install with `sudo apt install espeak-ng` (Debian/Ubuntu) or `sudo dnf install espeak-ng` (Fedora).

## Not Implemented APIs

For APIs that have not been implemented in Microsoft.Maui.Essentials APIs, they will fall back to the default .NET MAUI standard implementations. These will most likely throw `NotImplementedException` as most of them are platform specific implementations. We are adding more support for the areas we can cover.
