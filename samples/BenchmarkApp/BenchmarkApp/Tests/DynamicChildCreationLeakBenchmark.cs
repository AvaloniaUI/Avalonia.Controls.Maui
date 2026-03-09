using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests the AlohaAI LessonPage pattern of dynamically creating UI elements in code-behind.
/// LessonPage.OnViewModelPropertyChanged clears ContentArea.Children and adds new rendered
/// markdown elements, and PopulateKeywords() creates Border chips in a FlexLayout.
/// This pattern of repeated clear-and-recreate can leak if handlers aren't properly cleaned up.
/// </summary>
[BenchmarkTest("DynamicChildCreationLeak", Description = "Verifies dynamically created/cleared children don't leak (AlohaAI LessonPage pattern)")]
public class DynamicChildCreationLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();
        const int cycles = 10;

        await CreateAndClearDynamicContent(trackedObjects, cycles, cancellationToken);

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

        int markdownLeaks = leaked.Count(n => n.Contains("Markdown"));
        int chipLeaks = leaked.Count(n => n.Contains("Chip"));

        var metrics = new Dictionary<string, object>
        {
            ["Cycles"] = cycles,
            ["TotalObjectsTracked"] = totalTracked,
            ["ObjectsLeaked"] = leaked.Count,
            ["LeakRate"] = leakRate,
            ["MarkdownElementLeaks"] = markdownLeaks,
            ["KeywordChipLeaks"] = chipLeaks,
            ["LeakedObjects"] = leaked.Count > 0 ? string.Join(", ", leaked.Take(20)) : "none",
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        if (leakRate > 0.05)
        {
            logger.LogWarning(
                "Dynamic child creation leak detected: {Leaked}/{Total} objects survived (rate: {Rate:P1})",
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
            "Dynamic children: {Leaked}/{Total} objects survived after {Cycles} cycles (rate: {Rate:P1})",
            leaked.Count, totalTracked, cycles, leakRate);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task CreateAndClearDynamicContent(
        Dictionary<string, WeakReference<object>> trackedObjects,
        int cycles,
        CancellationToken cancellationToken)
    {
        // Set up a page structure matching LessonPage:
        // ScrollView > VerticalStackLayout > [ContentArea, KeywordsArea, ProgressBar, Button]
        var scrollView = new ScrollView();
        var mainLayout = new VerticalStackLayout { Spacing = 10, Padding = new Thickness(16) };

        var titleLabel = new Label { Text = "Lesson Title", FontSize = 24, FontAttributes = FontAttributes.Bold };
        mainLayout.Children.Add(titleLabel);

        // ContentArea - where markdown-rendered content goes (cleared and repopulated)
        var contentArea = new VerticalStackLayout { Spacing = 8 };
        mainLayout.Children.Add(contentArea);

        // KeywordsArea - where keyword chips go (cleared and repopulated)
        var keywordsArea = new FlexLayout
        {
            Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap,
            JustifyContent = Microsoft.Maui.Layouts.FlexJustify.Start,
        };
        mainLayout.Children.Add(keywordsArea);

        var progressBar = new ProgressBar { Progress = 0.5 };
        mainLayout.Children.Add(progressBar);

        var completeButton = new Button { Text = "Complete Lesson" };
        completeButton.Clicked += (_, _) => { };
        mainLayout.Children.Add(completeButton);

        scrollView.Content = mainLayout;
        Content = scrollView;

        // Allow initial rendering
        await Task.Delay(50, cancellationToken);

        for (int cycle = 0; cycle < cycles; cycle++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Simulate LessonPage markdown content loading
            PopulateMarkdownContent(contentArea, trackedObjects, cycle);
            await Task.Delay(20, cancellationToken);

            // Simulate LessonPage keyword chips loading
            PopulateKeywordChips(keywordsArea, trackedObjects, cycle);
            await Task.Delay(20, cancellationToken);

            // Track all children before clearing
            TrackAreaChildren(trackedObjects, contentArea, $"Cycle{cycle}.Markdown");
            TrackAreaChildren(trackedObjects, keywordsArea, $"Cycle{cycle}.Chip");

            // Clear and repopulate (simulating navigation to a different lesson)
            contentArea.Children.Clear();
            keywordsArea.Children.Clear();

            await Task.Delay(20, cancellationToken);
        }

        // Final teardown
        DisconnectLayout(mainLayout);
        Content = new Label { Text = "Dynamic child creation test complete" };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void PopulateMarkdownContent(
        VerticalStackLayout contentArea,
        Dictionary<string, WeakReference<object>> trackedObjects,
        int cycle)
    {
        // Simulates MarkdownRenderer.Render() output from AlohaAI LessonPage
        // Creates a hierarchy of Labels, Borders, and nested layouts

        // H2 heading
        contentArea.Children.Add(new Label
        {
            Text = $"Section {cycle}: Understanding Neural Networks",
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            Margin = new Thickness(0, 8, 0, 4),
        });

        // Paragraph text
        contentArea.Children.Add(new Label
        {
            Text = "Neural networks are computing systems inspired by biological neural networks. " +
                   "They consist of layers of interconnected nodes that process information.",
            FontSize = 15,
            LineHeight = 1.4,
        });

        // Code block (Border with Label)
        var codeBlock = new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 8 },
            BackgroundColor = Color.FromArgb("#1E1E2E"),
            Stroke = Color.FromArgb("#30FFFFFF"),
            Padding = new Thickness(16, 12),
            Content = new Label
            {
                Text = "model = Sequential([\n  Dense(128, activation='relu'),\n  Dense(10, activation='softmax')\n])",
                FontSize = 13,
                FontFamily = "monospace",
            },
        };
        contentArea.Children.Add(codeBlock);

        // Another paragraph
        contentArea.Children.Add(new Label
        {
            Text = "The key components of a neural network include:",
            FontSize = 15,
            LineHeight = 1.4,
        });

        // Bullet points (indented labels)
        string[] bullets = ["Input layer", "Hidden layers", "Output layer", "Activation functions"];
        foreach (var bullet in bullets)
        {
            contentArea.Children.Add(new Label
            {
                Text = $"  • {bullet}",
                FontSize = 15,
                Margin = new Thickness(8, 2, 0, 2),
            });
        }

        // Callout box
        var callout = new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
            BackgroundColor = Color.FromArgb("#1A5B8FD4"),
            Stroke = Color.FromArgb("#305B8FD4"),
            Padding = new Thickness(14, 10),
            Content = new Label
            {
                Text = "💡 Tip: Start with simple architectures before exploring complex ones.",
                FontSize = 14,
            },
        };
        contentArea.Children.Add(callout);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void PopulateKeywordChips(
        FlexLayout keywordsArea,
        Dictionary<string, WeakReference<object>> trackedObjects,
        int cycle)
    {
        // Matches AlohaAI LessonPage.PopulateKeywords() pattern exactly
        string[] keywords = ["Neural Network", "Deep Learning", "Backpropagation", "Gradient Descent", "Activation"];
        string[] colors = ["#5B8FD4", "#7B68AE", "#E88BBF", "#7EB4F0", "#4CAF50"];

        for (int i = 0; i < keywords.Length; i++)
        {
            var color = Color.FromArgb(colors[i % colors.Length]);
            var chip = new Border
            {
                BackgroundColor = color.WithAlpha(0.15f),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 },
                Stroke = color.WithAlpha(0.3f),
                StrokeThickness = 1,
                Padding = new Thickness(12, 6),
                Margin = new Thickness(0, 0, 6, 6),
                Content = new Label
                {
                    Text = keywords[i],
                    FontSize = 12,
                    TextColor = color,
                    FontAttributes = FontAttributes.Bold,
                },
            };
            keywordsArea.Children.Add(chip);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void TrackAreaChildren(
        Dictionary<string, WeakReference<object>> trackedObjects,
        Layout area,
        string prefix)
    {
        int childIndex = 0;
        foreach (var child in area.Children)
        {
            if (child is VisualElement ve)
            {
                trackedObjects[$"{prefix}[{childIndex}]"] = new WeakReference<object>(ve);

                if (ve.Handler is object handler)
                {
                    trackedObjects[$"{prefix}[{childIndex}].Handler"] = new WeakReference<object>(handler);
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
