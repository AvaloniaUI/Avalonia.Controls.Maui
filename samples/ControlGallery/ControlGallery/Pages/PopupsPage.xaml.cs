using Microsoft.Maui.Controls;

namespace ControlGallery.Pages;

public partial class PopupsPage : ContentPage
{
    public PopupsPage()
    {
        InitializeComponent();
    }

    // Alerts
    private async void OnSimpleAlertClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Alert", "This is a simple alert.", "OK");
        ResultLabel.Text = "Result: OK";
    }

    private async void OnAlertYesNoClicked(object sender, EventArgs e)
    {
        bool answer = await DisplayAlert("Question?", "Would you like to proceed?", "Yes", "No");
        ResultLabel.Text = $"Answer: {(answer ? "Yes" : "No")}";
    }
    
    // Action Sheets
    private async void OnSimpleActionSheetClicked(object sender, EventArgs e)
    {
        string action = await DisplayActionSheet("Share with", "Cancel", null, "Email", "Messages", "Teams", "Twitter");
        ResultLabel.Text = $"Action: {action}";
    }

    private async void OnActionSheetCancelDeleteClicked(object sender, EventArgs e)
    {
        string action = await DisplayActionSheet("Save?", "Cancel", "Delete", "Photo Roll", "Files");
        ResultLabel.Text = $"Action: {action}";
    }

    // Prompts
    private async void OnSimplePromptClicked(object sender, EventArgs e)
    {
        string result = await DisplayPromptAsync("Your name", "What's your name?");
        ResultLabel.Text = $"Name: {result ?? "Cancelled"}";
    }

    private async void OnNumberPromptClicked(object sender, EventArgs e)
    {
        string result = await DisplayPromptAsync("Favorite number", "Pick a number", initialValue: "10", maxLength: 3, keyboard: Keyboard.Numeric);
        ResultLabel.Text = $"Number: {result ?? "Cancelled"}";
    }

    // Busy Indicator
    private async void OnShowBusyClicked(object sender, EventArgs e)
    {
        IsBusy = true;
        ResultLabel.Text = "Page is busy...";
        await Task.Delay(2000);
        IsBusy = false;
        ResultLabel.Text = "Busy indicator dismissed";
    }
}
