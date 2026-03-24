using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

[BenchmarkTest("ShellNavigationStackPerformance", Description = "Measures push, pop, and pop-to-root latency inside a Shell navigation stack")]
public class ShellNavigationStackPerformanceBenchmark : BenchmarkTestPage
{
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var metrics = new Dictionary<string, object>();
        var memoryBefore = MemorySnapshot.Capture(forceGC: true);

        var rootPage = ShellPerformanceBenchmarkHelpers.CreateContentHeavyPage(
            "Shell Stack Root",
            sectionCount: 4,
            toolbarCount: 1);

        var shell = new Shell { FlyoutBehavior = FlyoutBehavior.Disabled };
        var shellContent = new ShellContent
        {
            Title = "Stack Root",
            Route = "shell-stack-root",
            ContentTemplate = new DataTemplate(() => rootPage),
        };
        var shellSection = new ShellSection { Title = "Stack Section" };
        shellSection.Items.Add(shellContent);
        var shellItem = new ShellItem { Title = "Stack Item" };
        shellItem.Items.Add(shellSection);
        shell.Items.Add(shellItem);

        window.Page = shell;
        await ShellPerformanceBenchmarkHelpers.WaitForShellCurrentPageAsync(shell, rootPage, cancellationToken);

        var pushTimes = new List<double>();
        var popTimes = new List<double>();

        const int cycles = 8;
        const int deepStackDepth = 4;

        try
        {
            for (int index = 0; index < cycles; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var detailPage = ShellPerformanceBenchmarkHelpers.CreateContentHeavyPage(
                    $"Shell Detail {index}",
                    sectionCount: 3,
                    toolbarCount: 2);

                var pushStopwatch = Stopwatch.StartNew();
                await shell.Navigation.PushAsync(detailPage, animated: false);
                await BenchmarkUiHelpers.WaitUntilAsync(
                    () => shell.Navigation.NavigationStack.Count == 2 &&
                        ReferenceEquals(shell.CurrentPage, detailPage) &&
                        detailPage.Handler != null,
                    cancellationToken,
                    timeoutMs: 4000,
                    pollDelayMs: 15);
                await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 15);
                pushStopwatch.Stop();
                pushTimes.Add(pushStopwatch.Elapsed.TotalMilliseconds);

                var popStopwatch = Stopwatch.StartNew();
                await shell.Navigation.PopAsync(animated: false);
                await BenchmarkUiHelpers.WaitUntilAsync(
                    () => shell.Navigation.NavigationStack.Count == 1 &&
                        ReferenceEquals(shell.CurrentPage, rootPage),
                    cancellationToken,
                    timeoutMs: 4000,
                    pollDelayMs: 15);
                await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 15);
                popStopwatch.Stop();
                popTimes.Add(popStopwatch.Elapsed.TotalMilliseconds);

                ShellPerformanceBenchmarkHelpers.DisconnectPage(detailPage);
            }

            var deepPages = new List<ContentPage>(deepStackDepth);
            var deepPushStopwatch = Stopwatch.StartNew();
            for (int index = 0; index < deepStackDepth; index++)
            {
                var detailPage = ShellPerformanceBenchmarkHelpers.CreateContentHeavyPage(
                    $"Deep Shell Detail {index}",
                    sectionCount: 2 + index,
                    toolbarCount: 1);
                deepPages.Add(detailPage);
                await shell.Navigation.PushAsync(detailPage, animated: false);
                await ShellPerformanceBenchmarkHelpers.WaitForNavigationStackCountAsync(
                    shell.Navigation,
                    index + 2,
                    cancellationToken);
            }
            deepPushStopwatch.Stop();

            var popToRootStopwatch = Stopwatch.StartNew();
            await shell.Navigation.PopToRootAsync(animated: false);
            await BenchmarkUiHelpers.WaitUntilAsync(
                () => shell.Navigation.NavigationStack.Count == 1 &&
                    ReferenceEquals(shell.CurrentPage, rootPage),
                cancellationToken,
                timeoutMs: 4000,
                pollDelayMs: 15);
            await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 20);
            popToRootStopwatch.Stop();

            foreach (var detailPage in deepPages)
            {
                ShellPerformanceBenchmarkHelpers.DisconnectPage(detailPage);
            }

            ShellPerformanceBenchmarkHelpers.AddLatencyMetrics(metrics, "Push", pushTimes);
            ShellPerformanceBenchmarkHelpers.AddLatencyMetrics(metrics, "Pop", popTimes);
            metrics["Cycles"] = cycles;
            metrics["DeepStackDepth"] = deepStackDepth;
            metrics["DeepStackPushAllMs"] = deepPushStopwatch.Elapsed.TotalMilliseconds;
            metrics["DeepStackPopToRootMs"] = popToRootStopwatch.Elapsed.TotalMilliseconds;

            foreach (var (key, value) in MemorySnapshot.Capture(forceGC: true).Compare(memoryBefore).ToMetrics())
            {
                metrics[key] = value;
            }

            logger.LogInformation(
                "Shell stack push: avg={AvgMs:F2}ms, p95={P95Ms:F2}ms, max={MaxMs:F2}ms over {Cycles} cycles",
                pushTimes.Average(),
                BenchmarkUiHelpers.CalculatePercentile(pushTimes, 0.95),
                pushTimes.Max(),
                cycles);
            logger.LogInformation(
                "Shell stack pop: avg={AvgMs:F2}ms, p95={P95Ms:F2}ms, max={MaxMs:F2}ms over {Cycles} cycles",
                popTimes.Average(),
                BenchmarkUiHelpers.CalculatePercentile(popTimes, 0.95),
                popTimes.Max(),
                cycles);
            logger.LogInformation(
                "Shell stack depth {Depth}: push all={PushMs:F2}ms, pop-to-root={PopMs:F2}ms",
                deepStackDepth,
                deepPushStopwatch.Elapsed.TotalMilliseconds,
                popToRootStopwatch.Elapsed.TotalMilliseconds);

            return BenchmarkResult.Pass(metrics);
        }
        finally
        {
            shell.Handler?.DisconnectHandler();
            window.Page = this;
            Content = new Label { Text = "Shell navigation stack performance benchmark complete" };
            ShellPerformanceBenchmarkHelpers.DisconnectPage(rootPage);
            await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 20);
        }
    }
}
