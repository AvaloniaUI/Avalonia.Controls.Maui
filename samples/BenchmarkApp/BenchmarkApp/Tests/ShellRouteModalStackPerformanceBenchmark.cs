using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

[BenchmarkTest("ShellRouteModalStackPerformance", Description = "Measures Shell route navigation combined with modal push/pop on the active routed page")]
public class ShellRouteModalStackPerformanceBenchmark : BenchmarkTestPage
{
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        const int cycles = 4;

        var routePrefix = $"benchmark_route_modal_{Guid.NewGuid():N}";
        var detailRoute = $"{routePrefix}_detail";
        var lessonRoute = $"{routePrefix}_lesson";

        Routing.RegisterRoute(detailRoute, typeof(RouteDetailPage));
        Routing.RegisterRoute(lessonRoute, typeof(RouteLessonPage));

        var shell = CreateShell(routePrefix);
        var routeForwardSamples = new List<double>(cycles * 2);
        var routeReturnSamples = new List<double>(cycles * 2);
        var modalPushSamples = new List<double>(cycles * 2);
        var modalPopSamples = new List<double>(cycles * 2);
        var metrics = new Dictionary<string, object>();
        var memoryBefore = MemorySnapshot.Capture(forceGC: true);

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

                routeForwardSamples.Add(await MeasureRouteAsync(shell, detailRoute, expectedDepth: 2, cancellationToken));
                modalPushSamples.Add(await MeasureModalPushAsync(shell, cycle * 2, cancellationToken));
                modalPopSamples.Add(await MeasureModalPopAsync(shell, cancellationToken));

                routeForwardSamples.Add(await MeasureRouteAsync(shell, lessonRoute, expectedDepth: 3, cancellationToken));
                modalPushSamples.Add(await MeasureModalPushAsync(shell, (cycle * 2) + 1, cancellationToken));
                modalPopSamples.Add(await MeasureModalPopAsync(shell, cancellationToken));

                routeReturnSamples.Add(await MeasureRouteAsync(shell, "..", expectedDepth: 2, cancellationToken));
                routeReturnSamples.Add(await MeasureRouteAsync(shell, "..", expectedDepth: 1, cancellationToken));
            }

            ShellPerformanceBenchmarkHelpers.AddLatencyMetrics(metrics, "RouteForward", routeForwardSamples);
            ShellPerformanceBenchmarkHelpers.AddLatencyMetrics(metrics, "RouteReturn", routeReturnSamples);
            ShellPerformanceBenchmarkHelpers.AddLatencyMetrics(metrics, "ModalPush", modalPushSamples);
            ShellPerformanceBenchmarkHelpers.AddLatencyMetrics(metrics, "ModalPop", modalPopSamples);
            metrics["Cycles"] = cycles;
            metrics["Operations"] = routeForwardSamples.Count + routeReturnSamples.Count + modalPushSamples.Count + modalPopSamples.Count;
            metrics["FinalRouteStackDepth"] = shell.Navigation.NavigationStack.Count;
            metrics["FinalModalStackDepth"] = shell.Navigation.ModalStack.Count;

            foreach (var (key, value) in MemorySnapshot.Capture(forceGC: true).Compare(memoryBefore).ToMetrics())
            {
                metrics[key] = value;
            }

            logger.LogInformation(
                "Shell route+modal: route forward avg={ForwardMs:F2}ms, route return avg={ReturnMs:F2}ms, modal push avg={PushMs:F2}ms, modal pop avg={PopMs:F2}ms",
                routeForwardSamples.Average(),
                routeReturnSamples.Average(),
                modalPushSamples.Average(),
                modalPopSamples.Average());

            return BenchmarkResult.Pass(metrics);
        }
        finally
        {
            shell.Handler?.DisconnectHandler();
            window.Page = this;
            Content = new Label { Text = "Shell route modal stack performance benchmark complete" };
            BenchmarkUiHelpers.DisconnectElementTree(shell.CurrentPage);
            await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 20);
        }
    }

    private static async Task<double> MeasureRouteAsync(
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

    private static async Task<double> MeasureModalPushAsync(
        Shell shell,
        int modalIndex,
        CancellationToken cancellationToken)
    {
        var modalPage = CreateModalPage(modalIndex);
        var stopwatch = Stopwatch.StartNew();
        await shell.Navigation.PushModalAsync(modalPage, animated: false);
        await BenchmarkUiHelpers.WaitUntilAsync(
            () => shell.Navigation.ModalStack.Count == 1 && modalPage.Handler != null,
            cancellationToken,
            timeoutMs: 4000,
            pollDelayMs: 15);
        await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 10);
        stopwatch.Stop();

        return stopwatch.Elapsed.TotalMilliseconds;
    }

    private static async Task<double> MeasureModalPopAsync(
        Shell shell,
        CancellationToken cancellationToken)
    {
        var modalPage = shell.Navigation.ModalStack.LastOrDefault();
        var stopwatch = Stopwatch.StartNew();
        await shell.Navigation.PopModalAsync(animated: false);
        await BenchmarkUiHelpers.WaitUntilAsync(
            () => shell.Navigation.ModalStack.Count == 0,
            cancellationToken,
            timeoutMs: 4000,
            pollDelayMs: 15);
        await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 10);
        stopwatch.Stop();

        if (modalPage != null)
        {
            ShellPerformanceBenchmarkHelpers.DisconnectPage(modalPage);
        }

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

    private static ContentPage CreateModalPage(int index)
    {
        var viewModel = new RouteModalViewModel(index);
        var page = new ContentPage
        {
            Title = $"Modal {index}",
            BindingContext = viewModel,
        };

        var titleLabel = new Label
        {
            FontSize = 22,
            FontAttributes = FontAttributes.Bold,
        };
        titleLabel.SetBinding(Label.TextProperty, nameof(RouteModalViewModel.Title));

        var subtitleLabel = new Label
        {
            FontSize = 13,
            Opacity = 0.8,
        };
        subtitleLabel.SetBinding(Label.TextProperty, nameof(RouteModalViewModel.Subtitle));

        var entry = new Entry
        {
            Placeholder = "Filter",
        };
        entry.SetBinding(Entry.TextProperty, nameof(RouteModalViewModel.Query));

        var cardsLayout = new VerticalStackLayout { Spacing = 10 };
        BindableLayout.SetItemsSource(cardsLayout, viewModel.Cards);
        BindableLayout.SetItemTemplate(cardsLayout, new DataTemplate(() =>
        {
            var title = new Label
            {
                FontSize = 15,
                FontAttributes = FontAttributes.Bold,
            };
            title.SetBinding(Label.TextProperty, nameof(RouteModalCard.Title));

            var detail = new Label
            {
                FontSize = 12,
                Opacity = 0.8,
            };
            detail.SetBinding(Label.TextProperty, nameof(RouteModalCard.Detail));

            var border = new Border
            {
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 },
                StrokeThickness = 1,
                Padding = new Thickness(10),
                Content = new VerticalStackLayout
                {
                    Spacing = 6,
                    Children =
                    {
                        title,
                        detail,
                        new Button { Text = "Select" },
                    },
                },
            };

            var tap = new TapGestureRecognizer
            {
                Command = viewModel.CardTapCommand,
            };
            tap.SetBinding(TapGestureRecognizer.CommandParameterProperty, ".");
            border.GestureRecognizers.Add(tap);

            return border;
        }));

        var rootLayout = new VerticalStackLayout
        {
            Spacing = 12,
            Padding = new Thickness(18),
            Children =
            {
                titleLabel,
                subtitleLabel,
                entry,
                cardsLayout,
            },
        };

        var rootTap = new TapGestureRecognizer
        {
            Command = viewModel.RootTapCommand,
        };
        rootLayout.GestureRecognizers.Add(rootTap);

        page.Content = new ScrollView { Content = rootLayout };
        return page;
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
                        new Label { Text = "Route and modal benchmark content to create meaningful page creation work.", FontSize = 13 },
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

    private sealed class RouteModalViewModel : BindableObject
    {
        public RouteModalViewModel(int index)
        {
            Title = $"Route modal {index}";
            Subtitle = $"Bound modal on routed page #{index}";
            Query = $"Route query {index}";
            Cards = new ObservableCollection<RouteModalCard>(
                Enumerable.Range(1, 4).Select(cardIndex => new RouteModalCard(
                    $"Card {cardIndex}",
                    $"Route modal {index} card {cardIndex}")));

            RootTapCommand = new Command(() => RootTapCount++);
            CardTapCommand = new Command<RouteModalCard?>(_ => CardTapCount++);
        }

        public string Title { get; }

        public string Subtitle { get; }

        public string Query { get; set; }

        public ObservableCollection<RouteModalCard> Cards { get; }

        public Command RootTapCommand { get; }

        public Command<RouteModalCard?> CardTapCommand { get; }

        public int RootTapCount { get; private set; }

        public int CardTapCount { get; private set; }
    }

    private sealed record RouteModalCard(string Title, string Detail);
}
