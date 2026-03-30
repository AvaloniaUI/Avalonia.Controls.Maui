using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests modal pages with animated presentation/dismissal containing data-bound controls
/// and gesture recognizers. MauiAvaloniaWindow.PresentModal() creates TranslateTransform +
/// animation loop that may still be running when DismissModal() removes the wrapper.
/// </summary>
/// <remarks>
/// Gap: ModalPageLeakBenchmark uses animated:false, so it never triggers the TranslateTransform
/// animation loop. It also uses simple content with no gestures or bindings.
/// </remarks>
[BenchmarkTest("ModalWithGesturesAndBindingsLeak", Description = "Verifies animated modal pages with gestures and bindings are collected")]
public class ModalWithGesturesAndBindingsLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();
        const int modalCycles = 8;

        await PushPopAnimatedModals(trackedObjects, modalCycles, logger, cancellationToken);

        // Wait 1s after last dismiss to let animations complete
        await Task.Delay(1000, cancellationToken);

        // Force GC multiple times with delays
        for (int gc = 0; gc < 3; gc++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            await Task.Delay(100, cancellationToken);
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

        long totalMemoryGrowth = memAfter.TotalMemory - memBefore.TotalMemory;

        var metrics = new Dictionary<string, object>
        {
            ["ModalCycles"] = modalCycles,
            ["TotalObjectsTracked"] = trackedObjects.Count,
            ["ObjectsLeaked"] = leaked.Count,
            ["LeakedObjects"] = leaked.Count > 0 ? string.Join(", ", leaked) : "none",
            ["TotalMemoryGrowthBytes"] = totalMemoryGrowth,
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        // Allow at most 1 leaked object (last animation tolerance)
        if (leaked.Count > 1)
        {
            var leakedNames = string.Join(", ", leaked);
            logger.LogWarning("Modal+gestures+bindings leak detected: {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Objects leaked: {leakedNames}", metrics);
        }

        if (totalMemoryGrowth > 1024 * 1024)
        {
            logger.LogWarning("Excessive total memory growth: {Growth:N0} bytes", totalMemoryGrowth);
            return BenchmarkResult.Fail($"Total memory growth {totalMemoryGrowth:N0} bytes exceeds 1 MB", metrics);
        }

        if (leaked.Count == 1)
        {
            logger.LogWarning(
                "1 of {Count} objects survived GC (animation tolerance): {Name}",
                trackedObjects.Count, leaked[0]);
        }
        else
        {
            logger.LogInformation(
                "All {Count} modal objects collected after {Cycles} animated cycles",
                trackedObjects.Count, modalCycles);
        }

        if (CreateNativeMemoryFailure(memoryDelta, logger, metrics) is { } nativeMemoryFailure)
            return nativeMemoryFailure;

        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task PushPopAnimatedModals(
        Dictionary<string, WeakReference<object>> trackedObjects,
        int cycles,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        Content = new Label { Text = "Modal+gestures+bindings test running..." };

        for (int i = 0; i < cycles; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await PushPopSingleAnimatedModal(trackedObjects, i, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning("Modal cycle {Cycle} failed: {Error}", i, ex.Message);
                break;
            }
        }

        Content = new Label { Text = "Modal+gestures+bindings test complete" };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task PushPopSingleAnimatedModal(
        Dictionary<string, WeakReference<object>> trackedObjects,
        int index,
        CancellationToken cancellationToken)
    {
        var viewModel = new SimpleViewModel { Title = $"Modal {index}", Count = index * 10 };
        var modalPage = CreateRichModalPage(trackedObjects, index, viewModel);

        // Push with animation (triggers TranslateTransform animation loop)
        await Navigation.PushModalAsync(modalPage, animated: true);

        // Wait only 100ms (less than full 400ms animation) to test interruption
        await Task.Delay(100, cancellationToken);

        // Pop with animation while push animation may still be running
        await Navigation.PopModalAsync(animated: true);
        await Task.Delay(50, cancellationToken);

        // Disconnect handler
        modalPage.Handler?.DisconnectHandler();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ContentPage CreateRichModalPage(
        Dictionary<string, WeakReference<object>> trackedObjects,
        int index,
        SimpleViewModel viewModel)
    {
        var page = new ContentPage
        {
            Title = $"Modal {index}",
            BindingContext = viewModel,
        };
        trackedObjects[$"Modal{index}.Page"] = new WeakReference<object>(page);
        trackedObjects[$"Modal{index}.ViewModel"] = new WeakReference<object>(viewModel);

        var scrollView = new ScrollView();
        var layout = new VerticalStackLayout { Spacing = 10, Padding = new Thickness(16) };

        // Data-bound labels
        var titleLabel = new Label { FontSize = 24 };
        titleLabel.SetBinding(Label.TextProperty, nameof(SimpleViewModel.Title));
        layout.Children.Add(titleLabel);

        var countLabel = new Label { FontSize = 16 };
        countLabel.SetBinding(Label.TextProperty, nameof(SimpleViewModel.Count), stringFormat: "Count: {0}");
        layout.Children.Add(countLabel);

        // Borders with TapGestureRecognizers
        for (int i = 0; i < 3; i++)
        {
            var border = new Border
            {
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
                Padding = new Thickness(12),
                Content = new Label { Text = $"Tappable item {i}" },
            };

            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => { };
            border.GestureRecognizers.Add(tap);
            layout.Children.Add(border);
            trackedObjects[$"Modal{index}.Border{i}"] = new WeakReference<object>(border);
            trackedObjects[$"Modal{index}.Tap{i}"] = new WeakReference<object>(tap);
        }

        // Entry bound to ViewModel
        var entry = new Entry();
        entry.SetBinding(Entry.TextProperty, nameof(SimpleViewModel.Title));
        layout.Children.Add(entry);
        trackedObjects[$"Modal{index}.Entry"] = new WeakReference<object>(entry);

        scrollView.Content = layout;
        page.Content = scrollView;
        trackedObjects[$"Modal{index}.ScrollView"] = new WeakReference<object>(scrollView);

        return page;
    }

    private class SimpleViewModel : BindableObject
    {
        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(SimpleViewModel), string.Empty);

        public static readonly BindableProperty CountProperty =
            BindableProperty.Create(nameof(Count), typeof(int), typeof(SimpleViewModel), 0);

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public int Count
        {
            get => (int)GetValue(CountProperty);
            set => SetValue(CountProperty, value);
        }
    }
}
