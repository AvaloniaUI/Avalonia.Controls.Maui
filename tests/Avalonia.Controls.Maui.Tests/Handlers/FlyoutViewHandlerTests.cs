using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Microsoft.Maui.Controls;
using MauiFlyoutBehavior = Microsoft.Maui.FlyoutBehavior;
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
        Assert.IsType<DrawerPage>(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Popover Behavior Maps Correctly")]
    public async Task PopoverBehaviorMapsCorrectly()
    {
        var flyoutPage = CreateBasicFlyoutPage();
        flyoutPage.FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;

        var handler = await CreateHandlerAsync<FlyoutViewHandler>(flyoutPage);
        var drawerPage = handler.PlatformView as DrawerPage;

        Assert.NotNull(drawerPage);
        Assert.Equal(DrawerBehavior.Flyout, drawerPage.DrawerBehavior);
    }

    [AvaloniaFact(DisplayName = "Locked Behavior Maps Correctly")]
    public async Task LockedBehaviorMapsCorrectly()
    {
        var flyoutPage = CreateBasicFlyoutPage();
        flyoutPage.FlyoutLayoutBehavior = FlyoutLayoutBehavior.Split;

        var handler = await CreateHandlerAsync<FlyoutViewHandler>(flyoutPage);
        var drawerPage = handler.PlatformView as DrawerPage;

        Assert.NotNull(drawerPage);
        Assert.Equal(DrawerBehavior.Locked, drawerPage.DrawerBehavior);
    }

    [AvaloniaFact(DisplayName = "IsPresented True Opens Flyout")]
    public async Task IsPresentedTrueOpensFlyout()
    {
        var flyoutPage = CreateBasicFlyoutPage();
        flyoutPage.FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;
        flyoutPage.IsPresented = true;

        var handler = await CreateHandlerAsync<FlyoutViewHandler>(flyoutPage);
        var drawerPage = handler.PlatformView as DrawerPage;

        Assert.NotNull(drawerPage);
        Assert.True(drawerPage.IsOpen);
    }

    [AvaloniaFact(DisplayName = "IsPresented False Closes Flyout")]
    public async Task IsPresentedFalseClosesFlyout()
    {
        var flyoutPage = CreateBasicFlyoutPage();
        flyoutPage.FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;
        flyoutPage.IsPresented = false;

        var handler = await CreateHandlerAsync<FlyoutViewHandler>(flyoutPage);
        var drawerPage = handler.PlatformView as DrawerPage;

        Assert.NotNull(drawerPage);
        Assert.False(drawerPage.IsOpen);
    }

    [AvaloniaFact(DisplayName = "FlyoutWidth Defaults Correctly")]
    public async Task FlyoutWidthDefaultsCorrectly()
    {
        var flyoutPage = CreateBasicFlyoutPage();

        var handler = await CreateHandlerAsync<FlyoutViewHandler>(flyoutPage);
        var drawerPage = handler.PlatformView as DrawerPage;

        Assert.NotNull(drawerPage);
        Assert.Equal(320, drawerPage.DrawerLength);
    }

    [AvaloniaFact(DisplayName = "Flyout Content Is Set")]
    public async Task FlyoutContentIsSet()
    {
        var flyoutPage = CreateBasicFlyoutPage();

        var handler = await CreateHandlerAsync<FlyoutViewHandler>(flyoutPage);
        var drawerPage = handler.PlatformView as DrawerPage;

        Assert.NotNull(drawerPage);
        Assert.NotNull(drawerPage.Drawer);
    }

    [AvaloniaFact(DisplayName = "Detail Content Is Set")]
    public async Task DetailContentIsSet()
    {
        var flyoutPage = CreateBasicFlyoutPage();

        var handler = await CreateHandlerAsync<FlyoutViewHandler>(flyoutPage);
        var drawerPage = handler.PlatformView as DrawerPage;

        Assert.NotNull(drawerPage);
        Assert.NotNull(drawerPage.Content);
    }

    [AvaloniaFact(DisplayName = "SetDrawer With Same Content Is Idempotent")]
    public void SetDrawerWithSameContentIsIdempotent()
    {
        var drawerPage = new DrawerPage
        {
            ContentTemplate = null,
            DrawerTemplate = null
        };
        var flyoutPanel = new Avalonia.Controls.Panel();

        drawerPage.Drawer = flyoutPanel;
        var drawerAfterFirst = drawerPage.Drawer;

        // Setting the same content again should keep it
        drawerPage.Drawer = flyoutPanel;
        var drawerAfterSecond = drawerPage.Drawer;

        Assert.Same(drawerAfterFirst, drawerAfterSecond);
    }

    private FlyoutPage CreateBasicFlyoutPage()
    {
        var flyoutPage = new Microsoft.Maui.Controls.FlyoutPage
        {
            Flyout = new Microsoft.Maui.Controls.ContentPage { Title = "Menu" },
            Detail = new Microsoft.Maui.Controls.ContentPage { Title = "Detail" },
            WidthRequest = 800,
            HeightRequest = 600,
        };
        return flyoutPage;
    }
}
