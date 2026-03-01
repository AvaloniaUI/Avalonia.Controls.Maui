using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that rapidly changing BindingContext on a deep control hierarchy doesn't leak
/// old binding contexts. PropertyChanged handlers from data binding should not accumulate
/// when the BindingContext is replaced repeatedly.
/// </summary>
[BenchmarkTest("BindingContextChurnLeak", Description = "Verifies rapid BindingContext changes don't leak old view models")]
public class BindingContextChurnLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();
        const int churnCycles = 20;
        const int childCount = 10;

        await ChurnBindingContexts(trackedObjects, churnCycles, childCount, cancellationToken);

        // Force GC multiple times with delays
        for (int gc = 0; gc < 3; gc++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            await Task.Delay(50, cancellationToken);
        }

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
            ["ChurnCycles"] = churnCycles,
            ["ChildCount"] = childCount,
            ["TotalObjectsTracked"] = totalTracked,
            ["ObjectsLeaked"] = leaked.Count,
            ["LeakRate"] = leakRate,
            ["LeakedObjects"] = leaked.Count > 0 ? string.Join(", ", leaked.Take(20)) : "none",
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        // Allow up to 5% leak rate for binding context churn
        if (leakRate > 0.05)
        {
            logger.LogWarning(
                "BindingContext churn leak: {Leaked}/{Total} objects survived (rate: {Rate:P1})",
                leaked.Count, totalTracked, leakRate);
            return BenchmarkResult.Fail(
                $"Leak rate {leakRate:P1} exceeds 5% threshold ({leaked.Count}/{totalTracked} objects)",
                metrics);
        }

        if (memoryDelta.WorkingSetDelta > 50 * 1024 * 1024)
        {
            return BenchmarkResult.Fail(
                $"Native memory growth {memoryDelta.WorkingSetDelta / (1024.0 * 1024):F1} MB exceeds 50 MB threshold",
                metrics);
        }

        logger.LogInformation(
            "BindingContext churn: {Leaked}/{Total} objects survived after {Cycles} cycles (rate: {Rate:P1})",
            leaked.Count, totalTracked, churnCycles, leakRate);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task ChurnBindingContexts(
        Dictionary<string, WeakReference<object>> trackedObjects,
        int cycles,
        int childCount,
        CancellationToken cancellationToken)
    {
        var layout = new VerticalStackLayout();
        Content = layout;

        // Build a hierarchy with bound controls
        for (int i = 0; i < childCount; i++)
        {
            var label = new Label();
            label.SetBinding(Label.TextProperty, "DisplayName");

            var entry = new Entry();
            entry.SetBinding(Entry.TextProperty, "InputValue");

            layout.Children.Add(label);
            layout.Children.Add(entry);
        }

        await Task.Delay(30, cancellationToken);

        // Rapidly swap BindingContext
        for (int c = 0; c < cycles; c++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var viewModel = CreateViewModel(c);
            trackedObjects[$"ViewModel{c}"] = new WeakReference<object>(viewModel);

            layout.BindingContext = viewModel;
            await Task.Delay(10, cancellationToken);
        }

        // Set final null binding context to release the last one
        layout.BindingContext = null;

        // Disconnect and tear down
        foreach (var child in layout.Children)
        {
            if (child is VisualElement ve)
            {
                ve.Handler?.DisconnectHandler();
            }
        }

        layout.Children.Clear();
        layout.Handler?.DisconnectHandler();
        Content = new Label { Text = "BindingContext churn test complete" };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static SimpleViewModel CreateViewModel(int index)
    {
        return new SimpleViewModel
        {
            DisplayName = $"Item {index}",
            InputValue = $"Value {index}",
        };
    }

    private sealed class SimpleViewModel : INotifyPropertyChanged
    {
        private string? displayName;
        private string? inputValue;

        public string? DisplayName
        {
            get => displayName;
            set
            {
                if (displayName != value)
                {
                    displayName = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayName)));
                }
            }
        }

        public string? InputValue
        {
            get => inputValue;
            set
            {
                if (inputValue != value)
                {
                    inputValue = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InputValue)));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
