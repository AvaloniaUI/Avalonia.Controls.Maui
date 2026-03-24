using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

[BenchmarkTest("ShellRouteNavigationPerformance", Description = "Measures Shell GoToAsync latency for route resolution, page creation, and return navigation")]
public class ShellRouteNavigationPerformanceBenchmark : BenchmarkTestPage
{
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        const int cycles = 4;

        var routePrefix = $"benchmark_route_{Guid.NewGuid():N}";
        var detailRoute = $"{routePrefix}_detail";
        var lessonRoute = $"{routePrefix}_lesson";
        var quizRoute = $"{routePrefix}_quiz";

        Routing.RegisterRoute(detailRoute, typeof(RouteDetailPage));
        Routing.RegisterRoute(lessonRoute, typeof(RouteLessonPage));
        Routing.RegisterRoute(quizRoute, typeof(RouteQuizPage));

        var shell = CreateShell(routePrefix);
        var forwardSamples = new List<double>(cycles * 3);
        var returnSamples = new List<double>(cycles * 3);
        var metrics = new Dictionary<string, object>();

        try
        {
            window.Page = shell;
            await BenchmarkUiHelpers.WaitUntilAsync(
                () => shell.Handler != null && shell.Navigation.NavigationStack.Count == 1,
                cancellationToken,
                timeoutMs: 5000,
                pollDelayMs: 20);

            for (int cycle = 0; cycle < cycles; cycle++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                forwardSamples.Add(await MeasureNavigationAsync(shell, detailRoute, expectedDepth: 2, cancellationToken));
                forwardSamples.Add(await MeasureNavigationAsync(shell, lessonRoute, expectedDepth: 3, cancellationToken));
                forwardSamples.Add(await MeasureNavigationAsync(shell, quizRoute, expectedDepth: 4, cancellationToken));

                returnSamples.Add(await MeasureNavigationAsync(shell, "..", expectedDepth: 3, cancellationToken));
                returnSamples.Add(await MeasureNavigationAsync(shell, "..", expectedDepth: 2, cancellationToken));
                returnSamples.Add(await MeasureNavigationAsync(shell, "..", expectedDepth: 1, cancellationToken));
            }

            AddLatencyMetrics("Forward", forwardSamples, metrics);
            AddLatencyMetrics("Return", returnSamples, metrics);
            metrics["Cycles"] = cycles;
            metrics["Operations"] = forwardSamples.Count + returnSamples.Count;
            metrics["FinalStackDepth"] = shell.Navigation.NavigationStack.Count;

            logger.LogInformation(
                "Shell routes: forward avg={ForwardMs:F2}ms, p95={ForwardP95:F2}ms, return avg={ReturnMs:F2}ms, p95={ReturnP95:F2}ms",
                forwardSamples.Average(),
                BenchmarkUiHelpers.CalculatePercentile(forwardSamples, 0.95),
                returnSamples.Average(),
                BenchmarkUiHelpers.CalculatePercentile(returnSamples, 0.95));

            return BenchmarkResult.Pass(metrics);
        }
        finally
        {
            shell.Handler?.DisconnectHandler();
            window.Page = this;
            Content = new Label { Text = "Shell route performance benchmark complete" };
            BenchmarkUiHelpers.DisconnectElementTree(shell.CurrentPage);

            Routing.UnRegisterRoute(detailRoute);
            Routing.UnRegisterRoute(lessonRoute);
            Routing.UnRegisterRoute(quizRoute);

            await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 20);
        }
    }

    private static async Task<double> MeasureNavigationAsync(
        Shell shell,
        string route,
        int expectedDepth,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        await shell.GoToAsync(route, false);
        await BenchmarkUiHelpers.WaitUntilAsync(
            () => shell.Navigation.NavigationStack.Count == expectedDepth,
            cancellationToken,
            timeoutMs: 5000,
            pollDelayMs: 20);
        await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 10);
        stopwatch.Stop();

        return stopwatch.Elapsed.TotalMilliseconds;
    }

    private static Shell CreateShell(string routePrefix)
    {
        var shell = new Shell
        {
            FlyoutBehavior = FlyoutBehavior.Disabled,
        };

        var homeContent = new ShellContent
        {
            Title = "Home",
            Route = $"{routePrefix}_home_content",
            ContentTemplate = new DataTemplate(typeof(RouteHomePage)),
        };

        var section = new ShellSection
        {
            Title = "Home Section",
            Route = $"{routePrefix}_home_section",
        };
        section.Items.Add(homeContent);

        var item = new ShellItem
        {
            Title = "Home",
            Route = $"{routePrefix}_home_item",
        };
        item.Items.Add(section);

        shell.Items.Add(item);
        shell.CurrentItem = item;

        return shell;
    }

    private static void AddLatencyMetrics(string prefix, IReadOnlyList<double> samples, Dictionary<string, object> metrics)
    {
        metrics[$"{prefix}.AverageElapsedMs"] = samples.Count > 0 ? samples.Average() : 0;
        metrics[$"{prefix}.P95ElapsedMs"] = BenchmarkUiHelpers.CalculatePercentile(samples, 0.95);
        metrics[$"{prefix}.MaxElapsedMs"] = samples.Count > 0 ? samples.Max() : 0;
    }

    private sealed class RouteHomePage : ContentPage
    {
        public RouteHomePage()
        {
            Title = "Home";
            Content = CreatePageLayout("Home", "Route entry page");
        }
    }

    private sealed class RouteDetailPage : ContentPage
    {
        public RouteDetailPage()
        {
            Title = "Detail";
            Content = CreatePageLayout("Detail", "Route detail page");
        }
    }

    private sealed class RouteLessonPage : ContentPage
    {
        public RouteLessonPage()
        {
            Title = "Lesson";
            Content = CreatePageLayout("Lesson", "Route lesson page");
        }
    }

    private sealed class RouteQuizPage : ContentPage
    {
        public RouteQuizPage()
        {
            Title = "Quiz";
            Content = CreatePageLayout("Quiz", "Route quiz page");
        }
    }

    private static ScrollView CreatePageLayout(string title, string subtitle)
    {
        var layout = new VerticalStackLayout
        {
            Padding = new Thickness(18),
            Spacing = 12,
            Children =
            {
                new Label { Text = title, FontSize = 24, FontAttributes = FontAttributes.Bold },
                new Label { Text = subtitle, FontSize = 14, Opacity = 0.8 },
            },
        };

        for (int i = 0; i < 8; i++)
        {
            layout.Children.Add(new Border
            {
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
                Padding = new Thickness(12),
                Content = new VerticalStackLayout
                {
                    Spacing = 8,
                    Children =
                    {
                        new Label { Text = $"{title} section {i + 1}", FontSize = 17, FontAttributes = FontAttributes.Bold },
                        new Label { Text = "Route navigation benchmark content to create meaningful page creation work.", FontSize = 13 },
                        new ProgressBar { Progress = ((i + 1) * 11 % 100) / 100.0 },
                        new Button { Text = $"Action {i + 1}" },
                    },
                },
            });
        }

        return new ScrollView
        {
            Content = layout,
        };
    }
}
