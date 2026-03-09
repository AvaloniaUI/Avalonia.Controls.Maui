using AlohaAI.Services;
using AlohaAI.ViewModels;

namespace AlohaAI.Views;

public partial class LoadingPage : ContentPage
{
    private readonly IProgressService _progressService;

    public LoadingPage(IProgressService progressService)
    {
        InitializeComponent();
        _progressService = progressService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await _progressService.InitializeAsync();
            var onboardingDone = await _progressService.GetSettingAsync("onboarding_completed");

            if (onboardingDone == "true")
            {
                // Pre-load home data before navigating
                var homeVm = IPlatformApplication.Current!.Services.GetRequiredService<HomeViewModel>();
                homeVm.LoadDataCommand.Execute(null);
                await Task.Delay(1500);
            }

            Application.Current.Dispatcher.Dispatch(() =>
            {
                if (Application.Current?.Windows.Count > 0)
                {
                    if (onboardingDone == "true")
                        Application.Current.Windows[0].Page = new AppShell();
                    else
                        Application.Current.Windows[0].Page = new OnboardingPage(_progressService);
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Loading error: {ex.Message}");
            Application.Current.Dispatcher.Dispatch(() =>
            {
                if (Application.Current?.Windows.Count > 0)
                    Application.Current.Windows[0].Page = new AppShell();
            });
        }
    }
}
