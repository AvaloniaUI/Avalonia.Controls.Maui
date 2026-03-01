

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that switching Shell.CurrentItem releases the old item's section handler resources.
/// Without cleanup, every ShellItem that was ever active keeps its section handler's full
/// tree (navigation stack, page handlers, platform views) alive even though only the current
/// item is visible. This test checks that inactive items' sections are disconnected.
/// </summary>
[BenchmarkTest("ShellItemSwitchLeak", Description = "Verifies old section handlers are released when switching Shell.CurrentItem")]
public class ShellItemSwitchLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var result = await RunShellLifecycle(window, this, logger, cancellationToken);

        var memAfter = MemorySnapshot.Capture(forceGC: true);
        var memoryDelta = memAfter.Compare(memBefore);

        var metrics = new Dictionary<string, object>
        {
            ["TotalSwitches"] = result.TotalSwitches,
            ["StaleHandlersFound"] = result.StaleHandlers.Count,
            ["StaleHandlers"] = result.StaleHandlers.Count > 0
                ? string.Join(", ", result.StaleHandlers)
                : "none",
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        if (result.StaleHandlers.Count > 0)
        {
            var staleNames = string.Join(", ", result.StaleHandlers);
            logger.LogWarning(
                "Section handlers not disconnected after switching away: {StaleHandlers}",
                staleNames);
            return BenchmarkResult.Fail(
                $"Section handlers still connected after item switch: {staleNames}", metrics);
        }

        logger.LogInformation(
            "All inactive items' section handlers properly disconnected after {Switches} switches",
            result.TotalSwitches);
        return BenchmarkResult.Pass(metrics);
    }

    private record SwitchResult(int TotalSwitches, List<string> StaleHandlers);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static async Task<SwitchResult> RunShellLifecycle(
        Window window,
        Page restorePage,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var shell = CreateShellWithItems();
        window.Page = shell;

        await Task.Delay(150, cancellationToken);

        var staleHandlers = new List<string>();
        int totalSwitches = 0;

        // Verify initial state: Item 0's section should have a handler
        var section0 = ((ShellItem)shell.Items[0]).Items[0];
        logger.LogInformation("Initial: Section[0].Handler = {HasHandler}", section0.Handler != null);

        // Switch from Item 0 to Item 1
        shell.CurrentItem = shell.Items[1];
        await Task.Delay(100, cancellationToken);
        totalSwitches++;

        // After switching to Item 1, Item 0's section handler should be disconnected.
        // Without the fix, Item 0's section keeps its full handler tree alive.
        if (section0.Handler != null)
        {
            staleHandlers.Add("Item[0].Section (after switch to Item[1])");
            logger.LogWarning("Item[0]'s section handler still connected after switching to Item[1]");
        }

        // Switch from Item 1 to Item 2
        var section1 = ((ShellItem)shell.Items[1]).Items[0];
        shell.CurrentItem = shell.Items[2];
        await Task.Delay(100, cancellationToken);
        totalSwitches++;

        if (section1.Handler != null)
        {
            staleHandlers.Add("Item[1].Section (after switch to Item[2])");
            logger.LogWarning("Item[1]'s section handler still connected after switching to Item[2]");
        }

        // Switch back to Item 0 (creates fresh section handler)
        var section2 = ((ShellItem)shell.Items[2]).Items[0];
        shell.CurrentItem = shell.Items[0];
        await Task.Delay(100, cancellationToken);
        totalSwitches++;

        if (section2.Handler != null)
        {
            staleHandlers.Add("Item[2].Section (after switch to Item[0])");
            logger.LogWarning("Item[2]'s section handler still connected after switching to Item[0]");
        }

        // Restore page to clean up
        window.Page = restorePage;

        return new SwitchResult(totalSwitches, staleHandlers);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Shell CreateShellWithItems()
    {
        var shell = new Shell { FlyoutBehavior = FlyoutBehavior.Disabled };

        for (int i = 0; i < 3; i++)
        {
            var page = new ContentPage
            {
                Title = $"Item {i}",
                Content = new VerticalStackLayout
                {
                    Children =
                    {
                        new Label { Text = $"ShellItem page {i}" },
                        new Button { Text = $"Button {i}" },
                        new Entry { Placeholder = $"Entry {i}" },
                        new Image { Source = "dotnet_bot.png", WidthRequest = 64, HeightRequest = 64 },
                        new Border
                        {
                            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
                            Padding = new Thickness(10),
                            Content = new Label { Text = $"Border content {i}" },
                        },
                    },
                },
            };

            var shellContent = new ShellContent
            {
                Title = $"Item {i}",
                ContentTemplate = new DataTemplate(() => page),
                Route = $"item{i}",
            };

            var shellSection = new ShellSection { Title = $"Section {i}" };
            shellSection.Items.Add(shellContent);

            var shellItem = new ShellItem { Title = $"Item {i}" };
            shellItem.Items.Add(shellSection);

            shell.Items.Add(shellItem);
        }

        return shell;
    }
}
