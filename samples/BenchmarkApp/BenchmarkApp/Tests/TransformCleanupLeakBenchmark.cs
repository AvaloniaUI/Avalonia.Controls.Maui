

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using AvaloniaControl = Avalonia.Controls.Control;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that RenderTransform (TransformGroup containing ScaleTransform, RotateTransform,
/// TranslateTransform) is cleared when a handler disconnects. Without cleanup, the
/// TransformGroup stays attached to the platform view, keeping ScaleTransform,
/// RotateTransform, and TranslateTransform objects alive unnecessarily.
/// This test verifies that after handler disconnect, the platform view's RenderTransform is null.
/// </summary>
[BenchmarkTest("TransformCleanupLeak", Description = "Verifies TransformGroup is cleared on handler disconnect")]
public class TransformCleanupLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var result = await RunLifecycle(window, this, logger, cancellationToken);

        var memAfter = MemorySnapshot.Capture(forceGC: true);
        var memoryDelta = memAfter.Compare(memBefore);

        var metrics = new Dictionary<string, object>
        {
            ["ControlsTested"] = result.TotalControls,
            ["TransformsNotCleared"] = result.StaleTransforms.Count,
            ["StaleTransforms"] = result.StaleTransforms.Count > 0
                ? string.Join(", ", result.StaleTransforms)
                : "none",
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        if (result.StaleTransforms.Count > 0)
        {
            var staleNames = string.Join(", ", result.StaleTransforms);
            logger.LogWarning("RenderTransform not cleared after disconnect: {StaleTransforms}", staleNames);
            return BenchmarkResult.Fail($"RenderTransform not cleared: {staleNames}", metrics);
        }

        logger.LogInformation(
            "All {Count} platform views had RenderTransform cleared after disconnect",
            result.TotalControls);
        return BenchmarkResult.Pass(metrics);
    }

    private record CleanupResult(int TotalControls, List<string> StaleTransforms);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static async Task<CleanupResult> RunLifecycle(
        Window window,
        Page restorePage,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        // Create controls with transform properties that trigger UpdateTransformation()
        var layout = new VerticalStackLayout { Spacing = 10, Padding = new Thickness(16) };
        var page = new ContentPage { Title = "Transforms", Content = layout };

        var controls = new List<(string Name, View View)>
        {
            ("RotatedLabel", new Label { Text = "Rotated", Rotation = 45 }),
            ("ScaledButton", new Button { Text = "Scaled", Scale = 1.5 }),
            ("TranslatedEntry", new Entry { Placeholder = "Translated", TranslationX = 10, TranslationY = 5 }),
            ("FullTransformBox", new BoxView
            {
                Rotation = 30, Scale = 2.0, TranslationX = 20, TranslationY = 15,
                WidthRequest = 50, HeightRequest = 50, BackgroundColor = Colors.Red,
            }),
            ("RotationXYImage", new Image
            {
                Source = "dotnet_bot.png", WidthRequest = 64, HeightRequest = 64,
                RotationX = 15, RotationY = 20, Scale = 1.2,
            }),
        };

        foreach (var (_, view) in controls)
        {
            layout.Children.Add(view);
        }

        window.Page = page;

        // Wait for rendering — UpdateTransformation dispatches to UI thread
        await Task.Delay(300, cancellationToken);

        // Capture the platform views before disconnect
        var platformViews = new List<(string Name, AvaloniaControl PlatformView)>();
        foreach (var (name, view) in controls)
        {
            if (view.Handler?.PlatformView is AvaloniaControl pv)
            {
                // Verify the transform was actually applied
                if (pv.RenderTransform != null)
                {
                    logger.LogInformation("{Name}: RenderTransform = {Type}", name, pv.RenderTransform.GetType().Name);
                    platformViews.Add((name, pv));
                }
                else
                {
                    logger.LogWarning("{Name}: RenderTransform was null before disconnect (transform not applied)", name);
                }
            }
        }

        // Navigate away — this triggers DisconnectHandlers on the page tree.
        // The fix clears RenderTransform in ViewHandlerOfT.DisconnectHandler().
        window.Page = restorePage;

        // Allow dispatcher to drain
        await Task.Delay(100, cancellationToken);

        // Check: after disconnect, RenderTransform should be null
        var staleTransforms = new List<string>();
        foreach (var (name, pv) in platformViews)
        {
            if (pv.RenderTransform != null)
            {
                staleTransforms.Add(name);
                logger.LogWarning("{Name}: RenderTransform still set after disconnect ({Type})", name, pv.RenderTransform.GetType().Name);
            }
            else
            {
                logger.LogInformation("{Name}: RenderTransform properly cleared", name);
            }
        }

        return new CleanupResult(platformViews.Count, staleTransforms);
    }
}
