namespace MyConference.Services;

public interface IFavoritesService
{
    bool IsFavorite(string sessionId);
    void ToggleFavorite(string sessionId);
    HashSet<string> GetAllFavorites();
    event EventHandler? FavoritesChanged;
}
