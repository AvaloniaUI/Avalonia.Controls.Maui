using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyConference.Models;
using MyConference.Services;
using System.Collections.ObjectModel;

namespace MyConference.ViewModels;

[QueryProperty(nameof(SpeakerId), "SpeakerId")]
public partial class SpeakerDetailViewModel : ObservableObject
{
    private readonly ISessionizeService _sessionizeService;
    private readonly IFavoritesService _favoritesService;

    [ObservableProperty]
    private string _speakerId = string.Empty;

    [ObservableProperty]
    private Speaker? _speaker;

    [ObservableProperty]
    private ObservableCollection<Session> _sessions = [];

    [ObservableProperty]
    private bool _isLoading;

    public SpeakerDetailViewModel(ISessionizeService sessionizeService, IFavoritesService favoritesService)
    {
        _sessionizeService = sessionizeService;
        _favoritesService = favoritesService;
    }

    partial void OnSpeakerIdChanged(string value) => _ = LoadSpeakerAsync();

    private async Task LoadSpeakerAsync()
    {
        IsLoading = true;
        try
        {
            var data = await _sessionizeService.GetConferenceDataAsync();
            if (data == null) return;

            Speaker = data.Speakers.FirstOrDefault(s => s.Id == SpeakerId);
            if (Speaker == null) return;

            var speakerSessions = data.Sessions
                .Where(s => s.Speakers.Contains(Speaker.Id))
                .OrderBy(s => s.StartsAt)
                .ToList();

            foreach (var session in speakerSessions)
            {
                session.RoomName = data.Rooms.FirstOrDefault(r => r.Id == session.RoomId)?.Name;
                session.IsFavorite = _favoritesService.IsFavorite(session.Id);
            }

            Sessions = new ObservableCollection<Session>(speakerSessions);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToSession(Session? session)
    {
        if (session is null) return;
        await Shell.Current.GoToAsync($"sessiondetail?SessionId={session.Id}");
    }
}
