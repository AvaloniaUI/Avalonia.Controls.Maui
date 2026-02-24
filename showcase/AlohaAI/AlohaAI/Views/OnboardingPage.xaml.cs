using AlohaAI.Services;

namespace AlohaAI.Views;

public partial class OnboardingPage : ContentPage
{
    private readonly IProgressService _progressService;
    private int _currentStep;
    private readonly VerticalStackLayout[] _steps;
    private readonly Border[] _dots;
    private string? _pickedPhotoPath;

    public OnboardingPage(IProgressService progressService)
    {
        InitializeComponent();
        _progressService = progressService;
        _steps = [Step0, Step1, Step2, Step3, Step4];
        _dots = [Dot0, Dot1, Dot2, Dot3, Dot4];
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Opacity = 0;
        await this.FadeToAsync(1, 500, Easing.CubicOut);
    }

    private async void OnPickPhotoClicked(object? sender, EventArgs e)
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

            _pickedPhotoPath = destPath;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error picking photo: {ex.Message}");
        }
    }

    private async void OnNextClicked(object? sender, EventArgs e)
    {
        if (_currentStep < _steps.Length - 1)
        {
            var outgoing = _steps[_currentStep];
            _currentStep++;
            var incoming = _steps[_currentStep];

            // Animate out
            await outgoing.FadeToAsync(0, 200, Easing.CubicIn);
            outgoing.IsVisible = false;

            // Update dots
            for (var i = 0; i < _dots.Length; i++)
            {
                _dots[i].BackgroundColor = i == _currentStep ? Colors.White : Color.FromArgb("#FFFFFF40");
                _dots[i].WidthRequest = i == _currentStep ? 24 : 8;
            }

            // Animate in
            incoming.Opacity = 0;
            incoming.TranslationX = 60;
            incoming.IsVisible = true;
            await Task.WhenAll(
                incoming.FadeToAsync(1, 300, Easing.CubicOut),
                incoming.TranslateToAsync(0, 0, 300, Easing.CubicOut)
            );

            // Update button on last step
            if (_currentStep == _steps.Length - 1)
                NextButton.Text = "Get Started →";
            else if (_currentStep == _steps.Length - 2)
                NextButton.Text = "Set Up Profile →";
        }
        else
        {
            // Save profile data before finishing
            var name = NameEntry?.Text?.Trim();
            if (!string.IsNullOrEmpty(name))
                await _progressService.SaveSettingAsync("user_display_name", name);
            if (!string.IsNullOrEmpty(_pickedPhotoPath))
                await _progressService.SaveSettingAsync("user_profile_image", _pickedPhotoPath);

            await _progressService.SaveSettingAsync("onboarding_completed", "true");
            if (Application.Current?.Windows.Count > 0)
                Application.Current.Windows[0].Page = new AppShell();
        }
    }
}
