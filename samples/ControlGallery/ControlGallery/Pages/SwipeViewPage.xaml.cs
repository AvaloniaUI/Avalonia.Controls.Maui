using System.Collections.ObjectModel;

namespace ControlGallery.Pages;

public partial class SwipeViewPage : ContentPage
{
    public ObservableCollection<SwipeSampleItem> SwipeItemsCollection { get; }
    public SwipeViewPage()
    {
        SwipeItemsCollection =
        [
            new SwipeSampleItem("Flight to Avalonia", "AE-1042"),
            new SwipeSampleItem("Conference Tickets", "CT-3408"),
            new SwipeSampleItem("Hotel Reservation", "HR-2271"),
            new SwipeSampleItem("Car Rental", "CR-9981"),
            new SwipeSampleItem("Team Dinner", "TD-5566")
        ];

        BindingContext = this;
        InitializeComponent();
    }

    private async void OnFavoriteInvoked(object sender, EventArgs e)
    {
        await DisplayAlertAsync("SwipeView", "Favorite invoked!", "OK");
    }

    private async void OnDeleteInvoked(object sender, EventArgs e)
    {
        await DisplayAlertAsync("SwipeView", "Delete invoked!", "OK");
    }

    private async void OnRevealDeleteInvoked(object sender, EventArgs e)
    {
        await DisplayAlertAsync("SwipeView", "Reveal delete invoked!", "OK");
    }

    private async void OnArchiveInvoked(object sender, EventArgs e)
    {
        await DisplayAlertAsync("SwipeView", "Archive invoked!", "OK");
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
        await DisplayAlertAsync("Execute Mode", "Execute mode triggered! SwipeView auto-closed after execution.", "OK");
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
        await DisplayAlertAsync("Custom Action", "Custom button clicked.", "OK");
    }

    private async void OnRevealInvoked(object sender, EventArgs e)
    {
        await DisplayAlertAsync("SwipeView", "Reveal item invoked (remain open).", "OK");
    }

    private async void OnExecuteMixedInvoked(object sender, EventArgs e)
    {
        await DisplayAlertAsync("SwipeView", "Execute item invoked (auto-close).", "OK");
    }

    private async void OnRemainOpenInvoked(object sender, EventArgs e)
    {
        await DisplayAlertAsync("SwipeView", "RemainOpen invoked (stays open).", "OK");
    }

    private async void OnAutoCloseInvoked(object sender, EventArgs e)
    {
        await DisplayAlertAsync("SwipeView", "AutoClose invoked (closes after).", "OK");
    }

    private async void OnAllDirectionsInvoked(object sender, EventArgs e)
    {
        await DisplayAlertAsync("SwipeView", "All directions item invoked.", "OK");
    }

    private void OnEventOpenLeftClicked(object sender, EventArgs e)
    {
        EventSwipeView.Open(OpenSwipeItem.LeftItems);
    }

    private void OnEventCloseClicked(object sender, EventArgs e)
    {
        EventSwipeView.Close();
    }

    private async void OnKeepItemInvoked(object sender, EventArgs e)
    {
        if (GetSwipeSampleItem(sender) is { } item)
        {
            await DisplayAlertAsync("Save", $"{item.Title} pinned.", "OK");
        }
    }

    private async void OnRemoveItemInvoked(object sender, EventArgs e)
    {
        if (GetSwipeSampleItem(sender) is { } item)
        {
            var confirm = await DisplayAlertAsync("Remove item", $"Remove \"{item.Title}\"?", "Remove", "Cancel");
            if (confirm)
            {
                SwipeItemsCollection.Remove(item);
            }
        }
    }

    private SwipeSampleItem? GetSwipeSampleItem(object sender)
    {
        return sender switch
        {
            SwipeItem swipeItem => swipeItem.BindingContext as SwipeSampleItem,
            _ => null
        };
    }

    public record SwipeSampleItem(string Title, string Subtitle);

}
