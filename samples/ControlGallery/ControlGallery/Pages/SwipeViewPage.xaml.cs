namespace ControlGallery.Pages;

public partial class SwipeViewPage : ContentPage
{
    public SwipeViewPage()
    {
        InitializeComponent();
    }

    private async void OnFavoriteInvoked(object sender, EventArgs e)
    {
        await DisplayAlert("SwipeView", "Favorite invoked!", "OK");
    }

    private async void OnDeleteInvoked(object sender, EventArgs e)
    {
        await DisplayAlert("SwipeView", "Delete invoked!", "OK");
    }

    private async void OnRevealDeleteInvoked(object sender, EventArgs e)
    {
        await DisplayAlert("SwipeView", "Reveal delete invoked!", "OK");
    }

    private async void OnArchiveInvoked(object sender, EventArgs e)
    {
        await DisplayAlert("SwipeView", "Archive invoked!", "OK");
    }

    private void OnOpenLeftClicked(object sender, EventArgs e)
    {
        ProgrammaticSwipeView.Open(OpenSwipeItem.LeftItems);
    }

    private void OnOpenRightClicked(object sender, EventArgs e)
    {
        ProgrammaticSwipeView.Open(OpenSwipeItem.RightItems);
    }

    private void OnOpenTopClicked(object sender, EventArgs e)
    {
        ProgrammaticSwipeView.Open(OpenSwipeItem.TopItems);
    }

    private void OnOpenBottomClicked(object sender, EventArgs e)
    {
        ProgrammaticSwipeView.Open(OpenSwipeItem.BottomItems);
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        ProgrammaticSwipeView.Close();
    }

    private void OnSwipeStarted(object sender, SwipeStartedEventArgs e)
    {
        SwipeStatusLabel.Text = $"Status: Swipe Started ({e.SwipeDirection})";
    }

    private void OnSwipeEnded(object sender, SwipeEndedEventArgs e)
    {
        SwipeStatusLabel.Text = $"Status: Swipe Ended (IsOpen: {e.IsOpen})";
    }

    private async void OnExecuteSwipeInvoked(object sender, EventArgs e)
    {
        await DisplayAlert("Execute Mode", "Execute mode triggered! SwipeView auto-closed after execution.", "OK");
    }

    private void OnOpenRequested(object sender, OpenRequestedEventArgs e)
    {
        EventStatusLabel.Text = $"Event: OpenRequested fired ({e.OpenSwipeItem})";
    }

    private void OnCloseRequested(object sender, CloseRequestedEventArgs e)
    {
        EventStatusLabel.Text = "Event: CloseRequested fired";
    }

    private void OnSwipeChanging(object sender, SwipeChangingEventArgs e)
    {
        SwipeChangingLabel.Text = $"Changing: {e.SwipeDirection} | Offset: {e.Offset:F0}";
    }

    private async void OnCustomActionClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Custom Action", "Custom button clicked.", "OK");
    }

    private async void OnRevealInvoked(object sender, EventArgs e)
    {
        await DisplayAlert("SwipeView", "Reveal item invoked (remain open).", "OK");
    }

    private async void OnExecuteMixedInvoked(object sender, EventArgs e)
    {
        await DisplayAlert("SwipeView", "Execute item invoked (auto-close).", "OK");
    }

    private async void OnRemainOpenInvoked(object sender, EventArgs e)
    {
        await DisplayAlert("SwipeView", "RemainOpen invoked (stays open).", "OK");
    }

    private async void OnAutoCloseInvoked(object sender, EventArgs e)
    {
        await DisplayAlert("SwipeView", "AutoClose invoked (closes after).", "OK");
    }

    private async void OnAllDirectionsInvoked(object sender, EventArgs e)
    {
        await DisplayAlert("SwipeView", "All directions item invoked.", "OK");
    }

    private void OnEventOpenLeftClicked(object sender, EventArgs e)
    {
        EventSwipeView.Open(OpenSwipeItem.LeftItems);
    }

    private void OnEventCloseClicked(object sender, EventArgs e)
    {
        EventSwipeView.Close();
    }
}
