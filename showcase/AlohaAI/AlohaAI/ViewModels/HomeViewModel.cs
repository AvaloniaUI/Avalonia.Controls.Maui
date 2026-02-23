using System.Collections.ObjectModel;
using System.Windows.Input;
using AlohaAI.Services;

namespace AlohaAI.ViewModels;

public class HomePathItem
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconImage { get; set; } = "icon_books.png";
    public Color Color { get; set; } = Colors.Blue;
    public int CompletedLessons { get; set; }
    public int TotalLessons { get; set; }
    public double Progress { get; set; }
    public string ProgressText => $"{CompletedLessons}/{TotalLessons} Lessons";
    public string LevelTag { get; set; } = "Beginner";
    public string ContinueText { get; set; } = string.Empty;
    public bool HasContinueText => !string.IsNullOrEmpty(ContinueText);
}

public class HomeViewModel : BaseViewModel
{
    private readonly IContentService _contentService;
    private readonly IProgressService _progressService;
    private readonly IStreakService _streakService;

    public ObservableCollection<HomePathItem> PathItems { get; } = [];

    private int _currentStreak;
    public int CurrentStreak
    {
        get => _currentStreak;
        set => SetProperty(ref _currentStreak, value);
    }

    private int _totalXp;
    public int TotalXp
    {
        get => _totalXp;
        set => SetProperty(ref _totalXp, value);
    }

    private int _todayLessons;
    public int TodayLessons
    {
        get => _todayLessons;
        set => SetProperty(ref _todayLessons, value);
    }

    private string _continuePathId = string.Empty;
    public string ContinuePathId
    {
        get => _continuePathId;
        set => SetProperty(ref _continuePathId, value);
    }

    private string _continuePathTitle = string.Empty;
    public string ContinuePathTitle
    {
        get => _continuePathTitle;
        set => SetProperty(ref _continuePathTitle, value);
    }

    private string _continueLessonTitle = string.Empty;
    public string ContinueLessonTitle
    {
        get => _continueLessonTitle;
        set => SetProperty(ref _continueLessonTitle, value);
    }

    private bool _hasContinue;
    public bool HasContinue
    {
        get => _hasContinue;
        set => SetProperty(ref _hasContinue, value);
    }

    private double _dailyProgress;
    public double DailyProgress
    {
        get => _dailyProgress;
        set => SetProperty(ref _dailyProgress, value);
    }

    private bool _dailyGoalMet;
    public bool DailyGoalMet
    {
        get => _dailyGoalMet;
        set => SetProperty(ref _dailyGoalMet, value);
    }

    private string _welcomeText = "Aloha! Welcome back!";
    public string WelcomeText
    {
        get => _welcomeText;
        set => SetProperty(ref _welcomeText, value);
    }

    private string _dailyTip = string.Empty;
    public string DailyTip
    {
        get => _dailyTip;
        set => SetProperty(ref _dailyTip, value);
    }

    private string _dailyTipCategory = string.Empty;
    public string DailyTipCategory
    {
        get => _dailyTipCategory;
        set => SetProperty(ref _dailyTipCategory, value);
    }

    private string _wordOfDay = string.Empty;
    public string WordOfDay
    {
        get => _wordOfDay;
        set => SetProperty(ref _wordOfDay, value);
    }

    private string _wordDefinition = string.Empty;
    public string WordDefinition
    {
        get => _wordDefinition;
        set => SetProperty(ref _wordDefinition, value);
    }

    public ICommand LoadDataCommand { get; }
    public ICommand NavigateToPathsCommand { get; }
    public ICommand ContinueLearningCommand { get; }
    public ICommand NavigateToPathCommand { get; }
    public ICommand NavigateToSettingsCommand { get; }

    public HomeViewModel(IContentService contentService, IProgressService progressService, IStreakService streakService)
    {
        _contentService = contentService;
        _progressService = progressService;
        _streakService = streakService;
        Title = "Home";

        // Pre-compute daily content synchronously (deterministic, no I/O)
        LoadDailyContent();

        LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
        NavigateToPathsCommand = new AsyncRelayCommand(async () => await Shell.Current.GoToAsync("//paths"));
        NavigateToPathCommand = new AsyncRelayCommand<string>(async pathId =>
        {
            if (!string.IsNullOrEmpty(pathId))
                await Shell.Current.GoToAsync($"pathdetail?pathId={pathId}");
        });
        ContinueLearningCommand = new AsyncRelayCommand(async () =>
        {
            if (!string.IsNullOrEmpty(ContinuePathId))
                await Shell.Current.GoToAsync($"pathdetail?pathId={ContinuePathId}");
        });
        NavigateToSettingsCommand = new AsyncRelayCommand(async () =>
        {
            await Shell.Current.GoToAsync("settings");
        });
    }

    private async Task LoadDataAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            await _progressService.InitializeAsync();

            // Personalize greeting
            var savedName = await _progressService.GetSettingAsync("user_display_name");
            WelcomeText = string.IsNullOrEmpty(savedName)
                ? "Aloha! Welcome back!"
                : $"Aloha, {savedName}!";

            CurrentStreak = await _streakService.GetCurrentStreakAsync();
            TotalXp = await _progressService.GetTotalXpAsync();
            TodayLessons = await _streakService.GetTodayLessonsCountAsync();
            DailyProgress = Math.Min(1.0, TodayLessons / 3.0);
            DailyGoalMet = TodayLessons >= 3;

            var lastLesson = await _progressService.GetLastCompletedLessonAsync();
            if (lastLesson != null)
            {
                ContinuePathId = lastLesson.PathId;
                var path = await _contentService.GetPathAsync(lastLesson.PathId);
                ContinuePathTitle = path?.Title ?? "Continue Learning";
                HasContinue = true;

                // Find next uncompleted lesson in the path
                if (path != null)
                {
                    var modules = await _contentService.GetModulesAsync(path.Id);
                    foreach (var module in modules)
                    {
                        foreach (var lesson in module.Lessons)
                        {
                            var lessonId = System.IO.Path.GetFileNameWithoutExtension(lesson.File);
                            var completed = await _progressService.IsLessonCompletedAsync(path.Id, module.Id, lessonId);
                            if (!completed)
                            {
                                ContinueLessonTitle = lesson.Title;
                                goto foundNext;
                            }
                        }
                    }
                    ContinueLessonTitle = "All lessons complete!";
                    foundNext:;
                }
            }

            // Load path items with progress
            var paths = await _contentService.GetPathsAsync();
            PathItems.Clear();
            foreach (var path in paths)
            {
                var modules = await _contentService.GetModulesAsync(path.Id);
                var totalLessons = modules.Sum(m => m.Lessons.Count);
                var progress = await _progressService.GetPathProgressAsync(path.Id, totalLessons);
                var completedLessons = await _progressService.GetCompletedLessonCountAsync(path.Id);

                PathItems.Add(new HomePathItem
                {
                    Id = path.Id,
                    Title = path.Title,
                    Description = path.Description,
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
                    Color = Color.FromArgb(path.Color),
                    CompletedLessons = completedLessons,
                    TotalLessons = totalLessons,
                    Progress = progress,
                    LevelTag = path.Id switch
                    {
                        "agentic-ai" => "Intermediate",
                        "ml-fundamentals" => "Beginner",
                        "ai-in-practice" => "Intermediate",
                        "prompt-engineering" => "Intermediate",
                        "vision-multimodal" => "Advanced",
                        "generative-ai" => "Intermediate",
                        "ai-safety" => "Beginner",
                        "mlops-engineering" => "Advanced",
                        _ => "Beginner"
                    },
                    ContinueText = await GetContinueTextAsync(path.Id, modules)
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading home data: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

        private void LoadDailyContent()
        {
            var dayIndex = DateTime.Now.DayOfYear;
            var tips = new (string Category, string Tip)[]
            {
                ("Prompt Engineering", "Be specific in your prompts — instead of \"write code\", try \"write a Python function that sorts a list using merge sort\"."),
                ("ML Basics", "Overfitting happens when your model memorizes training data instead of learning patterns. Use validation sets to detect it early."),
                ("AI Safety", "Always test AI outputs for bias before deploying. Small biases in training data can amplify into large real-world harms."),
                ("Agentic AI", "The ReAct pattern (Reasoning + Acting) lets AI agents think step-by-step before taking actions, improving accuracy."),
                ("Deep Learning", "Transfer learning lets you use a pre-trained model as a starting point — saving hours of training time and less data."),
                ("MLOps", "Version your datasets just like you version your code. Data drift is a top cause of model degradation in production."),
                ("Generative AI", "Temperature controls randomness in LLM outputs. Use 0 for factual tasks, 0.7-1.0 for creative writing."),
                ("Computer Vision", "Data augmentation (flipping, rotating, cropping) can effectively 10x your training dataset without collecting new data."),
                ("NLP", "Embeddings convert words into vectors where similar meanings are close together — the foundation of modern search and RAG."),
                ("AI Ethics", "Explainability matters. If you can't explain why your model made a decision, don't deploy it in high-stakes scenarios."),
                ("RAG", "Retrieval-Augmented Generation reduces hallucinations by grounding LLM responses in actual documents from your knowledge base."),
                ("Fine-tuning", "LoRA lets you fine-tune large models by training only 0.1% of parameters — making it fast and affordable."),
            };
            var words = new (string Word, string Definition)[]
            {
                ("Hallucination", "When an AI model generates plausible-sounding but factually incorrect or fabricated information."),
                ("Embedding", "A numerical vector representation of data that captures semantic meaning in a high-dimensional space."),
                ("Fine-tuning", "Adapting a pre-trained model to a specific task by training it further on a smaller, specialized dataset."),
                ("Tokenization", "Breaking text into smaller units (tokens) that a language model can process — words, subwords, or characters."),
                ("Inference", "The process of using a trained model to make predictions on new, unseen data."),
                ("Attention", "A mechanism that lets neural networks focus on the most relevant parts of the input when generating output."),
                ("Latent Space", "A compressed, abstract representation of data learned by a model, where similar items cluster together."),
                ("Epoch", "One complete pass through the entire training dataset during model training."),
                ("Transformer", "A neural network architecture based on self-attention, powering GPT, BERT, and most modern LLMs."),
                ("Gradient Descent", "An optimization algorithm that iteratively adjusts model parameters to minimize the loss function."),
                ("Retrieval", "Fetching relevant documents from a knowledge base to provide context for AI generation (used in RAG)."),
                ("Quantization", "Reducing model precision (e.g. 32-bit to 4-bit) to shrink size and speed up inference with minimal quality loss."),
            };
            var tip = tips[dayIndex % tips.Length];
            DailyTip = tip.Tip;
            DailyTipCategory = tip.Category;
            var word = words[dayIndex % words.Length];
            WordOfDay = word.Word;
            WordDefinition = word.Definition;
        }

    private async Task<string> GetContinueTextAsync(string pathId, List<Models.Module> modules)
    {
        foreach (var module in modules)
        {
            foreach (var lesson in module.Lessons)
            {
                var lessonId = System.IO.Path.GetFileNameWithoutExtension(lesson.File);
                var completed = await _progressService.IsLessonCompletedAsync(pathId, module.Id, lessonId);
                if (!completed)
                    return $"Continue \"{lesson.Title}\"";
            }
        }
        return string.Empty;
    }
}
