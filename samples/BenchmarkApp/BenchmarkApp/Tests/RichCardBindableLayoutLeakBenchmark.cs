using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests the AlohaAI HomePage pattern of complex cards rendered via BindableLayout.
/// Each card includes Border with rounded corners, gradient stroke, Shadow, TapGestureRecognizer,
/// nested Grid, BoxView accent bar, multiple Labels, tag chips, and a progress bar.
/// This is a much more complex DataTemplate than the basic BindableLayoutLeak test.
/// </summary>
[BenchmarkTest("RichCardBindableLayoutLeak", Description = "Verifies complex AlohaAI-style card items in BindableLayout are collected")]
public class RichCardBindableLayoutLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();
        const int repopulationCycles = 5;
        const int itemsPerCycle = 6;

        await BuildAndRepopulateRichCards(trackedObjects, repopulationCycles, itemsPerCycle, cancellationToken);

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

        // Categorize leaks
        int borderLeaks = leaked.Count(n => n.Contains("Border") || n.Contains("Card"));
        int gestureLeaks = leaked.Count(n => n.Contains("Gesture") || n.Contains("Tap"));
        int shadowLeaks = leaked.Count(n => n.Contains("Shadow"));
        int gradientLeaks = leaked.Count(n => n.Contains("Gradient"));

        var metrics = new Dictionary<string, object>
        {
            ["RepopulationCycles"] = repopulationCycles,
            ["ItemsPerCycle"] = itemsPerCycle,
            ["TotalObjectsTracked"] = totalTracked,
            ["ObjectsLeaked"] = leaked.Count,
            ["LeakRate"] = leakRate,
            ["BorderLeaks"] = borderLeaks,
            ["GestureLeaks"] = gestureLeaks,
            ["ShadowLeaks"] = shadowLeaks,
            ["GradientLeaks"] = gradientLeaks,
            ["LeakedObjects"] = leaked.Count > 0 ? string.Join(", ", leaked.Take(20)) : "none",
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        if (leakRate > 0.05)
        {
            logger.LogWarning(
                "Rich card BindableLayout leak detected: {Leaked}/{Total} objects survived (rate: {Rate:P1})",
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
            "Rich cards: {Leaked}/{Total} objects survived after {Cycles} cycles (rate: {Rate:P1})",
            leaked.Count, totalTracked, repopulationCycles, leakRate);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task BuildAndRepopulateRichCards(
        Dictionary<string, WeakReference<object>> trackedObjects,
        int cycles,
        int itemsPerCycle,
        CancellationToken cancellationToken)
    {
        var scrollView = new ScrollView();
        var outerLayout = new VerticalStackLayout();
        scrollView.Content = outerLayout;
        Content = scrollView;

        var bindableStack = new VerticalStackLayout { Spacing = 14, Padding = new Thickness(16) };
        outerLayout.Children.Add(bindableStack);

        var collection = new ObservableCollection<CardItemModel>();

        // Set up BindableLayout with a rich DataTemplate matching AlohaAI cards
        BindableLayout.SetItemTemplate(bindableStack, new DataTemplate(() =>
        {
            return CreateRichCardTemplate();
        }));

        BindableLayout.SetItemsSource(bindableStack, collection);

        for (int cycle = 0; cycle < cycles; cycle++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Populate with items
            for (int i = 0; i < itemsPerCycle; i++)
            {
                collection.Add(new CardItemModel
                {
                    Title = $"Path {i}: AI Fundamentals",
                    ProgressText = $"Module {i}/{itemsPerCycle} complete",
                    Progress = (double)i / itemsPerCycle,
                    LevelTag = i % 2 == 0 ? "Beginner" : "Advanced",
                });
            }

            // Allow handlers to connect
            await Task.Delay(30, cancellationToken);

            // Track the generated children before clearing
            TrackRichCardChildren(trackedObjects, bindableStack, cycle);

            // Clear collection (simulates OnAppearing data refresh)
            collection.Clear();

            // Allow cleanup
            await Task.Delay(30, cancellationToken);
        }

        // Final teardown
        BindableLayout.SetItemsSource(bindableStack, null);
        DisconnectLayout(outerLayout);
        Content = new Label { Text = "Rich card BindableLayout test complete" };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static View CreateRichCardTemplate()
    {
        // Matches AlohaAI HomePage card structure exactly
        var card = new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 22 },
            StrokeThickness = 1,
            // Gradient stroke (glassmorphism effect from AlohaAI)
            Stroke = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb("#30FFFFFF"), 0.0f),
                    new GradientStop(Color.FromArgb("#10FFFFFF"), 0.5f),
                    new GradientStop(Color.FromArgb("#05FFFFFF"), 1.0f),
                },
            },
            Shadow = new Shadow
            {
                Brush = Colors.Black,
                Offset = new Point(0, 6),
                Radius = 20,
                Opacity = 0.6f,
            },
        };

        // TapGestureRecognizer (like AlohaAI's NavigateCommand binding)
        card.GestureRecognizers.Add(new TapGestureRecognizer());

        var cardGrid = new Grid();

        // Left accent bar (like AlohaAI's colored BoxView)
        var accentBar = new BoxView
        {
            WidthRequest = 3,
            BackgroundColor = Color.FromArgb("#5B8FD4"),
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Fill,
            Margin = new Thickness(0, 14, 0, 14),
            Shadow = new Shadow
            {
                Brush = Color.FromArgb("#5B8FD4"),
                Offset = new Point(2, 0),
                Radius = 8,
                Opacity = 0.4f,
            },
        };
        cardGrid.Children.Add(accentBar);

        // Content overlay
        var contentLayout = new VerticalStackLayout
        {
            Spacing = 8,
            Padding = new Thickness(18, 14, 16, 14),
        };

        // Title
        var titleLabel = new Label { FontSize = 19, FontAttributes = FontAttributes.Bold };
        titleLabel.SetBinding(Label.TextProperty, nameof(CardItemModel.Title));
        contentLayout.Children.Add(titleLabel);

        // Progress text
        var progressLabel = new Label { FontSize = 13 };
        progressLabel.SetBinding(Label.TextProperty, nameof(CardItemModel.ProgressText));
        contentLayout.Children.Add(progressLabel);

        // Tag chips (HorizontalStackLayout with Borders)
        var tagLayout = new HorizontalStackLayout { Spacing = 6 };
        tagLayout.Children.Add(new Border
        {
            BackgroundColor = Color.FromArgb("#305B8FD4"),
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 },
            Stroke = Colors.Transparent,
            Padding = new Thickness(10, 4),
            Content = new Label { Text = "Course", FontSize = 10, FontAttributes = FontAttributes.Bold },
        });

        var levelChip = new Border
        {
            BackgroundColor = Color.FromArgb("#305B8FD4"),
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 },
            Stroke = Colors.Transparent,
            Padding = new Thickness(10, 4),
        };
        var levelLabel = new Label { FontSize = 10, FontAttributes = FontAttributes.Bold };
        levelLabel.SetBinding(Label.TextProperty, nameof(CardItemModel.LevelTag));
        levelChip.Content = levelLabel;
        tagLayout.Children.Add(levelChip);
        contentLayout.Children.Add(tagLayout);

        // Progress bar (Grid with two Borders)
        var progressGrid = new Grid { HeightRequest = 5 };
        progressGrid.Children.Add(new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 2.5 },
            Stroke = Colors.Transparent,
            BackgroundColor = Color.FromArgb("#1AFFFFFF"),
        });
        progressGrid.Children.Add(new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 2.5 },
            Stroke = Colors.Transparent,
            HorizontalOptions = LayoutOptions.Start,
            WidthRequest = 150,
            BackgroundColor = Color.FromArgb("#5B8FD4"),
            Shadow = new Shadow
            {
                Brush = Color.FromArgb("#5B8FD4"),
                Offset = new Point(0, 0),
                Radius = 6,
                Opacity = 0.6f,
            },
        });
        contentLayout.Children.Add(progressGrid);

        cardGrid.Children.Add(contentLayout);
        card.Content = cardGrid;

        return card;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void TrackRichCardChildren(
        Dictionary<string, WeakReference<object>> trackedObjects,
        VerticalStackLayout bindableStack,
        int cycle)
    {
        int childIndex = 0;
        foreach (var child in bindableStack.Children)
        {
            if (child is Border border)
            {
                trackedObjects[$"Cycle{cycle}.Card[{childIndex}]"] = new WeakReference<object>(border);

                if (border.Shadow is Shadow shadow)
                {
                    trackedObjects[$"Cycle{cycle}.Shadow[{childIndex}]"] = new WeakReference<object>(shadow);
                }

                if (border.Stroke is LinearGradientBrush gradient)
                {
                    trackedObjects[$"Cycle{cycle}.GradientStroke[{childIndex}]"] = new WeakReference<object>(gradient);
                }

                foreach (var gesture in border.GestureRecognizers)
                {
                    trackedObjects[$"Cycle{cycle}.TapGesture[{childIndex}]"] = new WeakReference<object>(gesture);
                }

                if (border.Handler is object handler)
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

            if (child is View view)
            {
                view.GestureRecognizers.Clear();
                view.Handler?.DisconnectHandler();
            }
        }

        layout.Children.Clear();
        layout.Handler?.DisconnectHandler();
    }

    /// <summary>
    /// Model matching AlohaAI's HomePathItem structure.
    /// </summary>
    private sealed class CardItemModel
    {
        public string Title { get; set; } = string.Empty;
        public string ProgressText { get; set; } = string.Empty;
        public double Progress { get; set; }
        public string LevelTag { get; set; } = string.Empty;
    }
}
