using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests the pattern found in AlohaAI's LessonPage and QuizPage where pages subscribe to
/// ViewModel.PropertyChanged in the constructor but never unsubscribe. This creates a strong
/// reference from the ViewModel to the Page, preventing the Page from being GC'd if the
/// ViewModel outlives it.
/// </summary>
[BenchmarkTest("PropertyChangedSubscriptionLeak", Description = "Verifies pages with PropertyChanged subscriptions are collected after disconnect")]
public class PropertyChangedSubscriptionLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();
        const int cycles = 10;

        await CreateAndDestroySubscribedPages(window, trackedObjects, cycles, cancellationToken);

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

        int pageLeaks = leaked.Count(n => n.Contains("Page"));
        int handlerLeaks = leaked.Count(n => n.Contains("Handler"));
        int controlLeaks = leaked.Count(n => !n.Contains("Page") && !n.Contains("Handler"));

        var metrics = new Dictionary<string, object>
        {
            ["Cycles"] = cycles,
            ["TotalObjectsTracked"] = trackedObjects.Count,
            ["ObjectsLeaked"] = leaked.Count,
            ["PagesLeaked"] = pageLeaks,
            ["HandlersLeaked"] = handlerLeaks,
            ["ControlsLeaked"] = controlLeaks,
            ["LeakedObjects"] = leaked.Count > 0 ? string.Join(", ", leaked.Take(20)) : "none",
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        if (leaked.Count > 0)
        {
            var leakedNames = string.Join(", ", leaked);
            logger.LogWarning("PropertyChanged subscription leak detected: {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Objects leaked: {leakedNames}", metrics);
        }

        if (memoryDelta.WorkingSetDelta > 50 * 1024 * 1024)
        {
            return BenchmarkResult.Warn(
                $"Native memory growth {memoryDelta.WorkingSetDelta / (1024.0 * 1024):F1} MB exceeds 50 MB threshold",
                metrics);
        }

        logger.LogInformation(
            "All {Count} objects collected after {Cycles} cycles",
            trackedObjects.Count,
            cycles);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task CreateAndDestroySubscribedPages(
        Window window,
        Dictionary<string, WeakReference<object>> trackedObjects,
        int cycles,
        CancellationToken cancellationToken)
    {
        for (int i = 0; i < cycles; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Create a page that subscribes to VM PropertyChanged (like AlohaAI LessonPage)
            CreateSubscribedPage(window, trackedObjects, i);

            // Allow handlers to connect
            await Task.Delay(30, cancellationToken);

            // Simulate navigating away - tear down the page
            TearDownPage(window, trackedObjects, i);

            await Task.Delay(20, cancellationToken);
        }

        // Restore test page
        window.Page = this;
        Content = new Label { Text = "PropertyChanged subscription test complete" };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateSubscribedPage(
        Window window,
        Dictionary<string, WeakReference<object>> trackedObjects,
        int index)
    {
        var viewModel = new LeakTestViewModel();

        var page = new ContentPage { Title = $"Lesson {index}" };
        page.BindingContext = viewModel;
        trackedObjects[$"Cycle{index}.Page"] = new WeakReference<object>(page);

        // This is the problematic pattern from AlohaAI:
        // Subscribe to PropertyChanged in constructor, never unsubscribe
        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(LeakTestViewModel.Content))
            {
                // Simulates LessonPage.OnViewModelPropertyChanged which accesses
                // page content and creates dynamic children
                if (page.Content is Layout layout)
                {
                    layout.Children.Clear();
                    layout.Children.Add(new Label { Text = viewModel.Content });
                }
            }
        };

        // Build content similar to LessonPage
        var scrollView = new ScrollView();
        var mainLayout = new VerticalStackLayout { Spacing = 10, Padding = new Thickness(16) };

        // Title
        var titleLabel = new Label { Text = $"Lesson {index}" };
        titleLabel.SetBinding(Label.TextProperty, nameof(LeakTestViewModel.Title));
        mainLayout.Children.Add(titleLabel);
        trackedObjects[$"Cycle{index}.TitleLabel"] = new WeakReference<object>(titleLabel);

        // Content area (dynamically populated like LessonPage's ContentArea)
        var contentArea = new VerticalStackLayout();
        mainLayout.Children.Add(contentArea);
        trackedObjects[$"Cycle{index}.ContentArea"] = new WeakReference<object>(contentArea);

        // Keywords area (FlexLayout populated dynamically like LessonPage's KeywordsArea)
        var keywordsArea = new FlexLayout { Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap };
        for (int k = 0; k < 5; k++)
        {
            var chip = new Border
            {
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 },
                Padding = new Thickness(12, 6),
                Content = new Label { Text = $"Keyword {k}", FontSize = 12 },
            };
            keywordsArea.Children.Add(chip);
        }

        mainLayout.Children.Add(keywordsArea);
        trackedObjects[$"Cycle{index}.KeywordsArea"] = new WeakReference<object>(keywordsArea);

        // Progress bar
        var progressBar = new ProgressBar { Progress = 0.5 };
        mainLayout.Children.Add(progressBar);
        trackedObjects[$"Cycle{index}.ProgressBar"] = new WeakReference<object>(progressBar);

        // Action button
        var actionButton = new Button { Text = "Complete Lesson" };
        actionButton.Clicked += (_, _) => { };
        mainLayout.Children.Add(actionButton);
        trackedObjects[$"Cycle{index}.ActionButton"] = new WeakReference<object>(actionButton);

        scrollView.Content = mainLayout;
        page.Content = scrollView;

        // Trigger PropertyChanged to simulate loading data (creates dynamic children)
        viewModel.Title = $"Lesson {index}: Introduction";
        viewModel.Content = $"This is the content for lesson {index}";

        window.Page = page;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void TearDownPage(
        Window window,
        Dictionary<string, WeakReference<object>> trackedObjects,
        int index)
    {
        if (window.Page is not ContentPage page)
            return;

        // Track handler before disconnect
        if (page.Handler is object handler)
        {
            trackedObjects[$"Cycle{index}.PageHandler"] = new WeakReference<object>(handler);
        }

        // Disconnect page content
        if (page.Content is ScrollView scrollView)
        {
            if (scrollView.Content is Layout layout)
            {
                DisconnectLayout(layout);
            }

            scrollView.Handler?.DisconnectHandler();
        }

        // Note: We intentionally do NOT unsubscribe PropertyChanged
        // This tests whether the framework properly handles this leak pattern
        page.Handler?.DisconnectHandler();
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

    /// <summary>
    /// Simple ViewModel that matches the AlohaAI LessonViewModel pattern.
    /// Implements INotifyPropertyChanged - the event subscription is the leak vector.
    /// </summary>
    private sealed class LeakTestViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private string _title = string.Empty;
        private string _content = string.Empty;

        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
                }
            }
        }

        public string Content
        {
            get => _content;
            set
            {
                if (_content != value)
                {
                    _content = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Content)));
                }
            }
        }
    }
}
