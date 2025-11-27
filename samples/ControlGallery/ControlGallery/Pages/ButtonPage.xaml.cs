using System.Windows.Input;

namespace ControlGallery.Pages;

public partial class ButtonPage : ContentPage
{
    private int _clickCount = 0;
    private int _dynamicButtonClickCount = 0;
    private int _counterButtonClickCount = 0;
    private bool _toggleState = false;
    
    // Commands for data binding
    public ICommand MyCommand { get; }
    public ICommand ParameterizedCommand { get; }
    public ICommand DisabledCommand { get; }
    
    public ButtonPage()
    {
        InitializeComponent();
        
        // Initialize simple command
        MyCommand = new Command(() => 
        {
            DisplayAlert("Command", "Simple command executed!", "OK");
            UpdateCommandResult("Simple command executed");
        });
        
        // Initialize command with parameter
        ParameterizedCommand = new Command<string>(async (parameter) => 
        {
            await DisplayAlert("Parameterized Command", $"Executed with parameter: {parameter}", "OK");
            UpdateCommandResult($"Parameterized command: {parameter}");
        });
        
        // Initialize disabled command (canExecute returns false)
        DisabledCommand = new Command(
            execute: () => 
            {
                DisplayAlert("Disabled Command", "This should not execute!", "OK");
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

    // Dynamic text button handlers
    private readonly string[] _dynamicTexts =
    {
        "Click me!",
        "Clicked once!",
        "Clicked again!",
        "Keep going!",
        "You're persistent!",
        "Almost there...",
        "One more time!",
        "Great job!"
    };

    private void OnDynamicTextButtonClicked(object sender, EventArgs e)
    {
        _dynamicButtonClickCount++;
        var index = Math.Min(_dynamicButtonClickCount, _dynamicTexts.Length - 1);
        DynamicTextButton.Text = _dynamicTexts[index];
    }

    private void OnCounterButtonClicked(object sender, EventArgs e)
    {
        _counterButtonClickCount++;
        var timesText = _counterButtonClickCount == 1 ? "time" : "times";
        CounterButton.Text = $"Clicked {_counterButtonClickCount} {timesText}";
    }

    private void OnToggleButtonClicked(object sender, EventArgs e)
    {
        _toggleState = !_toggleState;
        ToggleButton.Text = _toggleState ? "Toggle: ON" : "Toggle: OFF";
        ToggleButton.BackgroundColor = _toggleState ? Colors.Green : Colors.Gray;
    }
}