using System.Text.Json;
using MyConference.Models;

namespace MyConference.Services;

public class SessionizeService : ISessionizeService
{
    private const string CacheKey = "sessionize_data";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly ICacheService _cacheService;

    public SessionizeService(HttpClient httpClient, ICacheService cacheService)
    {
        _httpClient = httpClient;
        _cacheService = cacheService;
    }

    public async Task<SessionizeData?> GetConferenceDataAsync(bool forceRefresh = false)
    {
        if (!forceRefresh)
        {
            var cached = await _cacheService.GetAsync<SessionizeData>(CacheKey);
            if (cached is not null && _cacheService.IsFresh(CacheKey))
                return cached;
        }

        try
        {
            var json = await _httpClient.GetStringAsync(EventConfig.SessionizeApiUrl);
            var data = JsonSerializer.Deserialize<SessionizeData>(json, JsonOptions);

            if (data is not null)
                await _cacheService.SetAsync(CacheKey, data);

            return data;
        }
        catch (Exception)
        {
            // On failure, return stale cached data if available
            var staleData = await _cacheService.GetAsync<SessionizeData>(CacheKey);
            return staleData;
        }
    }
}
