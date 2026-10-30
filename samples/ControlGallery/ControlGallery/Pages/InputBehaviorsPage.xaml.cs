namespace ControlGallery.Pages;

public partial class InputBehaviorsPage : ContentPage
{
    public InputBehaviorsPage()
    {
        InitializeComponent();

        MaxLengthBehavior.Command = new Command<string>(text =>
            MaxLengthLabel.Text = $"Max length reached: {text}");

        StoppedTypingBehavior.Command = new Command<string>(text =>
            StoppedTypingLabel.Text = $"Search for: {text}");
    }
}
