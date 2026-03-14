using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyConference.Models;
using MyConference.Services;
using System.Collections.ObjectModel;

namespace MyConference.ViewModels;

public partial class SpeakersViewModel : ObservableObject
{
    private readonly ISessionizeService _sessionizeService;
    private List<Speaker> _allSpeakers = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Speaker> _speakers = [];

    public SpeakersViewModel(ISessionizeService sessionizeService)
    {
        _sessionizeService = sessionizeService;
    }

    public async Task InitializeAsync()
    {
        if (_allSpeakers.Count > 0) return;
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            var data = await _sessionizeService.GetConferenceDataAsync();
            if (data != null)
            {
                _allSpeakers = data.Speakers;
                FilterSpeakers();
            }
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
        await _sessionizeService.GetConferenceDataAsync(true);
        await LoadDataAsync();
    }

    partial void OnSearchTextChanged(string value) => FilterSpeakers();

    private void FilterSpeakers()
    {
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allSpeakers
            : _allSpeakers.Where(s =>
                s.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                (s.TagLine?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));

        Speakers = new ObservableCollection<Speaker>(filtered.OrderBy(s => s.FullName));
    }

    [RelayCommand]
    private async Task NavigateToSpeaker(Speaker? speaker)
    {
        if (speaker is null) return;
        await Shell.Current.GoToAsync($"speakerdetail?SpeakerId={speaker.Id}");
    }
}
