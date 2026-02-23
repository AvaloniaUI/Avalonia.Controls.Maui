using System.Collections.ObjectModel;
using System.Windows.Input;
using AlohaAI.Services;

namespace AlohaAI.ViewModels;

public class SearchViewModel : BaseViewModel
{
    private readonly IContentService _contentService;
    private CancellationTokenSource? _debounceToken;
    private List<BrowseTopicItem> _allBrowseItems = [];

    private string _searchQuery = string.Empty;
    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            SetProperty(ref _searchQuery, value);
            DebouncedSearch();
        }
    }

    private void DebouncedSearch()
    {
        _debounceToken?.Cancel();
        _debounceToken = new CancellationTokenSource();
        var token = _debounceToken.Token;

        Task.Delay(300, token).ContinueWith(_ =>
        {
            if (!token.IsCancellationRequested)
                Application.Current.Dispatcher.Dispatch(() => SearchCommand.Execute(null));
        }, TaskContinuationOptions.OnlyOnRanToCompletion);
    }

    private bool _hasResults;
    public bool HasResults
    {
        get => _hasResults;
        set => SetProperty(ref _hasResults, value);
    }

    private bool _showEmpty;
    public bool ShowEmpty
    {
        get => _showEmpty;
        set => SetProperty(ref _showEmpty, value);
    }

    private bool _showBrowse = true;
    public bool ShowBrowse
    {
        get => _showBrowse;
        set => SetProperty(ref _showBrowse, value);
    }

    private string _activeFilter = string.Empty;
    public string ActiveFilter
    {
        get => _activeFilter;
        set
        {
            SetProperty(ref _activeFilter, value);
            UpdateFilterChipStyles();
            FilterBrowseItems();
            DebouncedSearch();
        }
    }

    private string _allFilterBackground = "#5B8FD4";
    public string AllFilterBackground
    {
        get => _allFilterBackground;
        set => SetProperty(ref _allFilterBackground, value);
    }

    private string _allFilterStroke = "#5B8FD4";
    public string AllFilterStroke
    {
        get => _allFilterStroke;
        set => SetProperty(ref _allFilterStroke, value);
    }

    private string _allFilterText = "White";
    public string AllFilterText
    {
        get => _allFilterText;
        set => SetProperty(ref _allFilterText, value);
    }

    public ObservableCollection<SearchResultItem> Results { get; } = [];
    public ObservableCollection<FilterChipItem> FilterChips { get; } = [];
    public ObservableCollection<BrowseTopicItem> BrowseItems { get; } = [];

    public ICommand SearchCommand { get; }
    public ICommand SelectResultCommand { get; }
    public ICommand ClearCommand { get; }
    public ICommand FilterCommand { get; }
    public ICommand BrowseTopicCommand { get; }
    public ICommand LoadBrowseCommand { get; }

    public SearchViewModel(IContentService contentService)
    {
        _contentService = contentService;
        Title = "Search";

        SearchCommand = new AsyncRelayCommand(SearchAsync);
        SelectResultCommand = new AsyncRelayCommand<SearchResultItem>(async item =>
        {
            if (item != null)
                await Shell.Current.GoToAsync($"lesson?pathId={item.PathId}&moduleId={item.ModuleId}&lessonId={item.LessonId}");
        });
        ClearCommand = new RelayCommand(() =>
        {
            SearchQuery = string.Empty;
            Results.Clear();
            HasResults = false;
            ShowEmpty = false;
            ShowBrowse = true;
        });
        FilterCommand = new RelayCommand<string>(pathId =>
        {
            ActiveFilter = pathId ?? string.Empty;
        });
        BrowseTopicCommand = new AsyncRelayCommand<string>(async pathId =>
        {
            if (!string.IsNullOrEmpty(pathId))
                await Shell.Current.GoToAsync($"pathdetail?pathId={pathId}");
        });
        LoadBrowseCommand = new AsyncRelayCommand(LoadBrowseAsync);
        LoadBrowseCommand.Execute(null);
    }

    private async Task LoadBrowseAsync()
    {
        try
        {
            var paths = await _contentService.GetPathsAsync();
            FilterChips.Clear();
            _allBrowseItems.Clear();

            foreach (var path in paths)
            {
                FilterChips.Add(new FilterChipItem
                {
                    PathId = path.Id,
                    Title = path.Title,
                    Color = path.Color,
                    IsActive = false
                });

                var modules = await _contentService.GetModulesAsync(path.Id);
                var lessonCount = modules.Sum(m => m.Lessons.Count);
                _allBrowseItems.Add(new BrowseTopicItem
                {
                    PathId = path.Id,
                    Title = path.Title,
                    Color = path.Color,
                    LessonCount = lessonCount,
                    Icon = path.Id switch
                    {
                        "agentic-ai" => "icon_explore.png",
                        "ml-fundamentals" => "icon_books.png",
                        "ai-in-practice" => "icon_rocket.png",
                        "prompt-engineering" => "icon_gem.png",
                        "vision-multimodal" => "icon_island.png",
                        "generative-ai" => "icon_flowers.png",
                        "ai-safety" => "icon_trophy.png",
                        "mlops-engineering" => "icon_notebook.png",
                        _ => "icon_explore.png"
                    }
                });
            }
            FilterBrowseItems();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Browse load error: {ex.Message}");
        }
    }

    private void FilterBrowseItems()
    {
        BrowseItems.Clear();
        var filtered = string.IsNullOrEmpty(ActiveFilter)
            ? _allBrowseItems
            : _allBrowseItems.Where(b => b.PathId == ActiveFilter).ToList();
        foreach (var item in filtered)
            BrowseItems.Add(item);
    }

    private void UpdateFilterChipStyles()
    {
        var isAll = string.IsNullOrEmpty(ActiveFilter);
        AllFilterBackground = isAll ? "#5B8FD4" : "Transparent";
        AllFilterStroke = isAll ? "#5B8FD4" : "#555555";
        AllFilterText = isAll ? "White" : "#AAAAAA";

        foreach (var chip in FilterChips)
        {
            chip.IsActive = chip.PathId == ActiveFilter;
        }
    }

    private async Task SearchAsync()
    {
        var query = SearchQuery?.Trim();
        if (string.IsNullOrEmpty(query) || query.Length < 1)
        {
            Results.Clear();
            HasResults = false;
            ShowEmpty = false;
            ShowBrowse = true;
            return;
        }

        ShowBrowse = false;

        try
        {
            Results.Clear();
            var paths = await _contentService.GetPathsAsync();

            foreach (var path in paths)
            {
                if (!string.IsNullOrEmpty(ActiveFilter) && path.Id != ActiveFilter)
                    continue;

                var modules = await _contentService.GetModulesAsync(path.Id);
                foreach (var module in modules)
                {
                    foreach (var lesson in module.Lessons)
                    {
                        var matchesSearch = query.Length < 2 ||
                            lesson.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                            module.Title.Contains(query, StringComparison.OrdinalIgnoreCase);

                        if (matchesSearch)
                        {
                            Results.Add(new SearchResultItem
                            {
                                PathId = path.Id,
                                ModuleId = module.Id,
                                LessonId = lesson.Id,
                                LessonTitle = lesson.Title,
                                ModuleTitle = module.Title,
                                PathTitle = path.Title,
                                Xp = lesson.Xp,
                                PathColor = path.Color
                            });
                        }
                    }
                }
            }

            HasResults = Results.Count > 0;
            ShowEmpty = Results.Count == 0 && query.Length >= 2;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Search error: {ex.Message}");
        }
    }
}

public class SearchResultItem
{
    public string PathId { get; set; } = string.Empty;
    public string ModuleId { get; set; } = string.Empty;
    public string LessonId { get; set; } = string.Empty;
    public string LessonTitle { get; set; } = string.Empty;
    public string ModuleTitle { get; set; } = string.Empty;
    public string PathTitle { get; set; } = string.Empty;
    public int Xp { get; set; }
    public string PathColor { get; set; } = "#5B8FD4";
    public string Subtitle => $"{PathTitle} → {ModuleTitle}";
}

public class FilterChipItem : BaseViewModel
{
    public string PathId { get; set; } = string.Empty;
    public new string Title { get; set; } = string.Empty;
    public string Color { get; set; } = "#5B8FD4";

    private bool _isActive;
    public bool IsActive
    {
        get => _isActive;
        set
        {
            SetProperty(ref _isActive, value);
            OnPropertyChanged(nameof(Background));
            OnPropertyChanged(nameof(StrokeColor));
            OnPropertyChanged(nameof(TextColor));
        }
    }

    public string Background => IsActive ? Color : "Transparent";
    public string StrokeColor => IsActive ? Color : "#555555";
    public string TextColor => IsActive ? "White" : "#AAAAAA";
}

public class BrowseTopicItem
{
    public string PathId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Color { get; set; } = "#5B8FD4";
    public string Icon { get; set; } = "icon_explore.png";
    public int LessonCount { get; set; }
}
