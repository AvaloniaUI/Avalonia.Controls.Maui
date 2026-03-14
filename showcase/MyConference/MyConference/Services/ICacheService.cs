using System.Text.Json.Serialization.Metadata;

namespace MyConference.Services;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, JsonTypeInfo<T> jsonTypeInfo) where T : class;
    Task SetAsync<T>(string key, T data, JsonTypeInfo<T> jsonTypeInfo) where T : class;
    bool IsFresh(string key);
    Task ClearAsync(string key);
}
