

using System.Reflection;
using System.Text.Json;

namespace Avalonia.Controls.Maui.Essentials;

public partial class AvaloniaPreferences
{
    static readonly string AppPreferencesPath = GetPreferencesPath();

    partial void LoadPreferences()
    {
        if (!File.Exists(AppPreferencesPath))
            return;

        try
        {
            using var stream = File.OpenRead(AppPreferencesPath);
            var readPreferences = JsonSerializer.Deserialize(stream, AvaloniaPreferencesJsonSerializerContext.Default.PreferencesDictionary);

            if (readPreferences != null)
            {
                _preferences.Clear();
                foreach (var pair in readPreferences)
                    _preferences.TryAdd(pair.Key, pair.Value);
            }
        }
        catch (JsonException)
        {
            // if deserialization fails proceed with empty settings
        }
    }

    partial void SavePreferences()
    {
        var dir = Path.GetDirectoryName(AppPreferencesPath);
        if (dir != null)
            Directory.CreateDirectory(dir);

        using var stream = File.Create(AppPreferencesPath);
        JsonSerializer.Serialize(stream, _preferences, AvaloniaPreferencesJsonSerializerContext.Default.PreferencesDictionary);
    }

    static string GetPreferencesPath()
    {
        var appName = Assembly.GetEntryAssembly()?.GetName().Name ?? "AvaloniaApp";
        var appDataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            appName);

        return Path.Combine(appDataDirectory, "Settings", "preferences.dat");
    }
}
