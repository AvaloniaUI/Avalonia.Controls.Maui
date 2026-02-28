// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that a complex control tree can be collected after simulated navigation away.
/// </summary>
[BenchmarkTest("PageNavigationLeak", Description = "Verifies complex control trees are collected after navigation")]
public class PageNavigationLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();

        // Build the control tree and track objects in a separate method
        // to prevent local variables from rooting objects on the async state machine.
        BuildAndTrackControlTree(trackedObjects);

        // Allow handlers to connect
        await Task.Delay(50, cancellationToken);

        // Disconnect handlers and tear down in a separate method
        // to avoid handler references staying alive on this method's state machine.
        DisconnectAndTearDown(trackedObjects);

        // Force GC twice with dispatcher yield between
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        await Task.Delay(100, cancellationToken);
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

        var metrics = new Dictionary<string, object>
        {
            ["TotalObjectsTracked"] = trackedObjects.Count,
            ["ObjectsLeaked"] = leaked.Count,
            ["LeakedObjects"] = leaked.Count > 0 ? string.Join(", ", leaked) : "none",
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        if (leaked.Count > 0)
        {
            var leakedNames = string.Join(", ", leaked);
            logger.LogWarning("Memory leak detected: {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Objects leaked: {leakedNames}", metrics);
        }

        logger.LogInformation(
            "All {Count} objects collected successfully",
            trackedObjects.Count);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void BuildAndTrackControlTree(Dictionary<string, WeakReference<object>> trackedObjects)
    {
        var rootLayout = new VerticalStackLayout();
        trackedObjects["RootLayout"] = new WeakReference<object>(rootLayout);

        var headerLabel = new Label { Text = "Page Header" };
        rootLayout.Children.Add(headerLabel);
        trackedObjects["HeaderLabel"] = new WeakReference<object>(headerLabel);

        var actionButton = new Button { Text = "Action" };
        actionButton.Clicked += (_, _) => { };
        rootLayout.Children.Add(actionButton);
        trackedObjects["ActionButton"] = new WeakReference<object>(actionButton);

        var inputEntry = new Entry { Placeholder = "Enter text" };
        rootLayout.Children.Add(inputEntry);
        trackedObjects["InputEntry"] = new WeakReference<object>(inputEntry);

        var nestedLayout = new StackLayout { Orientation = StackOrientation.Horizontal };
        rootLayout.Children.Add(nestedLayout);
        trackedObjects["NestedLayout"] = new WeakReference<object>(nestedLayout);

        var nestedLabel = new Label { Text = "Nested" };
        nestedLayout.Children.Add(nestedLabel);
        trackedObjects["NestedLabel"] = new WeakReference<object>(nestedLabel);

        var nestedButton = new Button { Text = "Nested Action" };
        nestedLayout.Children.Add(nestedButton);
        trackedObjects["NestedButton"] = new WeakReference<object>(nestedButton);

        // Set as content to trigger handler connect
        Content = rootLayout;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void DisconnectAndTearDown(Dictionary<string, WeakReference<object>> trackedObjects)
    {
        if (Content is not Layout rootLayout)
            return;

        // Track handlers before disconnecting — use a helper to avoid
        // keeping handler references on this method's stack frame.
        TrackHandler(trackedObjects, "HeaderLabel.Handler", rootLayout.Children.OfType<Label>().FirstOrDefault());
        TrackHandler(trackedObjects, "ActionButton.Handler", rootLayout.Children.OfType<Button>().FirstOrDefault());
        TrackHandler(trackedObjects, "InputEntry.Handler", rootLayout.Children.OfType<Entry>().FirstOrDefault());

        // Disconnect all handlers
        DisconnectHandlers(rootLayout);

        // Simulate navigation away
        Content = new Label { Text = "Navigated away" };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void TrackHandler(Dictionary<string, WeakReference<object>> trackedObjects, string name, VisualElement? element)
    {
        if (element?.Handler is object handler)
        {
            trackedObjects[name] = new WeakReference<object>(handler);
        }
    }

    private static void DisconnectHandlers(Layout layout)
    {
        foreach (var child in layout.Children)
        {
            if (child is Layout childLayout)
            {
                DisconnectHandlers(childLayout);
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
