using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

internal static class ShellPerformanceBenchmarkHelpers
{
    public static void AddLatencyMetrics(
        Dictionary<string, object> metrics,
        string prefix,
        IReadOnlyList<double> samples)
    {
        metrics[$"{prefix}.Count"] = samples.Count;
        metrics[$"{prefix}.AverageElapsedMs"] = samples.Count > 0 ? samples.Average() : 0;
        metrics[$"{prefix}.P95ElapsedMs"] = BenchmarkUiHelpers.CalculatePercentile(samples, 0.95);
        metrics[$"{prefix}.MaxElapsedMs"] = samples.Count > 0 ? samples.Max() : 0;
    }

    public static async Task WaitForShellCurrentPageAsync(
        Shell shell,
        Page expectedPage,
        CancellationToken cancellationToken,
        int timeoutMs = 4000,
        int pollDelayMs = 15)
    {
        await BenchmarkUiHelpers.WaitUntilAsync(
            () => ReferenceEquals(shell.CurrentPage, expectedPage) && expectedPage.Handler != null,
            cancellationToken,
            timeoutMs,
            pollDelayMs);

        await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, pollDelayMs);
    }

    public static async Task WaitForNavigationStackCountAsync(
        INavigation navigation,
        int expectedCount,
        CancellationToken cancellationToken,
        int timeoutMs = 4000,
        int pollDelayMs = 15)
    {
        await BenchmarkUiHelpers.WaitUntilAsync(
            () => navigation.NavigationStack.Count == expectedCount,
            cancellationToken,
            timeoutMs,
            pollDelayMs);

        await BenchmarkUiHelpers.WaitForIdleAsync(cancellationToken, pollDelayMs);
    }

    public static ContentPage CreateContentHeavyPage(
        string title,
        int sectionCount,
        int toolbarCount = 0,
        bool addRootGesture = true)
    {
        var page = new ContentPage
        {
            Title = title,
        };

        for (int toolbarIndex = 0; toolbarIndex < toolbarCount; toolbarIndex++)
        {
            var toolbarItem = new ToolbarItem
            {
                Text = $"Action {toolbarIndex + 1}",
                Order = ToolbarItemOrder.Primary,
            };
            toolbarItem.Clicked += static (_, _) => { };
            page.ToolbarItems.Add(toolbarItem);
        }

        var rootLayout = new VerticalStackLayout
        {
            Spacing = 14,
            Padding = new Thickness(16),
        };

        if (addRootGesture)
        {
            var rootTap = new TapGestureRecognizer();
            rootTap.Tapped += static (_, _) => { };
            rootLayout.GestureRecognizers.Add(rootTap);
        }

        rootLayout.Children.Add(new Label
        {
            Text = title,
            FontSize = 24,
            FontAttributes = FontAttributes.Bold,
        });

        for (int sectionIndex = 0; sectionIndex < sectionCount; sectionIndex++)
        {
            rootLayout.Children.Add(new Label
            {
                Text = $"Section {sectionIndex + 1}",
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
            });

            for (int cardIndex = 0; cardIndex < 3; cardIndex++)
            {
                var progress = ((sectionIndex + 1) * (cardIndex + 2) % 10) / 10.0;
                var card = new Border
                {
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 14 },
                    StrokeThickness = 1,
                    Padding = new Thickness(14),
                    Content = new VerticalStackLayout
                    {
                        Spacing = 8,
                        Children =
                        {
                            new Grid
                            {
                                ColumnDefinitions =
                                {
                                    new ColumnDefinition { Width = GridLength.Star },
                                    new ColumnDefinition { Width = GridLength.Auto },
                                },
                                Children =
                                {
                                    new Label
                                    {
                                        Text = $"{title} card {sectionIndex + 1}-{cardIndex + 1}",
                                        FontSize = 16,
                                        FontAttributes = FontAttributes.Bold,
                                    },
                                    new Label
                                    {
                                        Text = $"{(int)(progress * 100)}%",
                                        FontSize = 12,
                                        HorizontalTextAlignment = TextAlignment.End,
                                    },
                                },
                            },
                            new Label
                            {
                                Text = "Nested content, controls, and gestures to simulate a real Shell page.",
                                FontSize = 13,
                                Opacity = 0.8,
                            },
                            new ProgressBar
                            {
                                Progress = progress,
                            },
                            new HorizontalStackLayout
                            {
                                Spacing = 10,
                                Children =
                                {
                                    new Button { Text = "Open" },
                                    new Button { Text = "Inspect" },
                                },
                            },
                        },
                    },
                };

                var tap = new TapGestureRecognizer();
                tap.Tapped += static (_, _) => { };
                card.GestureRecognizers.Add(tap);

                rootLayout.Children.Add(card);
            }
        }

        page.Content = new ScrollView { Content = rootLayout };
        return page;
    }

    public static void DisconnectPage(Page? page)
    {
        if (page is ContentPage contentPage && contentPage.Content is IView content)
        {
            BenchmarkUiHelpers.DisconnectElementTree(content);
        }

        if (page is VisualElement visualElement)
        {
            visualElement.Handler?.DisconnectHandler();
        }
    }
}
