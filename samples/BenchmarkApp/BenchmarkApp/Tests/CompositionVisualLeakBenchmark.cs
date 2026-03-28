using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests whether Avalonia composition/render resources (composition visuals, Skia surfaces,
/// byte[] render buffers) are properly released when visual trees with shadows, gradients,
/// and borders are created and destroyed.
///
/// The AlohaAI profiling showed the gcdump dominated by System.Byte[] in large size buckets
/// and Avalonia types like ICompositorSerializable[], ServerCompositionVisual[], and
/// IAffectsRender subscription entries accumulating continuously.
/// </summary>
[BenchmarkTest("CompositionVisualLeak", Description = "Detects Avalonia composition/render resource leaks from shadow/gradient/border visual trees")]
public class CompositionVisualLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        const int iterations = 20;
        const double maxLeakRate = 0.10; // 10% of tracked objects surviving
        const long maxMemoryGrowthBytes = 15 * 1024 * 1024; // 15 MB

        var memBefore = MemorySnapshot.Capture(forceGC: true);
        var allWeakRefs = new List<(string Name, WeakReference<object> Ref)>();
        var iterationMemory = new List<long>();
        var iterationWorkingSet = new List<long>();

        for (int iter = 0; iter < iterations; iter++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Build a complex visual tree with composition-heavy elements
            BuildAndTrackCompositionTree(allWeakRefs, $"Iter{iter}");

            // Allow rendering to process the visual tree
            await Task.Delay(100, cancellationToken);

            // Tear down the visual tree
            TearDownContent();

            // GC every 5 iterations
            if ((iter + 1) % 5 == 0)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                iterationMemory.Add(GC.GetTotalMemory(false));
                using (var proc = Process.GetCurrentProcess())
                {
                    iterationWorkingSet.Add(proc.WorkingSet64);
                }
            }
        }

        // Final aggressive GC
        Content = new Label { Text = "Done" };
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        await Task.Delay(200, cancellationToken);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        await Task.Delay(100, cancellationToken);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var memAfter = MemorySnapshot.Capture(forceGC: false);
        var memoryDelta = memAfter.Compare(memBefore);

        // Check survivors
        int totalTracked = allWeakRefs.Count;
        var leaked = new List<string>();
        foreach (var (name, weakRef) in allWeakRefs)
        {
            if (weakRef.TryGetTarget(out _))
            {
                leaked.Add(name);
            }
        }

        double leakRate = totalTracked > 0 ? (double)leaked.Count / totalTracked : 0;
        long memoryGrowth = memoryDelta.BytesDelta;

        // Log iteration memory trend
        if (iterationMemory.Count > 1)
        {
            logger.LogInformation("Memory at GC checkpoints:");
            for (int i = 0; i < iterationMemory.Count; i++)
            {
                long delta = i > 0 ? iterationMemory[i] - iterationMemory[i - 1] : 0;
                long wsDelta = i > 0 ? iterationWorkingSet[i] - iterationWorkingSet[i - 1] : 0;
                logger.LogInformation(
                    "  Checkpoint {Index}: {Memory:N0} bytes (delta: {Delta:+#,##0;-#,##0;0}), working set: {WorkingSet:N0} bytes (delta: {WsDelta:+#,##0;-#,##0;0})",
                    i, iterationMemory[i], delta, iterationWorkingSet[i], wsDelta);
            }
        }

        logger.LogInformation(
            "Created {Total} composition objects, {Leaked} survived GC (leak rate: {LeakRate:P1}), memory growth: {Growth:N0} bytes",
            totalTracked, leaked.Count, leakRate, memoryGrowth);

        if (leaked.Count > 0)
        {
            // Group leaked objects by prefix to show patterns
            var leakedGroups = leaked
                .Select(n => n.Contains('.') ? n[..n.LastIndexOf('.')] : n)
                .GroupBy(g => g)
                .OrderByDescending(g => g.Count())
                .Take(10);

            foreach (var group in leakedGroups)
            {
                logger.LogWarning("  Leaked group: {Group} ({Count} objects)", group.Key, group.Count());
            }
        }

        var metrics = new Dictionary<string, object>
        {
            ["Iterations"] = iterations,
            ["TotalObjectsTracked"] = totalTracked,
            ["ObjectsLeaked"] = leaked.Count,
            ["LeakRate"] = leakRate,
            ["MemoryGrowthBytes"] = memoryGrowth,
            ["BaselineWorkingSetBytes"] = iterationWorkingSet.Count > 0 ? iterationWorkingSet[0] : 0L,
            ["FinalCheckpointWorkingSetBytes"] = iterationWorkingSet.Count > 0 ? iterationWorkingSet[^1] : 0L,
            ["TotalWorkingSetGrowthBytes"] = iterationWorkingSet.Count > 1 ? iterationWorkingSet[^1] - iterationWorkingSet[0] : 0L,
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        var reasons = new List<string>();

        if (leakRate >= maxLeakRate)
        {
            reasons.Add($"leak rate {leakRate:P1} >= {maxLeakRate:P0} threshold ({leaked.Count}/{totalTracked} objects)");
        }

        if (memoryGrowth >= maxMemoryGrowthBytes)
        {
            reasons.Add($"memory growth {memoryGrowth / (1024.0 * 1024):F1} MB >= {maxMemoryGrowthBytes / (1024 * 1024)} MB threshold");
        }

        if (reasons.Count > 0)
        {
            return BenchmarkResult.Fail(string.Join("; ", reasons), metrics);
        }

        if (memoryDelta.WorkingSetDelta > 100 * 1024 * 1024)
        {
            return BenchmarkResult.Warn(
                $"Native memory growth {memoryDelta.WorkingSetDelta / (1024.0 * 1024):F1} MB exceeds 100 MB threshold",
                metrics);
        }

        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void BuildAndTrackCompositionTree(List<(string Name, WeakReference<object> Ref)> weakRefs, string prefix)
    {
        var layout = new VerticalStackLayout { Spacing = 8, Padding = new Thickness(12) };

        // Gradient background box (triggers composition layer creation)
        var gradientBox = new BoxView
        {
            HeightRequest = 150,
            Background = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb("#2A1E3D"), 0.0f),
                    new GradientStop(Color.FromArgb("#1E2A3D"), 0.6f),
                    new GradientStop(Color.FromArgb("#1A1E2D"), 1.0f),
                },
            },
        };
        layout.Children.Add(gradientBox);
        weakRefs.Add(($"{prefix}.GradientBox", new WeakReference<object>(gradientBox)));

        // Multiple borders with shadows (each shadow creates a render target/composition visual)
        for (int i = 0; i < 5; i++)
        {
            var shadow = new Shadow
            {
                Brush = Colors.Black,
                Offset = new Point(0, 4 + i),
                Radius = 12 + (i * 2),
                Opacity = 0.3f + (i * 0.05f),
            };
            weakRefs.Add(($"{prefix}.Shadow[{i}]", new WeakReference<object>(shadow)));

            var border = new Border
            {
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
                StrokeThickness = 1,
                Stroke = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(1, 1),
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromArgb("#30FFFFFF"), 0.0f),
                        new GradientStop(Color.FromArgb("#05FFFFFF"), 1.0f),
                    },
                },
                Shadow = shadow,
                Padding = new Thickness(14),
                Content = new Grid
                {
                    Children =
                    {
                        // Background image with low opacity (produces render buffer)
                        new Image
                        {
                            Source = "dotnet_bot.png",
                            Opacity = 0.1,
                            Aspect = Aspect.AspectFill,
                            InputTransparent = true,
                        },
                        new VerticalStackLayout
                        {
                            Children =
                            {
                                new Label { Text = $"Card {i}", FontSize = 18, FontAttributes = FontAttributes.Bold },
                                new Label { Text = "Sub-text with composition effects", FontSize = 13 },
                            },
                        },
                    },
                },
            };

            layout.Children.Add(border);
            weakRefs.Add(($"{prefix}.Border[{i}]", new WeakReference<object>(border)));
        }

        // Nested grids with overlapping elements (complex composition tree)
        var nestedGrid = new Grid
        {
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition(new GridLength(80)),
                new RowDefinition(new GridLength(80)),
            },
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
            },
        };

        for (int row = 0; row < 2; row++)
        {
            for (int col = 0; col < 2; col++)
            {
                var cell = new Border
                {
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
                    Shadow = new Shadow { Brush = Colors.Black, Offset = new Point(0, 2), Radius = 8, Opacity = 0.2f },
                    Content = new Label
                    {
                        Text = $"Cell [{row},{col}]",
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                    },
                };
                nestedGrid.Add(cell, col, row);
                weakRefs.Add(($"{prefix}.Cell[{row},{col}]", new WeakReference<object>(cell)));
            }
        }

        layout.Children.Add(nestedGrid);
        weakRefs.Add(($"{prefix}.NestedGrid", new WeakReference<object>(nestedGrid)));
        weakRefs.Add(($"{prefix}.Layout", new WeakReference<object>(layout)));

        Content = layout;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void TearDownContent()
    {
        if (Content is Layout layout)
        {
            DisconnectTree(layout);
        }

        Content = null;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void DisconnectTree(IView view)
    {
        if (view is Layout layout)
        {
            foreach (var child in layout.Children)
            {
                if (child is IView childView)
                {
                    DisconnectTree(childView);
                }
            }

            layout.Children.Clear();
        }
        else if (view is Border border)
        {
            if (border.Content is IView borderContent)
            {
                DisconnectTree(borderContent);
            }

            border.Content = null;
            border.Shadow = null!;
        }
        else if (view is ScrollView scrollView)
        {
            if (scrollView.Content is IView scrollContent)
            {
                DisconnectTree(scrollContent);
            }

            scrollView.Content = null;
        }
        else if (view is Image image)
        {
            image.Source = null;
        }

        if (view is View v)
        {
            v.GestureRecognizers.Clear();
        }

        if (view is VisualElement ve)
        {
            ve.Handler?.DisconnectHandler();
        }
    }
}
