

using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;

namespace Avalonia.Controls.Maui.Essentials;

public partial class AvaloniaPreferences
{
    const string LocalStorageKey = "__avalonia_maui_preferences";

    // Changes on every save, so reads can skip reloading when nothing changed.
    const string VersionKey = "__avalonia_maui_preferences_version";

    // Set when localStorage fails, so reloads don't drop values kept only in memory.
    bool _memoryOnly;

    string? _lastVersion;

    partial void ReloadPreferences()
    {
        if (!_memoryOnly)
            Load(force: false);
    }

    partial void LoadPreferences() => Load(force: true);

    void Load(bool force)
    {
        try
        {
            // Read the version first, since saves write it last.
            var version = LocalStorageInterop.GetItem(VersionKey);
            if (!force && version == _lastVersion)
                return;

            _lastVersion = version;

            var json = LocalStorageInterop.GetItem(LocalStorageKey);
            var readPreferences = string.IsNullOrEmpty(json)
                ? null
                : JsonSerializer.Deserialize(json, AvaloniaPreferencesJsonSerializerContext.Default.PreferencesDictionary);

            // Clear even if the key is missing, so cleared site data stays cleared.
            _preferences.Clear();
            if (readPreferences != null)
            {
                foreach (var pair in readPreferences)
                    _preferences.TryAdd(pair.Key, pair.Value);
            }
        }
        catch (JsonException)
        {
            // if deserialization fails proceed with empty settings
        }
        catch (JSException)
        {
            // localStorage is blocked, so keep preferences in memory
            _memoryOnly = true;
        }
    }

    partial void SavePreferences()
    {
        var json = JsonSerializer.Serialize(_preferences, AvaloniaPreferencesJsonSerializerContext.Default.PreferencesDictionary);
        try
        {
            LocalStorageInterop.SetItem(LocalStorageKey, json);

            var version = Guid.NewGuid().ToString("N");
            LocalStorageInterop.SetItem(VersionKey, version);
            _lastVersion = version;
            _memoryOnly = false;
        }
        catch (JSException)
        {
            // localStorage is blocked or full, so keep preferences in memory
            _memoryOnly = true;
        }
    }
}
