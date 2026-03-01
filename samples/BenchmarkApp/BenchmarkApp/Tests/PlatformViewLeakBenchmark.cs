

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Verifies that Avalonia platform views are collected after handler disconnect,
/// not just the MAUI virtual views.
/// </summary>
[BenchmarkTest("PlatformViewLeak", Description = "Verifies Avalonia platform views are GC'd after handler disconnect")]
public class PlatformViewLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var mauiWeakRefs = new Dictionary<string, WeakReference>();
        var platformViewWeakRefs = new Dictionary<string, WeakReference>();

        await CreateAndDisconnectControls(mauiWeakRefs, platformViewWeakRefs, cancellationToken);

        // Force GC multiple times with delays
        for (int i = 0; i < 3; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            await Task.Delay(100, cancellationToken);
        }

        var memAfter = MemorySnapshot.Capture(forceGC: false);
        var memoryDelta = memAfter.Compare(memBefore);

        // Check which MAUI controls leaked
        var leakedMaui = new List<string>();
        foreach (var (name, weakRef) in mauiWeakRefs)
        {
            if (weakRef.IsAlive)
                leakedMaui.Add(name);
        }

        // Check which platform views leaked
        var leakedPlatform = new List<string>();
        foreach (var (name, weakRef) in platformViewWeakRefs)
        {
            if (weakRef.IsAlive)
                leakedPlatform.Add(name);
        }

        var metrics = new Dictionary<string, object>
        {
            ["MauiControlsTested"] = mauiWeakRefs.Count,
            ["MauiControlsLeaked"] = leakedMaui.Count,
            ["PlatformViewsTested"] = platformViewWeakRefs.Count,
            ["PlatformViewsLeaked"] = leakedPlatform.Count,
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        if (leakedPlatform.Count > 0)
        {
            var leakedNames = string.Join(", ", leakedPlatform);
            logger.LogWarning("Platform view leak detected: {LeakedViews}", leakedNames);
            return BenchmarkResult.Fail($"Platform views leaked: {leakedNames}", metrics);
        }

        if (leakedMaui.Count > 0)
        {
            var leakedNames = string.Join(", ", leakedMaui);
            logger.LogWarning("MAUI control leak detected: {LeakedControls}", leakedNames);
            return BenchmarkResult.Fail($"MAUI controls leaked: {leakedNames}", metrics);
        }

        logger.LogInformation(
            "All {MauiCount} MAUI controls and {PlatformCount} platform views collected successfully",
            mauiWeakRefs.Count, platformViewWeakRefs.Count);

        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task CreateAndDisconnectControls(
        Dictionary<string, WeakReference> mauiWeakRefs,
        Dictionary<string, WeakReference> platformViewWeakRefs,
        CancellationToken cancellationToken)
    {
        var layout = new VerticalStackLayout();
        Content = layout;

        // Wait for layout to be attached so handlers create platform views
        await Task.Delay(100, cancellationToken);

        // Create controls and add to layout
        var button = new Button { Text = "Platform leak test" };
        layout.Children.Add(button);
        mauiWeakRefs["Button"] = new WeakReference(button);

        var label = new Label { Text = "Platform leak test" };
        layout.Children.Add(label);
        mauiWeakRefs["Label"] = new WeakReference(label);

        var entry = new Entry { Placeholder = "Platform leak test" };
        layout.Children.Add(entry);
        mauiWeakRefs["Entry"] = new WeakReference(entry);

        var innerLayout = new VerticalStackLayout();
        innerLayout.Children.Add(new Label { Text = "Inner" });
        layout.Children.Add(innerLayout);
        mauiWeakRefs["InnerLayout"] = new WeakReference(innerLayout);

        var contentView = new ContentView { Content = new Label { Text = "CV content" } };
        layout.Children.Add(contentView);
        mauiWeakRefs["ContentView"] = new WeakReference(contentView);

        // Wait for rendering to ensure platform views are created
        await Task.Delay(100, cancellationToken);

        // Capture weak refs to the Avalonia platform views
        CapturePlatformViewRef(platformViewWeakRefs, "Button.PlatformView", button.Handler);
        CapturePlatformViewRef(platformViewWeakRefs, "Label.PlatformView", label.Handler);
        CapturePlatformViewRef(platformViewWeakRefs, "Entry.PlatformView", entry.Handler);
        CapturePlatformViewRef(platformViewWeakRefs, "InnerLayout.PlatformView", innerLayout.Handler);
        CapturePlatformViewRef(platformViewWeakRefs, "ContentView.PlatformView", contentView.Handler);

        // Remove from layout (fires Clear command which clears LayoutPanel children)
        layout.Children.Clear();

        // Disconnect handlers
        button.Handler?.DisconnectHandler();
        label.Handler?.DisconnectHandler();
        entry.Handler?.DisconnectHandler();
        innerLayout.Handler?.DisconnectHandler();
        contentView.Handler?.DisconnectHandler();

        // Replace content so layout handler can also be collected
        Content = new Label { Text = "Done" };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CapturePlatformViewRef(
        Dictionary<string, WeakReference> refs,
        string name,
        Microsoft.Maui.IElementHandler? handler)
    {
        if (handler?.PlatformView is object platformView)
        {
            refs[name] = new WeakReference(platformView);
        }
    }
}
