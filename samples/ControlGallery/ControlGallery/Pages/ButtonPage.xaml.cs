using System.Windows.Input;

namespace ControlGallery.Pages;

public partial class ButtonPage : ContentPage
{
    private int _clickCount = 0;

    // Commands for Button data binding
    public ICommand MyCommand { get; }
    public ICommand ParameterizedCommand { get; }
    public ICommand DisabledCommand { get; }

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
}