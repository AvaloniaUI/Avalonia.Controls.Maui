using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that rapidly adding and removing Shell flyout items while the Shell stays connected
/// doesn't leak ShellItem/ShellSection objects. ShellExtensions.UpdateFlyoutItems subscribes
/// to PropertyChanged on each flyout item — this test verifies those subscriptions are properly
/// cleaned up when items are replaced.
/// </summary>
[BenchmarkTest("ShellFlyoutItemChurnLeak", Description = "Verifies Shell flyout item add/remove churn doesn't leak")]
public class ShellFlyoutItemChurnLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();
        const int churnCycles = 5;

        await CreateAndChurnShellItems(window, trackedObjects, churnCycles, cancellationToken);

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

        var metrics = new Dictionary<string, object>
        {
            ["ChurnCycles"] = churnCycles,
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
            logger.LogWarning("Shell flyout item churn leak detected: {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Objects leaked: {leakedNames}", metrics);
        }

        if (CreateNativeMemoryFailure(memoryDelta, logger, metrics) is { } nativeMemoryFailure)
            return nativeMemoryFailure;

        logger.LogInformation(
            "All {Count} Shell flyout churn objects collected after {Cycles} cycles",
            trackedObjects.Count,
            churnCycles);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task CreateAndChurnShellItems(
        Window window,
        Dictionary<string, WeakReference<object>> trackedObjects,
        int churnCycles,
        CancellationToken cancellationToken)
    {
        // Create a Shell with initial items
        var shell = CreateShell();
        window.Page = shell;

        // Allow initial handlers to connect
        await Task.Delay(100, cancellationToken);

        // Rapidly add/remove items to exercise flyout subscription management
        for (int cycle = 0; cycle < churnCycles; cycle++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            AddShellItems(shell, trackedObjects, cycle);
            await Task.Delay(30, cancellationToken);

            RemoveShellItems(shell, trackedObjects, cycle);
            await Task.Delay(30, cancellationToken);
        }

        // Track final Shell handler
        if (shell.Handler is object shellHandler)
        {
            trackedObjects["Shell.Handler"] = new WeakReference<object>(shellHandler);
        }

        // Disconnect Shell
        shell.Handler?.DisconnectHandler();

        // Restore test page
        window.Page = this;
        Content = new Label { Text = "Shell flyout churn test complete" };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Shell CreateShell()
    {
        var shell = new Shell();

        var mainTab = new ShellContent
        {
            Title = "Home",
            ContentTemplate = new DataTemplate(() => new ContentPage
            {
                Content = new Label { Text = "Home Page" },
            }),
        };

        shell.Items.Add(mainTab);
        return shell;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void AddShellItems(
        Shell shell,
        Dictionary<string, WeakReference<object>> trackedObjects,
        int cycle)
    {
        // Add 3 additional flyout items
        for (int i = 0; i < 3; i++)
        {
            var item = new FlyoutItem
            {
                Title = $"Cycle{cycle}_Item{i}",
                FlyoutDisplayOptions = FlyoutDisplayOptions.AsSingleItem,
                Items =
                {
                    new ShellContent
                    {
                        Title = $"Cycle{cycle}_Content{i}",
                        ContentTemplate = new DataTemplate(() => new ContentPage
                        {
                            Content = new Label { Text = $"Cycle {cycle} Item {i}" },
                        }),
                    },
                },
            };

            shell.Items.Add(item);
            trackedObjects[$"Cycle{cycle}.FlyoutItem{i}"] = new WeakReference<object>(item);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void RemoveShellItems(
        Shell shell,
        Dictionary<string, WeakReference<object>> trackedObjects,
        int cycle)
    {
        // Remove the items we just added (keep the first "Home" item)
        while (shell.Items.Count > 1)
        {
            var item = shell.Items[shell.Items.Count - 1];

            if (item.Handler is object handler)
            {
                trackedObjects[$"Cycle{cycle}.FlyoutItemHandler_{item.Title}"] = new WeakReference<object>(handler);
            }

            item.Handler?.DisconnectHandler();
            shell.Items.RemoveAt(shell.Items.Count - 1);
        }
    }
}
