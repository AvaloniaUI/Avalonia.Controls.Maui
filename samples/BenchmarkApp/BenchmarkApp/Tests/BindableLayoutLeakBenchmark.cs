// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that templated items from BindableLayout don't leak when the source collection is
/// cleared and repopulated. Targets handler disconnect for dynamically generated children.
/// </summary>
[BenchmarkTest("BindableLayoutLeak", Description = "Verifies BindableLayout templated items are collected after source changes")]
public class BindableLayoutLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();
        const int repopulationCycles = 10;
        const int itemsPerCycle = 15;

        // Build layout with BindableLayout and repopulate multiple times
        await BuildAndRepopulate(trackedObjects, repopulationCycles, itemsPerCycle, cancellationToken);

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

        int totalTracked = trackedObjects.Count;
        double leakRate = totalTracked > 0 ? (double)leaked.Count / totalTracked : 0;

        var metrics = new Dictionary<string, object>
        {
            ["RepopulationCycles"] = repopulationCycles,
            ["ItemsPerCycle"] = itemsPerCycle,
            ["TotalObjectsTracked"] = totalTracked,
            ["ObjectsLeaked"] = leaked.Count,
            ["LeakRate"] = leakRate,
            ["LeakedObjects"] = leaked.Count > 0 ? string.Join(", ", leaked.Take(20)) : "none",
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        // Allow up to 5% leak rate for BindableLayout (templated items have complex lifecycle)
        if (leakRate > 0.05)
        {
            logger.LogWarning(
                "BindableLayout leak detected: {Leaked}/{Total} objects survived (rate: {Rate:P1})",
                leaked.Count, totalTracked, leakRate);
            return BenchmarkResult.Fail(
                $"Leak rate {leakRate:P1} exceeds 5% threshold ({leaked.Count}/{totalTracked} objects)",
                metrics);
        }

        logger.LogInformation(
            "BindableLayout: {Leaked}/{Total} objects survived after {Cycles} cycles (rate: {Rate:P1})",
            leaked.Count, totalTracked, repopulationCycles, leakRate);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task BuildAndRepopulate(
        Dictionary<string, WeakReference<object>> trackedObjects,
        int cycles,
        int itemsPerCycle,
        CancellationToken cancellationToken)
    {
        var layout = new VerticalStackLayout();
        Content = layout;

        var bindableStack = new VerticalStackLayout();
        layout.Children.Add(bindableStack);

        var collection = new ObservableCollection<string>();

        BindableLayout.SetItemTemplate(bindableStack, new DataTemplate(() =>
        {
            var label = new Label();
            label.SetBinding(Label.TextProperty, ".");
            return label;
        }));

        BindableLayout.SetItemsSource(bindableStack, collection);

        for (int cycle = 0; cycle < cycles; cycle++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Populate
            for (int i = 0; i < itemsPerCycle; i++)
            {
                collection.Add($"Cycle{cycle}_Item{i}");
            }

            // Allow handlers to connect
            await Task.Delay(20, cancellationToken);

            // Track the generated children before clearing
            TrackBindableLayoutChildren(trackedObjects, bindableStack, cycle);

            // Clear collection (triggers item removal)
            collection.Clear();

            // Allow cleanup
            await Task.Delay(20, cancellationToken);
        }

        // Final teardown
        BindableLayout.SetItemsSource(bindableStack, null);
        DisconnectLayout(layout);
        Content = new Label { Text = "BindableLayout test complete" };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void TrackBindableLayoutChildren(
        Dictionary<string, WeakReference<object>> trackedObjects,
        VerticalStackLayout bindableStack,
        int cycle)
    {
        int childIndex = 0;
        foreach (var child in bindableStack.Children)
        {
            if (child is VisualElement ve)
            {
                trackedObjects[$"Cycle{cycle}.Child[{childIndex}]"] = new WeakReference<object>(ve);

                if (ve.Handler is object handler)
                {
                    trackedObjects[$"Cycle{cycle}.Handler[{childIndex}]"] = new WeakReference<object>(handler);
                }
            }

            childIndex++;
        }
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
