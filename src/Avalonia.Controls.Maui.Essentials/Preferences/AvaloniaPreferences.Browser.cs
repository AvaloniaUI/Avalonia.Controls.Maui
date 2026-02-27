// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;

namespace Avalonia.Controls.Maui.Essentials;

public partial class AvaloniaPreferences
{
    const string LocalStorageKey = "__avalonia_maui_preferences";

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
    }

    partial void SavePreferences()
    {
        var json = JsonSerializer.Serialize(_preferences, AvaloniaPreferencesJsonSerializerContext.Default.PreferencesDictionary);
        LocalStorageInterop.SetItem(LocalStorageKey, json);
    }
}
