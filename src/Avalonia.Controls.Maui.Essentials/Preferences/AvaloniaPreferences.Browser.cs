

using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;

namespace Avalonia.Controls.Maui.Essentials;

public partial class AvaloniaPreferences
{
    const string LocalStorageKey = "__avalonia_maui_preferences";

    // Set while some values exist only in memory because saving failed, so a reload doesn't replace them.
    bool _unsaved;

    // Other tabs of the app may have saved since the last read, so every read and write starts from localStorage.
    partial void ReloadPreferences()
    {
        if (!_unsaved)
            LoadPreferences();
    }

    partial void LoadPreferences()
    {
        try
        {
            var json = LocalStorageInterop.GetItem(LocalStorageKey);
            if (string.IsNullOrEmpty(json))
                return;

            var readPreferences = JsonSerializer.Deserialize(json, AvaloniaPreferencesJsonSerializerContext.Default.PreferencesDictionary);

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
        catch (JSException)
        {
            // localStorage is unavailable, for example when the user blocks site data; keep preferences in memory
        }
    }

    partial void SavePreferences()
    {
        var json = JsonSerializer.Serialize(_preferences, AvaloniaPreferencesJsonSerializerContext.Default.PreferencesDictionary);
        try
        {
            LocalStorageInterop.SetItem(LocalStorageKey, json);
            _unsaved = false;
        }
        catch (JSException)
        {
            // localStorage is unavailable or full; keep preferences in memory
            _unsaved = true;
        }
    }
}
