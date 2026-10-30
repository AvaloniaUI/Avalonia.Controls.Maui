using Avalonia.Headless.XUnit;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using ControlsContentPage = Microsoft.Maui.Controls.ContentPage;
using MauiLabel = Microsoft.Maui.Controls.Label;

namespace Avalonia.Controls.Maui.Tests.CommunityToolkit;

public class ToolkitPopupTests : ToolkitTestBase
{
    [AvaloniaFact(DisplayName = "Popup Shows Its Content Over The Page")]
    public async Task PopupShowsContentOverPage()
    {
        var page = new ControlsContentPage { Content = new MauiLabel { Text = "Page" } };
        var content = new MauiLabel { Text = "Popup" };

        await RunInWindowAsync(page, async window =>
        {
            var showing = page.ShowPopupAsync(new Popup { Content = content });

            await WaitUntilAsync(window, () => IsShown(content), message: "The popup content was not shown.");
            Assert.Single(page.Navigation.ModalStack);
            Assert.True(new Rect(window.Platform.Bounds.Size).Contains(BoundsInWindow(window, content)));

            await page.ClosePopupAsync();
            await WaitUntilAsync(window, () => showing.IsCompleted && page.Navigation.ModalStack.Count == 0,
                message: "The popup did not close.");
            Assert.False((await showing).WasDismissedByTappingOutsideOfPopup);
        });
    }

    [AvaloniaFact(DisplayName = "Popup Returns The Result It Was Closed With")]
    public async Task PopupReturnsResult()
    {
        var page = new ControlsContentPage { Content = new MauiLabel { Text = "Page" } };
        var popup = new Popup<string> { Content = new MauiLabel { Text = "Pick" } };

        await RunInWindowAsync(page, async window =>
        {
            var showing = page.ShowPopupAsync<string>(popup);
            await WaitUntilAsync(window, () => IsShown(popup.Content!), message: "The popup was not shown.");

            await popup.CloseAsync("Picked");
            await WaitUntilAsync(window, () => showing.IsCompleted, message: "The popup did not close.");

            var result = await showing;
            Assert.Equal("Picked", result.Result);
            Assert.False(result.WasDismissedByTappingOutsideOfPopup);
        });
    }

    [AvaloniaFact(DisplayName = "Clicking Outside A Popup Dismisses It")]
    public async Task ClickingOutsidePopupDismissesIt()
    {
        var page = new ControlsContentPage { Content = new MauiLabel { Text = "Page" } };
        var content = new MauiLabel { Text = "Popup", WidthRequest = 100, HeightRequest = 50 };
        var tappedOutside = 0;
        var options = new PopupOptions { OnTappingOutsideOfPopup = () => tappedOutside++ };

        await RunInWindowAsync(page, async window =>
        {
            var showing = page.ShowPopupAsync(new Popup { Content = content }, options);
            await WaitUntilAsync(window, () => IsShown(content), message: "The popup was not shown.");

            ClickAt(window, new Point(5, 5));

            await WaitUntilAsync(window, () => showing.IsCompleted, message: "Clicking outside did not dismiss the popup.");
            Assert.True((await showing).WasDismissedByTappingOutsideOfPopup);
            Assert.Equal(1, tappedOutside);
            Assert.Empty(page.Navigation.ModalStack);
        });
    }

    [AvaloniaFact(DisplayName = "Clicking Outside A Popup That Cannot Be Dismissed Keeps It Open")]
    public async Task ClickingOutsideUndismissablePopupKeepsItOpen()
    {
        var page = new ControlsContentPage { Content = new MauiLabel { Text = "Page" } };
        var popup = new Popup { Content = new MauiLabel { Text = "Popup", WidthRequest = 100, HeightRequest = 50 } };
        var options = new PopupOptions { CanBeDismissedByTappingOutsideOfPopup = false };

        await RunInWindowAsync(page, async window =>
        {
            var showing = page.ShowPopupAsync(popup, options);
            await WaitUntilAsync(window, () => IsShown(popup.Content!), message: "The popup was not shown.");

            ClickAt(window, new Point(5, 5));
            await Task.Delay(100);
            await WaitUntilAsync(window, () => true);

            Assert.False(showing.IsCompleted);
            Assert.Single(page.Navigation.ModalStack);

            await popup.CloseAsync();
            await WaitUntilAsync(window, () => showing.IsCompleted, message: "The popup did not close.");
        });
    }

    [AvaloniaFact(DisplayName = "Clicking Inside A Popup Does Not Dismiss It")]
    public async Task ClickingInsidePopupDoesNotDismissIt()
    {
        var page = new ControlsContentPage { Content = new MauiLabel { Text = "Page" } };
        var button = new Microsoft.Maui.Controls.Button { Text = "Inside" };
        var clicks = 0;
        button.Clicked += (_, _) => clicks++;
        var popup = new Popup { Content = button };

        await RunInWindowAsync(page, async window =>
        {
            var showing = page.ShowPopupAsync(popup);
            await WaitUntilAsync(window, () => IsShown(button), message: "The popup was not shown.");

            Click(window, button);
            await WaitUntilAsync(window, () => clicks == 1, message: "The button in the popup was not clicked.");

            Assert.False(showing.IsCompleted);

            await popup.CloseAsync();
            await WaitUntilAsync(window, () => showing.IsCompleted, message: "The popup did not close.");
        });
    }

    static bool IsShown(Microsoft.Maui.IView view) =>
        view.Handler?.PlatformView is Control control && control.IsEffectivelyVisible && control.Bounds.Width > 0 && control.Bounds.Height > 0;

    static void ClickAt(ToolkitWindow window, Point point)
    {
        Avalonia.Headless.HeadlessWindowExtensions.MouseDown(window.Platform, point, Avalonia.Input.MouseButton.Left);
        Avalonia.Headless.HeadlessWindowExtensions.MouseUp(window.Platform, point, Avalonia.Input.MouseButton.Left);
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
    }
}
