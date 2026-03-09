using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Measures navigation performance by timing push/pop operations on a NavigationPage.
/// Reports per-page navigation latency to detect performance regressions in the
/// navigation stack management.
/// </summary>
[BenchmarkTest("NavigationPerformance", Description = "Measures push/pop navigation latency on NavigationPage")]
public class NavigationPerformanceBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);
        var metrics = new Dictionary<string, object>();

        const int pushPopCycles = 10;
        const int deepStackDepth = 5;

        // Set up NavigationPage
        var rootPage = new ContentPage
        {
            Title = "Nav Perf Root",
            Content = new Label { Text = "Navigation performance root" },
        };
        var navPage = new NavigationPage(rootPage);
        window.Page = navPage;
        await Task.Delay(100, cancellationToken);

        // Phase 1: Measure individual push/pop cycles
        var pushTimes = new List<double>();
        var popTimes = new List<double>();

        for (int i = 0; i < pushPopCycles; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var page = new ContentPage
            {
                Title = $"Page {i}",
                Content = new VerticalStackLayout
                {
                    Children =
                    {
                        new Label { Text = $"Nav perf page {i}" },
                        new Button { Text = $"Action {i}" },
                    },
                },
            };

            var pushSw = Stopwatch.StartNew();
            await navPage.PushAsync(page, animated: false);
            pushSw.Stop();
            pushTimes.Add(pushSw.Elapsed.TotalMilliseconds);

            await Task.Delay(20, cancellationToken);

            var popSw = Stopwatch.StartNew();
            await navPage.PopAsync(animated: false);
            popSw.Stop();
            popTimes.Add(popSw.Elapsed.TotalMilliseconds);

            await Task.Delay(20, cancellationToken);
        }

        metrics["PushPopCycles"] = pushPopCycles;
        metrics["Push.AvgMs"] = pushTimes.Count > 0 ? pushTimes.Average() : 0;
        metrics["Push.MinMs"] = pushTimes.Count > 0 ? pushTimes.Min() : 0;
        metrics["Push.MaxMs"] = pushTimes.Count > 0 ? pushTimes.Max() : 0;
        metrics["Pop.AvgMs"] = popTimes.Count > 0 ? popTimes.Average() : 0;
        metrics["Pop.MinMs"] = popTimes.Count > 0 ? popTimes.Min() : 0;
        metrics["Pop.MaxMs"] = popTimes.Count > 0 ? popTimes.Max() : 0;

        logger.LogInformation(
            "Push: avg={AvgMs:F2}ms min={MinMs:F2}ms max={MaxMs:F2}ms over {Cycles} cycles",
            pushTimes.Average(), pushTimes.Min(), pushTimes.Max(), pushPopCycles);
        logger.LogInformation(
            "Pop: avg={AvgMs:F2}ms min={MinMs:F2}ms max={MaxMs:F2}ms over {Cycles} cycles",
            popTimes.Average(), popTimes.Min(), popTimes.Max(), pushPopCycles);

        // Phase 2: Measure deep stack push then pop-to-root
        var deepPushSw = Stopwatch.StartNew();
        for (int i = 0; i < deepStackDepth; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var page = new ContentPage
            {
                Title = $"Deep {i}",
                Content = new Label { Text = $"Deep stack page {i}" },
            };

            await navPage.PushAsync(page, animated: false);
            await Task.Delay(10, cancellationToken);
        }

        deepPushSw.Stop();

        var popToRootSw = Stopwatch.StartNew();
        await navPage.PopToRootAsync(animated: false);
        popToRootSw.Stop();

        metrics["DeepStack.Depth"] = deepStackDepth;
        metrics["DeepStack.PushAllMs"] = deepPushSw.Elapsed.TotalMilliseconds;
        metrics["DeepStack.PopToRootMs"] = popToRootSw.Elapsed.TotalMilliseconds;

        logger.LogInformation(
            "Deep stack ({Depth} pages): push all={PushMs:F2}ms, pop-to-root={PopMs:F2}ms",
            deepStackDepth, deepPushSw.Elapsed.TotalMilliseconds, popToRootSw.Elapsed.TotalMilliseconds);

        await Task.Delay(50, cancellationToken);

        // Clean up: restore test page
        navPage.Handler?.DisconnectHandler();
        window.Page = this;
        Content = new Label { Text = "Navigation performance test complete" };

        var memAfter = MemorySnapshot.Capture(forceGC: true);
        var memoryDelta = memAfter.Compare(memBefore);

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        // Performance benchmark always passes — it's for measurement only
        return BenchmarkResult.Pass(metrics);
    }
}
