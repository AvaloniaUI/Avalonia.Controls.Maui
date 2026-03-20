using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

[BenchmarkTest("ShellFlyoutPerformance", Description = "Measures Shell flyout open/close latency and flyout item switch latency")]
public class ShellFlyoutPerformanceBenchmark : BenchmarkTestPage
{
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var metrics = new Dictionary<string, object>();
        var memoryBefore = MemorySnapshot.Capture(forceGC: true);

        var (shell, pages) = CreateShell();
        window.Page = shell;
        await ShellPerformanceBenchmarkHelpers.WaitForShellCurrentPageAsync(shell, pages[0], cancellationToken);

        const int openCloseCycles = 8;
        var openTimes = new List<double>(openCloseCycles);
        var closeTimes = new List<double>(openCloseCycles);

        try
        {
            for (int cycle = 0; cycle < openCloseCycles; cycle++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var openStopwatch = Stopwatch.StartNew();
                shell.FlyoutIsPresented = true;
                await BenchmarkUiHelpers.WaitUntilAsync(
                    () => shell.FlyoutIsPresented,
                    cancellationToken,
                    timeoutMs: 2000,
                    pollDelayMs: 10);
                await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 20);
                openStopwatch.Stop();
                openTimes.Add(openStopwatch.Elapsed.TotalMilliseconds);

                var closeStopwatch = Stopwatch.StartNew();
                shell.FlyoutIsPresented = false;
                await BenchmarkUiHelpers.WaitUntilAsync(
                    () => !shell.FlyoutIsPresented,
                    cancellationToken,
                    timeoutMs: 2000,
                    pollDelayMs: 10);
                await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 20);
                closeStopwatch.Stop();
                closeTimes.Add(closeStopwatch.Elapsed.TotalMilliseconds);
            }

            var switchTargets = new[] { 1, 2, 3, 0, 2, 1, 3, 0 };
            var switchTimes = new List<double>(switchTargets.Length);

            foreach (var targetIndex in switchTargets)
            {
                cancellationToken.ThrowIfCancellationRequested();

                shell.FlyoutIsPresented = true;
                await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 15);

                var stopwatch = Stopwatch.StartNew();
                shell.CurrentItem = shell.Items[targetIndex];
                await ShellPerformanceBenchmarkHelpers.WaitForShellCurrentPageAsync(shell, pages[targetIndex], cancellationToken);
                stopwatch.Stop();

                switchTimes.Add(stopwatch.Elapsed.TotalMilliseconds);

                shell.FlyoutIsPresented = false;
                await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 15);
            }

            ShellPerformanceBenchmarkHelpers.AddLatencyMetrics(metrics, "FlyoutOpen", openTimes);
            ShellPerformanceBenchmarkHelpers.AddLatencyMetrics(metrics, "FlyoutClose", closeTimes);
            ShellPerformanceBenchmarkHelpers.AddLatencyMetrics(metrics, "ItemSwitch", switchTimes);
            metrics["FlyoutItemCount"] = shell.Items.Count;
            metrics["OpenCloseCycles"] = openCloseCycles;

            foreach (var (key, value) in MemorySnapshot.Capture(forceGC: true).Compare(memoryBefore).ToMetrics())
            {
                metrics[key] = value;
            }

            logger.LogInformation(
                "Shell flyout open: avg={AvgMs:F2}ms, p95={P95Ms:F2}ms, max={MaxMs:F2}ms",
                openTimes.Average(),
                BenchmarkUiHelpers.CalculatePercentile(openTimes, 0.95),
                openTimes.Max());
            logger.LogInformation(
                "Shell flyout close: avg={AvgMs:F2}ms, p95={P95Ms:F2}ms, max={MaxMs:F2}ms",
                closeTimes.Average(),
                BenchmarkUiHelpers.CalculatePercentile(closeTimes, 0.95),
                closeTimes.Max());
            logger.LogInformation(
                "Shell flyout item switch: avg={AvgMs:F2}ms, p95={P95Ms:F2}ms, max={MaxMs:F2}ms over {Switches} switches",
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
            Content = new Label { Text = "Shell flyout performance benchmark complete" };

            foreach (var page in pages)
            {
                ShellPerformanceBenchmarkHelpers.DisconnectPage(page);
            }

            await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 20);
        }
    }

    private static (Shell Shell, IReadOnlyList<ContentPage> Pages) CreateShell()
    {
        var shell = new Shell { FlyoutBehavior = FlyoutBehavior.Flyout };
        var pages = new List<ContentPage>();

        for (int index = 0; index < 4; index++)
        {
            var page = ShellPerformanceBenchmarkHelpers.CreateContentHeavyPage(
                $"Flyout Item {index}",
                sectionCount: 3 + index,
                toolbarCount: index == 0 ? 1 : 0);
            pages.Add(page);

            var shellContent = new ShellContent
            {
                Title = $"Item {index}",
                Route = $"perf-flyout-{index}",
                ContentTemplate = new DataTemplate(() => page),
            };

            var shellSection = new ShellSection { Title = $"Section {index}" };
            shellSection.Items.Add(shellContent);

            var flyoutItem = new FlyoutItem { Title = $"Item {index}" };
            flyoutItem.Items.Add(shellSection);

            shell.Items.Add(flyoutItem);
        }

        return (shell, pages);
    }
}
