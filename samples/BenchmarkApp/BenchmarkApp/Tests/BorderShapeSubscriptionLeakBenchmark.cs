

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that MauiBorder cleans up its Shape.PropertyChanged subscription when detached
/// from the visual tree. MauiBorder subscribes to Shape.PropertyChanged in OnPropertyChanged
/// when the Shape property is set. Without cleanup on detach, the shape's event delegate list
/// holds a reference to the MauiBorder instance, preventing it from being collected when the
/// shape object outlives the border (e.g., shared shapes or shapes held by view models).
///
/// This test creates borders that share a common RoundRectangle shape, navigates the page away
/// to detach the borders, then verifies the MauiBorder platform views are collectible
/// (which they won't be if the shared shape still holds delegate references).
/// </summary>
[BenchmarkTest("BorderShapeSubscriptionLeak", Description = "Verifies MauiBorder cleans up Shape PropertyChanged subscription on detach")]
public class BorderShapeSubscriptionLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var platformViewWeakRefs = new Dictionary<string, WeakReference>();
        // This shape instance will be kept alive after the borders are detached.
        // If MauiBorder doesn't unsubscribe, the shape's PropertyChanged delegate list
        // roots all the MauiBorder instances.
        var sharedShape = new RoundRectangle { CornerRadius = new CornerRadius(16) };

        await RunLifecycle(window, this, sharedShape, platformViewWeakRefs, logger, cancellationToken);

        await Task.Delay(200, cancellationToken);

        for (int i = 0; i < 3; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            await Task.Delay(100, cancellationToken);
        }

        var memAfter = MemorySnapshot.Capture(forceGC: false);
        var memoryDelta = memAfter.Compare(memBefore);

        var leakedPlatform = new List<string>();
        foreach (var (name, weakRef) in platformViewWeakRefs)
        {
            if (weakRef.IsAlive)
                leakedPlatform.Add(name);
        }

        var metrics = new Dictionary<string, object>
        {
            ["PlatformViewsTested"] = platformViewWeakRefs.Count,
            ["PlatformViewsLeaked"] = leakedPlatform.Count,
            ["SharedShapeAlive"] = true, // shape is kept alive intentionally
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        if (leakedPlatform.Count > 0)
        {
            var leakedNames = string.Join(", ", leakedPlatform);
            logger.LogWarning(
                "MauiBorder platform views leaked via shared shape subscription: {LeakedViews}",
                leakedNames);
            return BenchmarkResult.Fail(
                $"MauiBorder platform views leaked (shape subscription not cleaned): {leakedNames}",
                metrics);
        }

        logger.LogInformation(
            "All {Count} MauiBorder platform views collected despite shared shape staying alive",
            platformViewWeakRefs.Count);

        // Keep sharedShape alive through the GC check window
        GC.KeepAlive(sharedShape);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static async Task RunLifecycle(
        Window window,
        Page restorePage,
        RoundRectangle sharedShape,
        Dictionary<string, WeakReference> platformViewWeakRefs,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var layout = new VerticalStackLayout { Spacing = 10, Padding = new Thickness(16) };

        // Create borders that all share the same RoundRectangle shape.
        // Each Border gets its StrokeShape set to the shared instance,
        // which causes MauiBorder.OnPropertyChanged to subscribe to
        // sharedShape.PropertyChanged for EACH border.
        for (int i = 0; i < 6; i++)
        {
            var border = new Border
            {
                StrokeShape = sharedShape,
                Stroke = Colors.Blue,
                StrokeThickness = 2,
                Padding = new Thickness(12),
                Content = new VerticalStackLayout
                {
                    Children =
                    {
                        new Label { Text = $"Border {i} title", FontSize = 16, FontAttributes = FontAttributes.Bold },
                        new Label { Text = $"Border {i} content", FontSize = 12 },
                    },
                },
            };
            layout.Children.Add(border);
        }

        var page = new ContentPage
        {
            Title = "Shared Shape Borders",
            Content = new ScrollView { Content = layout },
        };
        window.Page = page;

        // Wait for rendering to ensure platform views are created
        await Task.Delay(200, cancellationToken);

        // Capture weak refs to MauiBorder platform views
        for (int i = 0; i < layout.Children.Count; i++)
        {
            if (layout.Children[i] is Border border && border.Handler?.PlatformView is object pv)
            {
                var key = $"Border[{i}].PlatformView";
                platformViewWeakRefs[key] = new WeakReference(pv);
                logger.LogInformation("Tracked {Key}", key);
            }
        }

        // Navigate away — detaches borders from visual tree.
        // Without OnDetachedFromVisualTree cleanup, the sharedShape's PropertyChanged
        // delegate list still holds references to all 6 MauiBorder instances.
        window.Page = restorePage;

        page = null!;
        layout = null!;
    }
}
