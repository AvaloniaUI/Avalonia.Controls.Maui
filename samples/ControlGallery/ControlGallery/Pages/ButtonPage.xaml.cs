namespace ControlGallery.Pages;

public partial class ButtonPage : ContentPage
{
    private int _clickCount = 0;
    private int _interactiveClickCount = 0;

    public ButtonPage()
    {
        InitializeComponent();
    }

    private void OnButtonClicked(object? sender, EventArgs e)
    {
        _clickCount++;
        ClickCountLabel.Text = $"Click count: {_clickCount}";
    }

    private void OnInteractiveButtonClicked(object? sender, EventArgs e)
    {
        _interactiveClickCount++;
        InteractionLabel.Text = $"Button clicked {_interactiveClickCount} time{(_interactiveClickCount == 1 ? "" : "s")}!";
        InteractionLabel.TextColor = Colors.Green;
    }

    // Color change handlers
    private void OnSetBlue(object? sender, EventArgs e)
    {
        InteractiveButton.BackgroundColor = Colors.Blue;
    }

    private void OnSetGreen(object? sender, EventArgs e)
    {
        InteractiveButton.BackgroundColor = Colors.Green;
    }

    private void OnSetRed(object? sender, EventArgs e)
    {
        InteractiveButton.BackgroundColor = Colors.Red;
    }

    private void OnSetOrange(object? sender, EventArgs e)
    {
        InteractiveButton.BackgroundColor = Colors.Orange;
    }

    // Corner radius change handlers
    private void OnSetRadius0(object? sender, EventArgs e)
    {
        InteractiveButton.CornerRadius = 0;
    }

    private void OnSetRadius5(object? sender, EventArgs e)
    {
        InteractiveButton.CornerRadius = 5;
    }

    private void OnSetRadius15(object? sender, EventArgs e)
    {
        InteractiveButton.CornerRadius = 15;
    }

    private void OnSetRadius30(object? sender, EventArgs e)
    {
        InteractiveButton.CornerRadius = 30;
    }
}