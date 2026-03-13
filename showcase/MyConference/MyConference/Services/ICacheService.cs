namespace MyConference.Services;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T data) where T : class;
    bool IsFresh(string key);
    Task ClearAsync(string key);
}
