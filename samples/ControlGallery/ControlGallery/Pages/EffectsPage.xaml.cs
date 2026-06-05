using ControlGallery.Effects;

namespace ControlGallery.Pages;

public partial class EffectsPage : ContentPage
{
    public EffectsPage()
    {
        InitializeComponent();
        UpdateDiagnostics();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        FocusPlatformEffect.DiagnosticsChanged += OnEffectDiagnosticsChanged;
        UpdateDiagnostics();
    }

    protected override void OnDisappearing()
    {
        FocusPlatformEffect.DiagnosticsChanged -= OnEffectDiagnosticsChanged;
        base.OnDisappearing();
    }

    private void OnEffectDiagnosticsChanged(object? sender, EventArgs e)
    {
        UpdateDiagnostics();
    }

    private void UpdateDiagnostics()
    {
        EffectStatusLabel.Text = FocusPlatformEffect.AttachedCount == 0
            ? $"Effects.Count={FocusEffectEntry.Effects.Count}; PlatformEffect not attached"
            : $"Effects.Count={FocusEffectEntry.Effects.Count}; PlatformEffect attached";
    }
}
