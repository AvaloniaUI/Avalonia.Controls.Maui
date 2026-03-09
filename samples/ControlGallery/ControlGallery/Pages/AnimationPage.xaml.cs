namespace ControlGallery.Pages;

public partial class AnimationPage : ContentPage
{
	public AnimationPage()
	{
		InitializeComponent();
	}

    // Rotation
    private async void OnRotate360Clicked(object? sender, EventArgs e)
    {
        await RotateImage.RotateTo(360, 2000);
        RotateImage.Rotation = 0; // Reset silently for re-run
    }

    private async void OnRotateX360Clicked(object? sender, EventArgs e)
    {
        await RotateImage.RotateXTo(360, 2000);
        RotateImage.RotationX = 0;
    }

    private async void OnRotateY360Clicked(object? sender, EventArgs e)
    {
        await RotateImage.RotateYTo(360, 2000);
        RotateImage.RotationY = 0;
    }

    private async void OnRotateResetClicked(object? sender, EventArgs e)
    {
        // Cancel any running animations if possible (MAUI doesn't strictly have Cancel without tokens in ViewExtensions easily, but setting property often stops it or jumps)
        // Ideally we just reset properties
        Microsoft.Maui.Controls.ViewExtensions.CancelAnimations(RotateImage);
        RotateImage.Rotation = 0;
        RotateImage.RotationX = 0;
        RotateImage.RotationY = 0;
    }

    // Scaling
    private async void OnScale2xClicked(object? sender, EventArgs e)
    {
        await ScaleBox.ScaleTo(2.0, 1000, Easing.SpringOut);
    }

    private async void OnScaleHalfClicked(object? sender, EventArgs e)
    {
        await ScaleBox.ScaleTo(0.5, 1000, Easing.CubicOut);
    }

    private async void OnScalePulsateClicked(object? sender, EventArgs e)
    {
        await ScaleBox.ScaleTo(1.2, 200);
        await ScaleBox.ScaleTo(0.8, 200);
        await ScaleBox.ScaleTo(1.0, 200);
    }

    private void OnScaleResetClicked(object? sender, EventArgs e)
    {
        Microsoft.Maui.Controls.ViewExtensions.CancelAnimations(ScaleBox);
        ScaleBox.Scale = 1.0;
    }

    // Translation
    private async void OnTranslateLeftClicked(object? sender, EventArgs e)
    {
        await TranslateLabel.TranslateTo(-50, 0, 500, Easing.Linear);
    }

    private async void OnTranslateRightClicked(object? sender, EventArgs e)
    {
        await TranslateLabel.TranslateTo(50, 0, 500, Easing.Linear);
    }

    private async void OnTranslateUpClicked(object? sender, EventArgs e)
    {
        await TranslateLabel.TranslateTo(0, -50, 500, Easing.Linear);
    }

    private async void OnTranslateDownClicked(object? sender, EventArgs e)
    {
        await TranslateLabel.TranslateTo(0, 50, 500, Easing.Linear);
    }

    private async void OnTranslateShakeClicked(object? sender, EventArgs e)
    {
        uint duration = 50;
        for (int i = 0; i < 3; i++)
        {
            await TranslateLabel.TranslateTo(10, 0, duration);
            await TranslateLabel.TranslateTo(-10, 0, duration);
        }
        await TranslateLabel.TranslateTo(0, 0, duration);
    }

    private void OnTranslateResetClicked(object? sender, EventArgs e)
    {
        Microsoft.Maui.Controls.ViewExtensions.CancelAnimations(TranslateLabel);
        TranslateLabel.TranslationX = 0;
        TranslateLabel.TranslationY = 0;
    }

    // Fading
    private async void OnFadeOutClicked(object? sender, EventArgs e)
    {
        await FadeImage.FadeTo(0, 1000);
    }

    private async void OnFadeInClicked(object? sender, EventArgs e)
    {
        await FadeImage.FadeTo(1, 1000);
    }

    // Compound
    private async void OnCompoundAnimationClicked(object? sender, EventArgs e)
    {
        // Reset first
        CompoundBox.Rotation = 0;
        CompoundBox.Scale = 1;
        CompoundBox.TranslationX = 0;
        CompoundBox.Opacity = 1;

        // Run multiple animations concurrently
        var rotateTask = CompoundBox.RotateTo(360, 2000, Easing.BounceOut);
        var scaleTask = CompoundBox.ScaleTo(1.5, 2000, Easing.BounceOut);
        var fadeTask = CompoundBox.FadeTo(0.5, 2000);

        await Task.WhenAll(rotateTask, scaleTask, fadeTask);

        // Then bring it back
        await CompoundBox.FadeTo(1, 500);
        await CompoundBox.ScaleTo(1, 500);
    }
}
