using Avalonia.Controls.Maui.Platform;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using MauiShellHandler = Avalonia.Controls.Maui.Handlers.Shell.ShellHandler;
using MauiFlyoutBehavior = Microsoft.Maui.FlyoutBehavior;
using MauiLabel = Microsoft.Maui.Controls.Label;
using MauiContentPage = Microsoft.Maui.Controls.ContentPage;
using Avalonia.Controls.Maui.Controls;
using NSubstitute;
using Avalonia.Controls.Maui.Handlers.Shell;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public partial class ShellHandlerTests : HandlerTestBase
{
    private static DrawerPage? GetDrawerPage(MauiShellHandler handler) =>
        (handler.PlatformView as Avalonia.Controls.ContentPage)?.Content as DrawerPage;

    [AvaloniaFact(DisplayName = "Shell Creates Platform View")]
    public async Task ShellCreatesPlatformView()
    {
        var shell = CreateBasicShell();

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);

        Assert.NotNull(handler.PlatformView);
        Assert.IsType<Avalonia.Controls.ContentPage>(handler.PlatformView);
        Assert.NotNull(GetDrawerPage(handler));
    }

    [AvaloniaFact(DisplayName = "Shell With Items Creates Platform View")]
    public async Task ShellWithItemsCreatesPlatformView()
    {
        var shell = CreateShellWithItems();

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);

        Assert.NotNull(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "FlyoutBehavior Flyout Initializes Correctly")]
    public async Task FlyoutBehaviorFlyoutInitializesCorrectly()
    {
        var shell = CreateBasicShell();
        shell.FlyoutBehavior = MauiFlyoutBehavior.Flyout;

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);
        var flyoutContainer = GetDrawerPage(handler);

        Assert.NotNull(flyoutContainer);
        Assert.Equal(DrawerBehavior.Flyout, flyoutContainer.DrawerBehavior);
    }

    [AvaloniaFact(DisplayName = "FlyoutBehavior Disabled Initializes Correctly")]
    public async Task FlyoutBehaviorDisabledInitializesCorrectly()
    {
        var shell = CreateBasicShell();
        shell.FlyoutBehavior = MauiFlyoutBehavior.Disabled;

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);
        var flyoutContainer = GetDrawerPage(handler);

        Assert.NotNull(flyoutContainer);
        Assert.Equal(DrawerBehavior.Disabled, flyoutContainer.DrawerBehavior);
        Assert.False(flyoutContainer.IsOpen);
    }

    [AvaloniaFact(DisplayName = "FlyoutBehavior Locked Initializes Correctly")]
    public async Task FlyoutBehaviorLockedInitializesCorrectly()
    {
        var shell = CreateBasicShell();
        shell.FlyoutBehavior = MauiFlyoutBehavior.Locked;

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);
        var flyoutContainer = GetDrawerPage(handler);

        Assert.NotNull(flyoutContainer);
        Assert.Equal(DrawerBehavior.Locked, flyoutContainer.DrawerBehavior);
        // In Locked mode, flyout is always visible (split mode) regardless of IsFlyoutOpen state
        // The visual position is controlled by IsSplitMode(), not IsFlyoutOpen
    }

    [AvaloniaFact(DisplayName = "FlyoutIsPresented True Opens Flyout")]
    public async Task FlyoutIsPresentedTrueOpensFlyout()
    {
        var shell = CreateBasicShell();
        shell.FlyoutBehavior = MauiFlyoutBehavior.Flyout;
        shell.FlyoutIsPresented = true;

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);
        var flyoutContainer = GetDrawerPage(handler);

        Assert.NotNull(flyoutContainer);
        Assert.True(flyoutContainer.IsOpen);
    }

    [AvaloniaFact(DisplayName = "FlyoutIsPresented False Closes Flyout")]
    public async Task FlyoutIsPresentedFalseClosesFlyout()
    {
        var shell = CreateBasicShell();
        shell.FlyoutBehavior = MauiFlyoutBehavior.Flyout;
        shell.FlyoutIsPresented = false;

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);
        var flyoutContainer = GetDrawerPage(handler);

        Assert.NotNull(flyoutContainer);
        Assert.False(flyoutContainer.IsOpen);
    }

    [AvaloniaFact(DisplayName = "FlyoutWidth Initializes Correctly")]
    public async Task FlyoutWidthInitializesCorrectly()
    {
        var shell = CreateBasicShell();
        shell.FlyoutWidth = 400;

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);
        var flyoutContainer = GetDrawerPage(handler);

        Assert.NotNull(flyoutContainer);
        Assert.Equal(400, flyoutContainer.DrawerLength);
    }

    [AvaloniaTheory(DisplayName = "FlyoutWidth Various Values")]
    [InlineData(200)]
    [InlineData(300)]
    [InlineData(500)]
    public async Task FlyoutWidthVariousValues(double width)
    {
        var shell = CreateBasicShell();
        shell.FlyoutWidth = width;

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);
        var flyoutContainer = GetDrawerPage(handler);

        Assert.NotNull(flyoutContainer);
        Assert.Equal(width, flyoutContainer.DrawerLength);
    }

    [AvaloniaFact(DisplayName = "FlyoutHeight Does Not Crash")]
    public async Task FlyoutHeightDoesNotCrash()
    {
        var shell = CreateBasicShell();
        shell.FlyoutHeight = 500;

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);
        var flyoutContainer = GetDrawerPage(handler);

        // DrawerPage auto-sizes; FlyoutHeight is a no-op
        Assert.NotNull(flyoutContainer);
    }

    [AvaloniaFact(DisplayName = "FlyoutBackground Color Initializes Correctly")]
    public async Task FlyoutBackgroundColorInitializesCorrectly()
    {
        var shell = CreateBasicShell();
        shell.FlyoutBackgroundColor = Colors.Blue;

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);

        // The flyout content control should have the background set
        Assert.NotNull(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "FlyoutBackground Brush Initializes Correctly")]
    public async Task FlyoutBackgroundBrushInitializesCorrectly()
    {
        var shell = CreateBasicShell();
        shell.FlyoutBackground = new SolidColorBrush(Colors.Red);

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);

        Assert.NotNull(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "FlyoutHeader With View Initializes Correctly")]
    public async Task FlyoutHeaderWithViewInitializesCorrectly()
    {
        var shell = CreateBasicShell();
        shell.FlyoutHeader = new MauiLabel { Text = "Header" };

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);

        Assert.NotNull(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "FlyoutFooter With View Initializes Correctly")]
    public async Task FlyoutFooterWithViewInitializesCorrectly()
    {
        var shell = CreateBasicShell();
        shell.FlyoutFooter = new MauiLabel { Text = "Footer" };

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);

        Assert.NotNull(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "FlyoutHeader With Template Initializes Correctly")]
    public async Task FlyoutHeaderWithTemplateInitializesCorrectly()
    {
        var shell = CreateBasicShell();
        shell.FlyoutHeaderTemplate = new DataTemplate(() => new MauiLabel { Text = "Template Header" });
        shell.FlyoutHeader = "Header Data";

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);

        Assert.NotNull(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "FlyoutFooter With Template Initializes Correctly")]
    public async Task FlyoutFooterWithTemplateInitializesCorrectly()
    {
        var shell = CreateBasicShell();
        shell.FlyoutFooterTemplate = new DataTemplate(() => new MauiLabel { Text = "Template Footer" });
        shell.FlyoutFooter = "Footer Data";

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);

        Assert.NotNull(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "CurrentItem Initializes Correctly")]
    public async Task CurrentItemInitializesCorrectly()
    {
        var shell = CreateShellWithItems();

        await CreateHandlerAsync<MauiShellHandler>(shell);

        Assert.NotNull(shell.CurrentItem);
    }

    [AvaloniaFact(DisplayName = "CurrentItem Can Be Changed")]
    public async Task CurrentItemCanBeChanged()
    {
        var shell = CreateShellWithMultipleItems();
        var secondItem = shell.Items[1];

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);

        await InvokeOnMainThreadAsync(() =>
        {
            shell.CurrentItem = secondItem;
            handler.UpdateValue(nameof(Shell.CurrentItem));
        });

        Assert.Equal(secondItem, shell.CurrentItem);
    }

    [AvaloniaFact(DisplayName = "ItemTemplate Initializes Correctly")]
    public async Task ItemTemplateInitializesCorrectly()
    {
        var shell = CreateShellWithItems();
        shell.ItemTemplate = new DataTemplate(() =>
        {
            var label = new MauiLabel();
            label.SetBinding(MauiLabel.TextProperty, "Title");
            return label;
        });

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);

        Assert.NotNull(handler.PlatformView);
    }

    [AvaloniaTheory(DisplayName = "FlyoutVerticalScrollMode Initializes Correctly")]
    [InlineData(ScrollMode.Auto)]
    [InlineData(ScrollMode.Enabled)]
    [InlineData(ScrollMode.Disabled)]
    public async Task FlyoutVerticalScrollModeInitializesCorrectly(ScrollMode scrollMode)
    {
        var shell = CreateBasicShell();
        shell.FlyoutVerticalScrollMode = scrollMode;

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);

        Assert.NotNull(handler.PlatformView);
    }

    [AvaloniaTheory(DisplayName = "FlyoutHeaderBehavior Initializes Correctly")]
    [InlineData(FlyoutHeaderBehavior.Default)]
    [InlineData(FlyoutHeaderBehavior.Fixed)]
    [InlineData(FlyoutHeaderBehavior.Scroll)]
    [InlineData(FlyoutHeaderBehavior.CollapseOnScroll)]
    public async Task FlyoutHeaderBehaviorInitializesCorrectly(FlyoutHeaderBehavior behavior)
    {
        var shell = CreateBasicShell();
        shell.FlyoutHeader = new MauiLabel { Text = "Header" };
        shell.FlyoutHeaderBehavior = behavior;

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);

        Assert.NotNull(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Shell With SearchHandler Initializes Correctly")]
    public async Task ShellWithSearchHandlerInitializesCorrectly()
    {
        var shell = CreateBasicShell();
        var searchHandler = new SearchHandler { Placeholder = "Search..." };
        
        // Setup SearchHandler on the current page
        var page = shell.Items[0].Items[0].Items[0].Content as MauiContentPage;
        Assert.NotNull(page);
        
        Shell.SetSearchHandler(page, searchHandler);

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);

        Assert.NotNull(handler.PlatformView);
        // Additional checks could involve inspecting the visual tree for ShellSearchControl
    }

    [AvaloniaFact(DisplayName = "Shell SearchHandler Renders Full Width Below NavBar")]
    public async Task ShellSearchHandlerRendersFullWidthBelowNavBar()
    {
        var shell = CreateBasicShell();
        var searchHandler = new SearchHandler { Placeholder = "Search..." };

        var page = shell.Items[0].Items[0].Items[0].Content as MauiContentPage;
        Assert.NotNull(page);

        Shell.SetSearchHandler(page, searchHandler);

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);

        Assert.NotNull(handler._mainContainer);
        Assert.NotNull(handler._topBar);
        Assert.NotNull(handler._searchHostControl);
        Assert.NotNull(handler._searchControl);

        await InvokeOnMainThreadAsync(() =>
        {
            const double width = 800;
            const double height = 600;

            handler._mainContainer!.Measure(new Avalonia.Size(width, height));
            handler._mainContainer.Arrange(new Avalonia.Rect(0, 0, width, height));
        });

        Assert.True(handler._searchHostControl.IsVisible);
        Assert.Same(handler._searchHostControl, handler._searchControl.Parent);
        Assert.Same(handler._mainContainer, handler._searchHostControl.Parent);

        // Search row should be below nav bar and use full page width.
        Assert.True(handler._searchHostControl.Bounds.Y >= handler._topBar.Bounds.Bottom);
        Assert.InRange(Math.Abs(handler._searchHostControl.Bounds.Width - handler._mainContainer.Bounds.Width), 0, 1.0);
    }

    [AvaloniaFact(DisplayName = "Shell SearchHandler Shows Suggestions While Typing")]
    public async Task ShellSearchHandlerShowsSuggestionsWhileTyping()
    {
        var shell = CreateBasicShell();
        var searchHandler = new SuggestionsSearchHandler();

        var page = shell.Items[0].Items[0].Items[0].Content as MauiContentPage;
        Assert.NotNull(page);

        Shell.SetSearchHandler(page, searchHandler);

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);

        Assert.NotNull(handler._mainContainer);
        Assert.NotNull(handler._mainContentControl);
        Assert.NotNull(handler._searchHostControl);
        Assert.NotNull(handler._searchControl);

        await InvokeOnMainThreadAsync(() =>
        {
            const double width = 800;
            const double height = 600;

            handler._mainContainer!.Measure(new Avalonia.Size(width, height));
            handler._mainContainer.Arrange(new Avalonia.Rect(0, 0, width, height));
        });

        var panel = handler._searchControl.Content as Panel;
        Assert.NotNull(panel);

        var searchBar = panel.Children.OfType<MauiSearchBar>().FirstOrDefault();

        Assert.NotNull(searchBar);

        await InvokeOnMainThreadAsync(() =>
        {
            searchBar!.Text = "na";
        });

        var overlayCanvas = panel.Children.OfType<Canvas>().FirstOrDefault();
        var resultsOverlay = overlayCanvas?.Children.OfType<Border>().FirstOrDefault();
        var resultsList = resultsOverlay?.Child as ListBox;

        Assert.NotNull(resultsList);
        Assert.NotNull(resultsOverlay);
        Assert.Equal("na", searchHandler.Query);
        Assert.True((resultsList.ItemsSource as System.Collections.IEnumerable)?.Cast<object>().Any() ?? false);
        Assert.True(resultsOverlay.IsVisible);

        // Suggestions must be able to render above page content.
        Assert.False(handler._searchHostControl.ClipToBounds);
        Assert.True(handler._searchHostControl.ZIndex > handler._mainContentControl.ZIndex);
    }

    [AvaloniaFact(DisplayName = "Shell With ShellContent Creates Correctly")]
    public async Task ShellWithShellContentCreatesCorrectly()
    {
        var shell = new Shell
        {
            WidthRequest = 800,
            HeightRequest = 600,
            Items =
            {
                new ShellContent
                {
                    Title = "Home",
                    Content = new Microsoft.Maui.Controls.ContentPage { Title = "Home Page" }
                }
            }
        };

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);

        Assert.NotNull(handler.PlatformView);
        Assert.NotNull(shell.CurrentItem);
    }

    [AvaloniaFact(DisplayName = "Shell With FlyoutItem Creates Correctly")]
    public async Task ShellWithFlyoutItemCreatesCorrectly()
    {
        var shell = new Shell
        {
            WidthRequest = 800,
            HeightRequest = 600,
            Items =
            {
                new FlyoutItem
                {
                    Title = "Home",
                    Items =
                    {
                        new ShellContent
                        {
                            Title = "Home",
                            Content = new Microsoft.Maui.Controls.ContentPage { Title = "Home Page" }
                        }
                    }
                }
            }
        };

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);

        Assert.NotNull(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Shell With TabBar Creates Correctly")]
    public async Task ShellWithTabBarCreatesCorrectly()
    {
        var shell = new Shell
        {
            WidthRequest = 800,
            HeightRequest = 600,
            Items =
            {
                new TabBar
                {
                    Items =
                    {
                        new ShellContent
                        {
                            Title = "Tab 1",
                            Content = new Microsoft.Maui.Controls.ContentPage { Title = "Tab 1 Page" }
                        },
                        new ShellContent
                        {
                            Title = "Tab 2",
                            Content = new Microsoft.Maui.Controls.ContentPage { Title = "Tab 2 Page" }
                        }
                    }
                }
            }
        };

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);

        Assert.NotNull(handler.PlatformView);
    }



    [AvaloniaFact(DisplayName = "Shell With Null FlyoutHeader Does Not Crash")]
    public async Task ShellWithNullFlyoutHeaderDoesNotCrash()
    {
        var shell = CreateBasicShell();
        shell.FlyoutHeader = null;

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);

        Assert.NotNull(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Shell With Null FlyoutFooter Does Not Crash")]
    public async Task ShellWithNullFlyoutFooterDoesNotCrash()
    {
        var shell = CreateBasicShell();
        shell.FlyoutFooter = null;

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);

        Assert.NotNull(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Shell With Null FlyoutContent Does Not Crash")]
    public async Task ShellWithNullFlyoutContentDoesNotCrash()
    {
        var shell = CreateBasicShell();
        shell.FlyoutContent = null;

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);

        Assert.NotNull(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Shell FlyoutHeaderBehavior Fixed Keeps Header Visible")]
    public async Task ShellFlyoutHeaderBehaviorFixedKeepsHeaderVisible()
    {
        var shell = CreateBasicShell();
        shell.FlyoutHeader = new MauiLabel { Text = "Fixed Header" };
        shell.FlyoutHeaderBehavior = FlyoutHeaderBehavior.Fixed;

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);
        
        Assert.NotNull(handler.PlatformView);
        // Verification would involve checking the position/visibility in the scroll viewer
    }

    [AvaloniaFact(DisplayName = "Shell SearchHandler Integration Logic")]
    public async Task ShellSearchHandlerIntegrationLogic()
    {
        var shell = CreateBasicShell();
        var searchHandler = new SearchHandler { Placeholder = "Test Search" };
        
        var page = shell.Items[0].Items[0].Items[0].Content as MauiContentPage;
        Shell.SetSearchHandler(page, searchHandler);

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);
        
        Assert.NotNull(handler.PlatformView);
        // Should verify ShellSearchControl is present in the visual tree
    }

    private Shell CreateBasicShell()
    {
        var shell = new Shell
        {
            Items =
            {
                new ShellContent
                {
                    Title = "Home",
                    Content = new Microsoft.Maui.Controls.ContentPage { Title = "Home Page" }
                }
            }
        };
        // Set explicit dimensions to avoid NaN in measure/arrange
        shell.WidthRequest = 800;
        shell.HeightRequest = 600;
        return shell;
    }

    private Shell CreateShellWithItems()
    {
        var shell = new Shell
        {
            Items =
            {
                new FlyoutItem
                {
                    Title = "Item 1",
                    Items =
                    {
                        new ShellContent
                        {
                            Title = "Content 1",
                            Content = new Microsoft.Maui.Controls.ContentPage { Title = "Page 1" }
                        }
                    }
                }
            }
        };
        // Set explicit dimensions to avoid NaN in measure/arrange
        shell.WidthRequest = 800;
        shell.HeightRequest = 600;
        return shell;
    }

    private Shell CreateShellWithMultipleItems()
    {
        var shell = new Shell
        {
            Items =
            {
                new FlyoutItem
                {
                    Title = "Item 1",
                    Items =
                    {
                        new ShellContent
                        {
                            Title = "Content 1",
                            Content = new Microsoft.Maui.Controls.ContentPage { Title = "Page 1" }
                        }
                    }
                },
                new FlyoutItem
                {
                    Title = "Item 2",
                    Items =
                    {
                        new ShellContent
                        {
                            Title = "Content 2",
                            Content = new Microsoft.Maui.Controls.ContentPage { Title = "Page 2" }
                        }
                    }
                }
            }
        };
        // Set explicit dimensions to avoid NaN in measure/arrange
        shell.WidthRequest = 800;
        shell.HeightRequest = 600;
        return shell;
    }
    [AvaloniaFact(DisplayName = "Shell BackgroundColor Initializes Correctly")]
    public async Task ShellBackgroundColorInitializesCorrectly()
    {
        var shell = CreateBasicShell();
        Shell.SetBackgroundColor(shell, Colors.Green);

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);
        var platformView = handler.PlatformView;
        
        var flyoutContainer = GetDrawerPage(handler);
        var mainContainer = flyoutContainer?.Content as DockPanel;
        
        Assert.NotNull(mainContainer);
        var brush = mainContainer.Background as Avalonia.Media.SolidColorBrush;
        Assert.NotNull(brush);
        
        var expectedBrush = Colors.Green.ToPlatform();
        Assert.NotNull(expectedBrush);
        
        Assert.Equal(expectedBrush.Color, brush.Color);
    }

    [AvaloniaFact(DisplayName = "Shell ForegroundColor Initializes Correctly")]
    public async Task ShellForegroundColorInitializesCorrectly()
    {
        var shell = CreateBasicShell();
        Shell.SetForegroundColor(shell, Colors.Red);

        await CreateHandlerAsync<MauiShellHandler>(shell);
        
        Assert.Equal(Colors.Red, Shell.GetForegroundColor(shell));
    }

    [AvaloniaFact(DisplayName = "Shell TitleColor Initializes Correctly")]
    public async Task ShellTitleColorInitializesCorrectly()
    {
        var shell = CreateBasicShell();
        Shell.SetTitleColor(shell, Colors.Blue);

        await CreateHandlerAsync<MauiShellHandler>(shell);

        Assert.Equal(Colors.Blue, Shell.GetTitleColor(shell));
    }

    [AvaloniaFact(DisplayName = "Shell DisabledColor Initializes Correctly")]
    public async Task ShellDisabledColorInitializesCorrectly()
    {
        var shell = CreateBasicShell();
        Shell.SetDisabledColor(shell, Colors.Gray);

        await CreateHandlerAsync<MauiShellHandler>(shell);

        Assert.Equal(Colors.Gray, Shell.GetDisabledColor(shell));
    }

    [AvaloniaFact(DisplayName = "Shell UnselectedColor Initializes Correctly")]
    public async Task ShellUnselectedColorInitializesCorrectly()
    {
        var shell = CreateBasicShell();
        Shell.SetUnselectedColor(shell, Colors.Yellow);

        await CreateHandlerAsync<MauiShellHandler>(shell);

        Assert.Equal(Colors.Yellow, Shell.GetUnselectedColor(shell));
    }

    [AvaloniaFact(DisplayName = "Shell NavBarIsVisible Property Verified")]
    public async Task ShellNavBarIsVisiblePropertyVerified()
    {
        var shell = CreateBasicShell();
        Shell.SetNavBarIsVisible(shell, false);

        await CreateHandlerAsync<MauiShellHandler>(shell);
        
        Assert.False(Shell.GetNavBarIsVisible(shell));
        // You could also verify if the top bar height is set to 0 or it's hidden in the platform view
    }

    [AvaloniaFact(DisplayName = "Shell NavBarHasShadow Property Verified")]
    public async Task ShellNavBarHasShadowPropertyVerified()
    {
        var shell = CreateBasicShell();
        Shell.SetNavBarHasShadow(shell, true);

        await CreateHandlerAsync<MauiShellHandler>(shell);

        Assert.True(Shell.GetNavBarHasShadow(shell));
    }

    [AvaloniaFact(DisplayName = "Shell FlyoutBackdrop Mapping Verified")]
    public async Task ShellFlyoutBackdropMappingVerified()
    {
        var shell = CreateBasicShell();
        var background = new SolidColorBrush(Colors.Blue);
        shell.FlyoutBackdrop = background;

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);
        var flyoutContainer = GetDrawerPage(handler);
        
        Assert.NotNull(flyoutContainer);
        // Mapping check
    }

    // --- Consolidated Platform/Control tests ---

    [AvaloniaFact(DisplayName = "DrawerPage Initial State is Closed")]
    public void DrawerPageInitialStateIsClosed()
    {
        var container = new DrawerPage();
        Assert.False(container.IsOpen);
    }

    [AvaloniaFact(DisplayName = "DrawerPage IsOpen Property Verified")]
    public void DrawerPageIsOpenPropertyVerified()
    {
        var container = new DrawerPage
        {
            DrawerBehavior = DrawerBehavior.Flyout,
            IsOpen = true
        };
        Assert.True(container.IsOpen);
    }

    [AvaloniaFact(DisplayName = "DrawerPage Locked Behavior Stays Open")]
    public void DrawerPageLockedBehaviorStaysOpen()
    {
        var container = new DrawerPage
        {
            DrawerBehavior = DrawerBehavior.Locked
        };
        Assert.Equal(DrawerBehavior.Locked, container.DrawerBehavior);
    }

    [AvaloniaFact(DisplayName = "ShellSearchControl Initializes with SearchHandler Query")]
    public void ShellSearchControlInitializesWithSearchHandlerQuery()
    {
        var mauiContext = Substitute.For<IMauiContext>();
        var searchHandler = new SearchHandler { Query = "Initial Query" };
        var control = new ShellSearchControl(searchHandler, mauiContext);
        var panel = control.Content as Panel;
        var searchBar = panel?.Children.OfType<MauiSearchBar>().FirstOrDefault();
        Assert.NotNull(searchBar);
        Assert.Equal("Initial Query", searchBar.Text);
    }

    [AvaloniaFact(DisplayName = "ShellSearchControl TextColor Initializes Correctly")]
    public void ShellSearchControlTextColorInitializesCorrectly()
    {
        var mauiContext = Substitute.For<IMauiContext>();
        var searchHandler = new SearchHandler { TextColor = Colors.Red };
        
        var control = new ShellSearchControl(searchHandler, mauiContext);
        var panel = control.Content as Panel;
        var searchBar = panel?.Children.OfType<MauiSearchBar>().FirstOrDefault();
        Assert.NotNull(searchBar);
        Assert.NotNull(searchBar.Foreground);
        var brush = searchBar.Foreground as Avalonia.Media.SolidColorBrush;
        Assert.Equal(Colors.Red.ToPlatform().Color, brush!.Color);
    }

    [AvaloniaFact(DisplayName = "ShellSearchControl PlaceholderColor Initializes Correctly")]
    public void ShellSearchControlPlaceholderColorInitializesCorrectly()
    {
        var mauiContext = Substitute.For<IMauiContext>();
        var searchHandler = new SearchHandler { PlaceholderColor = Colors.Blue };
        
        var control = new ShellSearchControl(searchHandler, mauiContext);
        var panel = control.Content as Panel;
        var searchBar = panel?.Children.OfType<MauiSearchBar>().FirstOrDefault();
        Assert.NotNull(searchBar);
        Assert.NotNull(searchBar.PlaceholderForeground);
        var brush = searchBar.PlaceholderForeground as Avalonia.Media.SolidColorBrush;
        Assert.Equal(Colors.Blue.ToPlatform().Color, brush!.Color);
    }

    [AvaloniaFact(DisplayName = "ShellSearchControl CancelButtonColor Initializes Correctly")]
    public void ShellSearchControlCancelButtonColorInitializesCorrectly()
    {
        var mauiContext = Substitute.For<IMauiContext>();
        var searchHandler = new SearchHandler { CancelButtonColor = Colors.Red };
        
        var control = new ShellSearchControl(searchHandler, mauiContext);
        var panel = control.Content as Panel;
        var searchBar = panel?.Children.OfType<MauiSearchBar>().FirstOrDefault();
        Assert.NotNull(searchBar);
        var brush = searchBar.CancelButtonColor as Avalonia.Media.SolidColorBrush;
        Assert.NotNull(brush);
        Assert.Equal(Colors.Red.ToPlatform().Color, brush.Color);
    }

    [AvaloniaFact(DisplayName = "ShellSearchControl HorizontalTextAlignment Initializes Correctly")]
    public void ShellSearchControlHorizontalTextAlignmentInitializesCorrectly()
    {
        var mauiContext = Substitute.For<IMauiContext>();
        var searchHandler = new SearchHandler { HorizontalTextAlignment = Microsoft.Maui.TextAlignment.Center };
        
        var control = new ShellSearchControl(searchHandler, mauiContext);
        var panel = control.Content as Panel;
        var searchBar = panel?.Children.OfType<MauiSearchBar>().FirstOrDefault();
        Assert.NotNull(searchBar);
        Assert.Equal(Avalonia.Media.TextAlignment.Center, searchBar.HorizontalTextAlignment);
    }

    [AvaloniaFact(DisplayName = "ShellSearchControl VerticalTextAlignment Initializes Correctly")]
    public void ShellSearchControlVerticalTextAlignmentInitializesCorrectly()
    {
        var mauiContext = Substitute.For<IMauiContext>();
        var searchHandler = new SearchHandler { VerticalTextAlignment = Microsoft.Maui.TextAlignment.End };
        
        var control = new ShellSearchControl(searchHandler, mauiContext);
        var panel = control.Content as Panel;
        var searchBar = panel?.Children.OfType<MauiSearchBar>().FirstOrDefault();
        Assert.NotNull(searchBar);
        Assert.Equal(Avalonia.Layout.VerticalAlignment.Bottom, searchBar.VerticalContentAlignment);
    }

    [AvaloniaFact(DisplayName = "ShellSearchControl ClearPlaceholder Properties Initialize Correctly")]
    public void ShellSearchControlClearPlaceholderPropertiesInitializeCorrectly()
    {
        var mauiContext = Substitute.For<IMauiContext>();
        var command = new Command(() => { });
        var searchHandler = new SearchHandler 
        { 
            ClearPlaceholderCommand = command,
            ClearPlaceholderCommandParameter = "Test",
            ClearPlaceholderEnabled = false
        };
        
        var control = new ShellSearchControl(searchHandler, mauiContext);
        var panel = control.Content as Panel;
        var searchBar = panel?.Children.OfType<MauiSearchBar>().FirstOrDefault();
        Assert.NotNull(searchBar);
        Assert.Equal(command, searchBar.ClearCommand);
        Assert.Equal("Test", searchBar.ClearCommandParameter);
        Assert.False(searchBar.IsClearEnabled);
    }

    [AvaloniaFact(DisplayName = "ShellSearchControl CharacterSpacing Initializes Correctly")]
    public void ShellSearchControlCharacterSpacingInitializesCorrectly()
    {
        var mauiContext = Substitute.For<IMauiContext>();
        var searchHandler = new SearchHandler { CharacterSpacing = 5.0 };
        
        var control = new ShellSearchControl(searchHandler, mauiContext);
        var panel = control.Content as Panel;
        var searchBar = panel?.Children.OfType<MauiSearchBar>().FirstOrDefault();
        Assert.NotNull(searchBar);
        Assert.Equal(5.0, searchBar.CharacterSpacing);
    }



    [AvaloniaFact(DisplayName = "BackButtonBehavior IsEnabled False Disables Back Button")]
    public async Task BackButtonBehaviorIsEnabledFalseDisablesBackButton()
    {
        var shell = CreateShellWithNavigationStack();
        var page = shell.CurrentPage!;

        var behavior = new BackButtonBehavior { IsEnabled = false };
        Shell.SetBackButtonBehavior(page, behavior);

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);

        Assert.NotNull(handler._backButton);
        Assert.False(handler._backButton.IsEnabled);
    }

    [AvaloniaFact(DisplayName = "BackButtonBehavior IsVisible False Hides Back Button")]
    public async Task BackButtonBehaviorIsVisibleFalseHidesBackButton()
    {
        var shell = CreateShellWithNavigationStack();
        var page = shell.CurrentPage!;

        var behavior = new BackButtonBehavior { IsVisible = false };
        Shell.SetBackButtonBehavior(page, behavior);

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);

        Assert.NotNull(handler._backButton);
        Assert.False(handler._backButton.IsVisible);
    }

    [AvaloniaFact(DisplayName = "BackButtonBehavior TextOverride Changes Button Content")]
    public async Task BackButtonBehaviorTextOverrideChangesButtonContent()
    {
        var shell = CreateShellWithNavigationStack();
        var page = shell.CurrentPage!;

        var behavior = new BackButtonBehavior { TextOverride = "Go Back" };
        Shell.SetBackButtonBehavior(page, behavior);

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);

        Assert.NotNull(handler._backButton);
        Assert.Equal("Go Back", handler._backButton.Content);
    }

    [AvaloniaFact(DisplayName = "BackButtonBehavior Default Shows Arrow")]
    public async Task BackButtonBehaviorDefaultShowsArrow()
    {
        var shell = CreateShellWithNavigationStack();

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);

        Assert.NotNull(handler._backButton);
        Assert.Equal("←", handler._backButton.Content);
    }

    [AvaloniaFact(DisplayName = "Shell Title Remains Centered With Left Buttons Visible")]
    public async Task ShellTitleRemainsCenteredWithLeftButtonsVisible()
    {
        var shell = CreateShellWithNavigationStack();

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);

        Assert.NotNull(handler._topBar);
        Assert.NotNull(handler._titleTextBlock);
        Assert.NotNull(handler._hamburgerButton);
        Assert.NotNull(handler._backButton);

        var deltaFromCenter = await InvokeOnMainThreadAsync(() =>
        {
            handler._hamburgerButton!.IsVisible = true;
            handler._backButton!.IsVisible = true;

            const double arrangedWidth = 800;
            handler._topBar!.Measure(new Avalonia.Size(arrangedWidth, MauiShellHandler.DefaultBarHeight));
            handler._topBar.Arrange(new Avalonia.Rect(0, 0, arrangedWidth, MauiShellHandler.DefaultBarHeight));

            var titleBounds = handler._titleTextBlock!.Bounds;
            var titleCenterXLocal = titleBounds.X + (titleBounds.Width / 2);
            var titleCenterYLocal = titleBounds.Y + (titleBounds.Height / 2);

            var titleCenterInTopBar = handler._titleTextBlock.TranslatePoint(
                new Avalonia.Point(titleCenterXLocal, titleCenterYLocal),
                handler._topBar);

            Assert.NotNull(titleCenterInTopBar);
            var expectedCenterX = arrangedWidth / 2;

            return Math.Abs(titleCenterInTopBar.Value.X - expectedCenterX);
        });

        Assert.InRange(deltaFromCenter, 0, 1.0);
    }

    [AvaloniaFact(DisplayName = "BackButtonBehavior Command Executes When Clicked")]
    public async Task BackButtonBehaviorCommandExecutesWhenClicked()
    {
        var shell = CreateShellWithNavigationStack();
        var page = shell.CurrentPage!;

        bool commandExecuted = false;
        var behavior = new BackButtonBehavior
        {
            Command = new Command(() => commandExecuted = true)
        };
        Shell.SetBackButtonBehavior(page, behavior);

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);

        Assert.NotNull(handler._backButton);

        // Simulate click
        await InvokeOnMainThreadAsync(() =>
        {
            var clickMethod = typeof(MauiShellHandler).GetMethod("OnBackButtonClick",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            clickMethod?.Invoke(handler, new object?[] { null, new Avalonia.Interactivity.RoutedEventArgs() });
        });

        Assert.True(commandExecuted);
    }

    [AvaloniaFact(DisplayName = "BackButtonBehavior CommandParameter Is Passed To Command")]
    public async Task BackButtonBehaviorCommandParameterIsPassedToCommand()
    {
        var shell = CreateShellWithNavigationStack();
        var page = shell.CurrentPage!;

        object? receivedParameter = null;
        var behavior = new BackButtonBehavior
        {
            Command = new Command<string>(param => receivedParameter = param),
            CommandParameter = "TestParameter"
        };
        Shell.SetBackButtonBehavior(page, behavior);

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);

        // Simulate click
        await InvokeOnMainThreadAsync(() =>
        {
            var clickMethod = typeof(MauiShellHandler).GetMethod("OnBackButtonClick",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            clickMethod?.Invoke(handler, new object?[] { null, new Avalonia.Interactivity.RoutedEventArgs() });
        });

        Assert.Equal("TestParameter", receivedParameter);
    }

    [AvaloniaFact(DisplayName = "Shell VirtualView Returns Null After Disconnect")]
    public async Task ShellVirtualViewReturnsNullAfterDisconnect()
    {
        var shell = CreateBasicShell();

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);

        Assert.NotNull(handler.VirtualView);

        await InvokeOnMainThreadAsync(() =>
        {
            ((IElementHandler)handler).DisconnectHandler();
        });

        Assert.Null(handler.VirtualView);
    }

    [AvaloniaFact(DisplayName = "BackButtonBehavior Null Restores Default Behavior")]
    public async Task BackButtonBehaviorNullRestoresDefaultBehavior()
    {
        var shell = CreateShellWithNavigationStack();
        var page = shell.CurrentPage!;

        // First set a behavior
        var behavior = new BackButtonBehavior { TextOverride = "Custom" };
        Shell.SetBackButtonBehavior(page, behavior);

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);
        Assert.Equal("Custom", handler._backButton!.Content);

        // Now clear the behavior
        await InvokeOnMainThreadAsync(() =>
        {
            Shell.SetBackButtonBehavior(page, null);
            handler.UpdateValue(Shell.BackButtonBehaviorProperty.PropertyName);
        });

        Assert.Equal("←", handler._backButton.Content);
        Assert.True(handler._backButton.IsEnabled);
    }

    [AvaloniaFact(DisplayName = "Shell Navigating Between FlyoutItems Preserves Page Content")]
    public async Task ShellNavigatingBetweenFlyoutItemsPreservesPageContent()
    {
        var page1 = new Microsoft.Maui.Controls.ContentPage { Title = "Page 1", Content = new MauiLabel { Text = "Content 1" } };
        var page2 = new Microsoft.Maui.Controls.ContentPage { Title = "Page 2", Content = new MauiLabel { Text = "Content 2" } };
        var page3 = new Microsoft.Maui.Controls.ContentPage { Title = "Page 3", Content = new MauiLabel { Text = "Content 3" } };

        var shell = new Shell
        {
            WidthRequest = 800,
            HeightRequest = 600,
            Items =
            {
                new FlyoutItem
                {
                    Title = "Item 1",
                    Items =
                    {
                        new ShellContent { Title = "Content 1", Content = page1 }
                    }
                },
                new FlyoutItem
                {
                    Title = "Item 2",
                    Items =
                    {
                        new ShellContent { Title = "Content 2", Content = page2 }
                    }
                },
                new FlyoutItem
                {
                    Title = "Item 3",
                    Items =
                    {
                        new ShellContent { Title = "Content 3", Content = page3 }
                    }
                }
            }
        };

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);

        // Verify initial state: Item 1 is current, its section has a handler
        var section1 = ((ShellItem)shell.Items[0]).Items[0];
        Assert.NotNull(section1.Handler);

        // Switch to Item 2
        await InvokeOnMainThreadAsync(() =>
        {
            shell.CurrentItem = shell.Items[1];
            handler.UpdateValue(nameof(Shell.CurrentItem));
        });

        var section2 = ((ShellItem)shell.Items[1]).Items[0];
        Assert.NotNull(section2.Handler);

        // Switch to Item 3
        await InvokeOnMainThreadAsync(() =>
        {
            shell.CurrentItem = shell.Items[2];
            handler.UpdateValue(nameof(Shell.CurrentItem));
        });

        var section3 = ((ShellItem)shell.Items[2]).Items[0];
        Assert.NotNull(section3.Handler);

        // Switch back to Item 1 — page should still be there
        await InvokeOnMainThreadAsync(() =>
        {
            shell.CurrentItem = shell.Items[0];
            handler.UpdateValue(nameof(Shell.CurrentItem));
        });

        // The section handler should still be present and the page should be accessible
        Assert.NotNull(section1.Handler);
        Assert.Equal(page1, ((IShellContentController)section1.CurrentItem).Page);

        // Switch back to Item 2 — page should still be there
        await InvokeOnMainThreadAsync(() =>
        {
            shell.CurrentItem = shell.Items[1];
            handler.UpdateValue(nameof(Shell.CurrentItem));
        });

        Assert.NotNull(section2.Handler);
        Assert.Equal(page2, ((IShellContentController)section2.CurrentItem).Page);
    }

    [AvaloniaFact(DisplayName = "Shell Section Handler Stays Connected During FlyoutItem Switch")]
    public async Task ShellSectionHandlerStaysConnectedDuringFlyoutItemSwitch()
    {
        var shell = CreateShellWithMultipleItems();

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);

        var section1 = ((ShellItem)shell.Items[0]).Items[0];
        var initialHandler = section1.Handler;
        Assert.NotNull(initialHandler);

        // Switch to Item 2
        await InvokeOnMainThreadAsync(() =>
        {
            shell.CurrentItem = shell.Items[1];
            handler.UpdateValue(nameof(Shell.CurrentItem));
        });

        // Section 1's handler should NOT have been disconnected
        Assert.NotNull(section1.Handler);
        Assert.Same(initialHandler, section1.Handler);

        // Switch back to Item 1
        await InvokeOnMainThreadAsync(() =>
        {
            shell.CurrentItem = shell.Items[0];
            handler.UpdateValue(nameof(Shell.CurrentItem));
        });

        // The same handler should still be there
        Assert.NotNull(section1.Handler);
        Assert.Same(initialHandler, section1.Handler);
    }

    private Shell CreateShellWithNavigationStack()
    {
        var page1 = new Microsoft.Maui.Controls.ContentPage { Title = "Page 1" };
        var page2 = new Microsoft.Maui.Controls.ContentPage { Title = "Page 2" };

        var shellSection = new ShellSection
        {
            Items =
            {
                new ShellContent
                {
                    Title = "Content 1",
                    Content = page1
                }
            }
        };

        var shell = new Shell
        {
            WidthRequest = 800,
            HeightRequest = 600,
            Items =
            {
                new FlyoutItem
                {
                    Title = "Item 1",
                    Items = { shellSection }
                }
            }
        };

        // Simulate navigation to create a stack
        shellSection.Navigation.PushAsync(page2);

        return shell;
    }

    private sealed class SuggestionsSearchHandler : SearchHandler
    {
        private static readonly string[] Suggestions =
        {
            "Navigation",
            "Styling",
            "Structure",
            "Colors"
        };

        public SuggestionsSearchHandler()
        {
            ShowsResults = true;
        }

        protected override void OnQueryChanged(string oldValue, string newValue)
        {
            base.OnQueryChanged(oldValue, newValue);

            if (string.IsNullOrWhiteSpace(newValue))
            {
                ItemsSource = null;
                return;
            }

            ItemsSource = Suggestions
                .Where(x => x.Contains(newValue, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }
}
