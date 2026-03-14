using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyConference.Models;
using MyConference.Services;
using System.Collections.ObjectModel;

namespace MyConference.ViewModels;

public partial class FavoritesViewModel : ObservableObject
{
    private readonly ISessionizeService _sessionizeService;
    private readonly IFavoritesService _favoritesService;

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private ObservableCollection<Session> _favoriteSessions = [];
    [ObservableProperty] private bool _hasFavorites;

    public FavoritesViewModel(ISessionizeService sessionizeService, IFavoritesService favoritesService)
    {
        _sessionizeService = sessionizeService;
        _favoritesService = favoritesService;
    }

    public async Task InitializeAsync()
    {
        await LoadFavoritesAsync();
    }

    [RelayCommand]
    private async Task LoadFavoritesAsync()
    {
        IsLoading = true;
        try
        {
            var data = await _sessionizeService.GetConferenceDataAsync();
            if (data == null) return;

            var favoriteIds = _favoritesService.GetAllFavorites();
            var favorites = data.Sessions
                .Where(s => favoriteIds.Contains(s.Id))
                .OrderBy(s => s.StartsAt)
                .ToList();

            foreach (var session in favorites)
            {
                session.SpeakerProfiles = data.Speakers
                    .Where(sp => session.Speakers.Contains(sp.Id))
                    .ToList();
                session.RoomName = data.Rooms.FirstOrDefault(r => r.Id == session.RoomId)?.Name;
                session.IsFavorite = true;
            }

            FavoriteSessions = new ObservableCollection<Session>(favorites);
            HasFavorites = FavoriteSessions.Count > 0;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void RemoveFavorite(Session session)
    {
        _favoritesService.ToggleFavorite(session.Id);
        FavoriteSessions.Remove(session);
        HasFavorites = FavoriteSessions.Count > 0;
    }

    [RelayCommand]
    private async Task NavigateToSession(Session? session)
    {
        if (session is null) return;
        await Shell.Current.GoToAsync($"sessiondetail?SessionId={session.Id}");
    }
}
