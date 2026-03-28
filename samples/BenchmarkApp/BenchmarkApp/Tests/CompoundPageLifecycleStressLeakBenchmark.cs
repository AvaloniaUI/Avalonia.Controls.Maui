using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// The "everything at once" test. Creates a single complex page exercising ALL identified
/// leak vectors simultaneously: Images (ImageSourcePartLoader), gesture recognizers
/// (GestureManager), data bindings (PropertyChanged chains), clip shapes (ClipSubscriptions),
/// shadows (compositor resources), gradients, ToolbarItems, ScrollView. Tests whether the
/// combined retention graph prevents collection.
/// </summary>
/// <remarks>
/// Gap: Every existing test exercises 1-2 subsystems. The compound effect is multiplicative —
/// one leaked subscription from any subsystem holds everything via the shared page reference.
/// </remarks>
[BenchmarkTest("CompoundPageLifecycleStressLeak", Description = "Stress test with all subsystems active on a single complex page")]
public class CompoundPageLifecycleStressLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();
        const int cycles = 12;

        var (cycleMemory, cycleWorkingSet) = await PushPopComplexPages(window, trackedObjects, cycles, logger, cancellationToken);

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

        // Categorize leaks
        var leakedPages = leaked.Where(n => n.EndsWith(".Page") || n.EndsWith(".Handler")).ToList();
        var leakedNonPage = leaked.Except(leakedPages).ToList();

        // Analyze per-cycle memory growth
        long avgGrowthPerCycle = 0;
        if (cycleMemory.Count > 2)
        {
            long growthAfterWarmup = cycleMemory[^1] - cycleMemory[1];
            avgGrowthPerCycle = growthAfterWarmup / (cycleMemory.Count - 2);
        }

        logger.LogInformation("Per-cycle memory (bytes after GC):");
        for (int i = 0; i < cycleMemory.Count; i++)
        {
            long delta = i > 0 ? cycleMemory[i] - cycleMemory[i - 1] : 0;
            long wsDelta = i > 0 ? cycleWorkingSet[i] - cycleWorkingSet[i - 1] : 0;
            logger.LogInformation(
                "  Cycle {Cycle}: {Memory:N0} bytes (delta: {Delta:+#,##0;-#,##0;0}) | WorkingSet: {WorkingSet:N0} bytes (delta: {WsDelta:+#,##0;-#,##0;0})",
                i, cycleMemory[i], delta, cycleWorkingSet[i], wsDelta);
        }

        // Analyze per-cycle working set growth (skip first cycle as warmup)
        long avgWsGrowthPerCycle = 0;
        if (cycleWorkingSet.Count > 2)
        {
            long wsGrowthAfterWarmup = cycleWorkingSet[^1] - cycleWorkingSet[1];
            avgWsGrowthPerCycle = wsGrowthAfterWarmup / (cycleWorkingSet.Count - 2);
        }

        var metrics = new Dictionary<string, object>
        {
            ["PushPopCycles"] = cycles,
            ["TotalObjectsTracked"] = trackedObjects.Count,
            ["ObjectsLeaked"] = leaked.Count,
            ["PagesAndHandlersLeaked"] = leakedPages.Count,
            ["NonPageObjectsLeaked"] = leakedNonPage.Count,
            ["LeakedObjects"] = leaked.Count > 0 ? string.Join(", ", leaked) : "none",
            ["AvgGrowthPerCycleBytes"] = avgGrowthPerCycle,
            ["AvgWorkingSetGrowthPerCycleBytes"] = avgWsGrowthPerCycle,
            ["WorkingSetStartBytes"] = cycleWorkingSet.Count > 0 ? cycleWorkingSet[0] : 0,
            ["WorkingSetEndBytes"] = cycleWorkingSet.Count > 0 ? cycleWorkingSet[^1] : 0,
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        bool memoryExceeded = avgGrowthPerCycle > 512 * 1024;

        if (leakedPages.Count > 0)
        {
            var leakedNames = string.Join(", ", leakedPages);
            logger.LogWarning("Compound page lifecycle leak (pages/handlers): {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Pages/handlers leaked: {leakedNames}", metrics);
        }

        // Tolerance: allow up to 2 non-page objects (GC non-determinism)
        if (leakedNonPage.Count > 2)
        {
            var leakedNames = string.Join(", ", leakedNonPage);
            logger.LogWarning("Compound page lifecycle leak (non-page): {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Non-page objects leaked ({leakedNonPage.Count} > 2 tolerance): {leakedNames}", metrics);
        }

        if (memoryExceeded)
        {
            logger.LogWarning("Excessive memory growth: {AvgGrowth:N0} bytes/cycle", avgGrowthPerCycle);
            return BenchmarkResult.Fail($"Avg memory growth {avgGrowthPerCycle:N0} bytes/cycle exceeds 512 KB", metrics);
        }

        if (leakedNonPage.Count > 0)
        {
            logger.LogWarning(
                "{Count} non-page objects survived GC (within tolerance): {Names}",
                leakedNonPage.Count, string.Join(", ", leakedNonPage));
        }
        else
        {
            logger.LogInformation(
                "All {Count} objects collected after {Cycles} cycles. Avg growth: {AvgGrowth:N0} bytes/cycle",
                trackedObjects.Count, cycles, avgGrowthPerCycle);
        }

        if (memoryDelta.WorkingSetDelta > 50 * 1024 * 1024)
        {
            return BenchmarkResult.Warn(
                $"Native memory growth {memoryDelta.WorkingSetDelta / (1024.0 * 1024):F1} MB exceeds 50 MB threshold",
                metrics);
        }

        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task<(List<long> ManagedMemory, List<long> WorkingSet)> PushPopComplexPages(
        Window window,
        Dictionary<string, WeakReference<object>> trackedObjects,
        int cycles,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var cycleMemory = new List<long>();
        var cycleWorkingSet = new List<long>();

        var rootPage = new ContentPage
        {
            Title = "Root",
            Content = new Label { Text = "Compound stress test root" },
        };
        var navPage = new NavigationPage(rootPage);
        window.Page = navPage;

        await Task.Delay(100, cancellationToken);

        // Baseline
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        cycleMemory.Add(GC.GetTotalMemory(false));
        using (var proc = Process.GetCurrentProcess())
        {
            cycleWorkingSet.Add(proc.WorkingSet64);
        }

        for (int i = 0; i < cycles; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await PushPopSingleComplexPage(navPage, trackedObjects, i, cancellationToken);

            // Capture memory
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            cycleMemory.Add(GC.GetTotalMemory(false));
            using (var proc = Process.GetCurrentProcess())
            {
                cycleWorkingSet.Add(proc.WorkingSet64);
            }
        }

        // Teardown
        if (navPage.Handler is object navHandler)
        {
            trackedObjects["NavigationPage.Handler"] = new WeakReference<object>(navHandler);
        }

        rootPage.Handler?.DisconnectHandler();
        navPage.Handler?.DisconnectHandler();

        window.Page = this;
        Content = new Label { Text = "Compound stress test complete" };

        return (cycleMemory, cycleWorkingSet);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static async Task PushPopSingleComplexPage(
        NavigationPage navPage,
        Dictionary<string, WeakReference<object>> trackedObjects,
        int index,
        CancellationToken cancellationToken)
    {
        var vm1 = new CompoundViewModel { Title = $"Page {index}", Count = index };
        var vm2 = new CompoundViewModel { Title = $"Page {index} Updated", Count = index + 100 };
        var vm3 = new CompoundViewModel { Title = $"Page {index} Final", Count = index + 200 };
        trackedObjects[$"Cycle{index}.VM1"] = new WeakReference<object>(vm1);
        trackedObjects[$"Cycle{index}.VM2"] = new WeakReference<object>(vm2);
        trackedObjects[$"Cycle{index}.VM3"] = new WeakReference<object>(vm3);

        var page = CreateComplexPage(trackedObjects, index, vm1);

        await navPage.PushAsync(page, animated: false);
        await Task.Delay(30, cancellationToken);

        if (page.Handler is object handler)
        {
            trackedObjects[$"Cycle{index}.Page.Handler"] = new WeakReference<object>(handler);
        }

        // Change BindingContext 3 times rapidly (exercises PropertyChanged firing into handlers)
        page.BindingContext = vm2;
        page.BindingContext = vm3;
        page.BindingContext = vm1;

        await Task.Delay(30, cancellationToken);

        await navPage.PopAsync(animated: false);
        await Task.Delay(10, cancellationToken);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ContentPage CreateComplexPage(
        Dictionary<string, WeakReference<object>> trackedObjects,
        int index,
        CompoundViewModel viewModel)
    {
        var page = new ContentPage
        {
            Title = $"Complex Page {index}",
            BindingContext = viewModel,
        };
        trackedObjects[$"Cycle{index}.Page"] = new WeakReference<object>(page);

        // ToolbarItems (2 items)
        var toolbar1 = new ToolbarItem { Text = "Save", Order = ToolbarItemOrder.Primary, IconImageSource = "dotnet_bot.png" };
        toolbar1.Clicked += (_, _) => { };
        page.ToolbarItems.Add(toolbar1);
        trackedObjects[$"Cycle{index}.Toolbar1"] = new WeakReference<object>(toolbar1);

        var toolbar2 = new ToolbarItem { Text = "Share", Order = ToolbarItemOrder.Secondary };
        toolbar2.Clicked += (_, _) => { };
        page.ToolbarItems.Add(toolbar2);
        trackedObjects[$"Cycle{index}.Toolbar2"] = new WeakReference<object>(toolbar2);

        // ScrollView wrapper
        var scrollView = new ScrollView();
        var layout = new VerticalStackLayout { Spacing = 8, Padding = new Thickness(16) };

        // 2 Images (ImageSourcePartLoader)
        for (int i = 0; i < 2; i++)
        {
            var image = new Image
            {
                Source = "dotnet_bot.png",
                HeightRequest = 60,
                WidthRequest = 60,
                Aspect = Aspect.AspectFit,
            };
            layout.Children.Add(image);
            trackedObjects[$"Cycle{index}.Image{i}"] = new WeakReference<object>(image);
        }

        // 3 Borders with StrokeShape (clip subscriptions)
        for (int i = 0; i < 3; i++)
        {
            var border = new Border
            {
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
                Padding = new Thickness(12),
                Content = new Label { Text = $"Clipped border {i}" },
            };
            layout.Children.Add(border);
            trackedObjects[$"Cycle{index}.ClipBorder{i}"] = new WeakReference<object>(border);
        }

        // 2 Borders with Shadow (compositor resources)
        for (int i = 0; i < 2; i++)
        {
            var shadowBorder = new Border
            {
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
                Shadow = new Shadow { Brush = Colors.Black, Offset = new Point(0, 4), Radius = 12, Opacity = 0.3f },
                Padding = new Thickness(12),
                Content = new Label { Text = $"Shadow border {i}" },
            };
            layout.Children.Add(shadowBorder);
            trackedObjects[$"Cycle{index}.ShadowBorder{i}"] = new WeakReference<object>(shadowBorder);
        }

        // LinearGradientBrush background
        var gradientBox = new BoxView
        {
            HeightRequest = 40,
            Background = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Colors.Blue, 0.0f),
                    new GradientStop(Colors.Purple, 0.5f),
                    new GradientStop(Colors.Red, 1.0f),
                },
            },
        };
        layout.Children.Add(gradientBox);

        // 4 data-bound Labels
        for (int i = 0; i < 4; i++)
        {
            var label = new Label();
            label.SetBinding(Label.TextProperty, nameof(CompoundViewModel.Title), stringFormat: $"Bound label {i}: {{0}}");
            layout.Children.Add(label);
        }

        // 2 Buttons with Clicked
        for (int i = 0; i < 2; i++)
        {
            var button = new Button { Text = $"Action {i}" };
            button.Clicked += (_, _) => { };
            layout.Children.Add(button);
        }

        // TapGestureRecognizer on 3 controls
        for (int i = 0; i < 3; i++)
        {
            var gestureLabel = new Label { Text = $"Tap label {i}", HeightRequest = 30 };
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => { };
            gestureLabel.GestureRecognizers.Add(tap);
            layout.Children.Add(gestureLabel);
            trackedObjects[$"Cycle{index}.GestureLabel{i}"] = new WeakReference<object>(gestureLabel);
            trackedObjects[$"Cycle{index}.Tap{i}"] = new WeakReference<object>(tap);
        }

        // SwipeGestureRecognizer on 1 control
        var swipeLabel = new Label { Text = "Swipe me", HeightRequest = 40 };
        var swipe = new SwipeGestureRecognizer { Direction = SwipeDirection.Right };
        swipe.Swiped += (_, _) => { };
        swipeLabel.GestureRecognizers.Add(swipe);
        layout.Children.Add(swipeLabel);
        trackedObjects[$"Cycle{index}.SwipeLabel"] = new WeakReference<object>(swipeLabel);
        trackedObjects[$"Cycle{index}.Swipe"] = new WeakReference<object>(swipe);

        scrollView.Content = layout;
        page.Content = scrollView;

        return page;
    }

    private class CompoundViewModel : BindableObject
    {
        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(CompoundViewModel), string.Empty);

        public static readonly BindableProperty CountProperty =
            BindableProperty.Create(nameof(Count), typeof(int), typeof(CompoundViewModel), 0);

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
