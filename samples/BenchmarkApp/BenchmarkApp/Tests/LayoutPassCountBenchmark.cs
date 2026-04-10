using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using AvMauiLayoutHandler = global::Avalonia.Controls.Maui.Handlers.LayoutHandler;
using AvLayoutPanel = global::Avalonia.Controls.Maui.Platform.LayoutPanel;

namespace BenchmarkApp.Tests;

[BenchmarkTest("LayoutPassCount", Description = "Measures total layout measure/arrange passes for visual, size, and batched size updates")]
public class LayoutPassCountBenchmark : BenchmarkTestPage
{
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var metrics = new Dictionary<string, object>();
        var counter = new PassCounter();
        var previousFactory = AvMauiLayoutHandler.PlatformViewFactory;

        CountingLayoutPanel.ActiveCounter = counter;
        AvMauiLayoutHandler.PlatformViewFactory = _ => new CountingLayoutPanel();

        try
        {
            var target = new Grid
            {
                WidthRequest = 180,
                HeightRequest = 120,
                Padding = new Thickness(12),
                BackgroundColor = Colors.CadetBlue
            };
            target.Children.Add(new Label { Text = "Layout target" });

            Content = new ScrollView
            {
                Content = new VerticalStackLayout
                {
                    Padding = new Thickness(16),
                    Spacing = 12,
                    Children =
                    {
                        new Label { Text = "Layout pass counter", FontSize = 20 },
                        target,
                        new HorizontalStackLayout
                        {
                            Spacing = 8,
                            Children =
                            {
                                new Button { Text = "Alpha" },
                                new Button { Text = "Beta" }
                            }
                        }
                    }
                }
            };

            await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 40);

            metrics["Initial.MeasurePasses"] = counter.MeasurePasses;
            metrics["Initial.ArrangePasses"] = counter.ArrangePasses;
            logger.LogInformation(
                "Initial layout completed with {MeasurePasses} measure passes and {ArrangePasses} arrange passes",
                counter.MeasurePasses,
                counter.ArrangePasses);

            counter.Reset();
            const int visualIterations = 50;
            var visualStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < visualIterations; i++)
            {
                target.Opacity = i % 2 == 0 ? 0.82 : 1;
                target.BackgroundColor = i % 2 == 0 ? Colors.CadetBlue : Colors.SteelBlue;
                target.ZIndex = i % 3;
            }

            visualStopwatch.Stop();
            await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 25);
            metrics["Visual.ElapsedMs"] = visualStopwatch.Elapsed.TotalMilliseconds;
            metrics["Visual.MeasurePasses"] = counter.MeasurePasses;
            metrics["Visual.ArrangePasses"] = counter.ArrangePasses;
            logger.LogInformation(
                "Visual updates: {ElapsedMs:F2}ms, measure={MeasurePasses}, arrange={ArrangePasses}",
                visualStopwatch.Elapsed.TotalMilliseconds,
                counter.MeasurePasses,
                counter.ArrangePasses);

            counter.Reset();
            const int sizeIterations = 40;
            var sizeStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < sizeIterations; i++)
            {
                target.WidthRequest = 180 + (i % 20);
                target.HeightRequest = 120 + (i % 15);
                target.Padding = new Thickness(12 + (i % 4));
                target.Margin = new Thickness(i % 6);
            }

            sizeStopwatch.Stop();
            await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 25);
            metrics["Size.ElapsedMs"] = sizeStopwatch.Elapsed.TotalMilliseconds;
            metrics["Size.MeasurePasses"] = counter.MeasurePasses;
            metrics["Size.ArrangePasses"] = counter.ArrangePasses;
            logger.LogInformation(
                "Size updates: {ElapsedMs:F2}ms, measure={MeasurePasses}, arrange={ArrangePasses}",
                sizeStopwatch.Elapsed.TotalMilliseconds,
                counter.MeasurePasses,
                counter.ArrangePasses);

            counter.Reset();
            var batchedStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < sizeIterations; i++)
            {
                target.BatchBegin();
                target.WidthRequest = 180 + (i % 20);
                target.HeightRequest = 120 + (i % 15);
                target.Padding = new Thickness(12 + (i % 4));
                target.Margin = new Thickness(i % 6);
                target.BatchCommit();
            }

            batchedStopwatch.Stop();
            await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 25);
            metrics["Batched.ElapsedMs"] = batchedStopwatch.Elapsed.TotalMilliseconds;
            metrics["Batched.MeasurePasses"] = counter.MeasurePasses;
            metrics["Batched.ArrangePasses"] = counter.ArrangePasses;
            metrics["Batched.MeasureReduction"] = Math.Max(0, (int)metrics["Size.MeasurePasses"] - counter.MeasurePasses);
            metrics["Batched.ArrangeReduction"] = Math.Max(0, (int)metrics["Size.ArrangePasses"] - counter.ArrangePasses);
            logger.LogInformation(
                "Batched size updates: {ElapsedMs:F2}ms, measure={MeasurePasses}, arrange={ArrangePasses}",
                batchedStopwatch.Elapsed.TotalMilliseconds,
                counter.MeasurePasses,
                counter.ArrangePasses);

            return BenchmarkResult.Pass(metrics);
        }
        finally
        {
            Content = new Label { Text = "Layout pass benchmark complete" };
            await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 15);
            AvMauiLayoutHandler.PlatformViewFactory = previousFactory;
            CountingLayoutPanel.ActiveCounter = null;
        }
    }

    private sealed class PassCounter
    {
        public int ArrangePasses { get; set; }

        public int MeasurePasses { get; set; }

        public void Reset()
        {
            MeasurePasses = 0;
            ArrangePasses = 0;
        }
    }

    private sealed class CountingLayoutPanel : AvLayoutPanel
    {
        public static PassCounter? ActiveCounter { get; set; }

        protected override global::Avalonia.Size ArrangeOverride(global::Avalonia.Size finalSize)
        {
            var activeCounter = ActiveCounter;
            if (activeCounter != null)
            {
                activeCounter.ArrangePasses++;
            }
            return base.ArrangeOverride(finalSize);
        }

        protected override global::Avalonia.Size MeasureOverride(global::Avalonia.Size availableSize)
        {
            var activeCounter = ActiveCounter;
            if (activeCounter != null)
            {
                activeCounter.MeasurePasses++;
            }
            return base.MeasureOverride(availableSize);
        }
    }
}
