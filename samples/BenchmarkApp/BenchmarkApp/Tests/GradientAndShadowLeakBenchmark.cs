using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests the heavy gradient and shadow usage found in AlohaAI's UI.
/// AlohaAI uses LinearGradientBrush on backgrounds, strokes, and overlays,
/// plus Shadow effects on nearly every card. This test creates and destroys
/// controls with these decorations to detect resource leaks.
/// </summary>
[BenchmarkTest("GradientAndShadowLeak", Description = "Verifies controls with gradients and shadows are collected after disconnect")]
public class GradientAndShadowLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();
        const int cycles = 8;

        await CreateAndDestroyGradientControls(trackedObjects, cycles, cancellationToken);

        // Force GC multiple times with delays
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

        // Check for survivors
        var leaked = new List<string>();
        foreach (var (name, weakRef) in trackedObjects)
        {
            if (weakRef.TryGetTarget(out _))
            {
                leaked.Add(name);
            }
        }

        int gradientLeaks = leaked.Count(n => n.Contains("Gradient"));
        int shadowLeaks = leaked.Count(n => n.Contains("Shadow"));
        int borderLeaks = leaked.Count(n => n.Contains("Border"));
        int boxViewLeaks = leaked.Count(n => n.Contains("BoxView"));

        var metrics = new Dictionary<string, object>
        {
            ["Cycles"] = cycles,
            ["TotalObjectsTracked"] = trackedObjects.Count,
            ["ObjectsLeaked"] = leaked.Count,
            ["GradientLeaks"] = gradientLeaks,
            ["ShadowLeaks"] = shadowLeaks,
            ["BorderLeaks"] = borderLeaks,
            ["BoxViewLeaks"] = boxViewLeaks,
            ["LeakedObjects"] = leaked.Count > 0 ? string.Join(", ", leaked) : "none",
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        if (leaked.Count > 0)
        {
            var leakedNames = string.Join(", ", leaked);
            logger.LogWarning("Gradient/Shadow leak detected: {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Objects leaked: {leakedNames}", metrics);
        }

        if (CreateNativeMemoryFailure(memoryDelta, logger, metrics) is { } nativeMemoryFailure)
            return nativeMemoryFailure;

        logger.LogInformation(
            "All {Count} gradient/shadow objects collected after {Cycles} cycles",
            trackedObjects.Count,
            cycles);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task CreateAndDestroyGradientControls(
        Dictionary<string, WeakReference<object>> trackedObjects,
        int cycles,
        CancellationToken cancellationToken)
    {
        for (int cycle = 0; cycle < cycles; cycle++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            CreateGradientPage(trackedObjects, cycle);

            // Allow handlers to connect
            await Task.Delay(30, cancellationToken);

            TearDownGradientPage(trackedObjects, cycle);

            await Task.Delay(20, cancellationToken);
        }

        Content = new Label { Text = "Gradient and shadow test complete" };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void CreateGradientPage(Dictionary<string, WeakReference<object>> trackedObjects, int cycle)
    {
        var layout = new VerticalStackLayout { Spacing = 10, Padding = new Thickness(16) };

        // 1. Full-page gradient background (like AlohaAI's header gradient overlay)
        var bgGradient = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(0, 1),
            GradientStops = new GradientStopCollection
            {
                new GradientStop(Color.FromArgb("#2A1E3D"), 0.0f),
                new GradientStop(Color.FromArgb("#1A1A2E"), 1.0f),
            },
        };
        var bgBox = new BoxView { Background = bgGradient };
        layout.Children.Add(bgBox);
        trackedObjects[$"Cycle{cycle}.BgGradient"] = new WeakReference<object>(bgGradient);
        trackedObjects[$"Cycle{cycle}.BgBoxView"] = new WeakReference<object>(bgBox);

        // 2. Gradient overlay with transparency (like AlohaAI's image overlay)
        var overlayGradient = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(0, 1),
            GradientStops = new GradientStopCollection
            {
                new GradientStop(Colors.Transparent, 0.0f),
                new GradientStop(Color.FromArgb("#80000000"), 0.5f),
                new GradientStop(Color.FromArgb("#FF1A1A2E"), 1.0f),
            },
        };
        var overlayBox = new BoxView { Background = overlayGradient, HeightRequest = 200 };
        layout.Children.Add(overlayBox);
        trackedObjects[$"Cycle{cycle}.OverlayGradient"] = new WeakReference<object>(overlayGradient);
        trackedObjects[$"Cycle{cycle}.OverlayBoxView"] = new WeakReference<object>(overlayBox);

        // 3. Card with gradient background (like AlohaAI's Word of Day card)
        var cardGradient = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 0.6),
            GradientStops = new GradientStopCollection
            {
                new GradientStop(Color.FromArgb("#2A1E3D"), 0.0f),
                new GradientStop(Color.FromArgb("#1E2A3D"), 0.6f),
                new GradientStop(Color.FromArgb("#1A1E2D"), 1.0f),
            },
        };
        var cardBorder = new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
            Stroke = Colors.Transparent,
            Background = cardGradient,
            Shadow = new Shadow
            {
                Brush = Colors.Black,
                Offset = new Point(0, 4),
                Radius = 12,
                Opacity = 0.3f,
            },
            Content = new Label { Text = $"Card {cycle}", FontSize = 16, Padding = new Thickness(16) },
        };
        layout.Children.Add(cardBorder);
        trackedObjects[$"Cycle{cycle}.CardGradient"] = new WeakReference<object>(cardGradient);
        trackedObjects[$"Cycle{cycle}.CardBorder"] = new WeakReference<object>(cardBorder);
        trackedObjects[$"Cycle{cycle}.CardShadow"] = new WeakReference<object>(cardBorder.Shadow);

        // 4. Border with gradient stroke (like AlohaAI's glassmorphism cards)
        var strokeGradient = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 1),
            GradientStops = new GradientStopCollection
            {
                new GradientStop(Color.FromArgb("#30FFFFFF"), 0.0f),
                new GradientStop(Color.FromArgb("#10FFFFFF"), 0.5f),
                new GradientStop(Color.FromArgb("#05FFFFFF"), 1.0f),
            },
        };
        var glassBorder = new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 22 },
            StrokeThickness = 1,
            Stroke = strokeGradient,
            Shadow = new Shadow
            {
                Brush = Colors.Black,
                Offset = new Point(0, 6),
                Radius = 20,
                Opacity = 0.6f,
            },
            Content = new Label { Text = $"Glass card {cycle}", FontSize = 16, Padding = new Thickness(16) },
        };
        layout.Children.Add(glassBorder);
        trackedObjects[$"Cycle{cycle}.StrokeGradient"] = new WeakReference<object>(strokeGradient);
        trackedObjects[$"Cycle{cycle}.GlassBorder"] = new WeakReference<object>(glassBorder);
        trackedObjects[$"Cycle{cycle}.GlassShadow"] = new WeakReference<object>(glassBorder.Shadow);

        // 5. Stats cards with shadows (like AlohaAI's Quick Stats)
        var statsGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
            },
            ColumnSpacing = 10,
        };

        for (int i = 0; i < 3; i++)
        {
            var statShadow = new Shadow
            {
                Brush = Colors.Black,
                Offset = new Point(0, 4),
                Radius = 12,
                Opacity = 0.3f,
            };
            var statBorder = new Border
            {
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
                Stroke = Colors.Transparent,
                Padding = new Thickness(12),
                Shadow = statShadow,
                Content = new VerticalStackLayout
                {
                    Children =
                    {
                        new BoxView
                        {
                            HeightRequest = 4,
                            Color = Color.FromArgb(i == 0 ? "#B8A0E0" : i == 1 ? "#7EB4F0" : "#6DD4A0"),
                            Clip = new Microsoft.Maui.Controls.Shapes.RoundRectangleGeometry
                            {
                                CornerRadius = new CornerRadius(16, 16, 0, 0),
                                Rect = new Rect(0, 0, 200, 4),
                            },
                        },
                        new Label { Text = $"{i * 42}", FontSize = 28, FontAttributes = FontAttributes.Bold },
                        new Label { Text = i == 0 ? "Streak" : i == 1 ? "XP" : "Today", FontSize = 11 },
                    },
                },
            };
            statsGrid.Add(statBorder, i, 0);
            trackedObjects[$"Cycle{cycle}.StatBorder[{i}]"] = new WeakReference<object>(statBorder);
            trackedObjects[$"Cycle{cycle}.StatShadow[{i}]"] = new WeakReference<object>(statShadow);
        }

        layout.Children.Add(statsGrid);

        // 6. Label with shadow (like AlohaAI's welcome text)
        var shadowLabel = new Label
        {
            Text = "Aloha! Welcome back!",
            FontSize = 24,
            FontAttributes = FontAttributes.Bold,
            Shadow = new Shadow
            {
                Brush = Color.FromArgb("#40000000"),
                Offset = new Point(0, 1),
                Radius = 4,
                Opacity = 0.4f,
            },
        };
        layout.Children.Add(shadowLabel);
        trackedObjects[$"Cycle{cycle}.ShadowLabel"] = new WeakReference<object>(shadowLabel);
        trackedObjects[$"Cycle{cycle}.LabelShadow"] = new WeakReference<object>(shadowLabel.Shadow);

        Content = layout;
        trackedObjects[$"Cycle{cycle}.Layout"] = new WeakReference<object>(layout);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void TearDownGradientPage(Dictionary<string, WeakReference<object>> trackedObjects, int cycle)
    {
        if (Content is not Layout layout)
            return;

        // Track handler
        if (layout.Handler is object handler)
        {
            trackedObjects[$"Cycle{cycle}.LayoutHandler"] = new WeakReference<object>(handler);
        }

        DisconnectLayout(layout);
        Content = null;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void DisconnectLayout(Layout layout)
    {
        foreach (var child in layout.Children)
        {
            if (child is Layout childLayout)
            {
                DisconnectLayout(childLayout);
            }

            if (child is VisualElement ve)
            {
                ve.Handler?.DisconnectHandler();
            }
        }

        layout.Children.Clear();
        layout.Handler?.DisconnectHandler();
    }
}
