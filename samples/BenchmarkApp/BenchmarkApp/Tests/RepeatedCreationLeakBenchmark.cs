

using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Creates and destroys controls in repeated batches to detect slow memory leaks.
/// </summary>
[BenchmarkTest("RepeatedCreationLeak", Description = "Creates/destroys controls in 50 batches of 10, checks for slow leaks")]
public class RepeatedCreationLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        const int batchCount = 50;
        const int controlsPerBatch = 10;
        const double maxLeakRate = 0.05;
        const long maxMemoryGrowthBytes = 10 * 1024 * 1024; // 10MB

        var memBefore = MemorySnapshot.Capture(forceGC: true);
        var allWeakRefs = new List<(string Name, WeakReference<VisualElement> Ref)>();

        var layout = new VerticalStackLayout();
        Content = layout;

        for (int batch = 0; batch < batchCount; batch++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var batchControls = new List<VisualElement>(controlsPerBatch);

            for (int i = 0; i < controlsPerBatch; i++)
            {
                var index = batch * controlsPerBatch + i;
                VisualElement control = (index % 3) switch
                {
                    0 => new Button { Text = $"B{index}" },
                    1 => new Label { Text = $"L{index}" },
                    _ => new Entry { Placeholder = $"E{index}" },
                };

                layout.Children.Add(control);
                allWeakRefs.Add(($"{control.GetType().Name}[{index}]", new WeakReference<VisualElement>(control)));
                batchControls.Add(control);
            }

            // Remove and disconnect
            layout.Children.Clear();
            foreach (var control in batchControls)
            {
                control.Handler?.DisconnectHandler();
            }

            batchControls.Clear();

            // Force GC every 10 batches
            if ((batch + 1) % 10 == 0)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }

        // Replace content
        Content = new Label { Text = "Done" };
        layout = null;

        // Triple GC with dispatcher yield
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        await Task.Delay(100, cancellationToken);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        await Task.Delay(50, cancellationToken);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var memAfter = MemorySnapshot.Capture(forceGC: false);
        var memoryDelta = memAfter.Compare(memBefore);

        // Count survivors
        int totalCreated = allWeakRefs.Count;
        var leaked = new List<string>();
        foreach (var (name, weakRef) in allWeakRefs)
        {
            if (weakRef.TryGetTarget(out _))
            {
                leaked.Add(name);
            }
        }

        double leakRate = totalCreated > 0 ? (double)leaked.Count / totalCreated : 0;
        long memoryGrowth = memoryDelta.BytesDelta;

        logger.LogInformation(
            "Created {Total} controls, {Leaked} survived GC (leak rate: {LeakRate:P1}), memory growth: {MemoryGrowth} bytes",
            totalCreated,
            leaked.Count,
            leakRate,
            memoryGrowth);

        var metrics = new Dictionary<string, object>
        {
            ["BatchCount"] = batchCount,
            ["ControlsPerBatch"] = controlsPerBatch,
            ["TotalControlsCreated"] = totalCreated,
            ["ControlsLeaked"] = leaked.Count,
            ["LeakRate"] = leakRate,
            ["MemoryGrowthBytes"] = memoryGrowth,
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        bool passed = leakRate < maxLeakRate && memoryGrowth < maxMemoryGrowthBytes;

        if (!passed)
        {
            var reasons = new List<string>();
            if (leakRate >= maxLeakRate)
            {
                reasons.Add($"leak rate {leakRate:P1} >= {maxLeakRate:P0} threshold ({leaked.Count}/{totalCreated} controls)");
            }

            if (memoryGrowth >= maxMemoryGrowthBytes)
            {
                reasons.Add($"memory growth {memoryGrowth / (1024 * 1024.0):F1}MB >= {maxMemoryGrowthBytes / (1024 * 1024)}MB threshold");
            }

            return BenchmarkResult.Fail(string.Join("; ", reasons), metrics);
        }

        if (memoryDelta.WorkingSetDelta > 50 * 1024 * 1024)
        {
            return BenchmarkResult.Warn(
                $"Native memory growth {memoryDelta.WorkingSetDelta / (1024.0 * 1024):F1} MB exceeds 50 MB threshold",
                metrics);
        }

        return BenchmarkResult.Pass(metrics);
    }
}
