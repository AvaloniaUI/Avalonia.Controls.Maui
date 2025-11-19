namespace ControlGallery.Pages;

public partial class ButtonPage : ContentPage
{
    private int _clickCount;

    public ButtonPage()
    {
        InitializeComponent();
    }

    private void OnButtonClicked(object? sender, EventArgs e)
    {
        _clickCount++;
        ClickCountLabel.Text = $"Click count: {_clickCount}";
    }
}