using Avalonia.Controls.Maui.Controls.Shell;
using Avalonia.Headless.XUnit;
using Microsoft.Maui.Controls;
using MauiFlyoutBehavior = Microsoft.Maui.FlyoutBehavior;
using AvaloniaFlyoutBehavior = Avalonia.Controls.Maui.Controls.Shell.FlyoutBehavior;
using FlyoutViewHandler = Avalonia.Controls.Maui.Handlers.FlyoutViewHandler;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public partial class FlyoutViewHandlerTests : HandlerTestBase
{
    [AvaloniaFact(DisplayName = "FlyoutView Creates Platform View")]
    public async Task FlyoutViewCreatesPlatformView()
    {
        var flyoutPage = CreateBasicFlyoutPage();

        var handler = await CreateHandlerAsync<FlyoutViewHandler>(flyoutPage);

        Assert.NotNull(handler.PlatformView);
        Assert.IsType<FlyoutContainer>(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Popover Behavior Maps Correctly")]
    public async Task PopoverBehaviorMapsCorrectly()
    {
        var flyoutPage = CreateBasicFlyoutPage();
        flyoutPage.FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;

        var handler = await CreateHandlerAsync<FlyoutViewHandler>(flyoutPage);
        var flyoutContainer = handler.PlatformView as FlyoutContainer;

        Assert.NotNull(flyoutContainer);
        Assert.Equal(AvaloniaFlyoutBehavior.Popover, flyoutContainer.FlyoutBehavior);
    }

    [AvaloniaFact(DisplayName = "Locked Behavior Maps Correctly")]
    public async Task LockedBehaviorMapsCorrectly()
    {
        var flyoutPage = CreateBasicFlyoutPage();
        flyoutPage.FlyoutLayoutBehavior = FlyoutLayoutBehavior.Split;

        var handler = await CreateHandlerAsync<FlyoutViewHandler>(flyoutPage);
        var flyoutContainer = handler.PlatformView as FlyoutContainer;

        Assert.NotNull(flyoutContainer);
        Assert.Equal(AvaloniaFlyoutBehavior.Locked, flyoutContainer.FlyoutBehavior);
    }

    [AvaloniaFact(DisplayName = "IsPresented True Opens Flyout")]
    public async Task IsPresentedTrueOpensFlyout()
    {
        var flyoutPage = CreateBasicFlyoutPage();
        flyoutPage.FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;
        flyoutPage.IsPresented = true;

        var handler = await CreateHandlerAsync<FlyoutViewHandler>(flyoutPage);
        var flyoutContainer = handler.PlatformView as FlyoutContainer;

        Assert.NotNull(flyoutContainer);
        Assert.True(flyoutContainer.IsFlyoutOpen);
    }

    [AvaloniaFact(DisplayName = "IsPresented False Closes Flyout")]
    public async Task IsPresentedFalseClosesFlyout()
    {
        var flyoutPage = CreateBasicFlyoutPage();
        flyoutPage.FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;
        flyoutPage.IsPresented = false;

        var handler = await CreateHandlerAsync<FlyoutViewHandler>(flyoutPage);
        var flyoutContainer = handler.PlatformView as FlyoutContainer;

        Assert.NotNull(flyoutContainer);
        Assert.False(flyoutContainer.IsFlyoutOpen);
    }

    [AvaloniaFact(DisplayName = "FlyoutWidth Defaults Correctly")]
    public async Task FlyoutWidthDefaultsCorrectly()
    {
        var flyoutPage = CreateBasicFlyoutPage();

        var handler = await CreateHandlerAsync<FlyoutViewHandler>(flyoutPage);
        var flyoutContainer = handler.PlatformView as FlyoutContainer;

        Assert.NotNull(flyoutContainer);
        Assert.Equal(FlyoutContainer.DefaultFlyoutWidth, flyoutContainer.FlyoutWidth);
    }

    [AvaloniaFact(DisplayName = "Flyout Content Is Set")]
    public async Task FlyoutContentIsSet()
    {
        var flyoutPage = CreateBasicFlyoutPage();

        var handler = await CreateHandlerAsync<FlyoutViewHandler>(flyoutPage);
        var flyoutContainer = handler.PlatformView as FlyoutContainer;

        Assert.NotNull(flyoutContainer);
        // FlyoutContainer should have children: scrim + flyout content + detail content
        Assert.True(flyoutContainer.Children.Count >= 2);
    }

    [AvaloniaFact(DisplayName = "Detail Content Is Set")]
    public async Task DetailContentIsSet()
    {
        var flyoutPage = CreateBasicFlyoutPage();

        var handler = await CreateHandlerAsync<FlyoutViewHandler>(flyoutPage);
        var flyoutContainer = handler.PlatformView as FlyoutContainer;

        Assert.NotNull(flyoutContainer);
        Assert.NotNull(flyoutContainer.DetailContent);
    }

    [AvaloniaFact(DisplayName = "SetFlyoutContent With Same Content Preserves Transform")]
    public void SetFlyoutContentWithSameContentPreservesTransform()
    {
        var container = new FlyoutContainer();
        var flyoutPanel = new Avalonia.Controls.Panel();

        container.SetFlyoutContent(flyoutPanel);
        var childCountAfterFirst = container.Children.Count;

        // Setting the same content again should be a no-op
        container.SetFlyoutContent(flyoutPanel);
        var childCountAfterSecond = container.Children.Count;

        Assert.Equal(childCountAfterFirst, childCountAfterSecond);
    }

    private FlyoutPage CreateBasicFlyoutPage()
    {
        var flyoutPage = new FlyoutPage
        {
            Flyout = new ContentPage { Title = "Menu" },
            Detail = new ContentPage { Title = "Detail" },
            WidthRequest = 800,
            HeightRequest = 600,
        };
        return flyoutPage;
    }
}
