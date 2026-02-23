using System.Windows.Input;
using AlohaAI.Services;

namespace AlohaAI.ViewModels;

[QueryProperty(nameof(PathId), "pathId")]
[QueryProperty(nameof(ModuleId), "moduleId")]
[QueryProperty(nameof(LessonId), "lessonId")]
public class LessonViewModel : BaseViewModel
{
    private readonly IContentService _contentService;
    private readonly IProgressService _progressService;
    private readonly IStreakService _streakService;

    private string _pathId = string.Empty;
    public string PathId
    {
        get => _pathId;
        set => SetProperty(ref _pathId, value);
    }

    private string _moduleId = string.Empty;
    public string ModuleId
    {
        get => _moduleId;
        set => SetProperty(ref _moduleId, value);
    }

    private string _lessonId = string.Empty;
    public string LessonId
    {
        get => _lessonId;
        set { SetProperty(ref _lessonId, value); LoadLessonCommand.Execute(null); }
    }

    private string _markdownContent = string.Empty;
    public string MarkdownContent
    {
        get => _markdownContent;
        set => SetProperty(ref _markdownContent, value);
    }

    private bool _isCompleted;
    public bool IsCompleted
    {
        get => _isCompleted;
        set => SetProperty(ref _isCompleted, value);
    }

    private int _lessonXp;
    public int LessonXp
    {
        get => _lessonXp;
        set => SetProperty(ref _lessonXp, value);
    }

    private string _headerImage = "header_agentic_ai.jpg";
    public string HeaderImage
    {
        get => _headerImage;
        set => SetProperty(ref _headerImage, value);
    }

    private string _nextLessonTitle = string.Empty;
    public string NextLessonTitle
    {
        get => _nextLessonTitle;
        set => SetProperty(ref _nextLessonTitle, value);
    }

    private bool _hasNextLesson;
    public bool HasNextLesson
    {
        get => _hasNextLesson;
        set => SetProperty(ref _hasNextLesson, value);
    }

    private string _lessonPositionText = string.Empty;
    public string LessonPositionText
    {
        get => _lessonPositionText;
        set => SetProperty(ref _lessonPositionText, value);
    }

    private double _lessonProgress;
    public double LessonProgress
    {
        get => _lessonProgress;
        set => SetProperty(ref _lessonProgress, value);
    }

    private string _moduleTitle = string.Empty;
    public string ModuleTitle
    {
        get => _moduleTitle;
        set => SetProperty(ref _moduleTitle, value);
    }

    private string _pathTitle = string.Empty;
    public string PathTitle
    {
        get => _pathTitle;
        set => SetProperty(ref _pathTitle, value);
    }

    private string _pathColor = "#5B8FD4";
    public string PathColor
    {
        get => _pathColor;
        set => SetProperty(ref _pathColor, value);
    }

    private List<string> _keywords = new();
    public List<string> Keywords
    {
        get => _keywords;
        set => SetProperty(ref _keywords, value);
    }

    private string _lessonDate = string.Empty;
    public string LessonDate
    {
        get => _lessonDate;
        set => SetProperty(ref _lessonDate, value);
    }

    public ICommand LoadLessonCommand { get; }
    public ICommand MarkCompleteCommand { get; }
    public ICommand GoBackCommand { get; }
    public ICommand NextLessonCommand { get; }

    public LessonViewModel(IContentService contentService, IProgressService progressService, IStreakService streakService)
    {
        _contentService = contentService;
        _progressService = progressService;
        _streakService = streakService;

        LoadLessonCommand = new AsyncRelayCommand(LoadLessonAsync);
        MarkCompleteCommand = new AsyncRelayCommand(MarkCompleteAsync);
        GoBackCommand = new AsyncRelayCommand(async () => await Shell.Current.GoToAsync(".."));
        NextLessonCommand = new AsyncRelayCommand(NavigateToNextLessonAsync);
    }

    private string? _nextLessonId;
    private string? _nextModuleId;

    private async Task LoadLessonAsync()
    {
        if (IsBusy || string.IsNullOrEmpty(LessonId)) return;
        IsBusy = true;

        try
        {
            var module = await _contentService.GetModuleAsync(PathId, ModuleId);
            var lessonInfo = module?.Lessons.FirstOrDefault(l => l.Id == LessonId);

            if (lessonInfo != null)
            {
                Title = lessonInfo.Title;
                LessonXp = lessonInfo.Xp;
                MarkdownContent = await _contentService.GetLessonContentAsync(PathId, ModuleId, lessonInfo.File);
            }

            IsCompleted = await _progressService.IsLessonCompletedAsync(PathId, ModuleId, LessonId);

            // Load path info
            var path = await _contentService.GetPathAsync(PathId);
            if (path != null)
            {
                PathTitle = path.Title;
                PathColor = path.Color;
                HeaderImage = PathId switch
                {
                    "agentic-ai" => "header_agentic_ai.jpg",
                    "ml-fundamentals" => "header_ml_fundamentals.jpg",
                    "ai-in-practice" => "header_ai_in_practice.jpg",
                    "prompt-engineering" => "header_prompt_engineering.jpg",
                    "vision-multimodal" => "header_vision_multimodal.jpg",
                    "generative-ai" => "header_generative_ai.jpg",
                    "ai-safety" => "header_ai_safety.jpg",
                    "mlops-engineering" => "header_mlops_engineering.jpg",
                    _ => "header_agentic_ai.jpg"
                };
            }

            // Compute lesson position and progress
            if (module != null)
            {
                ModuleTitle = module.Title;
                var currentIndex = module.Lessons.FindIndex(l => l.Id == LessonId);
                if (currentIndex >= 0)
                {
                    var position = currentIndex + 1;
                    var total = module.Lessons.Count;
                    LessonPositionText = $"Lesson {position} of {total}";
                    LessonProgress = (double)position / total;
                }
            }

            // Extract keywords from markdown content
            ExtractKeywords();

            // Set lesson date
            LessonDate = DateTime.Now.ToString("MMM dd, yyyy");

            // Find next lesson
            HasNextLesson = false;
            if (module != null)
            {
                var currentIndex = module.Lessons.FindIndex(l => l.Id == LessonId);
                if (currentIndex >= 0 && currentIndex < module.Lessons.Count - 1)
                {
                    var next = module.Lessons[currentIndex + 1];
                    _nextLessonId = next.Id;
                    _nextModuleId = ModuleId;
                    NextLessonTitle = next.Title;
                    HasNextLesson = true;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading lesson: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ExtractKeywords()
    {
        if (string.IsNullOrEmpty(MarkdownContent))
        {
            Keywords = new List<string>();
            return;
        }

        // Extract bold terms as keywords (up to 5)
        var boldPattern = new System.Text.RegularExpressions.Regex(@"\*\*([^*]+)\*\*");
        var matches = boldPattern.Matches(MarkdownContent);
        var terms = matches
            .Select(m => m.Groups[1].Value.Trim())
            .Where(t => t.Length > 2 && t.Length < 30 && !t.Contains('\n'))
            .Distinct()
            .Take(5)
            .ToList();

        Keywords = terms.Count > 0 ? terms : new List<string> { Title };
    }

    private async Task MarkCompleteAsync()
    {
        await _progressService.MarkLessonCompleteAsync(PathId, ModuleId, LessonId, LessonXp);
        await _streakService.RecordActivityAsync();
        IsCompleted = true;
        HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        // Delay to show XP popup animation before navigating back
        await Task.Delay(1800);
        if (!HasNextLesson)
            await Shell.Current.GoToAsync("..");
    }

    private async Task NavigateToNextLessonAsync()
    {
        if (_nextLessonId == null || _nextModuleId == null) return;
        await Shell.Current.GoToAsync($"..?pathId={PathId}&moduleId={_nextModuleId}&lessonId={_nextLessonId}");
        // Re-navigate to same route with new params
        await Shell.Current.GoToAsync($"lesson?pathId={PathId}&moduleId={_nextModuleId}&lessonId={_nextLessonId}");
    }
}
