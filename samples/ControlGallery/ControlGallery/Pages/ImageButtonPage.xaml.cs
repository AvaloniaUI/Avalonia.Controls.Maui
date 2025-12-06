using System.Windows.Input;

namespace ControlGallery.Pages;

public partial class ImageButtonPage : ContentPage
{
    private int _imageButtonClickCount = 0;

    // Commands for ImageButton data binding
    public ICommand ImageButtonCommand { get; }
    public ICommand ImageButtonParameterizedCommand { get; }

    public ImageButtonPage()
    {
        InitializeComponent();

        // Initialize ImageButton simple command
        ImageButtonCommand = new Command(async () =>
        {
            await DisplayAlertAsync("ImageButton Command", "ImageButton command executed!", "OK");
            UpdateImageButtonCommandResult("ImageButton command executed");
        });

        // Initialize ImageButton command with parameter
        ImageButtonParameterizedCommand = new Command<string>(async (parameter) =>
        {
            await DisplayAlertAsync("ImageButton Command", $"Executed with parameter: {parameter}", "OK");
            UpdateImageButtonCommandResult($"ImageButton command: {parameter}");
        });

        // Set the BindingContext to this page for command binding
        BindingContext = this;
    }

    // ImageButton event handlers
    private void OnImageButtonClicked(object sender, EventArgs e)
    {
        _imageButtonClickCount++;
        ImageButtonClickCountLabel.Text = $"ImageButton click count: {_imageButtonClickCount}";
    }

    private void UpdateImageButtonCommandResult(string message)
    {
        ImageButtonCommandResultLabel.Text = message;
    }

    // ImageButton Pressed and Released event handlers
    private void OnImageButton1Pressed(object sender, EventArgs e)
    {
        var imageButton = (ImageButton)sender;
        imageButton.BackgroundColor = Colors.DarkBlue;
        imageButton.Scale = 0.95;
    }

    private void OnImageButton1Released(object sender, EventArgs e)
    {
        var imageButton = (ImageButton)sender;
        imageButton.BackgroundColor = Colors.LightBlue;
        imageButton.Scale = 1.0;
    }

    private void OnImageButton2Pressed(object sender, EventArgs e)
    {
        var imageButton = (ImageButton)sender;
        imageButton.BackgroundColor = Colors.DarkGreen;
        imageButton.BorderWidth = 4;
    }

    private void OnImageButton2Released(object sender, EventArgs e)
    {
        var imageButton = (ImageButton)sender;
        imageButton.BackgroundColor = Colors.LightGreen;
        imageButton.BorderWidth = 2;
    }

    private void OnImageButton3Pressed(object sender, EventArgs e)
    {
        var imageButton = (ImageButton)sender;
        imageButton.BackgroundColor = Colors.DarkRed;
        imageButton.Scale = 0.9;
        imageButton.Rotation = 5;
    }

    private void OnImageButton3Released(object sender, EventArgs e)
    {
        var imageButton = (ImageButton)sender;
        imageButton.BackgroundColor = Colors.LightCoral;
        imageButton.Scale = 1.0;
        imageButton.Rotation = 0;
    }
}
