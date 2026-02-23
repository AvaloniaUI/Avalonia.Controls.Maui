using System.Windows.Input;
using AlohaAI.Services;

namespace AlohaAI.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private readonly IProgressService _progressService;

    private bool _isDarkMode;
    public bool IsDarkMode
    {
        get => _isDarkMode;
        set
        {
            if (SetProperty(ref _isDarkMode, value))
                ApplyTheme();
        }
    }

    public ICommand GoBackCommand { get; }
    public ICommand ResetProgressCommand { get; }
    public ICommand OpenGitHubCommand { get; }
    public ICommand ShowOnboardingCommand { get; }

    public SettingsViewModel(IProgressService progressService)
    {
        _progressService = progressService;
        Title = "Settings";
        _isDarkMode = Application.Current?.RequestedTheme == AppTheme.Dark;
        GoBackCommand = new AsyncRelayCommand(async () => await Shell.Current.GoToAsync(".."));
        ResetProgressCommand = new AsyncRelayCommand(ResetProgressAsync);
        OpenGitHubCommand = new AsyncRelayCommand(async () =>
        {
            try { await Browser.Default.OpenAsync("https://github.com/kubaflo/AlohaAI", BrowserLaunchMode.SystemPreferred); }
            catch { /* browser not available */ }
        });
        ShowOnboardingCommand = new AsyncRelayCommand(ShowOnboardingAsync);
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

    private void ApplyTheme()
    {
        if (Application.Current != null)
            Application.Current.UserAppTheme = IsDarkMode ? AppTheme.Dark : AppTheme.Light;
    }
}
