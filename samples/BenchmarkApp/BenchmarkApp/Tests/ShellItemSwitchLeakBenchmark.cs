

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that switching Shell.CurrentItem preserves page content and section handlers.
/// Section handlers should remain connected when switching between FlyoutItems so that
/// navigating back to a previously visited item shows the same page without re-creation.
/// This matches the native MAUI behavior where pages are cached via ShellContent.ContentCache.
/// </summary>
[BenchmarkTest("ShellItemSwitchLeak", Description = "Verifies section handlers stay connected and page content is preserved when switching Shell.CurrentItem")]
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
            ["MissingHandlersFound"] = result.MissingHandlers.Count,
            ["MissingHandlers"] = result.MissingHandlers.Count > 0
                ? string.Join(", ", result.MissingHandlers)
                : "none",
            ["MissingPages"] = result.MissingPages.Count > 0
                ? string.Join(", ", result.MissingPages)
                : "none",
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        if (result.MissingHandlers.Count > 0 || result.MissingPages.Count > 0)
        {
            var issues = new List<string>();
            if (result.MissingHandlers.Count > 0)
                issues.Add($"Section handlers missing: {string.Join(", ", result.MissingHandlers)}");
            if (result.MissingPages.Count > 0)
                issues.Add($"Pages missing: {string.Join(", ", result.MissingPages)}");

            var msg = string.Join("; ", issues);
            logger.LogWarning("Shell item switch issues: {Issues}", msg);
            return BenchmarkResult.Fail(msg, metrics);
        }

        logger.LogInformation(
            "All items preserve section handlers and page content after {Switches} switches",
            result.TotalSwitches);
        return BenchmarkResult.Pass(metrics);
    }

    private record SwitchResult(int TotalSwitches, List<string> MissingHandlers, List<string> MissingPages);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static async Task<SwitchResult> RunShellLifecycle(
        Window window,
        Page restorePage,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var pages = new ContentPage[3];
        var shell = CreateShellWithItems(pages);
        window.Page = shell;

        await Task.Delay(150, cancellationToken);

        var missingHandlers = new List<string>();
        var missingPages = new List<string>();
        int totalSwitches = 0;

        // Verify initial state: Item 0's section should have a handler
        var section0 = ((ShellItem)shell.Items[0]).Items[0];
        logger.LogInformation("Initial: Section[0].Handler = {HasHandler}", section0.Handler != null);

        // Switch from Item 0 to Item 1
        shell.CurrentItem = shell.Items[1];
        await Task.Delay(100, cancellationToken);
        totalSwitches++;

        // After switching to Item 1, Item 0's section handler should still be connected.
        // Section handlers are intentionally kept alive so navigating back shows the same page.
        if (section0.Handler == null)
        {
            missingHandlers.Add("Item[0].Section (after switch to Item[1])");
            logger.LogWarning("Item[0]'s section handler was disconnected after switching to Item[1]");
        }

        // Switch from Item 1 to Item 2
        var section1 = ((ShellItem)shell.Items[1]).Items[0];
        shell.CurrentItem = shell.Items[2];
        await Task.Delay(100, cancellationToken);
        totalSwitches++;

        if (section1.Handler == null)
        {
            missingHandlers.Add("Item[1].Section (after switch to Item[2])");
            logger.LogWarning("Item[1]'s section handler was disconnected after switching to Item[2]");
        }

        // Switch back to Item 0 — the same section handler should still be there
        var section2 = ((ShellItem)shell.Items[2]).Items[0];
        shell.CurrentItem = shell.Items[0];
        await Task.Delay(100, cancellationToken);
        totalSwitches++;

        if (section0.Handler == null)
        {
            missingHandlers.Add("Item[0].Section (after switch back)");
            logger.LogWarning("Item[0]'s section handler was missing after switching back");
        }

        // Verify that the page content is still the same (not re-created)
        var controller0 = (IShellContentController)section0.CurrentItem;
        if (controller0.Page != pages[0])
        {
            missingPages.Add("Item[0] page changed after switch back");
            logger.LogWarning("Item[0]'s page was not preserved after switching back");
        }

        if (section2.Handler == null)
        {
            missingHandlers.Add("Item[2].Section (after switch to Item[0])");
            logger.LogWarning("Item[2]'s section handler was disconnected after switching to Item[0]");
        }

        // Restore page to clean up
        window.Page = restorePage;

        return new SwitchResult(totalSwitches, missingHandlers, missingPages);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Shell CreateShellWithItems(ContentPage[] pages)
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

            pages[i] = page;

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
