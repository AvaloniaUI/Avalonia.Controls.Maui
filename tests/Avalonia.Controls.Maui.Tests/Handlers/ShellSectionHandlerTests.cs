using Avalonia.Headless.XUnit;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Avalonia.Controls.Maui.Animations;
using Avalonia.Controls.Maui.Handlers.Shell;
using Avalonia.Animation;
using MauiPage = Microsoft.Maui.Controls.Page;
using MauiContentPage = Microsoft.Maui.Controls.ContentPage;
using AvaloniaNavigationPage = Avalonia.Controls.NavigationPage;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public class ShellSectionHandlerTests : HandlerTestBase
{
    // --- Platform view creation ---

    [AvaloniaFact(DisplayName = "Handler creates AvaloniaNavigationPage as PlatformView")]
    public async Task HandlerCreatesNavigationPage()
    {
        var section = CreateSection();
        var handler = await CreateHandlerAsync<ShellSectionHandler>(section);

        Assert.NotNull(handler.PlatformView);
        Assert.IsType<AvaloniaNavigationPage>(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "NavigationPage has default MauiNavigationTransition")]
    public async Task NavigationPageHasDefaultTransition()
    {
        var section = CreateSection();
        var handler = await CreateHandlerAsync<ShellSectionHandler>(section);
        var navPage = (AvaloniaNavigationPage)handler.PlatformView;

        Assert.NotNull(navPage.PageTransition);
        Assert.IsType<MauiNavigationTransition>(navPage.PageTransition);
    }

    [AvaloniaFact(DisplayName = "NavigationPage hides navigation bar (Shell manages its own)")]
    public async Task NavigationPageHidesNavBar()
    {
        var section = CreateSection();
        var handler = await CreateHandlerAsync<ShellSectionHandler>(section);
        var navPage = (AvaloniaNavigationPage)handler.PlatformView;

        Assert.False(AvaloniaNavigationPage.GetHasNavigationBar(navPage));
    }

    // --- Handler does not implement IStackNavigation ---

    [AvaloniaFact(DisplayName = "Handler does not implement IStackNavigation")]
    public async Task HandlerDoesNotImplementIStackNavigation()
    {
        var section = CreateSection();
        var handler = await CreateHandlerAsync<ShellSectionHandler>(section);

        // The handler should NOT be an IStackNavigation proxy.
        // Navigation flows through VirtualView (ShellSection) directly.
        Assert.False(handler is IStackNavigation);
    }

    // --- Initial content pushed on connect ---

    [AvaloniaFact(DisplayName = "Initial content page is pushed on connect")]
    public async Task InitialContentPushedOnConnect()
    {
        var page = new MauiContentPage { Title = "Root" };
        var section = CreateSection(page);

        var handler = await CreateHandlerAsync<ShellSectionHandler>(section);
        var navPage = (AvaloniaNavigationPage)handler.PlatformView;

        // The NavigationPage should have content after connect
        Assert.True(navPage.StackDepth > 0);
    }

    // --- CurrentItem mapping ---

    [AvaloniaFact(DisplayName = "Switching CurrentItem updates navigation stack")]
    public async Task SwitchingCurrentItemUpdatesStack()
    {
        var page1 = new MauiContentPage { Title = "Page 1" };
        var page2 = new MauiContentPage { Title = "Page 2" };
        var content1 = new ShellContent { Content = page1 };
        var content2 = new ShellContent { Content = page2 };

        var section = new ShellSection { Items = { content1, content2 } };

        var handler = await CreateHandlerAsync<ShellSectionHandler>(section);
        var navPage = (AvaloniaNavigationPage)handler.PlatformView;

        // Initial state — page1 is shown
        Assert.True(navPage.StackDepth > 0);

        // Switch to content2
        await InvokeOnMainThreadAsync(() =>
        {
            section.CurrentItem = content2;
            handler.UpdateValue(nameof(ShellSection.CurrentItem));
        });

        // NavigationPage should still have content (the new page)
        Assert.True(navPage.StackDepth > 0);
    }

    [AvaloniaFact(DisplayName = "CurrentItem update with same item is idempotent")]
    public async Task CurrentItemUpdateSameItemIsIdempotent()
    {
        var page = new MauiContentPage { Title = "Page 1" };
        var section = CreateSection(page);

        var handler = await CreateHandlerAsync<ShellSectionHandler>(section);
        var navPage = (AvaloniaNavigationPage)handler.PlatformView;

        var stackDepthBefore = navPage.StackDepth;

        // Re-trigger CurrentItem mapping with the same item
        await InvokeOnMainThreadAsync(() =>
        {
            handler.UpdateValue(nameof(ShellSection.CurrentItem));
        });

        // Stack depth should remain the same — no duplicate push
        Assert.Equal(stackDepthBefore, navPage.StackDepth);
    }

    // --- Items mapping ---

    [AvaloniaFact(DisplayName = "Items change triggers sync")]
    public async Task ItemsChangeTriggerSync()
    {
        var page1 = new MauiContentPage { Title = "Page 1" };
        var content1 = new ShellContent { Content = page1 };
        var section = new ShellSection { Items = { content1 } };

        var handler = await CreateHandlerAsync<ShellSectionHandler>(section);
        var navPage = (AvaloniaNavigationPage)handler.PlatformView;

        Assert.True(navPage.StackDepth > 0);

        // Trigger Items mapping
        await InvokeOnMainThreadAsync(() =>
        {
            handler.UpdateValue(nameof(ShellSection.Items));
        });

        // Should still have content — no crash or empty state
        Assert.True(navPage.StackDepth > 0);
    }

    // --- Multiple ShellContent switching ---

    [AvaloniaFact(DisplayName = "Multiple ShellContent items - switching between them works")]
    public async Task MultipleShellContentSwitching()
    {
        var page1 = new MauiContentPage { Title = "Tab 1" };
        var page2 = new MauiContentPage { Title = "Tab 2" };
        var page3 = new MauiContentPage { Title = "Tab 3" };
        var content1 = new ShellContent { Content = page1 };
        var content2 = new ShellContent { Content = page2 };
        var content3 = new ShellContent { Content = page3 };

        var section = new ShellSection { Items = { content1, content2, content3 } };

        var handler = await CreateHandlerAsync<ShellSectionHandler>(section);
        var navPage = (AvaloniaNavigationPage)handler.PlatformView;

        // Switch to content2
        await InvokeOnMainThreadAsync(() =>
        {
            section.CurrentItem = content2;
            handler.UpdateValue(nameof(ShellSection.CurrentItem));
        });
        Assert.True(navPage.StackDepth > 0);

        // Switch to content3
        await InvokeOnMainThreadAsync(() =>
        {
            section.CurrentItem = content3;
            handler.UpdateValue(nameof(ShellSection.CurrentItem));
        });
        Assert.True(navPage.StackDepth > 0);

        // Switch back to content1
        await InvokeOnMainThreadAsync(() =>
        {
            section.CurrentItem = content1;
            handler.UpdateValue(nameof(ShellSection.CurrentItem));
        });
        Assert.True(navPage.StackDepth > 0);
    }

    // --- RequestNavigation command (push) ---

    [AvaloniaFact(DisplayName = "RequestNavigation command pushes page onto navigation stack")]
    public async Task RequestNavigationCommandPushesPage()
    {
        var rootPage = new MauiContentPage { Title = "Root" };
        var section = CreateSection(rootPage);

        var handler = await CreateHandlerAsync<ShellSectionHandler>(section);
        var navPage = (AvaloniaNavigationPage)handler.PlatformView;

        var initialDepth = navPage.StackDepth;

        // Simulate MAUI's TCS flow: build a stack with root + pushed page
        var pushedPage = new MauiContentPage { Title = "Pushed" };
        var newStack = new List<IView> { rootPage, pushedPage };
        var request = new NavigationRequest(newStack, animated: false);

        await InvokeOnMainThreadAsync(() =>
        {
            ShellSectionHandler.RequestNavigation(handler, section, request);
        });

        // Stack should have grown
        Assert.True(navPage.StackDepth > initialDepth);
    }

    [AvaloniaFact(DisplayName = "RequestNavigation with animated flag preserves animation intent")]
    public async Task RequestNavigationAnimatedFlagPreserved()
    {
        var rootPage = new MauiContentPage { Title = "Root" };
        var section = CreateSection(rootPage);

        var handler = await CreateHandlerAsync<ShellSectionHandler>(section);
        var navPage = (AvaloniaNavigationPage)handler.PlatformView;

        var pushedPage = new MauiContentPage { Title = "Pushed" };
        var newStack = new List<IView> { rootPage, pushedPage };

        // Call with animated: true — should not throw and should still navigate
        var request = new NavigationRequest(newStack, animated: true);

        await InvokeOnMainThreadAsync(() =>
        {
            ShellSectionHandler.RequestNavigation(handler, section, request);
        });

        Assert.True(navPage.StackDepth > 1);
    }

    // --- RequestNavigation command (pop / back navigation) ---

    [AvaloniaFact(DisplayName = "RequestNavigation pops page when stack shrinks")]
    public async Task RequestNavigationPopsPage()
    {
        var rootPage = new MauiContentPage { Title = "Root" };
        var section = CreateSection(rootPage);

        var handler = await CreateHandlerAsync<ShellSectionHandler>(section);
        var navPage = (AvaloniaNavigationPage)handler.PlatformView;

        // Push a page first
        var pushedPage = new MauiContentPage { Title = "Pushed" };
        var pushStack = new List<IView> { rootPage, pushedPage };
        await InvokeOnMainThreadAsync(() =>
        {
            ShellSectionHandler.RequestNavigation(handler, section,
                new NavigationRequest(pushStack, animated: false));
        });

        Assert.Equal(2, navPage.StackDepth);

        // Pop back to root
        var popStack = new List<IView> { rootPage };
        await InvokeOnMainThreadAsync(() =>
        {
            ShellSectionHandler.RequestNavigation(handler, section,
                new NavigationRequest(popStack, animated: false));
        });

        Assert.Equal(1, navPage.StackDepth);
    }

    [AvaloniaFact(DisplayName = "Back navigation with animated: true passes animation through")]
    public async Task BackNavigationAnimated()
    {
        var rootPage = new MauiContentPage { Title = "Root" };
        var section = CreateSection(rootPage);

        var handler = await CreateHandlerAsync<ShellSectionHandler>(section);
        var navPage = (AvaloniaNavigationPage)handler.PlatformView;

        // Push a page (non-animated to keep test fast)
        var pushedPage = new MauiContentPage { Title = "Pushed" };
        var pushStack = new List<IView> { rootPage, pushedPage };
        await InvokeOnMainThreadAsync(() =>
        {
            ShellSectionHandler.RequestNavigation(handler, section,
                new NavigationRequest(pushStack, animated: false));
        });

        Assert.Equal(2, navPage.StackDepth);

        // Pop with animated: true — should still reduce stack
        // (animation completes instantly in headless mode)
        var popStack = new List<IView> { rootPage };
        await InvokeOnMainThreadAsync(() =>
        {
            ShellSectionHandler.RequestNavigation(handler, section,
                new NavigationRequest(popStack, animated: true));
        });

        Assert.Equal(1, navPage.StackDepth);
    }

    [AvaloniaFact(DisplayName = "Push then pop round-trip returns to initial state")]
    public async Task PushPopRoundTrip()
    {
        var rootPage = new MauiContentPage { Title = "Root" };
        var section = CreateSection(rootPage);

        var handler = await CreateHandlerAsync<ShellSectionHandler>(section);
        var navPage = (AvaloniaNavigationPage)handler.PlatformView;

        var initialDepth = navPage.StackDepth;

        // Push
        var pushedPage = new MauiContentPage { Title = "Detail" };
        await InvokeOnMainThreadAsync(() =>
        {
            ShellSectionHandler.RequestNavigation(handler, section,
                new NavigationRequest(new List<IView> { rootPage, pushedPage }, animated: false));
        });

        Assert.Equal(initialDepth + 1, navPage.StackDepth);

        // Pop
        await InvokeOnMainThreadAsync(() =>
        {
            ShellSectionHandler.RequestNavigation(handler, section,
                new NavigationRequest(new List<IView> { rootPage }, animated: false));
        });

        Assert.Equal(initialDepth, navPage.StackDepth);
    }

    [AvaloniaFact(DisplayName = "Multiple push then pop to root")]
    public async Task MultiplePushThenPopToRoot()
    {
        var rootPage = new MauiContentPage { Title = "Root" };
        var section = CreateSection(rootPage);

        var handler = await CreateHandlerAsync<ShellSectionHandler>(section);
        var navPage = (AvaloniaNavigationPage)handler.PlatformView;

        // Push page2, page3
        var page2 = new MauiContentPage { Title = "Page 2" };
        var page3 = new MauiContentPage { Title = "Page 3" };

        await InvokeOnMainThreadAsync(() =>
        {
            ShellSectionHandler.RequestNavigation(handler, section,
                new NavigationRequest(new List<IView> { rootPage, page2 }, animated: false));
        });

        await InvokeOnMainThreadAsync(() =>
        {
            ShellSectionHandler.RequestNavigation(handler, section,
                new NavigationRequest(new List<IView> { rootPage, page2, page3 }, animated: false));
        });

        Assert.Equal(3, navPage.StackDepth);

        // Pop to root in one request
        await InvokeOnMainThreadAsync(() =>
        {
            ShellSectionHandler.RequestNavigation(handler, section,
                new NavigationRequest(new List<IView> { rootPage }, animated: false));
        });

        Assert.Equal(1, navPage.StackDepth);
    }

    // --- Disconnect ---

    [AvaloniaFact(DisplayName = "Handler disconnect cleans up without errors")]
    public async Task HandlerDisconnectCleansUp()
    {
        var section = CreateSection();
        var handler = await CreateHandlerAsync<ShellSectionHandler>(section);

        Assert.NotNull(handler.PlatformView);

        await InvokeOnMainThreadAsync(() =>
        {
            ((IElementHandler)handler).DisconnectHandler();
        });

        // Should not throw — event unsubscription and manager disconnect succeeded
    }

    [AvaloniaFact(DisplayName = "Disconnect then reconnect creates fresh manager")]
    public async Task DisconnectThenReconnectCreatesFreshManager()
    {
        var section = CreateSection();
        var handler = await CreateHandlerAsync<ShellSectionHandler>(section);
        var navPage = (AvaloniaNavigationPage)handler.PlatformView;

        await InvokeOnMainThreadAsync(() =>
        {
            ((IElementHandler)handler).DisconnectHandler();
        });

        // Re-create handler on the same section
        var handler2 = await CreateHandlerAsync<ShellSectionHandler>(section);
        var navPage2 = (AvaloniaNavigationPage)handler2.PlatformView;

        Assert.NotNull(navPage2);
        Assert.True(navPage2.StackDepth > 0);
    }

    // --- Helpers ---

    private static ShellSection CreateSection(MauiContentPage? page = null)
    {
        return new ShellSection
        {
            Items = { new ShellContent { Content = page ?? new MauiContentPage() } }
        };
    }
}
