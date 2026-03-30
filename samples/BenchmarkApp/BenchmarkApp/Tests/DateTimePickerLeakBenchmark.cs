using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that DatePicker and TimePicker controls and their handlers are collected after disconnect.
/// These handlers subscribe to value-changed events and hold format/range state.
/// </summary>
[BenchmarkTest("DateTimePickerLeak", Description = "Verifies DatePicker and TimePicker are collected after disconnect")]
public class DateTimePickerLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();

        await CreateAndDestroyDateTimePickers(trackedObjects, cancellationToken);

        // Force GC multiple times with delays
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
            ["ControlsTested"] = 2,
            ["TotalObjectsTracked"] = trackedObjects.Count,
            ["ObjectsLeaked"] = leaked.Count,
            ["DatePicker.Leaked"] = leaked.Any(n => n.StartsWith("DatePicker")),
            ["TimePicker.Leaked"] = leaked.Any(n => n.StartsWith("TimePicker")),
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        if (leaked.Count > 0)
        {
            var leakedNames = string.Join(", ", leaked);
            logger.LogWarning("DateTimePicker leak detected: {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Objects leaked: {leakedNames}", metrics);
        }

        if (CreateNativeMemoryFailure(memoryDelta, logger, metrics) is { } nativeMemoryFailure)
            return nativeMemoryFailure;

        logger.LogInformation(
            "All {Count} DateTimePicker objects collected successfully",
            trackedObjects.Count);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task CreateAndDestroyDateTimePickers(
        Dictionary<string, WeakReference<object>> trackedObjects,
        CancellationToken cancellationToken)
    {
        var layout = new VerticalStackLayout();
        Content = layout;

        CreateDateTimePickers(trackedObjects, layout);

        // Allow handlers to connect
        await Task.Delay(50, cancellationToken);

        // Track handlers before disconnect
        TrackHandlers(trackedObjects, layout);

        // Tear down
        TearDown(layout);

        Content = new Label { Text = "DateTimePicker test complete" };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateDateTimePickers(
        Dictionary<string, WeakReference<object>> trackedObjects,
        VerticalStackLayout layout)
    {
        var datePicker = new DatePicker
        {
            Date = new DateTime(2025, 6, 15),
            MinimumDate = new DateTime(2020, 1, 1),
            MaximumDate = new DateTime(2030, 12, 31),
            Format = "yyyy-MM-dd",
        };
        datePicker.DateSelected += (_, _) => { };
        datePicker.Date = new DateTime(2025, 12, 25);
        layout.Children.Add(datePicker);
        trackedObjects["DatePicker"] = new WeakReference<object>(datePicker);

        var timePicker = new TimePicker
        {
            Time = new TimeSpan(14, 30, 0),
            Format = "HH:mm",
        };
        timePicker.Time = new TimeSpan(9, 15, 0);
        layout.Children.Add(timePicker);
        trackedObjects["TimePicker"] = new WeakReference<object>(timePicker);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void TrackHandlers(
        Dictionary<string, WeakReference<object>> trackedObjects,
        VerticalStackLayout layout)
    {
        foreach (var child in layout.Children)
        {
            if (child is VisualElement ve && ve.Handler is object handler)
            {
                var typeName = ve.GetType().Name;
                trackedObjects[$"{typeName}.Handler"] = new WeakReference<object>(handler);
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void TearDown(VerticalStackLayout layout)
    {
        foreach (var child in layout.Children)
        {
            if (child is VisualElement ve)
            {
                ve.Handler?.DisconnectHandler();
            }
        }

        layout.Children.Clear();
        layout.Handler?.DisconnectHandler();
    }
}
