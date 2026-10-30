using CommunityToolkit.Maui.Layouts;

namespace ControlGallery.Pages;

public partial class StateContainerPage : ContentPage
{
    public StateContainerPage()
    {
        InitializeComponent();
    }

    private void OnContentStateClicked(object? sender, EventArgs e) => SetState(null);

    private void OnLoadingStateClicked(object? sender, EventArgs e) => SetState("Loading");

    private void OnEmptyStateClicked(object? sender, EventArgs e) => SetState("Empty");

    private void OnErrorStateClicked(object? sender, EventArgs e) => SetState("Error");

    private void SetState(string? state)
    {
        StateContainer.SetCurrentState(StatesLayout, state);
        CurrentStateLabel.Text = $"Current state: {state ?? "(none)"}";
    }

    private async void OnAnimatedToggleClicked(object? sender, EventArgs e)
    {
        if (!StateContainer.GetCanStateChange(AnimatedStatesGrid))
            return;

        var nextState = StateContainer.GetCurrentState(AnimatedStatesGrid) is null ? "Done" : null;
        await StateContainer.ChangeStateWithAnimation(
            AnimatedStatesGrid,
            nextState,
            static (element, token) => element.FadeToAsync(0, 150, Easing.Linear),
            static (element, token) => element.FadeToAsync(1, 150, Easing.Linear));
    }
}
