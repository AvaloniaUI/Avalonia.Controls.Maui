using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that RadioButton and Stepper controls and their handlers are collected after disconnect.
/// RadioButton has group membership state; Stepper has value-changed event subscriptions.
/// </summary>
[BenchmarkTest("RadioButtonStepperLeak", Description = "Verifies RadioButton and Stepper are collected after disconnect")]
public class RadioButtonStepperLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();

        await CreateAndDestroyControls(trackedObjects, cancellationToken);

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
            ["ControlsTested"] = 4,
            ["TotalObjectsTracked"] = trackedObjects.Count,
            ["ObjectsLeaked"] = leaked.Count,
            ["RadioButton.Leaked"] = leaked.Any(n => n.StartsWith("RadioButton")),
            ["Stepper.Leaked"] = leaked.Any(n => n.StartsWith("Stepper")),
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        if (leaked.Count > 0)
        {
            var leakedNames = string.Join(", ", leaked);
            logger.LogWarning("RadioButton/Stepper leak detected: {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Objects leaked: {leakedNames}", metrics);
        }

        logger.LogInformation(
            "All {Count} RadioButton/Stepper objects collected successfully",
            trackedObjects.Count);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task CreateAndDestroyControls(
        Dictionary<string, WeakReference<object>> trackedObjects,
        CancellationToken cancellationToken)
    {
        var layout = new VerticalStackLayout();
        Content = layout;

        CreateControls(trackedObjects, layout);

        // Allow handlers to connect
        await Task.Delay(50, cancellationToken);

        // Track handlers before disconnect
        TrackHandlers(trackedObjects, layout);

        // Tear down
        TearDown(layout);

        Content = new Label { Text = "RadioButton/Stepper test complete" };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateControls(
        Dictionary<string, WeakReference<object>> trackedObjects,
        VerticalStackLayout layout)
    {
        // Create 3 RadioButtons in the same group
        var radio1 = new RadioButton { Content = "Option A", GroupName = "TestGroup", IsChecked = true };
        radio1.CheckedChanged += (_, _) => { };
        layout.Children.Add(radio1);
        trackedObjects["RadioButton1"] = new WeakReference<object>(radio1);

        var radio2 = new RadioButton { Content = "Option B", GroupName = "TestGroup" };
        radio2.CheckedChanged += (_, _) => { };
        layout.Children.Add(radio2);
        trackedObjects["RadioButton2"] = new WeakReference<object>(radio2);

        var radio3 = new RadioButton { Content = "Option C", GroupName = "TestGroup" };
        radio3.CheckedChanged += (_, _) => { };
        layout.Children.Add(radio3);
        trackedObjects["RadioButton3"] = new WeakReference<object>(radio3);

        // Toggle selection to exercise event subscriptions
        radio2.IsChecked = true;
        radio3.IsChecked = true;

        // Create Stepper
        var stepper = new Stepper { Minimum = 0, Maximum = 10, Value = 5, Increment = 1 };
        stepper.ValueChanged += (_, _) => { };
        stepper.Value = 7;
        stepper.Value = 3;
        layout.Children.Add(stepper);
        trackedObjects["Stepper"] = new WeakReference<object>(stepper);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void TrackHandlers(
        Dictionary<string, WeakReference<object>> trackedObjects,
        VerticalStackLayout layout)
    {
        int radioIndex = 1;
        foreach (var child in layout.Children)
        {
            if (child is VisualElement ve && ve.Handler is object handler)
            {
                var typeName = ve.GetType().Name;
                if (ve is RadioButton)
                {
                    trackedObjects[$"RadioButton{radioIndex}.Handler"] = new WeakReference<object>(handler);
                    radioIndex++;
                }
                else
                {
                    trackedObjects[$"{typeName}.Handler"] = new WeakReference<object>(handler);
                }
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
