

using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Maui.Storage;

using PreferencesDictionary = System.Collections.Concurrent.ConcurrentDictionary<string, System.Collections.Concurrent.ConcurrentDictionary<string, string>>;
using ShareNameDictionary = System.Collections.Concurrent.ConcurrentDictionary<string, string>;

namespace Avalonia.Controls.Maui.Essentials;

/// <summary>
/// Avalonia implementation of <see cref="IPreferences"/> that persists key/value preferences to a JSON file.
/// </summary>
/// <remarks>
/// This implementation mirrors MAUI's <c>UnpackagedPreferencesImplementation</c> on Windows,
/// using a <see cref="ConcurrentDictionary{TKey, TValue}"/> backed by a JSON file
/// at <c>{AppDataDirectory}/../Settings/preferences.dat</c>.
/// It is fully cross-platform and has no native dependencies.
/// </remarks>
public partial class AvaloniaPreferences : IPreferences
{
    readonly PreferencesDictionary _preferences = new();
    readonly object _locker = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaloniaPreferences"/> class,
    /// loading any previously persisted preferences from disk.
    /// </summary>
    public AvaloniaPreferences()
    {
        LoadPreferences();
        _preferences.GetOrAdd(string.Empty, _ => new ShareNameDictionary());
    }

    /// <summary>
    /// Checks for the existence of a given key.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <param name="sharedName">Shared container name.</param>
    /// <returns><see langword="true"/> if the key exists in the preferences; otherwise, <see langword="false"/>.</returns>
    public bool ContainsKey(string key, string? sharedName = null)
    {
        lock (_locker)
        {
            if (_preferences.TryGetValue(CleanSharedName(sharedName), out var inner))
            {
                return inner.ContainsKey(key);
            }

            return false;
        }
    }

    /// <summary>
    /// Removes a key and its associated value if it exists.
    /// </summary>
    /// <param name="key">The key to remove.</param>
    /// <param name="sharedName">Shared container name.</param>
    public void Remove(string key, string? sharedName = null)
    {
        lock (_locker)
        {
            if (_preferences.TryGetValue(CleanSharedName(sharedName), out var inner))
            {
                inner.TryRemove(key, out _);
                SavePreferences();
            }
        }
    }

    /// <summary>
    /// Clears all keys and values.
    /// </summary>
    /// <param name="sharedName">Shared container name.</param>
    public void Clear(string? sharedName = null)
    {
        lock (_locker)
        {
            if (_preferences.TryGetValue(CleanSharedName(sharedName), out var prefs))
            {
                prefs.Clear();
                SavePreferences();
            }
        }
    }

    /// <summary>
    /// Sets a value for a given key.
    /// </summary>
    /// <typeparam name="T">Type of the object that is stored in this preference.</typeparam>
    /// <param name="key">The key to set the value for.</param>
    /// <param name="value">Value to set.</param>
    /// <param name="sharedName">Shared container name.</param>
    public void Set<T>(string key, T value, string? sharedName = null)
    {
        Preferences.CheckIsSupportedType<T>();

        lock (_locker)
        {
            var prefs = _preferences.GetOrAdd(CleanSharedName(sharedName), _ => new ShareNameDictionary());

            if (value is null)
                prefs.TryRemove(key, out _);
            else if (value is DateTime dt)
                prefs[key] = string.Format(CultureInfo.InvariantCulture, "{0}", dt.ToBinary());
            else if (value is DateTimeOffset dto)
                prefs[key] = dto.ToString("O");
            else
                prefs[key] = string.Format(CultureInfo.InvariantCulture, "{0}", value);

            SavePreferences();
        }
    }

    /// <summary>
    /// Gets the value for a given key, or the default specified if the key does not exist.
    /// </summary>
    /// <typeparam name="T">The type of the object stored for this preference.</typeparam>
    /// <param name="key">The key to retrieve the value for.</param>
    /// <param name="defaultValue">The default value to return when no existing value for <paramref name="key"/> exists.</param>
    /// <param name="sharedName">Shared container name.</param>
    /// <returns>Value for the given key, or the value in <paramref name="defaultValue"/> if it does not exist.</returns>
    public T Get<T>(string key, T defaultValue, string? sharedName = null)
    {
        lock (_locker)
        {
            if (_preferences.TryGetValue(CleanSharedName(sharedName), out var inner))
            {
                if (inner.TryGetValue(key, out var value) && value is not null)
                {
                    if (defaultValue is DateTime)
                    {
                        if (long.TryParse(value, CultureInfo.InvariantCulture, out var longValue))
                            return (T)(object)DateTime.FromBinary(longValue);
                        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, out var datetimeValue))
                            return (T)(object)datetimeValue;
                    }
                    else if (defaultValue is DateTimeOffset)
                    {
                        if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, out var dateTimeOffset))
                        {
                            return (T)(object)dateTimeOffset;
                        }
                    }

                    try
                    {
                        return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                        // bad get, fall back to default
                    }
                }
            }

            return defaultValue;
        }
    }

    partial void LoadPreferences();

    partial void SavePreferences();

    static string CleanSharedName(string? sharedName) =>
        string.IsNullOrEmpty(sharedName) ? string.Empty : sharedName;
}

/// <summary>
/// Source-generated JSON serializer context for AOT-compatible serialization of preferences data.
/// </summary>
[JsonSerializable(typeof(PreferencesDictionary), TypeInfoPropertyName = nameof(PreferencesDictionary))]
internal partial class AvaloniaPreferencesJsonSerializerContext : JsonSerializerContext
{
}
