using System.Text.Json;

namespace MyConference.Services;

public class FavoritesService : IFavoritesService
{
    private const string PreferencesKey = "favorite_sessions";

    private readonly HashSet<string> _favorites;

    public event EventHandler? FavoritesChanged;

    public FavoritesService()
    {
        _favorites = LoadFavorites();
    }

    public bool IsFavorite(string sessionId) => _favorites.Contains(sessionId);

    public void ToggleFavorite(string sessionId)
    {
        if (!_favorites.Add(sessionId))
            _favorites.Remove(sessionId);

        SaveFavorites();
        FavoritesChanged?.Invoke(this, EventArgs.Empty);
    }

    public HashSet<string> GetAllFavorites() => [.. _favorites];

    private static HashSet<string> LoadFavorites()
    {
        try
        {
            var json = Preferences.Default.Get(PreferencesKey, string.Empty);
            if (string.IsNullOrEmpty(json))
                return [];

            return JsonSerializer.Deserialize<HashSet<string>>(json) ?? [];
        }
        catch (Exception)
        {
            return [];
        }
    }

    private void SaveFavorites()
    {
        var json = JsonSerializer.Serialize(_favorites);
        Preferences.Default.Set(PreferencesKey, json);
    }
}
