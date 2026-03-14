using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyConference.Models;
using MyConference.Services;
using System.Collections.ObjectModel;

namespace MyConference.ViewModels;

public partial class SessionsViewModel : ObservableObject
{
    private readonly ISessionizeService _sessionizeService;
    private readonly IFavoritesService _favoritesService;
    private SessionizeData? _data;
    private bool _initialized;

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isRefreshing;
    [ObservableProperty] private bool _isOffline;
    [ObservableProperty] private string _lastUpdated = string.Empty;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private ObservableCollection<TimeSlotGroup> _sessionGroups = [];
    [ObservableProperty] private ObservableCollection<string> _days = [];
    [ObservableProperty] private string? _selectedDay;
    [ObservableProperty] private bool _isMultiDay;

    public SessionsViewModel(ISessionizeService sessionizeService, IFavoritesService favoritesService)
    {
        _sessionizeService = sessionizeService;
        _favoritesService = favoritesService;
    }

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            ResolveRelationships();
            FilterSessions();
            return;
        }

        _favoritesService.FavoritesChanged += (_, _) =>
        {
            ResolveRelationships();
            FilterSessions();
        };

        await LoadDataAsync();
        _initialized = true;
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            #if IOS || MACCATALYST || ANDROID || WINDOWS
            IsOffline = Connectivity.Current.NetworkAccess != NetworkAccess.Internet;
            #else
            IsOffline = false; // Assume online on platforms where we don't have connectivity info
            #endif
            _data = await _sessionizeService.GetConferenceDataAsync();

            if (_data is null)
                return;

            LastUpdated = $"Updated {DateTime.Now:HH:mm}";
            ResolveRelationships();

            var distinctDays = _data.Sessions
                .Where(s => !s.IsServiceSession && s.Day.HasValue)
                .Select(s => s.Day!.Value)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            IsMultiDay = distinctDays.Count > 1;
            Days = new ObservableCollection<string>(
                distinctDays.Select(d => d.ToString("ddd, MMM d")));

            SelectedDay = Days.FirstOrDefault();
        }
        finally
        {
            IsLoading = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        try
        {
            IsRefreshing = true;
            _data = await _sessionizeService.GetConferenceDataAsync(forceRefresh: true);

            if (_data is null)
                return;

            ResolveRelationships();
            FilterSessions();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    partial void OnSearchTextChanged(string value) => FilterSessions();

    partial void OnSelectedDayChanged(string? value) => FilterSessions();

    [RelayCommand]
    private void SelectDay(string day)
    {
        SelectedDay = day;
    }

    private void FilterSessions()
    {
        if (_data is null)
            return;

        var sessions = _data.Sessions
            .Where(s => !s.IsServiceSession);

        if (!string.IsNullOrWhiteSpace(SelectedDay) && IsMultiDay)
        {
            sessions = sessions.Where(s =>
                s.Day.HasValue &&
                s.Day.Value.ToString("ddd, MMM d") == SelectedDay);
        }

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.Trim();
            sessions = sessions.Where(s =>
                s.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                s.SpeakerProfiles.Any(sp =>
                    sp.FullName.Contains(search, StringComparison.OrdinalIgnoreCase)));
        }

        var groups = sessions
            .Where(s => s.StartsAt.HasValue)
            .OrderBy(s => s.StartsAt)
            .GroupBy(s => s.TimeSlot)
            .Select(g => new TimeSlotGroup(g.Key, g))
            .ToList();

        SessionGroups = new ObservableCollection<TimeSlotGroup>(groups);
    }

    private void ResolveRelationships()
    {
        if (_data is null)
            return;

        var speakersById = _data.Speakers.ToDictionary(s => s.Id);
        var roomsById = _data.Rooms.ToDictionary(r => r.Id);

        foreach (var session in _data.Sessions)
        {
            session.SpeakerProfiles = session.Speakers
                .Where(speakersById.ContainsKey)
                .Select(id => speakersById[id])
                .ToList();

            session.RoomName = session.RoomId.HasValue && roomsById.TryGetValue(session.RoomId.Value, out var room)
                ? room.Name
                : null;

            session.IsFavorite = _favoritesService.IsFavorite(session.Id);
        }
    }

    [RelayCommand]
    private void ToggleFavorite(Session? session)
    {
        if (session is null) return;
        _favoritesService.ToggleFavorite(session.Id);
        session.IsFavorite = _favoritesService.IsFavorite(session.Id);
        FilterSessions();
    }

    [RelayCommand]
    private async Task NavigateToSession(Session? session)
    {
        if (session is null) return;
        await Shell.Current.GoToAsync($"sessiondetail?SessionId={session.Id}");
    }
}
