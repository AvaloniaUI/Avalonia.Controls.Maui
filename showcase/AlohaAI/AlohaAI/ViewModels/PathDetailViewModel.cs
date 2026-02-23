using System.Collections.ObjectModel;
using System.Windows.Input;
using AlohaAI.Models;
using AlohaAI.Services;

namespace AlohaAI.ViewModels;

[QueryProperty(nameof(PathId), "pathId")]
public class PathDetailViewModel : BaseViewModel
{
    private readonly IContentService _contentService;
    private readonly IProgressService _progressService;

    private string _pathId = string.Empty;
    public string PathId
    {
        get => _pathId;
        set { SetProperty(ref _pathId, value); LoadDataCommand.Execute(null); }
    }

    private string _pathDescription = string.Empty;
    public string PathDescription
    {
        get => _pathDescription;
        set => SetProperty(ref _pathDescription, value);
    }

    private Color _pathColor = Colors.Blue;
    public Color PathColor
    {
        get => _pathColor;
        set => SetProperty(ref _pathColor, value);
    }

    private string _pathIcon = "icon_explore.png";
    public string PathIcon
    {
        get => _pathIcon;
        set => SetProperty(ref _pathIcon, value);
    }

    private string _headerImage = "header_agentic_ai.jpg";
    public string HeaderImage
    {
        get => _headerImage;
        set => SetProperty(ref _headerImage, value);
    }

    private double _overallProgress;
    public double OverallProgress
    {
        get => _overallProgress;
        set => SetProperty(ref _overallProgress, value);
    }

    private string _overallProgressText = "0%";
    public string OverallProgressText
    {
        get => _overallProgressText;
        set => SetProperty(ref _overallProgressText, value);
    }

    public ObservableCollection<ModuleDisplayItem> Modules { get; } = [];

    public ICommand LoadDataCommand { get; }
    public ICommand SelectLessonCommand { get; }
    public ICommand StartQuizCommand { get; }
    public ICommand GoBackCommand { get; }

    public PathDetailViewModel(IContentService contentService, IProgressService progressService)
    {
        _contentService = contentService;
        _progressService = progressService;

        LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
        SelectLessonCommand = new AsyncRelayCommand<LessonDisplayItem>(async lesson =>
        {
            if (lesson != null)
                await Shell.Current.GoToAsync($"lesson?pathId={PathId}&moduleId={lesson.ModuleId}&lessonId={lesson.Id}");
        });
        StartQuizCommand = new AsyncRelayCommand<string>(async moduleId =>
        {
            if (!string.IsNullOrEmpty(moduleId))
                await Shell.Current.GoToAsync($"quiz?pathId={PathId}&moduleId={moduleId}");
        });
        GoBackCommand = new AsyncRelayCommand(async () => await Shell.Current.GoToAsync(".."));
    }

    private async Task LoadDataAsync()
    {
        if (IsBusy || string.IsNullOrEmpty(PathId)) return;
        IsBusy = true;

        try
        {
            var path = await _contentService.GetPathAsync(PathId);
            if (path != null)
            {
                Title = path.Title;
                PathDescription = path.Description;
                PathColor = Color.FromArgb(path.Color);
                PathIcon = path.Id switch
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
                };
                HeaderImage = path.Id switch
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

            var modules = await _contentService.GetModulesAsync(PathId);
            Modules.Clear();
            int moduleNum = 0;

            foreach (var module in modules)
            {
                moduleNum++;
                var lessons = new ObservableCollection<LessonDisplayItem>();
                foreach (var lesson in module.Lessons)
                {
                    var completed = await _progressService.IsLessonCompletedAsync(PathId, module.Id, lesson.Id);
                    lessons.Add(new LessonDisplayItem
                    {
                        Id = lesson.Id,
                        ModuleId = module.Id,
                        Title = lesson.Title,
                        Xp = lesson.Xp,
                        IsCompleted = completed
                    });
                }

                var completedCount = lessons.Count(l => l.IsCompleted);
                Modules.Add(new ModuleDisplayItem
                {
                    Id = module.Id,
                    Title = module.Title,
                    Description = module.Description,
                    ModuleNumber = moduleNum,
                    Lessons = lessons,
                    CompletedCount = completedCount,
                    TotalCount = lessons.Count,
                    HasQuiz = !string.IsNullOrEmpty(module.Quiz)
                });
            }

            var totalLessons = Modules.Sum(m => m.TotalCount);
            var totalCompleted = Modules.Sum(m => m.CompletedCount);
            OverallProgress = totalLessons > 0 ? (double)totalCompleted / totalLessons : 0;
            OverallProgressText = $"{(int)(OverallProgress * 100)}% ({totalCompleted}/{totalLessons})";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading path detail: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}

public class ModuleDisplayItem
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ModuleNumber { get; set; }
    public ObservableCollection<LessonDisplayItem> Lessons { get; set; } = [];
    public int CompletedCount { get; set; }
    public int TotalCount { get; set; }
    public bool HasQuiz { get; set; }
    public string ProgressText => $"{CompletedCount}/{TotalCount}";
    public bool AllLessonsComplete => CompletedCount == TotalCount && TotalCount > 0;
    public string ModuleLabel => $"Module {ModuleNumber}";
    public double Progress => TotalCount > 0 ? (double)CompletedCount / TotalCount : 0;
}

public class LessonDisplayItem
{
    public string Id { get; set; } = string.Empty;
    public string ModuleId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Xp { get; set; }
    public bool IsCompleted { get; set; }
    public string StatusColor => IsCompleted ? "#4CAF50" : "#555555";
    public string StatusBackground => IsCompleted ? "#4CAF50" : "Transparent";
    public string TitleColor => IsCompleted ? "#AAAAAA" : "#FFFFFF";
}
