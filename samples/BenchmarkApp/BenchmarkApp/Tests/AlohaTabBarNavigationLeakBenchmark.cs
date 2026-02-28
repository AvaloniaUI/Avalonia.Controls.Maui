using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Simulates the AlohaAI app's TabBar Shell navigation pattern.
/// AlohaAI uses a TabBar with 4 ShellContent tabs (Home, Learning, Explore, Profile),
/// each containing pages with complex content. Switching between tabs repeatedly
/// should not leak pages or their content.
/// </summary>
[BenchmarkTest("AlohaTabBarNavigationLeak", Description = "Simulates AlohaAI TabBar tab switching to detect page/content leaks")]
public class AlohaTabBarNavigationLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();
        const int tabSwitchCycles = 20;

        var cycleMemory = await BuildShellAndSwitchTabs(window, trackedObjects, tabSwitchCycles, logger, cancellationToken);

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

        // Analyze per-cycle memory growth
        long baselineMemory = cycleMemory[0];
        long finalCycleMemory = cycleMemory[^1];
        long totalGrowth = finalCycleMemory - baselineMemory;

        // Compute average growth per cycle (skip first cycle as warmup)
        long avgGrowthPerCycle = 0;
        if (cycleMemory.Count > 2)
        {
            long growthAfterWarmup = cycleMemory[^1] - cycleMemory[1];
            avgGrowthPerCycle = growthAfterWarmup / (cycleMemory.Count - 2);
        }

        // Log per-cycle memory for analysis
        logger.LogInformation("Per-cycle memory (bytes after GC):");
        for (int i = 0; i < cycleMemory.Count; i++)
        {
            long delta = i > 0 ? cycleMemory[i] - cycleMemory[i - 1] : 0;
            logger.LogInformation(
                "  Cycle {Cycle}: {Memory:N0} bytes (delta: {Delta:+#,##0;-#,##0;0})",
                i, cycleMemory[i], delta);
        }

        var metrics = new Dictionary<string, object>
        {
            ["TabSwitchCycles"] = tabSwitchCycles,
            ["TotalObjectsTracked"] = trackedObjects.Count,
            ["ObjectsLeaked"] = leaked.Count,
            ["LeakedObjects"] = leaked.Count > 0 ? string.Join(", ", leaked) : "none",
            ["BaselineMemoryBytes"] = baselineMemory,
            ["FinalCycleMemoryBytes"] = finalCycleMemory,
            ["TotalMemoryGrowthBytes"] = totalGrowth,
            ["AvgGrowthPerCycleBytes"] = avgGrowthPerCycle,
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        if (leaked.Count > 0)
        {
            var leakedNames = string.Join(", ", leaked);
            logger.LogWarning("AlohaAI TabBar navigation leak detected: {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Objects leaked: {leakedNames}", metrics);
        }

        logger.LogInformation(
            "All {Count} objects collected after {Cycles} tab switch cycles. Memory growth: {Growth:N0} bytes total, {AvgGrowth:N0} bytes/cycle avg",
            trackedObjects.Count,
            tabSwitchCycles,
            totalGrowth,
            avgGrowthPerCycle);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task<List<long>> BuildShellAndSwitchTabs(
        Window window,
        Dictionary<string, WeakReference<object>> trackedObjects,
        int cycles,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var cycleMemory = new List<long>();

        // Create a Shell with TabBar matching AlohaAI's structure
        var shell = CreateAlohaStyleShell(trackedObjects);
        window.Page = shell;

        // Allow initial rendering
        await Task.Delay(100, cancellationToken);

        // Capture baseline memory after shell is rendered
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        cycleMemory.Add(GC.GetTotalMemory(false));

        // Switch between tabs repeatedly (simulating user tapping through tabs)
        var tabBar = (TabBar)shell.Items[0];
        for (int cycle = 0; cycle < cycles; cycle++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            for (int tabIndex = 0; tabIndex < tabBar.Items.Count; tabIndex++)
            {
                shell.CurrentItem = tabBar.Items[tabIndex];
                await Task.Delay(30, cancellationToken);
            }

            // Navigate back to first tab
            shell.CurrentItem = tabBar.Items[0];
            await Task.Delay(30, cancellationToken);

            // Capture memory after each full cycle through all tabs
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            cycleMemory.Add(GC.GetTotalMemory(false));
        }

        // Tear down the shell
        TearDownShell(shell, trackedObjects);

        // Restore test page
        window.Page = this;
        Content = new Label { Text = "TabBar navigation test complete" };

        return cycleMemory;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Shell CreateAlohaStyleShell(Dictionary<string, WeakReference<object>> trackedObjects)
    {
        var shell = new Shell { FlyoutBehavior = FlyoutBehavior.Disabled };
        trackedObjects["Shell"] = new WeakReference<object>(shell);

        var tabBar = new TabBar();
        trackedObjects["TabBar"] = new WeakReference<object>(tabBar);

        // Tab 1: Home - stats cards, word of day, learning path cards with BindableLayout
        // AlohaAI uses Icon="tab_home.png" on each ShellContent
        var homePage = CreateHomeTabPage(trackedObjects);
        var homeContent = new ShellContent
        {
            Title = "Home",
            Icon = "dotnet_bot.png",
            ContentTemplate = new DataTemplate(() => homePage),
            Route = "home",
        };
        tabBar.Items.Add(homeContent);
        trackedObjects["HomeShellContent"] = new WeakReference<object>(homeContent);

        // Tab 2: Learning - list of path cards
        var pathsPage = CreatePathsTabPage(trackedObjects);
        var pathsContent = new ShellContent
        {
            Title = "Learning",
            Icon = "dotnet_bot.png",
            ContentTemplate = new DataTemplate(() => pathsPage),
            Route = "paths",
        };
        tabBar.Items.Add(pathsContent);
        trackedObjects["PathsShellContent"] = new WeakReference<object>(pathsContent);

        // Tab 3: Explore - search bar, filters, results
        var searchPage = CreateSearchTabPage(trackedObjects);
        var searchContent = new ShellContent
        {
            Title = "Explore",
            Icon = "dotnet_bot.png",
            ContentTemplate = new DataTemplate(() => searchPage),
            Route = "search",
        };
        tabBar.Items.Add(searchContent);
        trackedObjects["SearchShellContent"] = new WeakReference<object>(searchContent);

        // Tab 4: Profile - stats, achievements
        var profilePage = CreateProfileTabPage(trackedObjects);
        var profileContent = new ShellContent
        {
            Title = "Profile",
            Icon = "dotnet_bot.png",
            ContentTemplate = new DataTemplate(() => profilePage),
            Route = "profile",
        };
        tabBar.Items.Add(profileContent);
        trackedObjects["ProfileShellContent"] = new WeakReference<object>(profileContent);

        shell.Items.Add(tabBar);
        return shell;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ContentPage CreateHomeTabPage(Dictionary<string, WeakReference<object>> trackedObjects)
    {
        var page = new ContentPage { Title = "Home" };
        trackedObjects["HomePage"] = new WeakReference<object>(page);

        var scrollView = new ScrollView();
        var mainLayout = new VerticalStackLayout { Spacing = 10, Padding = new Thickness(16) };

        // Header area with background image and gradient overlay (like AlohaAI's bg_sunset.png)
        var headerGrid = new Grid { HeightRequest = 200 };
        var headerImage = new Image
        {
            Source = "dotnet_bot.png",
            Aspect = Aspect.AspectFill,
            InputTransparent = true,
        };
        headerGrid.Children.Add(headerImage);
        trackedObjects["Home.HeaderImage"] = new WeakReference<object>(headerImage);

        // Gradient overlay on top of image (like AlohaAI's transparent-to-solid overlay)
        headerGrid.Children.Add(new BoxView
        {
            Background = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Colors.Transparent, 0.0f),
                    new GradientStop(Color.FromArgb("#80000000"), 0.5f),
                    new GradientStop(Color.FromArgb("#FF1A1A2E"), 1.0f),
                },
            },
        });
        mainLayout.Children.Add(headerGrid);

        // Settings gear icon (like AlohaAI's icon_settings.png in a Border with TapGestureRecognizer)
        var settingsGrid = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
        var settingsBorder = new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 14 },
            Stroke = Colors.Transparent,
            WidthRequest = 40,
            HeightRequest = 40,
            Shadow = new Shadow { Brush = Colors.Black, Offset = new Point(0, 2), Radius = 8, Opacity = 0.3f },
        };
        var settingsIcon = new Image
        {
            Source = "dotnet_bot.png",
            WidthRequest = 22,
            HeightRequest = 22,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
        };
        settingsBorder.Content = settingsIcon;
        settingsBorder.GestureRecognizers.Add(new TapGestureRecognizer());
        settingsGrid.Add(new ContentView(), 0, 0);
        settingsGrid.Add(settingsBorder, 1, 0);
        mainLayout.Children.Add(settingsGrid);
        trackedObjects["Home.SettingsIcon"] = new WeakReference<object>(settingsIcon);
        trackedObjects["Home.SettingsBorder"] = new WeakReference<object>(settingsBorder);

        // Logo image (like AlohaAI's logo_alohaai.png)
        var logoImage = new Image
        {
            Source = "dotnet_bot.png",
            HeightRequest = 40,
            Aspect = Aspect.AspectFit,
            HorizontalOptions = LayoutOptions.Center,
        };
        mainLayout.Children.Add(logoImage);
        trackedObjects["Home.LogoImage"] = new WeakReference<object>(logoImage);

        // Stats cards (3 Borders in a Grid, like AlohaAI)
        var statsGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
            },
            ColumnSpacing = 10,
        };

        for (int i = 0; i < 3; i++)
        {
            var statBorder = new Border
            {
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
                Stroke = Colors.Transparent,
                Padding = new Thickness(12),
                Shadow = new Shadow { Brush = Colors.Black, Offset = new Point(0, 4), Radius = 12, Opacity = 0.3f },
            };

            var statLayout = new VerticalStackLayout
            {
                Children =
                {
                    new Label { Text = $"{i * 42}", FontSize = 28, FontAttributes = FontAttributes.Bold },
                    new Label { Text = i == 0 ? "Streak" : i == 1 ? "XP" : "Today", FontSize = 11 },
                },
            };
            statBorder.Content = statLayout;
            statsGrid.Add(statBorder, i, 0);
            trackedObjects[$"Home.StatCard[{i}]"] = new WeakReference<object>(statBorder);
        }

        mainLayout.Children.Add(statsGrid);

        // Word of day card (Border with gradient)
        var wordBorder = new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
            Stroke = Colors.Transparent,
            Background = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0.6),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb("#2A1E3D"), 0.0f),
                    new GradientStop(Color.FromArgb("#1E2A3D"), 0.6f),
                    new GradientStop(Color.FromArgb("#1A1E2D"), 1.0f),
                },
            },
            Content = new VerticalStackLayout
            {
                Children =
                {
                    new Label { Text = "Neural Network", FontSize = 18, FontAttributes = FontAttributes.Bold },
                    new Label { Text = "A computing system inspired by biological neural networks", FontSize = 12 },
                },
            },
        };
        mainLayout.Children.Add(wordBorder);
        trackedObjects["Home.WordOfDay"] = new WeakReference<object>(wordBorder);

        // Learning path cards (simulating BindableLayout with DataTemplate)
        for (int i = 0; i < 4; i++)
        {
            var card = CreatePathCard($"Path {i}", $"Module {i}/5 complete", trackedObjects, $"Home.PathCard[{i}]");
            mainLayout.Children.Add(card);
        }

        scrollView.Content = mainLayout;
        page.Content = scrollView;
        trackedObjects["Home.ScrollView"] = new WeakReference<object>(scrollView);
        trackedObjects["Home.MainLayout"] = new WeakReference<object>(mainLayout);
        return page;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ContentPage CreatePathsTabPage(Dictionary<string, WeakReference<object>> trackedObjects)
    {
        var page = new ContentPage { Title = "Learning" };
        trackedObjects["PathsPage"] = new WeakReference<object>(page);

        var scrollView = new ScrollView();
        var layout = new VerticalStackLayout { Spacing = 14, Padding = new Thickness(16) };

        layout.Children.Add(new Label { Text = "All Learning Paths", FontSize = 22, FontAttributes = FontAttributes.Bold });

        for (int i = 0; i < 6; i++)
        {
            var card = CreatePathCard($"Learning Path {i}", $"{i * 20}% complete", trackedObjects, $"Paths.Card[{i}]");
            layout.Children.Add(card);
        }

        scrollView.Content = layout;
        page.Content = scrollView;
        trackedObjects["Paths.ScrollView"] = new WeakReference<object>(scrollView);
        return page;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ContentPage CreateSearchTabPage(Dictionary<string, WeakReference<object>> trackedObjects)
    {
        var page = new ContentPage { Title = "Explore" };
        trackedObjects["SearchPage"] = new WeakReference<object>(page);

        var layout = new VerticalStackLayout { Spacing = 10, Padding = new Thickness(16) };

        // Search bar
        var searchBar = new SearchBar { Placeholder = "Search lessons..." };
        layout.Children.Add(searchBar);
        trackedObjects["Search.SearchBar"] = new WeakReference<object>(searchBar);

        // Filter chips
        var chipLayout = new HorizontalStackLayout { Spacing = 8 };
        string[] filters = ["All", "Beginner", "Intermediate", "Advanced"];
        for (int i = 0; i < filters.Length; i++)
        {
            var chip = new Border
            {
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
                Padding = new Thickness(12, 6),
                Content = new Label { Text = filters[i], FontSize = 12 },
            };
            chip.GestureRecognizers.Add(new TapGestureRecognizer());
            chipLayout.Children.Add(chip);
            trackedObjects[$"Search.Chip[{i}]"] = new WeakReference<object>(chip);
        }

        layout.Children.Add(chipLayout);

        // Results (each with an icon image, like AlohaAI's search results with path icons)
        for (int i = 0; i < 5; i++)
        {
            var resultIcon = new Image
            {
                Source = "dotnet_bot.png",
                WidthRequest = 32,
                HeightRequest = 32,
                VerticalOptions = LayoutOptions.Center,
            };
            var resultBorder = new Border
            {
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
                Padding = new Thickness(12),
                Content = new HorizontalStackLayout
                {
                    Spacing = 12,
                    Children =
                    {
                        resultIcon,
                        new VerticalStackLayout
                        {
                            Children =
                            {
                                new Label { Text = $"Result {i}", FontSize = 16, FontAttributes = FontAttributes.Bold },
                                new Label { Text = $"Description for result {i}", FontSize = 13 },
                            },
                        },
                    },
                },
            };
            resultBorder.GestureRecognizers.Add(new TapGestureRecognizer());
            layout.Children.Add(resultBorder);
            trackedObjects[$"Search.Result[{i}]"] = new WeakReference<object>(resultBorder);
            trackedObjects[$"Search.ResultIcon[{i}]"] = new WeakReference<object>(resultIcon);
        }

        page.Content = layout;
        return page;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ContentPage CreateProfileTabPage(Dictionary<string, WeakReference<object>> trackedObjects)
    {
        var page = new ContentPage { Title = "Profile" };
        trackedObjects["ProfilePage"] = new WeakReference<object>(page);

        var layout = new VerticalStackLayout { Spacing = 10, Padding = new Thickness(16) };

        // Profile header with avatar image (like AlohaAI's profile page)
        var profileHeader = new HorizontalStackLayout { Spacing = 12 };
        var avatarImage = new Image
        {
            Source = "dotnet_bot.png",
            WidthRequest = 60,
            HeightRequest = 60,
            Aspect = Aspect.AspectFill,
        };
        profileHeader.Children.Add(avatarImage);
        profileHeader.Children.Add(new Label { Text = "Your Profile", FontSize = 22, FontAttributes = FontAttributes.Bold, VerticalOptions = LayoutOptions.Center });
        layout.Children.Add(profileHeader);
        trackedObjects["Profile.AvatarImage"] = new WeakReference<object>(avatarImage);

        // Stats grid
        var statsGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
            },
            ColumnSpacing = 10,
            RowSpacing = 10,
        };

        for (int i = 0; i < 4; i++)
        {
            var statCard = new Border
            {
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
                Padding = new Thickness(16),
                Shadow = new Shadow { Brush = Colors.Black, Offset = new Point(0, 4), Radius = 12, Opacity = 0.3f },
                Content = new VerticalStackLayout
                {
                    Children =
                    {
                        new Label { Text = $"{i * 100 + 50}", FontSize = 24, FontAttributes = FontAttributes.Bold },
                        new Label { Text = $"Stat {i}", FontSize = 12 },
                    },
                },
            };
            statsGrid.Add(statCard, i % 2, i / 2);
            trackedObjects[$"Profile.StatCard[{i}]"] = new WeakReference<object>(statCard);
        }

        layout.Children.Add(statsGrid);

        // Achievements (with icon images instead of emoji, like AlohaAI uses icon images)
        for (int i = 0; i < 3; i++)
        {
            var achievementIcon = new Image
            {
                Source = "dotnet_bot.png",
                WidthRequest = 32,
                HeightRequest = 32,
                VerticalOptions = LayoutOptions.Center,
            };
            var achievement = new Border
            {
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
                Padding = new Thickness(12),
                Content = new HorizontalStackLayout
                {
                    Spacing = 12,
                    Children =
                    {
                        achievementIcon,
                        new VerticalStackLayout
                        {
                            Children =
                            {
                                new Label { Text = $"Achievement {i}", FontSize = 14, FontAttributes = FontAttributes.Bold },
                                new Label { Text = "Completed", FontSize = 11 },
                            },
                        },
                    },
                },
            };
            layout.Children.Add(achievement);
            trackedObjects[$"Profile.Achievement[{i}]"] = new WeakReference<object>(achievement);
            trackedObjects[$"Profile.AchievementIcon[{i}]"] = new WeakReference<object>(achievementIcon);
        }

        page.Content = layout;
        return page;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Border CreatePathCard(
        string title,
        string progress,
        Dictionary<string, WeakReference<object>> trackedObjects,
        string trackingKey)
    {
        // Matches AlohaAI HomePage card structure:
        // Border (rounded, shadow, gradient stroke, tap gesture)
        //   > Grid
        //     > BoxView (accent bar)
        //     > VerticalStackLayout
        //       > Label (title)
        //       > Label (progress text)
        //       > HorizontalStackLayout (tag chips)
        //       > Grid (progress bar)
        var card = new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 22 },
            StrokeThickness = 1,
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
            Shadow = new Shadow { Brush = Colors.Black, Offset = new Point(0, 6), Radius = 20, Opacity = 0.6f },
        };

        var tapGesture = new TapGestureRecognizer();
        card.GestureRecognizers.Add(tapGesture);
        trackedObjects[$"{trackingKey}.TapGesture"] = new WeakReference<object>(tapGesture);

        var cardGrid = new Grid();

        // Large semi-transparent icon as card background (like AlohaAI's IconImage)
        var cardIcon = new Image
        {
            Source = "dotnet_bot.png",
            WidthRequest = 100,
            HeightRequest = 100,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Start,
            Opacity = 0.15,
            Margin = new Thickness(0, 8, 8, 0),
            InputTransparent = true,
        };
        cardGrid.Children.Add(cardIcon);
        trackedObjects[$"{trackingKey}.CardIcon"] = new WeakReference<object>(cardIcon);

        // Accent bar
        var accentBar = new BoxView
        {
            WidthRequest = 3,
            BackgroundColor = Color.FromArgb("#5B8FD4"),
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Fill,
            Shadow = new Shadow { Brush = Color.FromArgb("#5B8FD4"), Offset = new Point(2, 0), Radius = 8, Opacity = 0.4f },
        };
        cardGrid.Children.Add(accentBar);

        // Content
        var contentLayout = new VerticalStackLayout
        {
            Spacing = 8,
            Padding = new Thickness(18, 14, 16, 14),
        };

        contentLayout.Children.Add(new Label { Text = title, FontSize = 19, FontAttributes = FontAttributes.Bold });
        contentLayout.Children.Add(new Label { Text = progress, FontSize = 13 });

        // Tag chips
        var tagLayout = new HorizontalStackLayout { Spacing = 6 };
        tagLayout.Children.Add(new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 },
            Stroke = Colors.Transparent,
            Padding = new Thickness(10, 4),
            Content = new Label { Text = "Course", FontSize = 10 },
        });
        tagLayout.Children.Add(new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 },
            Stroke = Colors.Transparent,
            Padding = new Thickness(10, 4),
            Content = new Label { Text = "Beginner", FontSize = 10 },
        });
        contentLayout.Children.Add(tagLayout);

        // Progress bar
        var progressGrid = new Grid { HeightRequest = 5 };
        progressGrid.Children.Add(new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 2.5 },
            Stroke = Colors.Transparent,
        });
        progressGrid.Children.Add(new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 2.5 },
            Stroke = Colors.Transparent,
            HorizontalOptions = LayoutOptions.Start,
            WidthRequest = 150,
            BackgroundColor = Color.FromArgb("#5B8FD4"),
            Shadow = new Shadow { Brush = Color.FromArgb("#5B8FD4"), Offset = new Point(0, 0), Radius = 6, Opacity = 0.6f },
        });
        contentLayout.Children.Add(progressGrid);

        cardGrid.Children.Add(contentLayout);
        card.Content = cardGrid;

        trackedObjects[trackingKey] = new WeakReference<object>(card);
        return card;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void TearDownShell(Shell shell, Dictionary<string, WeakReference<object>> trackedObjects)
    {
        // Track shell handler
        if (shell.Handler is object shellHandler)
        {
            trackedObjects["Shell.Handler"] = new WeakReference<object>(shellHandler);
        }

        // Disconnect all pages in all tabs
        // Shell hierarchy: Shell > TabBar (ShellItem) > ShellSection > ShellContent
        if (shell.Items[0] is TabBar tabBar)
        {
            foreach (var section in tabBar.Items)
            {
                foreach (var content in section.Items)
                {
                    if (content.Content is ContentPage page)
                    {
                        DisconnectPage(page);
                    }
                }

                section.Items.Clear();
            }

            tabBar.Items.Clear();
        }

        shell.Items.Clear();
        shell.Handler?.DisconnectHandler();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void DisconnectPage(ContentPage page)
    {
        if (page.Content is Layout layout)
        {
            DisconnectLayout(layout);
        }
        else if (page.Content is ScrollView scrollView)
        {
            if (scrollView.Content is Layout scrollLayout)
            {
                DisconnectLayout(scrollLayout);
            }

            scrollView.Handler?.DisconnectHandler();
        }

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
            else if (child is ScrollView scrollView)
            {
                if (scrollView.Content is Layout scrollLayout)
                {
                    DisconnectLayout(scrollLayout);
                }

                scrollView.Handler?.DisconnectHandler();
            }
            else if (child is Border border && border.Content is View borderContent)
            {
                // Recurse into Border content (Borders contain nested layouts/images)
                if (borderContent is Layout borderLayout)
                {
                    DisconnectLayout(borderLayout);
                }
                else if (borderContent is Image borderImage)
                {
                    borderImage.Source = null;
                    borderImage.Handler?.DisconnectHandler();
                }

                borderContent.Handler?.DisconnectHandler();
            }

            // Clear image sources before disconnect to release image resources
            if (child is Image image)
            {
                image.Source = null;
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
}
