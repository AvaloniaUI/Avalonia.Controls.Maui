namespace ControlGallery.Pages;

public partial class CheckBoxPage : ContentPage
{
    private int _checkCount = 0;
    private int _interactionCount = 0;

    public CheckBoxPage()
    {
        InitializeComponent();
    }

    // Basic checkbox events
    private void OnCheckBoxChanged(object? sender, CheckedChangedEventArgs e)
    {
        _checkCount++;
        CheckCountLabel.Text = $"Check/Uncheck count: {_checkCount}";
    }

    // Fruit selection
    private void OnFruitCheckedChanged(object? sender, CheckedChangedEventArgs e)
    {
        var selected = new List<string>();
        
        if (AppleCheckBox.IsChecked) selected.Add("Apple");
        if (BananaCheckBox.IsChecked) selected.Add("Banana");
        if (OrangeCheckBox.IsChecked) selected.Add("Orange");
        if (GrapeCheckBox.IsChecked) selected.Add("Grape");

        if (selected.Count == 0)
        {
            FruitSelectionLabel.Text = "No fruits selected";
            FruitSelectionLabel.TextColor = Colors.Gray;
        }
        else
        {
            FruitSelectionLabel.Text = $"Selected: {string.Join(", ", selected)}";
            FruitSelectionLabel.TextColor = Colors.DarkBlue;
        }
    }

    // Settings
    private void OnSettingChanged(object? sender, CheckedChangedEventArgs e)
    {
        var enabled = new List<string>();
        
        if (NotificationsCheckBox.IsChecked) enabled.Add("Notifications");
        if (DarkModeCheckBox.IsChecked) enabled.Add("Dark Mode");
        if (AutoSaveCheckBox.IsChecked) enabled.Add("Auto-Save");
        if (LocationCheckBox.IsChecked) enabled.Add("Location");

        if (enabled.Count == 0)
        {
            SettingsLabel.Text = "Settings: None enabled";
        }
        else
        {
            SettingsLabel.Text = $"Settings: {string.Join(" ✓, ", enabled)} ✓";
        }
    }

    // Task list
    private void OnTaskCheckedChanged(object? sender, CheckedChangedEventArgs e)
    {
        int completed = 0;
        int total = 5;

        if (Task1CheckBox.IsChecked) completed++;
        if (Task2CheckBox.IsChecked) completed++;
        if (Task3CheckBox.IsChecked) completed++;
        if (Task4CheckBox.IsChecked) completed++;
        if (Task5CheckBox.IsChecked) completed++;

        double percentage = (double)completed / total * 100;
        
        TaskProgressLabel.Text = $"Progress: {completed}/{total} tasks completed ({percentage:F0}%)";
        TaskProgressBar.Progress = (double)completed / total;

        // Change color based on completion
        if (completed == total)
        {
            TaskProgressLabel.TextColor = Colors.Green;
        }
        else if (completed >= total / 2)
        {
            TaskProgressLabel.TextColor = Colors.Orange;
        }
        else
        {
            TaskProgressLabel.TextColor = Colors.Red;
        }
    }

    // Interactive checkbox
    private void OnInteractiveCheckChanged(object? sender, CheckedChangedEventArgs e)
    {
        _interactionCount++;
        
        if (InteractiveCheckBox.IsChecked)
        {
            InteractiveLabel.Text = "I'm checked ✓";
            InteractiveLabel.TextColor = Colors.Green;
        }
        else
        {
            InteractiveLabel.Text = "I'm unchecked";
            InteractiveLabel.TextColor = Colors.Red;
        }
        
        InteractionCountLabel.Text = $"Interactions: {_interactionCount}";
    }

    // Color change handlers
    private void OnSetColorRed(object? sender, EventArgs e)
    {
        InteractiveCheckBox.Color = Colors.Red;
    }

    private void OnSetColorGreen(object? sender, EventArgs e)
    {
        InteractiveCheckBox.Color = Colors.Green;
    }

    private void OnSetColorBlue(object? sender, EventArgs e)
    {
        InteractiveCheckBox.Color = Colors.Blue;
    }

    private void OnSetColorPurple(object? sender, EventArgs e)
    {
        InteractiveCheckBox.Color = Colors.Purple;
    }

    // Quick action handlers
    private void OnCheckInteractive(object? sender, EventArgs e)
    {
        InteractiveCheckBox.IsChecked = true;
    }

    private void OnUncheckInteractive(object? sender, EventArgs e)
    {
        InteractiveCheckBox.IsChecked = false;
    }

    private void OnToggleInteractive(object? sender, EventArgs e)
    {
        InteractiveCheckBox.IsChecked = !InteractiveCheckBox.IsChecked;
    }
}