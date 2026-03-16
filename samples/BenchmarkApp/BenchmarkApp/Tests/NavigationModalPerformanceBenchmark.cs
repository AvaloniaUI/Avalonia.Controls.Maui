using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

[BenchmarkTest("NavigationModalPerformance", Description = "Measures PushModalAsync and PopModalAsync latency with bindings and gestures on the modal root")]
public class NavigationModalPerformanceBenchmark : BenchmarkTestPage
{
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var metrics = new Dictionary<string, object>();
        var memoryBefore = MemorySnapshot.Capture(forceGC: true);

        const int cycles = 8;
        const int stackedDepth = 3;

        var rootPage = new NavigationPage(new ContentPage
        {
            Title = "Modal Perf Root",
            Content = new Label
            {
                Text = "Modal performance root",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
            },
        });

        window.Page = rootPage;
        await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 50);

        var pushTimes = new List<double>(cycles);
        var popTimes = new List<double>(cycles);

        try
        {
            for (int index = 0; index < cycles; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var modalPage = CreateModalPage(index);

                var pushStopwatch = Stopwatch.StartNew();
                await rootPage.Navigation.PushModalAsync(modalPage, animated: false);
                await BenchmarkUiHelpers.WaitUntilAsync(
                    () => rootPage.Navigation.ModalStack.Count == 1 && modalPage.Handler != null,
                    cancellationToken,
                    timeoutMs: 4000,
                    pollDelayMs: 15);
                await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 15);
                pushStopwatch.Stop();
                pushTimes.Add(pushStopwatch.Elapsed.TotalMilliseconds);

                var popStopwatch = Stopwatch.StartNew();
                await rootPage.Navigation.PopModalAsync(animated: false);
                await BenchmarkUiHelpers.WaitUntilAsync(
                    () => rootPage.Navigation.ModalStack.Count == 0,
                    cancellationToken,
                    timeoutMs: 4000,
                    pollDelayMs: 15);
                await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 15);
                popStopwatch.Stop();
                popTimes.Add(popStopwatch.Elapsed.TotalMilliseconds);

                ShellPerformanceBenchmarkHelpers.DisconnectPage(modalPage);
            }

            var stackedPushStopwatch = Stopwatch.StartNew();
            var stackedPages = new List<ContentPage>(stackedDepth);
            for (int index = 0; index < stackedDepth; index++)
            {
                var modalPage = CreateModalPage(100 + index);
                stackedPages.Add(modalPage);
                await rootPage.Navigation.PushModalAsync(modalPage, animated: false);
                await BenchmarkUiHelpers.WaitUntilAsync(
                    () => rootPage.Navigation.ModalStack.Count == index + 1 && modalPage.Handler != null,
                    cancellationToken,
                    timeoutMs: 4000,
                    pollDelayMs: 15);
            }
            await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 20);
            stackedPushStopwatch.Stop();

            var stackedPopStopwatch = Stopwatch.StartNew();
            for (int index = stackedPages.Count - 1; index >= 0; index--)
            {
                await rootPage.Navigation.PopModalAsync(animated: false);
                await BenchmarkUiHelpers.WaitUntilAsync(
                    () => rootPage.Navigation.ModalStack.Count == index,
                    cancellationToken,
                    timeoutMs: 4000,
                    pollDelayMs: 15);
                ShellPerformanceBenchmarkHelpers.DisconnectPage(stackedPages[index]);
            }
            await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 20);
            stackedPopStopwatch.Stop();

            ShellPerformanceBenchmarkHelpers.AddLatencyMetrics(metrics, "Push", pushTimes);
            ShellPerformanceBenchmarkHelpers.AddLatencyMetrics(metrics, "Pop", popTimes);
            metrics["Cycles"] = cycles;
            metrics["StackedDepth"] = stackedDepth;
            metrics["StackedPushAllMs"] = stackedPushStopwatch.Elapsed.TotalMilliseconds;
            metrics["StackedPopAllMs"] = stackedPopStopwatch.Elapsed.TotalMilliseconds;

            foreach (var (key, value) in MemorySnapshot.Capture(forceGC: true).Compare(memoryBefore).ToMetrics())
            {
                metrics[key] = value;
            }

            logger.LogInformation(
                "Modal push: avg={AvgMs:F2}ms, p95={P95Ms:F2}ms, max={MaxMs:F2}ms over {Cycles} cycles",
                pushTimes.Average(),
                BenchmarkUiHelpers.CalculatePercentile(pushTimes, 0.95),
                pushTimes.Max(),
                cycles);
            logger.LogInformation(
                "Modal pop: avg={AvgMs:F2}ms, p95={P95Ms:F2}ms, max={MaxMs:F2}ms over {Cycles} cycles",
                popTimes.Average(),
                BenchmarkUiHelpers.CalculatePercentile(popTimes, 0.95),
                popTimes.Max(),
                cycles);
            logger.LogInformation(
                "Stacked modal depth {Depth}: push all={PushMs:F2}ms, pop all={PopMs:F2}ms",
                stackedDepth,
                stackedPushStopwatch.Elapsed.TotalMilliseconds,
                stackedPopStopwatch.Elapsed.TotalMilliseconds);

            return BenchmarkResult.Pass(metrics);
        }
        finally
        {
            rootPage.Handler?.DisconnectHandler();
            window.Page = this;
            Content = new Label { Text = "Navigation modal performance benchmark complete" };
            await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, 20);
        }
    }

    private static ContentPage CreateModalPage(int index)
    {
        var viewModel = new ModalPerformanceViewModel(index);
        var page = new ContentPage
        {
            Title = $"Modal {index}",
            BindingContext = viewModel,
        };

        var titleLabel = new Label
        {
            FontSize = 24,
            FontAttributes = FontAttributes.Bold,
        };
        titleLabel.SetBinding(Label.TextProperty, nameof(ModalPerformanceViewModel.Title));

        var subtitleLabel = new Label
        {
            FontSize = 14,
            Opacity = 0.8,
        };
        subtitleLabel.SetBinding(Label.TextProperty, nameof(ModalPerformanceViewModel.Subtitle));

        var entry = new Entry
        {
            Placeholder = "Search",
        };
        entry.SetBinding(Entry.TextProperty, nameof(ModalPerformanceViewModel.Query));

        var progressBar = new ProgressBar();
        progressBar.SetBinding(ProgressBar.ProgressProperty, nameof(ModalPerformanceViewModel.Progress));

        var cardsLayout = new VerticalStackLayout { Spacing = 10 };
        BindableLayout.SetItemsSource(cardsLayout, viewModel.Cards);
        BindableLayout.SetItemTemplate(cardsLayout, new DataTemplate(() =>
        {
            var cardTitle = new Label
            {
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
            };
            cardTitle.SetBinding(Label.TextProperty, nameof(ModalPerformanceCard.Title));

            var cardDetail = new Label
            {
                FontSize = 13,
                Opacity = 0.8,
            };
            cardDetail.SetBinding(Label.TextProperty, nameof(ModalPerformanceCard.Detail));

            var border = new Border
            {
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
                StrokeThickness = 1,
                Padding = new Thickness(12),
                Content = new VerticalStackLayout
                {
                    Spacing = 6,
                    Children =
                    {
                        cardTitle,
                        cardDetail,
                        new Button { Text = "Open" },
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
                progressBar,
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

    private sealed class ModalPerformanceViewModel : BindableObject
    {
        public ModalPerformanceViewModel(int index)
        {
            Title = $"Modal {index}";
            Subtitle = $"Bindings and gestures benchmark #{index}";
            Query = $"Query {index}";
            Progress = ((index % 7) + 2) / 10.0;
            Cards = new ObservableCollection<ModalPerformanceCard>(
                Enumerable.Range(1, 5).Select(cardIndex => new ModalPerformanceCard(
                    $"Card {cardIndex}",
                    $"Detail for modal {index}, card {cardIndex}")));

            RootTapCommand = new Command(() => RootTapCount++);
            CardTapCommand = new Command<ModalPerformanceCard?>(_ => CardTapCount++);
        }

        public string Title { get; }

        public string Subtitle { get; }

        public string Query { get; set; }

        public double Progress { get; }

        public ObservableCollection<ModalPerformanceCard> Cards { get; }

        public Command RootTapCommand { get; }

        public Command<ModalPerformanceCard?> CardTapCommand { get; }

        public int RootTapCount { get; private set; }

        public int CardTapCount { get; private set; }
    }

    private sealed record ModalPerformanceCard(string Title, string Detail);
}
