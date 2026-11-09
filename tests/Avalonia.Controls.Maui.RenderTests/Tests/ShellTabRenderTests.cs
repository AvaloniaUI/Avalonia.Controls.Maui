using System.Runtime.CompilerServices;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;
using Avalonia.Headless.XUnit;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using MauiContentPage = Microsoft.Maui.Controls.ContentPage;

using AvaloniaBrushes = Avalonia.Media.Brushes;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class ShellTabRenderTests : RenderTestBase
{
    private async Task RenderShellToFile(Shell shell, double width = 400, double height = 300, double scale = 1, [CallerMemberName] string testName = "")
    {
        EnsureHandlerCreated();

        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var handler = shell.ToHandler(MauiContext);
            await RenderControlToFile((Control)handler.PlatformView!, width, height, scale, testName);
        });
    }

    [AvaloniaFact]
    public async Task Render_Shell_MultipleTabs_Default()
    {
        var shell = new Shell
        {
            Items =
            {
                new TabBar
                {
                    Items =
                    {
                        new ShellContent { Title = "Home", Content = new MauiContentPage { Content = new Microsoft.Maui.Controls.Label { Text = "Home Page", TextColor = Colors.Black } } },
                        new ShellContent { Title = "Settings", Content = new MauiContentPage { Content = new Microsoft.Maui.Controls.Label { Text = "Settings Page", TextColor = Colors.Black } } },
                        new ShellContent { Title = "About", Content = new MauiContentPage { Content = new Microsoft.Maui.Controls.Label { Text = "About Page", TextColor = Colors.Black } } }
                    }
                }
            }
        };

        await RenderShellToFile(shell);
        CompareImages(tolerance: 0.045);
    }

    [AvaloniaFact]
    public async Task Render_Shell_MultipleTabs_CustomColors()
    {
        var shell = new Shell
        {
            Items =
            {
                new TabBar
                {
                    Items =
                    {
                        new ShellContent { Title = "Home", Content = new MauiContentPage { Content = new Microsoft.Maui.Controls.Label { Text = "Home Page", TextColor = Colors.Black } } },
                        new ShellContent { Title = "Settings", Content = new MauiContentPage { Content = new Microsoft.Maui.Controls.Label { Text = "Settings Page", TextColor = Colors.Black } } }
                    }
                }
            }
        };

        Shell.SetTabBarBackgroundColor(shell, Colors.DarkBlue);
        Shell.SetTabBarForegroundColor(shell, Colors.Yellow);
        Shell.SetTabBarUnselectedColor(shell, Colors.LightGray);

        await RenderShellToFile(shell);
        CompareImages(tolerance: 0.045);
    }

    [AvaloniaFact]
    public async Task Render_Shell_SingleSection_NoTabs()
    {
        var shell = new Shell
        {
            Items =
            {
                new ShellContent { Title = "Only Page", Content = new MauiContentPage { Content = new Microsoft.Maui.Controls.Label { Text = "Single page, no tabs", TextColor = Colors.Black } } }
            }
        };

        await RenderShellToFile(shell);
        CompareImages(tolerance: 0.050);
    }

    static Shell CreateShellWithGlyphTabIcons()
    {
        static FontImageSource Icon(string glyph) => new() { Glyph = glyph, Size = 16, Color = Colors.Black };

        var shell = new Shell
        {
            Items =
            {
                new TabBar
                {
                    Items =
                    {
                        new ShellContent { Title = "Home", Icon = Icon("H"), Content = new MauiContentPage { Content = new Microsoft.Maui.Controls.Label { Text = "Home Page", TextColor = Colors.Black } } },
                        new ShellContent { Title = "Mail", Icon = Icon("M"), Content = new MauiContentPage { Content = new Microsoft.Maui.Controls.Label { Text = "Mail Page", TextColor = Colors.Black } } }
                    }
                }
            }
        };

        // The icons are tinted through an opacity mask, so the tab colours show in them
        Shell.SetTabBarForegroundColor(shell, Colors.DarkRed);
        Shell.SetTabBarUnselectedColor(shell, Colors.SteelBlue);
        return shell;
    }

    [AvaloniaFact]
    public async Task Render_Shell_TabIcons_FontImageSource()
    {
        await RenderShellToFile(CreateShellWithGlyphTabIcons(), height: 120);
        CompareImages(tolerance: 0.045);
    }

    [AvaloniaFact]
    public async Task Render_Shell_TabIcons_FontImageSource_2x()
    {
        await RenderShellToFile(CreateShellWithGlyphTabIcons(), height: 120, scale: 2);
        CompareImages(tolerance: 0.045);
    }

    static Shell CreateShellWithGlyphFlyoutIcons()
    {
        static FontImageSource Icon(string glyph) => new() { Glyph = glyph, Size = 16, Color = Colors.DarkRed };

        return new Shell
        {
            FlyoutBehavior = FlyoutBehavior.Locked,
            Items =
            {
                new FlyoutItem
                {
                    Title = "Home",
                    Icon = Icon("H"),
                    Items = { new ShellContent { Content = new MauiContentPage { Content = new Microsoft.Maui.Controls.Label { Text = "Home Page", TextColor = Colors.Black } } } }
                },
                new FlyoutItem
                {
                    Title = "Mail",
                    Icon = Icon("M"),
                    Items = { new ShellContent { Content = new MauiContentPage { Content = new Microsoft.Maui.Controls.Label { Text = "Mail Page", TextColor = Colors.Black } } } }
                }
            }
        };
    }

    [AvaloniaFact]
    public async Task Render_Shell_FlyoutItemIcons_FontImageSource()
    {
        await RenderShellToFile(CreateShellWithGlyphFlyoutIcons(), height: 160);
        CompareImages(tolerance: 0.045);
    }

    [AvaloniaFact]
    public async Task Render_Shell_FlyoutItemIcons_FontImageSource_2x()
    {
        await RenderShellToFile(CreateShellWithGlyphFlyoutIcons(), height: 160, scale: 2);
        CompareImages(tolerance: 0.045);
    }
}
