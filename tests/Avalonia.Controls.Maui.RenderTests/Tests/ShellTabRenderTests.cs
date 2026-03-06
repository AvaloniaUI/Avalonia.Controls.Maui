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
    private async Task RenderShellToFile(Shell shell, double width = 400, double height = 300, [CallerMemberName] string testName = "")
    {
        EnsureHandlerCreated();

        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var mauiContext = MauiContext;
            var handler = shell.ToHandler(mauiContext);
            var platformView = (Control)handler.PlatformView!;

            platformView.Margin = new Avalonia.Thickness(20);

            var container = new Panel
            {
                Children = { platformView },
                Background = AvaloniaBrushes.White,
                Width = width + 40,
                Height = height + 40
            };

            var window = new Window
            {
                Content = container,
                Width = width + 40,
                Height = height + 40,
                Title = testName,
                SystemDecorations = WindowDecorations.None,
                Background = AvaloniaBrushes.White,
                CanResize = false,
                WindowStartupLocation = WindowStartupLocation.Manual
            };

            window.Show();

            for (int i = 0; i < 5; i++)
            {
                Dispatcher.UIThread.RunJobs(DispatcherPriority.Normal);
                Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);
                await Task.Delay(20);
            }

            var pixelSize = new PixelSize((int)width + 40, (int)height + 40);
            var dpi = new Vector(96, 96);
            using var bitmap = new RenderTargetBitmap(pixelSize, dpi);
            bitmap.Render(window);

            var fileName = $"{GetType().Name}_{testName}.out.png";
            var outputPath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            bitmap.Save(outputPath);

            window.Close();
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
        CompareImages(tolerance: 0.045);
    }
}
