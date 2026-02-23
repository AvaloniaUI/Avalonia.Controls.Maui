using System.Windows.Input;
using AlohaAI.Services;

namespace AlohaAI.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private readonly IProgressService _progressService;

    private int _selectedThemeIndex;
    public int SelectedThemeIndex
    {
        get => _selectedThemeIndex;
        set
        {
            if (SetProperty(ref _selectedThemeIndex, value))
                ApplyAndSaveTheme();
        }
    }

    public List<string> ThemeOptions { get; } = ["System", "Light", "Dark"];

    public ICommand GoBackCommand { get; }
    public ICommand ResetProgressCommand { get; }
    public ICommand OpenGitHubCommand { get; }
    public ICommand ShowOnboardingCommand { get; }

    public SettingsViewModel(IProgressService progressService)
    {
        _progressService = progressService;
        Title = "Settings";

        // Load saved theme preference
        _ = LoadThemePreferenceAsync();

        GoBackCommand = new AsyncRelayCommand(async () => await Shell.Current.GoToAsync(".."));
        ResetProgressCommand = new AsyncRelayCommand(ResetProgressAsync);
        OpenGitHubCommand = new AsyncRelayCommand(async () =>
        {
            try { await Browser.Default.OpenAsync("https://github.com/kubaflo/AlohaAI", BrowserLaunchMode.SystemPreferred); }
            catch { /* browser not available */ }
        });
        ShowOnboardingCommand = new AsyncRelayCommand(ShowOnboardingAsync);
    }

    private async Task LoadThemePreferenceAsync()
    {
        try
        {
            var saved = await _progressService.GetSettingAsync("app_theme");
            _selectedThemeIndex = saved switch
            {
                "light" => 1,
                "dark" => 2,
                _ => 0 // system
            };
            OnPropertyChanged(nameof(SelectedThemeIndex));
        }
        catch
        {
            // Default to system
        }
    }

    private async Task ResetProgressAsync()
    {
        var confirmed = await Shell.Current.DisplayAlertAsync(
            "Reset Progress",
            "This will delete all your lesson progress, quiz scores, and streak data. This cannot be undone.",
            "Reset",
            "Cancel");

        if (confirmed)
        {
            await _progressService.ResetAllAsync();
            await Shell.Current.DisplayAlertAsync("Done", "All progress has been reset.", "OK");
        }
    }

    private async Task ShowOnboardingAsync()
    {
        await _progressService.SaveSettingAsync("onboarding_completed", "false");
        if (Application.Current?.Windows.Count > 0)
            Application.Current.Windows[0].Page = new Views.OnboardingPage(_progressService);
    }

    private async void ApplyAndSaveTheme()
    {
        var themeValue = _selectedThemeIndex switch
        {
            1 => "light",
            2 => "dark",
            _ => "system"
        };

        if (Application.Current != null)
        {
            Application.Current.UserAppTheme = _selectedThemeIndex switch
            {
                1 => AppTheme.Light,
                2 => AppTheme.Dark,
                _ => AppTheme.Unspecified
            };
        }

        try
        {
            await _progressService.SaveSettingAsync("app_theme", themeValue);
        }
        catch
        {
            // Non-critical save failure
        }
    }
}
