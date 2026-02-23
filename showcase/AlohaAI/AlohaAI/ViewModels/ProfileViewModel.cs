using System.Collections.ObjectModel;
using System.Windows.Input;
using AlohaAI.Services;

namespace AlohaAI.ViewModels;

public class ProfileViewModel : BaseViewModel
{
    private readonly IContentService _contentService;
    private readonly IProgressService _progressService;
    private readonly IStreakService _streakService;

    private int _totalXp;
    public int TotalXp
    {
        get => _totalXp;
        set => SetProperty(ref _totalXp, value);
    }

    private int _currentStreak;
    public int CurrentStreak
    {
        get => _currentStreak;
        set => SetProperty(ref _currentStreak, value);
    }

    private int _bestStreak;
    public int BestStreak
    {
        get => _bestStreak;
        set => SetProperty(ref _bestStreak, value);
    }

    private int _lessonsCompleted;
    public int LessonsCompleted
    {
        get => _lessonsCompleted;
        set => SetProperty(ref _lessonsCompleted, value);
    }

    private int _level;
    public int Level
    {
        get => _level;
        set => SetProperty(ref _level, value);
    }

    private string _displayName = "AI Learner";
    public string DisplayName
    {
        get => _displayName;
        set => SetProperty(ref _displayName, value);
    }

    private ImageSource? _profileImageSource;
    public ImageSource ProfileImageSource
    {
        get => _profileImageSource ?? ImageSource.FromFile("icon_island.png");
        set { SetProperty(ref _profileImageSource, value); OnPropertyChanged(nameof(HasProfileImage)); }
    }

    public bool HasProfileImage => _profileImageSource != null;

    private bool _showProfileSetup;
    public bool ShowProfileSetup
    {
        get => _showProfileSetup;
        set => SetProperty(ref _showProfileSetup, value);
    }

    private string _editName = "";
    public string EditName
    {
        get => _editName;
        set => SetProperty(ref _editName, value);
    }

    private bool _isEditingProfile;
    public bool IsEditingProfile
    {
        get => _isEditingProfile;
        set => SetProperty(ref _isEditingProfile, value);
    }

    private int _dailyGoal = 3;
    public int DailyGoal
    {
        get => _dailyGoal;
        set => SetProperty(ref _dailyGoal, value);
    }

    private int _dailyProgress;
    public int DailyProgress
    {
        get => _dailyProgress;
        set => SetProperty(ref _dailyProgress, value);
    }

    private double _dailyGoalFraction;
    public double DailyGoalFraction
    {
        get => _dailyGoalFraction;
        set => SetProperty(ref _dailyGoalFraction, value);
    }

    private int _totalLessonsAvailable;
    public int TotalLessonsAvailable
    {
        get => _totalLessonsAvailable;
        set => SetProperty(ref _totalLessonsAvailable, value);
    }

    public ObservableCollection<PathProgressItem> PathProgress { get; } = [];
    public ObservableCollection<AchievementItem> Achievements { get; } = [];

    public ICommand LoadDataCommand { get; }
    public ICommand OpenSettingsCommand { get; }
    public ICommand SaveProfileCommand { get; }
    public ICommand PickPhotoCommand { get; }
    public ICommand EditProfileCommand { get; }
    public ICommand CancelEditCommand { get; }

    public ProfileViewModel(IContentService contentService, IProgressService progressService, IStreakService streakService)
    {
        _contentService = contentService;
        _progressService = progressService;
        _streakService = streakService;
        Title = "Profile";

        LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
        OpenSettingsCommand = new AsyncRelayCommand(async () => await Shell.Current.GoToAsync("settings"));
        SaveProfileCommand = new AsyncRelayCommand(SaveProfileAsync);
        PickPhotoCommand = new AsyncRelayCommand(PickPhotoAsync);
        EditProfileCommand = new RelayCommand(() =>
        {
            EditName = DisplayName == "AI Learner" ? "" : DisplayName;
            IsEditingProfile = true;
        });
        CancelEditCommand = new RelayCommand(() =>
        {
            IsEditingProfile = false;
            ShowProfileSetup = false;
        });
    }

    private async Task LoadDataAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            // Load profile info
            var savedName = await _progressService.GetSettingAsync("user_display_name");
            if (!string.IsNullOrEmpty(savedName))
            {
                DisplayName = savedName;
                ShowProfileSetup = false;
            }
            else
            {
                ShowProfileSetup = true;
            }

            var savedImage = await _progressService.GetSettingAsync("user_profile_image");
            if (!string.IsNullOrEmpty(savedImage) && File.Exists(savedImage))
                ProfileImageSource = ImageSource.FromFile(savedImage);

            TotalXp = await _progressService.GetTotalXpAsync();
            CurrentStreak = await _streakService.GetCurrentStreakAsync();
            BestStreak = await _streakService.GetBestStreakAsync();
            Level = TotalXp / 100 + 1;

            var paths = await _contentService.GetPathsAsync();
            var totalCompleted = 0;
            PathProgress.Clear();

            foreach (var path in paths)
            {
                var modules = await _contentService.GetModulesAsync(path.Id);
                var totalLessons = modules.Sum(m => m.Lessons.Count);
                var completedLessons = await _progressService.GetCompletedLessonCountAsync(path.Id);
                totalCompleted += completedLessons;
                var progress = totalLessons > 0 ? (double)completedLessons / totalLessons : 0;

                PathProgress.Add(new PathProgressItem
                {
                    Title = path.Title,
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
                    CompletedLessons = completedLessons,
                    TotalLessons = totalLessons,
                    Progress = progress,
                    BarColor = Color.FromArgb(path.Color),
                    Modules = new ObservableCollection<ModuleProgressItem>(
                        modules.Select(m =>
                        {
                            var modCompleted = m.Lessons.Count(l => completedLessons > 0); // simplified
                            return new ModuleProgressItem
                            {
                                Title = m.Title,
                                ProgressText = $"{Math.Min(completedLessons, m.Lessons.Count)}/{m.Lessons.Count}",
                                Progress = m.Lessons.Count > 0 ? Math.Min(1.0, (double)completedLessons / m.Lessons.Count) : 0,
                                BarColor = Color.FromArgb(path.Color)
                            };
                        })
                    )
                });
            }

            LessonsCompleted = totalCompleted;
            TotalLessonsAvailable = PathProgress.Sum(p => p.TotalLessons);

            // Daily progress: simulate based on today's completed lessons (min of goal)
            DailyProgress = Math.Min(totalCompleted, DailyGoal);
            DailyGoalFraction = DailyGoal > 0 ? (double)DailyProgress / DailyGoal : 0;

            // Compute achievements
            Achievements.Clear();
            Achievements.Add(new AchievementItem
            {
                Icon = "icon_rocket.png",
                Title = "First Steps",
                Description = "Complete your first lesson",
                IsUnlocked = totalCompleted >= 1
            });
            Achievements.Add(new AchievementItem
            {
                Icon = "icon_notebook.png",
                Title = "Curious Mind",
                Description = "Complete 10 lessons",
                IsUnlocked = totalCompleted >= 10
            });
            Achievements.Add(new AchievementItem
            {
                Icon = "icon_trophy.png",
                Title = "Knowledge Seeker",
                Description = "Complete 25 lessons",
                IsUnlocked = totalCompleted >= 25
            });
            Achievements.Add(new AchievementItem
            {
                Icon = "icon_gem.png",
                Title = "Streak Starter",
                Description = "Reach a 3-day streak",
                IsUnlocked = BestStreak >= 3
            });
            Achievements.Add(new AchievementItem
            {
                Icon = "icon_island.png",
                Title = "Week Warrior",
                Description = "Reach a 7-day streak",
                IsUnlocked = BestStreak >= 7
            });
            Achievements.Add(new AchievementItem
            {
                Icon = "icon_coins.png",
                Title = "XP Hunter",
                Description = "Earn 500 XP",
                IsUnlocked = TotalXp >= 500
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading profile: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SaveProfileAsync()
    {
        var name = EditName?.Trim();
        if (string.IsNullOrEmpty(name)) return;

        await _progressService.SaveSettingAsync("user_display_name", name);
        DisplayName = name;
        ShowProfileSetup = false;
        IsEditingProfile = false;
    }

    private async Task PickPhotoAsync()
    {
        try
        {
            var result = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Choose Profile Photo"
            });

            if (result == null) return;

            var destPath = Path.Combine(FileSystem.AppDataDirectory, "profile_photo.jpg");
            using var sourceStream = await result.OpenReadAsync();
            using var destStream = File.OpenWrite(destPath);
            await sourceStream.CopyToAsync(destStream);

            await _progressService.SaveSettingAsync("user_profile_image", destPath);
            ProfileImageSource = ImageSource.FromFile(destPath);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error picking photo: {ex.Message}");
        }
    }
}

public class PathProgressItem
{
    public string Title { get; set; } = string.Empty;
    public string IconImage { get; set; } = "icon_books.png";
    public int CompletedLessons { get; set; }
    public int TotalLessons { get; set; }
    public double Progress { get; set; }
    public Color BarColor { get; set; } = Colors.Blue;
    public string ProgressText => $"{CompletedLessons}/{TotalLessons}";
    public string PercentText => $"{(int)(Progress * 100)}%";
    public ObservableCollection<ModuleProgressItem> Modules { get; set; } = [];
}

public class AchievementItem
{
    public string Icon { get; set; } = "icon_trophy.png";
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsUnlocked { get; set; }
    public double IconOpacity => IsUnlocked ? 1.0 : 0.3;
    public Color TextColor => IsUnlocked ? Colors.White : Color.FromArgb("#666666");
    public Color DescColor => IsUnlocked ? Color.FromArgb("#AAAAAA") : Color.FromArgb("#444444");
    public string StatusText => IsUnlocked ? "Unlocked" : "Locked";
}

public class ModuleProgressItem
{
    public string Title { get; set; } = string.Empty;
    public string ProgressText { get; set; } = string.Empty;
    public double Progress { get; set; }
    public Color BarColor { get; set; } = Colors.Blue;
}
