

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

        if (options.RunAll)
        {
            var placeholder = new ContentPage();
            var window = new Window(placeholder);
            var started = false;
            placeholder.Loaded += (_, _) =>
            {
                if (started)
                    return;
                started = true;
                _ = RunAllBenchmarksAsync(window);
            };
            return window;
        }

        if (options.TestName is null)
        {
            return new Window(new ContentPage());
        }

        var testPage = BenchmarkRegistry.CreateTest(options.TestName);
        if (testPage is null)
        {
            return new Window(new ContentPage());
        }

        var testWindow = new Window(testPage);
        var benchmarkStarted = false;
        testPage.Loaded += (_, _) =>
        {
            if (benchmarkStarted)
                return;
            benchmarkStarted = true;
            _ = RunBenchmarkAsync(testWindow, testPage);
        };
        return testWindow;
    }

    private static async Task RunAllBenchmarksAsync(Window window)
    {
        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(
            () => { },
            Avalonia.Threading.DispatcherPriority.Background);

        var options = BenchmarkOptions.Current;
        var services = IPlatformApplication.Current!.Services;
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Benchmark");
        var tests = BenchmarkRegistry.GetTests();
        var allResults = new List<BenchmarkTestResult>();
        bool allPassed = true;

        foreach (var (testName, (description, factory)) in tests)
        {
            logger.LogInformation("=== Running benchmark: {TestName} ===", testName);

            var testPage = factory();
            window.Page = testPage;

            // Yield to let the page render.
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(
                () => { },
                Avalonia.Threading.DispatcherPriority.Background);

            var (passed, results) = await RunTestIterationsAsync(window, testPage, testName, logger, options.Iterations);
            allResults.AddRange(results);

            if (!passed)
            {
                allPassed = false;
            }
        }

        LogResultsSummary(logger, allResults);

        if (options.OutputPath is not null)
        {
            JUnitXmlWriter.Write(options.OutputPath, allResults);
            logger.LogInformation("JUnit XML results written to {OutputPath}", options.OutputPath);
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

    private static async Task RunBenchmarkAsync(Window window, BenchmarkTestPage testPage)
    {
        // Yield to let the Avalonia main loop start and render the window.
        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(
            () => { },
            Avalonia.Threading.DispatcherPriority.Background);

        var options = BenchmarkOptions.Current;
        var services = IPlatformApplication.Current!.Services;
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Benchmark");
        var testName = options.TestName ?? "Unknown";

        var (allPassed, results) = await RunTestIterationsAsync(window, testPage, testName, logger, options.Iterations);

        if (options.Iterations > 1)
        {
            LogResultsSummary(logger, results);
        }

        if (options.OutputPath is not null)
        {
            JUnitXmlWriter.Write(options.OutputPath, results);
            logger.LogInformation("JUnit XML results written to {OutputPath}", options.OutputPath);
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


    private static void LogResultsSummary(ILogger logger, List<BenchmarkTestResult> results)
    {
        var passed = results.Count(r => r.Passed);
        var failed = results.Count(r => !r.Passed);

        logger.LogInformation("=== Benchmark Results ===");
        logger.LogInformation("Total: {Total} | Passed: {Passed} | Failed: {Failed}", results.Count, passed, failed);

        if (failed > 0)
        {
            logger.LogInformation("Failed tests:");
            foreach (var result in results.Where(r => !r.Passed))
            {
                logger.LogError("  {TestName}: {Reason}", result.TestName, result.FailureReason ?? "Unknown");
            }
        }
    }

    private static async Task<(bool AllPassed, List<BenchmarkTestResult> Results)> RunTestIterationsAsync(
        Window window, BenchmarkTestPage testPage, string testName, ILogger logger, int iterations)
    {
        bool allPassed = true;
        var results = new List<BenchmarkTestResult>();

        for (int i = 1; i <= iterations; i++)
        {
            if (iterations > 1)
            {
                logger.LogInformation("--- Iteration {Iteration}/{Total} ---", i, iterations);
            }

            var before = MemorySnapshot.Capture(forceGC: true);
            var stopwatch = Stopwatch.StartNew();

            var result = await testPage.RunAsync(window, logger, CancellationToken.None);

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

            var displayName = iterations > 1 ? $"{testName} (iteration {i})" : testName;
            results.Add(new BenchmarkTestResult(
                displayName,
                result.Passed,
                result.FailureReason,
                stopwatch.Elapsed.TotalSeconds,
                allMetrics));
        }

        return (allPassed, results);
    }
}
