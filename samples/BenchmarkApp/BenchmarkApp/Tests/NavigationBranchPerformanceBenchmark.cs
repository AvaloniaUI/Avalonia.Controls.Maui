using System.Diagnostics;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using AvaloniaNavigationPage = global::Avalonia.Controls.NavigationPage;

namespace BenchmarkApp.Tests;

[BenchmarkTest("NavigationBranchPerformance", Description = "Measures StackNavigationManager branch latencies for initial connect, forward push, replace, and pop-to-existing-page")]
public class NavigationBranchPerformanceBenchmark : BenchmarkTestPage
{
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        const int cycles = 5;

        var initialSamples = new List<double>(cycles);
        var forwardSamples = new List<double>(cycles);
        var replaceSamples = new List<double>(cycles);
        var popToExistingSamples = new List<double>(cycles);
        var metrics = new Dictionary<string, object>();
        var mauiContext = (Handler as IElementHandler)?.MauiContext
            ?? throw new InvalidOperationException("Benchmark page handler does not have a MAUI context.");

        for (int cycle = 0; cycle < cycles; cycle++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var manager = new StackNavigationManager(mauiContext);
            var stackNavigation = new BenchmarkStackNavigation();
            var platformNavigationPage = new AvaloniaNavigationPage();
            manager.Connect(stackNavigation, platformNavigationPage);

            try
            {
                var rootPage = CreatePage($"Root {cycle}", "Initial connect branch");
                initialSamples.Add(await MeasureRequestAsync(
                    manager,
                    stackNavigation,
                    platformNavigationPage,
                    expectedDepth: 1,
                    new List<IView> { rootPage },
                    cancellationToken));

                var forwardPage = CreatePage($"Forward {cycle}", "Forward push branch");
                forwardSamples.Add(await MeasureRequestAsync(
                    manager,
                    stackNavigation,
                    platformNavigationPage,
                    expectedDepth: 2,
                    new List<IView> { rootPage, forwardPage },
                    cancellationToken));

                var replacementPage = CreatePage($"Replace {cycle}", "Replace same depth branch");
                replaceSamples.Add(await MeasureRequestAsync(
                    manager,
                    stackNavigation,
                    platformNavigationPage,
                    expectedDepth: 2,
                    new List<IView> { rootPage, replacementPage },
                    cancellationToken));

                var pushedPage = CreatePage($"Pushed {cycle}", "Setup for pop-to-existing branch");
                await MeasureRequestAsync(
                    manager,
                    stackNavigation,
                    platformNavigationPage,
                    expectedDepth: 3,
                    new List<IView> { rootPage, replacementPage, pushedPage },
                    cancellationToken);

                popToExistingSamples.Add(await MeasureRequestAsync(
                    manager,
                    stackNavigation,
                    platformNavigationPage,
                    expectedDepth: 2,
                    new List<IView> { rootPage, replacementPage },
                    cancellationToken));
            }
            finally
            {
                manager.Disconnect(stackNavigation, platformNavigationPage);
            }
        }

        AddLatencyMetrics("InitialConnect", initialSamples, metrics);
        AddLatencyMetrics("ForwardPush", forwardSamples, metrics);
        AddLatencyMetrics("ReplaceSameDepth", replaceSamples, metrics);
        AddLatencyMetrics("PopToExistingPage", popToExistingSamples, metrics);
        metrics["Cycles"] = cycles;

        logger.LogInformation(
            "Navigation branches: initial avg={InitialMs:F2}ms, forward avg={ForwardMs:F2}ms, replace avg={ReplaceMs:F2}ms, pop-existing avg={PopMs:F2}ms",
            initialSamples.Average(),
            forwardSamples.Average(),
            replaceSamples.Average(),
            popToExistingSamples.Average());

        return BenchmarkResult.Pass(metrics);
    }

    private static async Task<double> MeasureRequestAsync(
        StackNavigationManager manager,
        IStackNavigation stackNavigation,
        AvaloniaNavigationPage platformNavigationPage,
        int expectedDepth,
        List<IView> targetStack,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        await manager.NavigateTo(new NavigationRequest(targetStack, animated: false));

        await BenchmarkUiHelpers.WaitUntilAsync(
            () => platformNavigationPage.StackDepth == expectedDepth &&
                stackNavigation is BenchmarkStackNavigation benchmarkStack &&
                benchmarkStack.LastFinishedStack.Count == expectedDepth,
            cancellationToken,
            timeoutMs: 5000,
            pollDelayMs: 15);
        await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 10);
        stopwatch.Stop();

        return stopwatch.Elapsed.TotalMilliseconds;
    }

    private static void AddLatencyMetrics(string prefix, IReadOnlyList<double> samples, Dictionary<string, object> metrics)
    {
        metrics[$"{prefix}.AverageElapsedMs"] = samples.Count > 0 ? samples.Average() : 0;
        metrics[$"{prefix}.P95ElapsedMs"] = BenchmarkUiHelpers.CalculatePercentile(samples, 0.95);
        metrics[$"{prefix}.MaxElapsedMs"] = samples.Count > 0 ? samples.Max() : 0;
    }

    private static ContentPage CreatePage(string title, string subtitle)
    {
        return new ContentPage
        {
            Title = title,
            Content = new VerticalStackLayout
            {
                Padding = new Thickness(16),
                Spacing = 12,
                Children =
                {
                    new Label { Text = title, FontSize = 22, FontAttributes = FontAttributes.Bold },
                    new Label { Text = subtitle, FontSize = 14, Opacity = 0.8 },
                    new Entry { Placeholder = $"Input for {title}" },
                    new Button { Text = $"Action for {title}" },
                },
            },
        };
    }

    private sealed class BenchmarkStackNavigation : IStackNavigation
    {
        public IReadOnlyList<IView> LastFinishedStack { get; private set; } = Array.Empty<IView>();

        public void NavigationFinished(IReadOnlyList<IView> newStack)
        {
            LastFinishedStack = newStack;
        }

        public void RequestNavigation(NavigationRequest args)
        {
        }
    }
}
