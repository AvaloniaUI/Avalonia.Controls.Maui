using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

[BenchmarkTest("ShellTabSwitchPerformance", Description = "Measures warmed Shell tab switching latency across content-heavy tabs")]
public class ShellTabSwitchPerformanceBenchmark : BenchmarkTestPage
{
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var metrics = new Dictionary<string, object>();
        var memoryBefore = MemorySnapshot.Capture(forceGC: true);

        var (shell, tabBar, pages) = CreateShell();
        window.Page = shell;
        await ShellPerformanceBenchmarkHelpers.WaitForShellCurrentPageAsync(shell, pages[0], cancellationToken);

        try
        {
            for (int index = 1; index < tabBar.Items.Count; index++)
            {
                shell.CurrentItem = tabBar.Items[index];
                await ShellPerformanceBenchmarkHelpers.WaitForShellCurrentPageAsync(shell, pages[index], cancellationToken);
            }

            shell.CurrentItem = tabBar.Items[0];
            await ShellPerformanceBenchmarkHelpers.WaitForShellCurrentPageAsync(shell, pages[0], cancellationToken);

            var switchTargets = new[] { 1, 2, 3, 0, 2, 1, 3, 0, 1, 0 };
            var switchTimes = new List<double>(switchTargets.Length);

            foreach (var targetIndex in switchTargets)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var stopwatch = Stopwatch.StartNew();
                shell.CurrentItem = tabBar.Items[targetIndex];
                await ShellPerformanceBenchmarkHelpers.WaitForShellCurrentPageAsync(shell, pages[targetIndex], cancellationToken);
                stopwatch.Stop();

                switchTimes.Add(stopwatch.Elapsed.TotalMilliseconds);
            }

            ShellPerformanceBenchmarkHelpers.AddLatencyMetrics(metrics, "TabSwitch", switchTimes);
            metrics["SwitchTargets"] = switchTargets.Length;
            metrics["TabCount"] = tabBar.Items.Count;

            foreach (var (key, value) in MemorySnapshot.Capture(forceGC: true).Compare(memoryBefore).ToMetrics())
            {
                metrics[key] = value;
            }

            logger.LogInformation(
                "Shell tab switch: avg={AvgMs:F2}ms, p95={P95Ms:F2}ms, max={MaxMs:F2}ms across {Switches} switches",
                switchTimes.Average(),
                BenchmarkUiHelpers.CalculatePercentile(switchTimes, 0.95),
                switchTimes.Max(),
                switchTargets.Length);

            return BenchmarkResult.Pass(metrics);
        }
        finally
        {
            shell.Handler?.DisconnectHandler();
            window.Page = this;
            Content = new Label { Text = "Shell tab switch performance benchmark complete" };

            foreach (var page in pages)
            {
                ShellPerformanceBenchmarkHelpers.DisconnectPage(page);
            }

            await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 20);
        }
    }

    private static (Shell Shell, TabBar TabBar, IReadOnlyList<ContentPage> Pages) CreateShell()
    {
        var shell = new Shell { FlyoutBehavior = FlyoutBehavior.Disabled };
        var tabBar = new TabBar();
        var pages = new List<ContentPage>();

        for (int index = 0; index < 4; index++)
        {
            var page = ShellPerformanceBenchmarkHelpers.CreateContentHeavyPage(
                $"Tab {index} Home",
                sectionCount: 4 + index,
                toolbarCount: index % 2 == 0 ? 1 : 2);
            pages.Add(page);

            tabBar.Items.Add(new ShellContent
            {
                Title = $"Tab {index}",
                Route = $"perf-tab-{index}",
                ContentTemplate = new DataTemplate(() => page),
            });
        }

        shell.Items.Add(tabBar);
        return (shell, tabBar, pages);
    }
}
