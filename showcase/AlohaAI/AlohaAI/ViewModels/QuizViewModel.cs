using System.Collections.ObjectModel;
using System.Windows.Input;
using AlohaAI.Models;
using AlohaAI.Services;

namespace AlohaAI.ViewModels;

[QueryProperty(nameof(PathId), "pathId")]
[QueryProperty(nameof(ModuleId), "moduleId")]
public class QuizViewModel : BaseViewModel
{
    private readonly IContentService _contentService;
    private readonly IProgressService _progressService;
    private readonly IStreakService _streakService;

    private Quiz? _quiz;
    private int _currentIndex;
    private int _correctCount;

    private string _pathId = string.Empty;
    public string PathId
    {
        get => _pathId;
        set { SetProperty(ref _pathId, value); UpdateHeaderImage(); }
    }

    private string _headerImage = "header_agentic_ai.jpg";
    public string HeaderImage
    {
        get => _headerImage;
        set => SetProperty(ref _headerImage, value);
    }

    private void UpdateHeaderImage()
    {
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

    private string _moduleId = string.Empty;
    public string ModuleId
    {
        get => _moduleId;
        set { SetProperty(ref _moduleId, value); LoadQuizCommand.Execute(null); }
    }

    private string _questionText = string.Empty;
    public string QuestionText
    {
        get => _questionText;
        set => SetProperty(ref _questionText, value);
    }

    private string _progressText = string.Empty;
    public string ProgressText
    {
        get => _progressText;
        set => SetProperty(ref _progressText, value);
    }

    private double _progressValue;
    public double ProgressValue
    {
        get => _progressValue;
        set => SetProperty(ref _progressValue, value);
    }

    private string _quizTitle = "Quiz";
    public string QuizTitle
    {
        get => _quizTitle;
        set => SetProperty(ref _quizTitle, value);
    }

    private string _explanationText = string.Empty;
    public string ExplanationText
    {
        get => _explanationText;
        set => SetProperty(ref _explanationText, value);
    }

    private bool _showExplanation;
    public bool ShowExplanation
    {
        get => _showExplanation;
        set => SetProperty(ref _showExplanation, value);
    }

    private bool _showResults;
    public bool ShowResults
    {
        get => _showResults;
        set => SetProperty(ref _showResults, value);
    }

    private bool _showQuestion = true;
    public bool ShowQuestion
    {
        get => _showQuestion;
        set => SetProperty(ref _showQuestion, value);
    }

    private int _score;
    public int Score
    {
        get => _score;
        set => SetProperty(ref _score, value);
    }

    private int _totalQuestions;
    public int TotalQuestions
    {
        get => _totalQuestions;
        set => SetProperty(ref _totalQuestions, value);
    }

    private int _xpEarned;
    public int XpEarned
    {
        get => _xpEarned;
        set => SetProperty(ref _xpEarned, value);
    }

    private bool _passed;
    public bool Passed
    {
        get => _passed;
        set => SetProperty(ref _passed, value);
    }

    public ObservableCollection<QuizOptionItem> Options { get; } = [];

    public ICommand LoadQuizCommand { get; }
    public ICommand SelectOptionCommand { get; }
    public ICommand NextQuestionCommand { get; }
    public ICommand FinishCommand { get; }
    public ICommand GoBackCommand { get; }

    public string ResultIcon => Passed ? "icon_trophy.png" : "icon_books.png";
    public string ResultTitle => Passed ? "Quiz Passed!" : "Keep Learning!";
    public string ResultSubtitle => Passed
        ? $"Great job! You got {Score} out of {TotalQuestions} correct."
        : $"You got {Score} out of {TotalQuestions}. Review the lessons and try again!";
    public string ScoreDisplay => $"{Score}/{TotalQuestions}";

    public QuizViewModel(IContentService contentService, IProgressService progressService, IStreakService streakService)
    {
        _contentService = contentService;
        _progressService = progressService;
        _streakService = streakService;

        LoadQuizCommand = new AsyncRelayCommand(LoadQuizAsync);
        SelectOptionCommand = new RelayCommand<QuizOptionItem>(SelectOption);
        NextQuestionCommand = new RelayCommand(NextQuestion);
        FinishCommand = new AsyncRelayCommand(FinishQuizAsync);
        GoBackCommand = new AsyncRelayCommand(async () => await Shell.Current.GoToAsync(".."));
    }

    private async Task LoadQuizAsync()
    {
        if (IsBusy || string.IsNullOrEmpty(ModuleId)) return;
        IsBusy = true;

        try
        {
            _quiz = await _contentService.GetQuizAsync(PathId, ModuleId);
            if (_quiz == null || _quiz.Questions.Count == 0)
            {
                await Shell.Current.GoToAsync("..");
                return;
            }

            Title = "Quiz";
            QuizTitle = _quiz.Title ?? "Quiz";
            _currentIndex = 0;
            _correctCount = 0;
            TotalQuestions = _quiz.Questions.Count;
            ShowResults = false;
            ShowQuestion = true;
            DisplayQuestion();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading quiz: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void DisplayQuestion()
    {
        if (_quiz == null || _currentIndex >= _quiz.Questions.Count) return;

        var question = _quiz.Questions[_currentIndex];
        QuestionText = question.Question;
        ProgressText = $"Question {_currentIndex + 1} of {TotalQuestions}";
        ProgressValue = (double)_currentIndex / TotalQuestions;
        ShowExplanation = false;

        Options.Clear();
        for (var i = 0; i < question.Options.Count; i++)
        {
            Options.Add(new QuizOptionItem
            {
                Index = i,
                Text = question.Options[i],
                IsCorrect = i == question.CorrectIndex,
                State = OptionState.Default
            });
        }
    }

    private void SelectOption(QuizOptionItem? option)
    {
        if (option == null || ShowExplanation || _quiz == null) return;

        var question = _quiz.Questions[_currentIndex];
        HapticFeedback.Default.Perform(HapticFeedbackType.Click);

        foreach (var opt in Options)
        {
            if (opt.IsCorrect)
                opt.State = OptionState.Correct;
            else if (opt == option && !opt.IsCorrect)
                opt.State = OptionState.Incorrect;
            else
                opt.State = OptionState.Dimmed;
        }

        if (option.IsCorrect)
            _correctCount++;

        ExplanationText = question.Explanation;
        ShowExplanation = true;
    }

    private void NextQuestion()
    {
        _currentIndex++;
        if (_quiz != null && _currentIndex < _quiz.Questions.Count)
        {
            DisplayQuestion();
        }
        else
        {
            ShowResults = true;
            ShowQuestion = false;
            Score = _correctCount;
            XpEarned = _quiz?.Questions.Sum(q => q.Xp) ?? 0;
            var scorePercent = TotalQuestions > 0 ? (double)_correctCount / TotalQuestions * 100 : 0;
            Passed = scorePercent >= (_quiz?.PassingScore ?? 70);
            OnPropertyChanged(nameof(ResultIcon));
            OnPropertyChanged(nameof(ResultTitle));
            OnPropertyChanged(nameof(ResultSubtitle));
            OnPropertyChanged(nameof(ScoreDisplay));
        }
    }

    private async Task FinishQuizAsync()
    {
        if (_quiz == null) return;

        var scorePercent = TotalQuestions > 0 ? (double)_correctCount / TotalQuestions * 100 : 0;
        await _progressService.SaveQuizScoreAsync(PathId, ModuleId, scorePercent, XpEarned);
        await _streakService.RecordActivityAsync();
        await Shell.Current.GoToAsync("..");
    }
}

public class QuizOptionItem : BaseViewModel
{
    public int Index { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }

    private OptionState _state = OptionState.Default;
    public OptionState State
    {
        get => _state;
        set
        {
            SetProperty(ref _state, value);
            OnPropertyChanged(nameof(BackgroundColor));
            OnPropertyChanged(nameof(TextOpacity));
            OnPropertyChanged(nameof(IndicatorColor));
            OnPropertyChanged(nameof(IndicatorStroke));
            OnPropertyChanged(nameof(IndicatorText));
        }
    }

    public Color BackgroundColor => State switch
    {
        OptionState.Correct => Color.FromArgb("#CC1B3D2A"),
        OptionState.Incorrect => Color.FromArgb("#CC3D1B1B"),
        OptionState.Dimmed => Color.FromArgb("#AA1A1530"),
        _ => Color.FromArgb("#DD1A1530")
    };

    public Color IndicatorColor => State switch
    {
        OptionState.Correct => Color.FromArgb("#66BB6A"),
        OptionState.Incorrect => Color.FromArgb("#EF5350"),
        _ => Colors.Transparent
    };

    public Color IndicatorStroke => State switch
    {
        OptionState.Correct => Color.FromArgb("#66BB6A"),
        OptionState.Incorrect => Color.FromArgb("#EF5350"),
        OptionState.Dimmed => Color.FromArgb("#444444"),
        _ => Color.FromArgb("#666666")
    };

    public string IndicatorText => State switch
    {
        OptionState.Correct => "✓",
        OptionState.Incorrect => "✗",
        _ => ((char)('A' + Index)).ToString()
    };

    public double TextOpacity => State == OptionState.Dimmed ? 0.4 : 1.0;
}

public enum OptionState
{
    Default,
    Correct,
    Incorrect,
    Dimmed
}
