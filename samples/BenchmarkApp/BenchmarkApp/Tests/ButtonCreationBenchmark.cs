// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Benchmark that creates 100 buttons and measures elapsed time.
/// </summary>
[BenchmarkTest("ButtonCreation", Description = "Creates 100 buttons and measures elapsed time")]
public class ButtonCreationBenchmark : BenchmarkTestPage
{
    private readonly VerticalStackLayout layout;

    public ButtonCreationBenchmark()
    {
        layout = new VerticalStackLayout();
        Content = layout;
    }

    /// <inheritdoc/>
    public override Task<BenchmarkResult> RunAsync(ILogger logger, CancellationToken cancellationToken)
    {
        const int buttonCount = 100;

        layout.Children.Clear();

        var memBefore = MemorySnapshot.Capture(forceGC: true);
        var stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < buttonCount; i++)
        {
            layout.Children.Add(new Button { Text = $"Button {i + 1}" });
        }

        stopwatch.Stop();
        var memAfter = MemorySnapshot.Capture(forceGC: true);
        var memoryDelta = memAfter.Compare(memBefore);

        var elapsedMs = stopwatch.Elapsed.TotalMilliseconds;
        var perButtonMs = elapsedMs / buttonCount;

        logger.LogInformation(
            "Created {ButtonCount} buttons in {ElapsedMs:F2}ms ({PerButtonMs:F3}ms per button)",
            buttonCount,
            elapsedMs,
            perButtonMs);

        var metrics = new Dictionary<string, object>
        {
            ["ButtonCount"] = buttonCount,
            ["ElapsedMs"] = elapsedMs,
            ["PerButtonMs"] = perButtonMs,
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        return Task.FromResult(BenchmarkResult.Pass(metrics));
    }
}
