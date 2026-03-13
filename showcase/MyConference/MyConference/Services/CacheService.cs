using System.Text.Json;

namespace MyConference.Services;

public class CacheService : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static string GetDataPath(string key) =>
        Path.Combine(FileSystem.AppDataDirectory, $"cache_{key}.json");

    private static string GetTimestampPath(string key) =>
        Path.Combine(FileSystem.AppDataDirectory, $"cache_{key}_timestamp.txt");

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            var path = GetDataPath(key);
            if (!File.Exists(path))
                return null;

            var json = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T data) where T : class
    {
        try
        {
            var json = JsonSerializer.Serialize(data, JsonOptions);
            await File.WriteAllTextAsync(GetDataPath(key), json);
            await File.WriteAllTextAsync(GetTimestampPath(key), DateTime.UtcNow.ToString("O"));
        }
        catch (Exception)
        {
            // Silently fail on write errors
        }
    }

    public bool IsFresh(string key)
    {
        try
        {
            var path = GetTimestampPath(key);
            if (!File.Exists(path))
                return false;

            var text = File.ReadAllText(path);
            if (!DateTime.TryParse(text, null, System.Globalization.DateTimeStyles.RoundtripKind, out var timestamp))
                return false;

            return (DateTime.UtcNow - timestamp).TotalMinutes < EventConfig.CacheExpirationMinutes;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public Task ClearAsync(string key)
    {
        try
        {
            var dataPath = GetDataPath(key);
            if (File.Exists(dataPath))
                File.Delete(dataPath);

            var timestampPath = GetTimestampPath(key);
            if (File.Exists(timestampPath))
                File.Delete(timestampPath);
        }
        catch (Exception)
        {
            // Silently fail on delete errors
        }

        return Task.CompletedTask;
    }
}
