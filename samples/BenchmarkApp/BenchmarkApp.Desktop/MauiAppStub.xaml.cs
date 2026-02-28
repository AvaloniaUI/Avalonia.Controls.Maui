// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace BenchmarkApp;

public partial class MauiAppStub : Application
{
    public MauiAppStub()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var options = BenchmarkOptions.Current;

        if (options.TestName is null)
        {
            return new Window(new ContentPage());
        }

        var testPage = BenchmarkRegistry.CreateTest(options.TestName);
        if (testPage is null)
        {
            return new Window(new ContentPage());
        }

        testPage.Loaded += (_, _) => _ = RunBenchmarkAsync(testPage);
        return new Window(testPage);
    }

    private async Task RunBenchmarkAsync(BenchmarkTestPage testPage)
    {
        // Yield to let the Avalonia main loop start and render the window.
        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(
            () => { },
            Avalonia.Threading.DispatcherPriority.Background);

        var options = BenchmarkOptions.Current;
        var services = IPlatformApplication.Current!.Services;
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Benchmark");
        var testName = options.TestName ?? "Unknown";

        bool allPassed = true;

        for (int i = 1; i <= options.Iterations; i++)
        {
            if (options.Iterations > 1)
            {
                logger.LogInformation("--- Iteration {Iteration}/{Total} ---", i, options.Iterations);
            }

            var before = MemorySnapshot.Capture(forceGC: true);
            var stopwatch = Stopwatch.StartNew();

            var result = await testPage.RunAsync(logger, CancellationToken.None);

            stopwatch.Stop();
            var after = MemorySnapshot.Capture(forceGC: true);
            var memoryDelta = after.Compare(before);
            var elapsedMs = stopwatch.Elapsed.TotalMilliseconds;

            // Record metrics for dotnet-counters
            BenchmarkMetrics.RecordIteration(testName, elapsedMs, memoryDelta.BytesDelta, result.Passed);

            if (result.Passed)
            {
                logger.LogInformation("PASSED");
            }
            else
            {
                logger.LogError("FAILED: {Reason}", result.FailureReason);
                allPassed = false;
            }

            // Merge memory delta metrics with the result's own metrics
            var allMetrics = new Dictionary<string, object>(result.Metrics);
            foreach (var (key, value) in memoryDelta.ToMetrics())
            {
                allMetrics.TryAdd(key, value);
            }

            if (allMetrics.Count > 0)
            {
                logger.LogInformation("Metrics:");
                foreach (var (key, value) in allMetrics)
                {
                    logger.LogInformation("  {Key}: {Value}", key, value);
                }
            }
        }

        var exitCode = allPassed ? 0 : 1;

        if (!options.KeepOpen)
        {
            if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown(exitCode);
            }
        }
    }
}
