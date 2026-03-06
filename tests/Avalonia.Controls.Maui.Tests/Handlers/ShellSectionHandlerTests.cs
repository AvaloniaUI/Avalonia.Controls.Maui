using Avalonia.Headless.XUnit;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Avalonia.Controls.Maui.Handlers.Shell;
using Avalonia.Animation;
using MauiPage = Microsoft.Maui.Controls.Page;
using MauiContentPage = Microsoft.Maui.Controls.ContentPage;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public class ShellSectionHandlerTests : HandlerTestBase
{
    [AvaloniaFact(DisplayName = "ShellSectionHandler Uses TransitioningContentControl")]
    public async Task ShellSectionHandlerUsesTransitioningContentControl()

    {
        var section = new ShellSection
        {
            Items = { new ShellContent { Content = new Microsoft.Maui.Controls.ContentPage() } }
        };

        var handler = await CreateHandlerAsync<ShellSectionHandler>(section);

        Assert.NotNull(handler.PlatformView);
        Assert.IsType<TransitioningContentControl>(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Animated PresentationMode Sets PageTransition")]
    public async Task AnimatedPresentationModeSetsPageTransition()
    {
        var page1 = new Microsoft.Maui.Controls.ContentPage { Title = "Page 1" };
        var page2 = new Microsoft.Maui.Controls.ContentPage { Title = "Page 2" };
        Shell.SetPresentationMode(page2, PresentationMode.Animated);

        var section = new ShellSection
        {
            Items = { new ShellContent { Content = page1 } }
        };

        var handler = await CreateHandlerAsync<ShellSectionHandler>(section);
        var control = handler.PlatformView as TransitioningContentControl;
        
        Assert.NotNull(control);

        // Manually trigger navigation on the handler
        var stackNavigation = handler as IStackNavigation;
        var request = new NavigationRequest(new List<MauiPage> { page1, page2 }, true);
        
        await InvokeOnMainThreadAsync(() =>
        {
             stackNavigation.RequestNavigation(request);
        });

        // Check transition
        Assert.NotNull(control.PageTransition);
        Assert.IsType<PageSlide>(control.PageTransition);
        var slide = control.PageTransition as PageSlide;
        Assert.NotNull(slide);
        Assert.Equal(PageSlide.SlideAxis.Horizontal, slide.Orientation);
    }

    [AvaloniaFact(DisplayName = "NotAnimated PresentationMode Sets Null Transition")]
    public async Task NotAnimatedPresentationModeSetsNullTransition()
    {
        var page1 = new Microsoft.Maui.Controls.ContentPage { Title = "Page 1" };
        var page2 = new Microsoft.Maui.Controls.ContentPage { Title = "Page 2" };
        Shell.SetPresentationMode(page2, PresentationMode.NotAnimated);

        var section = new ShellSection
        {
            Items = { new ShellContent { Content = page1 } }
        };

        var handler = await CreateHandlerAsync<ShellSectionHandler>(section);
        var control = handler.PlatformView as TransitioningContentControl;
        
        Assert.NotNull(control);

        // Manually trigger navigation
        var stackNavigation = handler as IStackNavigation;
        var request = new NavigationRequest(new List<MauiPage> { page1, page2 }, true);

        await InvokeOnMainThreadAsync(() =>
        {
             stackNavigation.RequestNavigation(request);
        });

        // Check transition
        Assert.Null(control.PageTransition);
    }

    [AvaloniaFact(DisplayName = "Modal PresentationMode Sets Vertical Transition")]
    public async Task ModalPresentationModeSetsVerticalTransition()
    {
        var page1 = new Microsoft.Maui.Controls.ContentPage { Title = "Page 1" };
        var page2 = new Microsoft.Maui.Controls.ContentPage { Title = "Page 2" };
        Shell.SetPresentationMode(page2, PresentationMode.Modal);

        var section = new ShellSection
        {
            Items = { new ShellContent { Content = page1 } }
        };

        var handler = await CreateHandlerAsync<ShellSectionHandler>(section);
        var control = handler.PlatformView as TransitioningContentControl;

        Assert.NotNull(control);

        // Manually trigger navigation
        var stackNavigation = handler as IStackNavigation;
        var request = new NavigationRequest(new List<MauiPage> { page1, page2 }, true);

        await InvokeOnMainThreadAsync(() =>
        {
             stackNavigation.RequestNavigation(request);
        });

        // Check transition
        Assert.NotNull(control.PageTransition);
        Assert.IsType<PageSlide>(control.PageTransition);
        var slide = control.PageTransition as PageSlide;
        Assert.NotNull(slide);
        Assert.Equal(PageSlide.SlideAxis.Vertical, slide.Orientation);
    }

    [AvaloniaFact(DisplayName = "ShellSection CurrentItem Mapping Updates Content")]
    public async Task ShellSectionCurrentItemMappingUpdatesContent()
    {
        var page1 = new Microsoft.Maui.Controls.ContentPage { Title = "Page 1" };
        var page2 = new Microsoft.Maui.Controls.ContentPage { Title = "Page 2" };
        var content1 = new ShellContent { Content = page1 };
        var content2 = new ShellContent { Content = page2 };
        
        var section = new ShellSection
        {
            Items = { content1, content2 }
        };

        var handler = await CreateHandlerAsync<ShellSectionHandler>(section);
        var control = handler.PlatformView as TransitioningContentControl;

        await InvokeOnMainThreadAsync(() =>
        {
            section.CurrentItem = content2;
            handler.UpdateValue(nameof(ShellSection.CurrentItem));
        });

        Assert.NotNull(control);
    }

    [AvaloniaFact(DisplayName = "Modal PresentationMode Sets Vertical Page Transition")]
    public async Task ModalPresentationModeSetsVerticalPageTransition()
    {
        var page1 = new Microsoft.Maui.Controls.ContentPage { Title = "Page 1" };
        var page2 = new Microsoft.Maui.Controls.ContentPage { Title = "Page 2" };
        Shell.SetPresentationMode(page2, PresentationMode.Modal);

        var section = new ShellSection
        {
            Items = { new ShellContent { Content = page1 } }
        };

        var handler = await CreateHandlerAsync<ShellSectionHandler>(section);
        var control = handler.PlatformView as TransitioningContentControl;

        var stackNavigation = handler as IStackNavigation;
        var request = new NavigationRequest(new List<MauiPage> { page1, page2 }, true);

        await InvokeOnMainThreadAsync(() =>
        {
             stackNavigation!.RequestNavigation(request);
        });

        Assert.NotNull(control!.PageTransition);
        var slide = control.PageTransition as PageSlide;
        Assert.NotNull(slide);
        Assert.Equal(PageSlide.SlideAxis.Vertical, slide.Orientation);
    }

    [AvaloniaFact(DisplayName = "SyncNavigationStack Verification")]
    public async Task SyncNavigationStackVerification()
    {
        var page1 = new Microsoft.Maui.Controls.ContentPage { Title = "Page 1" };
        var page2 = new Microsoft.Maui.Controls.ContentPage { Title = "Page 2" };

        var section = new ShellSection
        {
            Items = { new ShellContent { Content = page1 } }
        };
        
        var handler = await CreateHandlerAsync<ShellSectionHandler>(section);
        var control = handler.PlatformView as TransitioningContentControl;

        await InvokeOnMainThreadAsync(() =>
        {
            // Verify that we can trigger sync without crashes
            handler.SyncNavigationStack();
        });

        Assert.NotNull(control);
    }
}
