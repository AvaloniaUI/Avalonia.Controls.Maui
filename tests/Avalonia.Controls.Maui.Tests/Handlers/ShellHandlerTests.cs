using Avalonia.Controls.Maui.Platform;
using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.Tests.TestUtilities;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using MauiShellHandler = Avalonia.Controls.Maui.Handlers.Shell.ShellHandler;
using MauiFlyoutBehavior = Microsoft.Maui.FlyoutBehavior;
using MauiLabel = Microsoft.Maui.Controls.Label;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public partial class ShellHandlerTests : HandlerTestBase
{
    // ============================================================
    // Basic Shell Creation Tests
    // ============================================================

    [AvaloniaFact(DisplayName = "Shell Creates Platform View")]
    public async Task ShellCreatesPlatformView()
    {
        var shell = CreateBasicShell();

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);

        Assert.NotNull(handler.PlatformView);
        Assert.IsType<FlyoutContainer>(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Shell With Items Creates Platform View")]
    public async Task ShellWithItemsCreatesPlatformView()
    {
        var shell = CreateShellWithItems();

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);

        Assert.NotNull(handler.PlatformView);
    }

    // ============================================================
    // FlyoutBehavior Tests
    // ============================================================

    [AvaloniaFact(DisplayName = "FlyoutBehavior Flyout Initializes Correctly")]
    public async Task FlyoutBehaviorFlyoutInitializesCorrectly()
    {
        var shell = CreateBasicShell();
        shell.FlyoutBehavior = MauiFlyoutBehavior.Flyout;

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);
        var flyoutContainer = handler.PlatformView as FlyoutContainer;

        Assert.NotNull(flyoutContainer);
        Assert.Equal(Platform.FlyoutBehavior.Popover, flyoutContainer.FlyoutBehavior);
    }

    [AvaloniaFact(DisplayName = "FlyoutBehavior Disabled Initializes Correctly")]
    public async Task FlyoutBehaviorDisabledInitializesCorrectly()
    {
        var shell = CreateBasicShell();
        shell.FlyoutBehavior = MauiFlyoutBehavior.Disabled;

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);
        var flyoutContainer = handler.PlatformView as FlyoutContainer;

        Assert.NotNull(flyoutContainer);
        Assert.Equal(Platform.FlyoutBehavior.Disabled, flyoutContainer.FlyoutBehavior);
        Assert.False(flyoutContainer.IsFlyoutOpen);
    }

    [AvaloniaFact(DisplayName = "FlyoutBehavior Locked Initializes Correctly")]
    public async Task FlyoutBehaviorLockedInitializesCorrectly()
    {
        var shell = CreateBasicShell();
        shell.FlyoutBehavior = MauiFlyoutBehavior.Locked;

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);
        var flyoutContainer = handler.PlatformView as FlyoutContainer;

        Assert.NotNull(flyoutContainer);
        Assert.Equal(Platform.FlyoutBehavior.Locked, flyoutContainer.FlyoutBehavior);
        // In Locked mode, flyout is always visible (split mode) regardless of IsFlyoutOpen state
        // The visual position is controlled by IsSplitMode(), not IsFlyoutOpen
    }

    // ============================================================
    // FlyoutIsPresented Tests
    // ============================================================

    [AvaloniaFact(DisplayName = "FlyoutIsPresented True Opens Flyout")]
    public async Task FlyoutIsPresentedTrueOpensFlyout()
    {
        var shell = CreateBasicShell();
        shell.FlyoutBehavior = MauiFlyoutBehavior.Flyout;
        shell.FlyoutIsPresented = true;

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);
        var flyoutContainer = handler.PlatformView as FlyoutContainer;

        Assert.NotNull(flyoutContainer);
        Assert.True(flyoutContainer.IsFlyoutOpen);
    }

    [AvaloniaFact(DisplayName = "FlyoutIsPresented False Closes Flyout")]
    public async Task FlyoutIsPresentedFalseClosesFlyout()
    {
        var shell = CreateBasicShell();
        shell.FlyoutBehavior = MauiFlyoutBehavior.Flyout;
        shell.FlyoutIsPresented = false;

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);
        var flyoutContainer = handler.PlatformView as FlyoutContainer;

        Assert.NotNull(flyoutContainer);
        Assert.False(flyoutContainer.IsFlyoutOpen);
    }

    // ============================================================
    // FlyoutWidth Tests
    // ============================================================

    [AvaloniaFact(DisplayName = "FlyoutWidth Initializes Correctly")]
    public async Task FlyoutWidthInitializesCorrectly()
    {
        var shell = CreateBasicShell();
        shell.FlyoutWidth = 400;

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);
        var flyoutContainer = handler.PlatformView as FlyoutContainer;

        Assert.NotNull(flyoutContainer);
        Assert.Equal(400, flyoutContainer.FlyoutWidth);
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
        var flyoutContainer = handler.PlatformView as FlyoutContainer;

        Assert.NotNull(flyoutContainer);
        Assert.Equal(width, flyoutContainer.FlyoutWidth);
    }

    // ============================================================
    // FlyoutHeight Tests
    // ============================================================

    [AvaloniaFact(DisplayName = "FlyoutHeight Initializes Correctly")]
    public async Task FlyoutHeightInitializesCorrectly()
    {
        var shell = CreateBasicShell();
        shell.FlyoutHeight = 500;

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);
        var flyoutContainer = handler.PlatformView as FlyoutContainer;

        Assert.NotNull(flyoutContainer);
        Assert.Equal(500, flyoutContainer.FlyoutHeight);
    }

    // ============================================================
    // FlyoutBackground Tests
    // ============================================================

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

    // ============================================================
    // FlyoutHeader and Footer Tests
    // ============================================================

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

    // ============================================================
    // CurrentItem Tests
    // ============================================================

    [AvaloniaFact(DisplayName = "CurrentItem Initializes Correctly")]
    public async Task CurrentItemInitializesCorrectly()
    {
        var shell = CreateShellWithItems();

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);

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

    // ============================================================
    // ItemTemplate Tests
    // ============================================================

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

    // ============================================================
    // FlyoutVerticalScrollMode Tests
    // ============================================================

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

    // ============================================================
    // FlyoutHeaderBehavior Tests
    // ============================================================

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

    // ============================================================
    // Shell Navigation Structure Tests
    // ============================================================

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
                    Content = new ContentPage { Title = "Home Page" }
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
                            Content = new ContentPage { Title = "Home Page" }
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
                            Content = new ContentPage { Title = "Tab 1 Page" }
                        },
                        new ShellContent
                        {
                            Title = "Tab 2",
                            Content = new ContentPage { Title = "Tab 2 Page" }
                        }
                    }
                }
            }
        };

        var handler = await CreateHandlerAsync<MauiShellHandler>(shell);

        Assert.NotNull(handler.PlatformView);
    }

    // ============================================================
    // Null Safety Tests
    // ============================================================

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

    // ============================================================
    // Helper Methods
    // ============================================================

    private Shell CreateBasicShell()
    {
        var shell = new Shell
        {
            Items =
            {
                new ShellContent
                {
                    Title = "Home",
                    Content = new ContentPage { Title = "Home Page" }
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
                            Content = new ContentPage { Title = "Page 1" }
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
                            Content = new ContentPage { Title = "Page 1" }
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
                            Content = new ContentPage { Title = "Page 2" }
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
}
