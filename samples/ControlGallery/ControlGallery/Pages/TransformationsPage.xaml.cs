namespace ControlGallery.Pages;

public partial class TransformationsPage : ContentPage
{
    public TransformationsPage()
    {
        InitializeComponent();

        // Wire up slider events
        ScaleSlider.ValueChanged += (s, e) => UpdateTransformations();
        ScaleXSlider.ValueChanged += (s, e) => UpdateTransformations();
        ScaleYSlider.ValueChanged += (s, e) => UpdateTransformations();
        RotationSlider.ValueChanged += (s, e) => UpdateTransformations();
        RotationXSlider.ValueChanged += (s, e) => UpdateTransformations();
        RotationYSlider.ValueChanged += (s, e) => UpdateTransformations();
        TranslationXSlider.ValueChanged += (s, e) => UpdateTransformations();
        TranslationYSlider.ValueChanged += (s, e) => UpdateTransformations();
        AnchorXSlider.ValueChanged += (s, e) => UpdateTransformations();
        AnchorYSlider.ValueChanged += (s, e) => UpdateTransformations();
    }

    private void UpdateTransformations()
    {
        if (TargetImage == null) return;

        TargetImage.Scale = ScaleSlider.Value;
        TargetImage.ScaleX = ScaleSlider.Value + ScaleXSlider.Value;
        TargetImage.ScaleY = ScaleSlider.Value + ScaleYSlider.Value;
        TargetImage.Rotation = RotationSlider.Value;
        TargetImage.RotationX = RotationXSlider.Value;
        TargetImage.RotationY = RotationYSlider.Value;
        TargetImage.TranslationX = TranslationXSlider.Value;
        TargetImage.TranslationY = TranslationYSlider.Value;
        TargetImage.AnchorX = AnchorXSlider.Value;
        TargetImage.AnchorY = AnchorYSlider.Value;
    }
    
    private void ResetButton_Clicked(object? sender, EventArgs e)
    {
        ScaleSlider.Value = 1.0;
        ScaleXSlider.Value = 0;
        ScaleYSlider.Value = 0;
        RotationSlider.Value = 0;
        RotationXSlider.Value = 0;
        RotationYSlider.Value = 0;
        TranslationXSlider.Value = 0;
        TranslationYSlider.Value = 0;
        AnchorXSlider.Value = 0.5;
        AnchorYSlider.Value = 0.5;

        UpdateTransformations();
    }
    
    private void RandomButton_Clicked(object? sender, EventArgs e)
    {
        // Applies random transformation values to demonstrate various effects.
        var random = new Random();

        ScaleSlider.Value = random.NextDouble() * 2.0 + 0.5;
        ScaleXSlider.Value = random.NextDouble() * 4.0 - 2.0;
        ScaleYSlider.Value = random.NextDouble() * 4.0 - 2.0;
        RotationSlider.Value = random.NextDouble() * 360;
        RotationXSlider.Value = random.NextDouble() * 360;
        RotationYSlider.Value = random.NextDouble() * 360;
        TranslationXSlider.Value = random.NextDouble() * 400 - 200;
        TranslationYSlider.Value = random.NextDouble() * 400 - 200;
        AnchorXSlider.Value = random.NextDouble();
        AnchorYSlider.Value = random.NextDouble();

        UpdateTransformations();
    }
    
    private async void AnimateButton_Clicked(object? sender, EventArgs e)
    {
        // Simple rotation animation
        await TargetImage.RotateTo(360, 2000);
        TargetImage.Rotation = 0;

        // Scale animation
        await TargetImage.ScaleTo(2.0, 1000);
        await TargetImage.ScaleTo(1.0, 1000);

        // Translation animation
        await TargetImage.TranslateTo(100, 100, 1000);
        await TargetImage.TranslateTo(0, 0, 1000);
    }
}