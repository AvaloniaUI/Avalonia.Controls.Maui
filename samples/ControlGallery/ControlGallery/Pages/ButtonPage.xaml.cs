using System.Windows.Input;

namespace ControlGallery.Pages;

public partial class ButtonPage : ContentPage
{
    private int _clickCount = 0;
    private int _imageButtonClickCount = 0;

    // Commands for Button data binding
    public ICommand MyCommand { get; }
    public ICommand ParameterizedCommand { get; }
    public ICommand DisabledCommand { get; }

    // Commands for ImageButton data binding
    public ICommand ImageButtonCommand { get; }
    public ICommand ImageButtonParameterizedCommand { get; }

    public ButtonPage()
    {
        InitializeComponent();

        // Initialize simple command
        MyCommand = new Command(async () =>
        {
            await DisplayAlertAsync("Command", "Simple command executed!", "OK");
            UpdateCommandResult("Simple command executed");
        });

        // Initialize command with parameter
        ParameterizedCommand = new Command<string>(async (parameter) =>
        {
            await DisplayAlertAsync("Parameterized Command", $"Executed with parameter: {parameter}", "OK");
            UpdateCommandResult($"Parameterized command: {parameter}");
        });

        // Initialize disabled command (canExecute returns false)
        DisabledCommand = new Command(
            execute: async () =>
            {
                await DisplayAlertAsync("Disabled Command", "This should not execute!", "OK");
            },
            canExecute: () => false
        );

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
    
    private void OnButtonClicked(object sender, EventArgs e)
    {
        _clickCount++;
        ClickCountLabel.Text = $"Click count: {_clickCount}";
    }
    
    private void UpdateCommandResult(string message)
    {
        CommandResultLabel.Text = message;
    }
    
    // Pressed and Released event handlers for interactive buttons
    private void OnButton1Pressed(object sender, EventArgs e)
    {
        var button = (Button)sender;
        button.BackgroundColor = Colors.DarkBlue;
        button.TextColor = Colors.White;
        button.Scale = 0.95;
    }

    private void OnButton1Released(object sender, EventArgs e)
    {
        var button = (Button)sender;
        button.BackgroundColor = Colors.LightBlue;
        button.TextColor = Colors.Black;
        button.Scale = 1.0;
    }

    private void OnButton2Pressed(object sender, EventArgs e)
    {
        var button = (Button)sender;
        button.BackgroundColor = Colors.DarkGreen;
        button.BorderWidth = 4;
    }

    private void OnButton2Released(object sender, EventArgs e)
    {
        var button = (Button)sender;
        button.BackgroundColor = Colors.Green;
        button.BorderWidth = 2;
    }

    private void OnButton3Pressed(object sender, EventArgs e)
    {
        var button = (Button)sender;
        button.BackgroundColor = Colors.DarkViolet;
        button.Scale = 0.9;
        button.Rotation = 2;
    }

    private void OnButton3Released(object sender, EventArgs e)
    {
        var button = (Button)sender;
        button.BackgroundColor = Colors.Purple;
        button.Scale = 1.0;
        button.Rotation = 0;
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