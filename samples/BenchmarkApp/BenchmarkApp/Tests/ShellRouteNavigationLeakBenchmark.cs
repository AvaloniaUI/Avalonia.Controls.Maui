using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests AlohaAI's deep-link route navigation pattern where pages are pushed onto the
/// Shell navigation stack via Shell.GoToAsync (e.g. Home → PathDetail → Lesson → Quiz).
/// Each pushed page has complex content. This tests whether pages are properly collected
/// when popped from the navigation stack.
/// </summary>
[BenchmarkTest("ShellRouteNavigationLeak", Description = "Verifies Shell route push/pop navigation doesn't leak pages")]
public class ShellRouteNavigationLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();
        const int cycles = 5;

        await SimulateRouteNavigation(window, trackedObjects, cycles, logger, cancellationToken);

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

        int detailPageLeaks = leaked.Count(n => n.Contains("Detail"));
        int lessonPageLeaks = leaked.Count(n => n.Contains("Lesson"));
        int quizPageLeaks = leaked.Count(n => n.Contains("Quiz"));

        var metrics = new Dictionary<string, object>
        {
            ["NavigationCycles"] = cycles,
            ["TotalObjectsTracked"] = trackedObjects.Count,
            ["ObjectsLeaked"] = leaked.Count,
            ["DetailPageLeaks"] = detailPageLeaks,
            ["LessonPageLeaks"] = lessonPageLeaks,
            ["QuizPageLeaks"] = quizPageLeaks,
            ["LeakedObjects"] = leaked.Count > 0 ? string.Join(", ", leaked) : "none",
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        if (leaked.Count > 0)
        {
            var leakedNames = string.Join(", ", leaked);
            logger.LogWarning("Shell route navigation leak detected: {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Objects leaked: {leakedNames}", metrics);
        }

        if (CreateNativeMemoryFailure(memoryDelta, logger, metrics) is { } nativeMemoryFailure)
            return nativeMemoryFailure;

        logger.LogInformation(
            "All {Count} objects collected after {Cycles} route navigation cycles",
            trackedObjects.Count,
            cycles);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task SimulateRouteNavigation(
        Window window,
        Dictionary<string, WeakReference<object>> trackedObjects,
        int cycles,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        // Use a NavigationPage to simulate the Shell push/pop navigation stack
        // Shell.GoToAsync internally pushes pages onto a NavigationPage
        var rootPage = new ContentPage
        {
            Title = "Home",
            Content = new VerticalStackLayout
            {
                Children = { new Label { Text = "Home Page" } },
            },
        };

        var navigationPage = new NavigationPage(rootPage);
        window.Page = navigationPage;
        await Task.Delay(50, cancellationToken);

        for (int cycle = 0; cycle < cycles; cycle++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Simulate: Home → PathDetail → Lesson → Quiz → pop back to Home
            // This matches AlohaAI's navigation flow

            // Push PathDetail page
            var detailPage = CreatePathDetailPage(trackedObjects, cycle);
            await navigationPage.PushAsync(detailPage);
            await Task.Delay(30, cancellationToken);

            // Push Lesson page
            var lessonPage = CreateLessonPage(trackedObjects, cycle);
            await navigationPage.PushAsync(lessonPage);
            await Task.Delay(30, cancellationToken);

            // Push Quiz page
            var quizPage = CreateQuizPage(trackedObjects, cycle);
            await navigationPage.PushAsync(quizPage);
            await Task.Delay(30, cancellationToken);

            // Pop back through the stack (simulating user going back)
            await navigationPage.PopAsync(); // Pop Quiz
            await Task.Delay(20, cancellationToken);

            await navigationPage.PopAsync(); // Pop Lesson
            await Task.Delay(20, cancellationToken);

            await navigationPage.PopAsync(); // Pop PathDetail
            await Task.Delay(20, cancellationToken);

            logger.LogInformation("Completed route navigation cycle {Cycle}/{Total}", cycle + 1, cycles);
        }

        // Tear down navigation page
        if (navigationPage.Handler is object navHandler)
        {
            trackedObjects["NavigationPage.Handler"] = new WeakReference<object>(navHandler);
        }

        window.Page = this;
        Content = new Label { Text = "Shell route navigation test complete" };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ContentPage CreatePathDetailPage(
        Dictionary<string, WeakReference<object>> trackedObjects,
        int cycle)
    {
        // Matches AlohaAI PathDetailPage structure:
        // Header image with gradient overlay, module list with lesson items
        var page = new ContentPage { Title = $"Path Detail {cycle}" };
        trackedObjects[$"Cycle{cycle}.DetailPage"] = new WeakReference<object>(page);

        var scrollView = new ScrollView();
        var layout = new VerticalStackLayout { Spacing = 10, Padding = new Thickness(16) };

        // Header area with gradient overlay
        var headerGrid = new Grid { HeightRequest = 200 };
        headerGrid.Children.Add(new BoxView
        {
            BackgroundColor = Color.FromArgb("#2A3D5E"),
        });
        headerGrid.Children.Add(new BoxView
        {
            Background = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Colors.Transparent, 0.0f),
                    new GradientStop(Color.FromArgb("#FF1A1A2E"), 1.0f),
                },
            },
        });
        layout.Children.Add(headerGrid);
        trackedObjects[$"Cycle{cycle}.DetailHeader"] = new WeakReference<object>(headerGrid);

        // Module list (3 modules with 3 lessons each)
        for (int m = 0; m < 3; m++)
        {
            var moduleLayout = new VerticalStackLayout { Spacing = 6, Padding = new Thickness(0, 8) };

            moduleLayout.Children.Add(new Label
            {
                Text = $"Module {m + 1}: Fundamentals",
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
            });

            for (int l = 0; l < 3; l++)
            {
                var lessonBorder = new Border
                {
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
                    Padding = new Thickness(12),
                    Content = new HorizontalStackLayout
                    {
                        Spacing = 10,
                        Children =
                        {
                            new Label { Text = "📄", FontSize = 18 },
                            new Label { Text = $"Lesson {l + 1}: Topic {l}", FontSize = 14 },
                        },
                    },
                };
                lessonBorder.GestureRecognizers.Add(new TapGestureRecognizer());
                moduleLayout.Children.Add(lessonBorder);
            }

            // Quiz button
            var quizButton = new Button
            {
                Text = $"Take Module {m + 1} Quiz",
                BackgroundColor = Color.FromArgb("#5B8FD4"),
            };
            quizButton.Clicked += (_, _) => { };
            moduleLayout.Children.Add(quizButton);

            layout.Children.Add(moduleLayout);
        }

        scrollView.Content = layout;
        page.Content = scrollView;
        trackedObjects[$"Cycle{cycle}.DetailScrollView"] = new WeakReference<object>(scrollView);
        return page;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ContentPage CreateLessonPage(
        Dictionary<string, WeakReference<object>> trackedObjects,
        int cycle)
    {
        // Matches AlohaAI LessonPage structure:
        // Title, content area, keywords, progress bar, action buttons
        var page = new ContentPage { Title = $"Lesson {cycle}" };
        trackedObjects[$"Cycle{cycle}.LessonPage"] = new WeakReference<object>(page);

        var scrollView = new ScrollView();
        var layout = new VerticalStackLayout { Spacing = 10, Padding = new Thickness(16) };

        // Title
        layout.Children.Add(new Label
        {
            Text = $"Introduction to Neural Networks",
            FontSize = 24,
            FontAttributes = FontAttributes.Bold,
        });

        // Content area (rendered markdown)
        var contentArea = new VerticalStackLayout { Spacing = 8 };
        for (int p = 0; p < 5; p++)
        {
            contentArea.Children.Add(new Label
            {
                Text = $"Paragraph {p}: Some lesson content explaining AI concepts in detail.",
                FontSize = 15,
                LineHeight = 1.4,
            });
        }

        // Code block
        contentArea.Children.Add(new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 8 },
            BackgroundColor = Color.FromArgb("#1E1E2E"),
            Padding = new Thickness(16, 12),
            Content = new Label { Text = "model.fit(X_train, y_train, epochs=10)", FontSize = 13 },
        });
        layout.Children.Add(contentArea);

        // Keywords
        var keywordsLayout = new FlexLayout { Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap };
        string[] keywords = ["Neural Network", "Deep Learning", "Backprop", "SGD"];
        foreach (var keyword in keywords)
        {
            keywordsLayout.Children.Add(new Border
            {
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 },
                Padding = new Thickness(12, 6),
                Margin = new Thickness(0, 0, 6, 6),
                Content = new Label { Text = keyword, FontSize = 12 },
            });
        }

        layout.Children.Add(keywordsLayout);

        // Progress
        layout.Children.Add(new ProgressBar { Progress = 0.75 });

        // Action card
        var actionButton = new Button { Text = "Mark as Complete" };
        actionButton.Clicked += (_, _) => { };
        layout.Children.Add(actionButton);

        var nextButton = new Button { Text = "Next Lesson →" };
        nextButton.Clicked += (_, _) => { };
        layout.Children.Add(nextButton);

        scrollView.Content = layout;
        page.Content = scrollView;
        trackedObjects[$"Cycle{cycle}.LessonScrollView"] = new WeakReference<object>(scrollView);
        trackedObjects[$"Cycle{cycle}.LessonContentArea"] = new WeakReference<object>(contentArea);
        return page;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ContentPage CreateQuizPage(
        Dictionary<string, WeakReference<object>> trackedObjects,
        int cycle)
    {
        // Matches AlohaAI QuizPage structure:
        // Question display, options grid, explanation, results
        var page = new ContentPage { Title = $"Quiz {cycle}" };
        trackedObjects[$"Cycle{cycle}.QuizPage"] = new WeakReference<object>(page);

        var layout = new VerticalStackLayout { Spacing = 14, Padding = new Thickness(16) };

        // Question header
        layout.Children.Add(new Label
        {
            Text = "Question 1 of 5",
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
        });

        // Question text
        layout.Children.Add(new Label
        {
            Text = "What is the primary purpose of an activation function in a neural network?",
            FontSize = 18,
        });

        // Answer options (4 borders with tap gestures)
        string[] options =
        [
            "A) To store training data",
            "B) To introduce non-linearity",
            "C) To reduce the number of parameters",
            "D) To normalize input data",
        ];

        for (int o = 0; o < options.Length; o++)
        {
            var optionBorder = new Border
            {
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 14 },
                Padding = new Thickness(16, 12),
                Margin = new Thickness(0, 4),
                Content = new Label { Text = options[o], FontSize = 15 },
            };
            optionBorder.GestureRecognizers.Add(new TapGestureRecognizer());
            layout.Children.Add(optionBorder);
            trackedObjects[$"Cycle{cycle}.QuizOption[{o}]"] = new WeakReference<object>(optionBorder);
        }

        // Explanation area (hidden initially, shown after answer)
        var explanation = new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
            Padding = new Thickness(14),
            BackgroundColor = Color.FromArgb("#1A4CAF50"),
            Content = new Label
            {
                Text = "Correct! Activation functions introduce non-linearity, allowing neural networks to learn complex patterns.",
                FontSize = 14,
            },
        };
        layout.Children.Add(explanation);

        // Navigation buttons
        var nextButton = new Button { Text = "Next Question" };
        nextButton.Clicked += (_, _) => { };
        layout.Children.Add(nextButton);

        page.Content = layout;
        return page;
    }
}
