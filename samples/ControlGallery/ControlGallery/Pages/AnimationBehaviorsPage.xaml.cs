namespace ControlGallery.Pages;

public partial class AnimationBehaviorsPage : ContentPage
{
    private int _clickCount;

    public AnimationBehaviorsPage()
    {
        InitializeComponent();

        ClickedToCommand.Command = new Command(() =>
        {
            _clickCount++;
            ClickCountLabel.Text = $"Clicks: {_clickCount}";
        });
    }

    private void OnProgressZeroClicked(object? sender, EventArgs e) => ProgressAnimation.Progress = 0;

    private void OnProgressHalfClicked(object? sender, EventArgs e) => ProgressAnimation.Progress = 0.5;

    private void OnProgressFullClicked(object? sender, EventArgs e) => ProgressAnimation.Progress = 1;
}
