using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyConference.Models;
using MyConference.Services;

namespace MyConference.ViewModels;

[QueryProperty(nameof(SessionId), "SessionId")]
public partial class SessionDetailViewModel : ObservableObject
{
    private readonly ISessionizeService _sessionizeService;
    private readonly IFavoritesService _favoritesService;

    [ObservableProperty] private string _sessionId = string.Empty;
    [ObservableProperty] private Session? _session;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isFavorite;

    public SessionDetailViewModel(ISessionizeService sessionizeService, IFavoritesService favoritesService)
    {
        _sessionizeService = sessionizeService;
        _favoritesService = favoritesService;
    }

    partial void OnSessionIdChanged(string value) => _ = LoadSessionAsync();

    private async Task LoadSessionAsync()
    {
        if (string.IsNullOrEmpty(SessionId))
            return;

        try
        {
            IsLoading = true;

            var data = await _sessionizeService.GetConferenceDataAsync();
            if (data is null)
                return;

            var session = data.Sessions.FirstOrDefault(s => s.Id == SessionId);
            if (session is null)
                return;

            var speakersById = data.Speakers.ToDictionary(s => s.Id);
            var roomsById = data.Rooms.ToDictionary(r => r.Id);

            session.SpeakerProfiles = session.Speakers
                .Where(speakersById.ContainsKey)
                .Select(id => speakersById[id])
                .ToList();

            session.RoomName = session.RoomId.HasValue && roomsById.TryGetValue(session.RoomId.Value, out var room)
                ? room.Name
                : null;

            session.IsFavorite = _favoritesService.IsFavorite(session.Id);

            Session = session;
            IsFavorite = session.IsFavorite;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ToggleFavorite()
    {
        if (Session is null)
            return;

        _favoritesService.ToggleFavorite(Session.Id);
        IsFavorite = _favoritesService.IsFavorite(Session.Id);
        Session.IsFavorite = IsFavorite;
    }

    [RelayCommand]
    private async Task GoBack()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task NavigateToSpeaker(Speaker? speaker)
    {
        if (speaker is null) return;
        await Shell.Current.GoToAsync($"speakerdetail?SpeakerId={speaker.Id}");
    }
}
