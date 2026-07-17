using Avalonia.Controls.Maui.Essentials;
using Microsoft.Maui.Authentication;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;

namespace Avalonia.Controls.Maui.Tests.Services;

public class UseAvaloniaEssentialsTests
{
    [Fact]
    public void UseAvaloniaEssentials_Installs_Avalonia_Implementations_Into_Static_Facades()
    {
        var builder = MauiApp.CreateBuilder(useDefaults: false);

        builder.UseAvaloniaEssentials();

        Assert.IsType<AvaloniaScreenshot>(Screenshot.Default);
        Assert.IsType<AvaloniaFilePicker>(FilePicker.Default);
        Assert.IsType<AvaloniaMediaPicker>(MediaPicker.Default);
        Assert.IsType<AvaloniaHapticFeedback>(HapticFeedback.Default);
        Assert.IsType<AvaloniaPreferences>(Preferences.Default);
        Assert.IsType<AvaloniaFileSystem>(FileSystem.Current);
        Assert.IsType<AvaloniaWebAuthenticator>(WebAuthenticator.Default);
    }
}
