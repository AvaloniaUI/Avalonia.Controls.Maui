using System.Collections.ObjectModel;
using System.Windows.Input;
using AlohaAI.Models;
using AlohaAI.Services;

namespace AlohaAI.ViewModels;

public class PathsViewModel : BaseViewModel
{
    private readonly IContentService _contentService;
    private readonly IProgressService _progressService;

    public ObservableCollection<PathDisplayItem> Paths { get; } = [];

    public ICommand LoadPathsCommand { get; }
    public ICommand SelectPathCommand { get; }

    public PathsViewModel(IContentService contentService, IProgressService progressService)
    {
        _contentService = contentService;
        _progressService = progressService;
        Title = "Paths";

        LoadPathsCommand = new AsyncRelayCommand(LoadPathsAsync);
        SelectPathCommand = new AsyncRelayCommand<string>(async pathId =>
        {
            if (!string.IsNullOrEmpty(pathId))
                await Shell.Current.GoToAsync($"pathdetail?pathId={pathId}");
        });
    }

    private async Task LoadPathsAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var paths = await _contentService.GetPathsAsync();
            Paths.Clear();

            foreach (var path in paths)
            {
                var modules = await _contentService.GetModulesAsync(path.Id);
                var totalLessons = modules.Sum(m => m.Lessons.Count);
                var progress = await _progressService.GetPathProgressAsync(path.Id, totalLessons);
                var completedLessons = await _progressService.GetCompletedLessonCountAsync(path.Id);

                Paths.Add(new PathDisplayItem
                {
                    Id = path.Id,
                    Title = path.Title,
                    Description = path.Description,
                    Color = Color.FromArgb(path.Color),
                    IconImage = path.Id switch
                    {
                        "agentic-ai" => "icon_explore.png",
                        "ml-fundamentals" => "icon_books.png",
                        "ai-in-practice" => "icon_rocket.png",
                        "prompt-engineering" => "icon_gem.png",
                        "vision-multimodal" => "icon_island.png",
                        "generative-ai" => "icon_flowers.png",
                        "ai-safety" => "icon_trophy.png",
                        "mlops-engineering" => "icon_notebook.png",
                        _ => "icon_books.png"
                    },
                    ModuleCount = path.ModuleCount,
                    EstimatedHours = path.EstimatedHours,
                    Progress = progress,
                    CompletedLessons = completedLessons,
                    TotalLessons = totalLessons
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading paths: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}

public class PathDisplayItem : BaseViewModel
{
    public string Id { get; set; } = string.Empty;
    public new string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Color Color { get; set; } = Colors.Blue;
    public string IconImage { get; set; } = "icon_books.png";
    public int ModuleCount { get; set; }
    public int EstimatedHours { get; set; }
    public double Progress { get; set; }
    public int CompletedLessons { get; set; }
    public int TotalLessons { get; set; }
    public string ProgressText => $"{CompletedLessons}/{TotalLessons} lessons";
    public int ProgressPercent => (int)(Progress * 100);
}
