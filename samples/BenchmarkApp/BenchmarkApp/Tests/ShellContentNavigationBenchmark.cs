using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that navigating between different ShellContent items (via FlyoutItem switching)
/// preserves page content when navigating back. The page should be the same instance and
/// its handler should remain connected, matching native MAUI Shell behavior.
/// </summary>
[BenchmarkTest("ShellContentNavigation", Description = "Verifies page content is preserved when navigating between Shell FlyoutItems and returning")]
public class ShellContentNavigationBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var result = await RunNavigationCycles(window, this, logger, cancellationToken);

        var memAfter = MemorySnapshot.Capture(forceGC: true);
        var memoryDelta = memAfter.Compare(memBefore);

        var metrics = new Dictionary<string, object>
        {
            ["NavigationCycles"] = result.NavigationCycles,
            ["Failures"] = result.Failures.Count > 0
                ? string.Join("; ", result.Failures)
                : "none",
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
            metrics[key] = value;

        if (result.Failures.Count > 0)
        {
            var msg = string.Join("; ", result.Failures);
            logger.LogWarning("Shell content navigation failures: {Failures}", msg);
            return BenchmarkResult.Fail(msg, metrics);
        }

        logger.LogInformation(
            "Shell content preserved across {Cycles} navigation cycles",
            result.NavigationCycles);
        return BenchmarkResult.Pass(metrics);
    }

    private record NavigationResult(int NavigationCycles, List<string> Failures);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static async Task<NavigationResult> RunNavigationCycles(
        Window window,
        Page restorePage,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        const int itemCount = 4;
        const int cycles = 3;
        var pages = new ContentPage[itemCount];
        var shell = CreateShell(pages, itemCount);
        window.Page = shell;

        await Task.Delay(150, cancellationToken);

        var failures = new List<string>();
        int navigationCycles = 0;

        for (int cycle = 0; cycle < cycles; cycle++)
        {
            // Navigate forward through all items
            for (int i = 0; i < itemCount; i++)
            {
                shell.CurrentItem = shell.Items[i];
                await Task.Delay(50, cancellationToken);

                var section = ((ShellItem)shell.Items[i]).Items[0];
                if (section.Handler == null)
                {
                    failures.Add($"Cycle {cycle}: Item[{i}] section handler is null after navigation");
                    logger.LogWarning("Cycle {Cycle}: Item[{I}] section handler null", cycle, i);
                }

                var controller = (IShellContentController)section.CurrentItem;
                if (controller.Page != pages[i])
                {
                    failures.Add($"Cycle {cycle}: Item[{i}] page is not the original instance");
                    logger.LogWarning("Cycle {Cycle}: Item[{I}] page changed", cycle, i);
                }
            }

            // Navigate backward through all items
            for (int i = itemCount - 1; i >= 0; i--)
            {
                shell.CurrentItem = shell.Items[i];
                await Task.Delay(50, cancellationToken);

                var section = ((ShellItem)shell.Items[i]).Items[0];
                if (section.Handler == null)
                {
                    failures.Add($"Cycle {cycle} (reverse): Item[{i}] section handler is null");
                    logger.LogWarning("Cycle {Cycle} (reverse): Item[{I}] section handler null", cycle, i);
                }

                var controller = (IShellContentController)section.CurrentItem;
                if (controller.Page != pages[i])
                {
                    failures.Add($"Cycle {cycle} (reverse): Item[{i}] page is not the original instance");
                    logger.LogWarning("Cycle {Cycle} (reverse): Item[{I}] page changed", cycle, i);
                }
            }

            navigationCycles++;
        }

        // Restore page to clean up
        window.Page = restorePage;

        return new NavigationResult(navigationCycles, failures);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Shell CreateShell(ContentPage[] pages, int itemCount)
    {
        var shell = new Shell { FlyoutBehavior = FlyoutBehavior.Disabled };

        for (int i = 0; i < itemCount; i++)
        {
            var page = new ContentPage
            {
                Title = $"Page {i}",
                Content = new VerticalStackLayout
                {
                    Children =
                    {
                        new Label { Text = $"Content for page {i}" },
                        new Button { Text = $"Action {i}" },
                    },
                },
            };

            pages[i] = page;

            var shellContent = new ShellContent
            {
                Title = $"Item {i}",
                ContentTemplate = new DataTemplate(() => page),
                Route = $"content{i}",
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
