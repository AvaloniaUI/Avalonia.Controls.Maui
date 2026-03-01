

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that TabBar-style navigation (like AlohaAI) releases old tab content handlers
/// when switching tabs. Uses a TabBar with multiple ShellContent items which is the
/// common pattern for bottom-tab navigation in MAUI apps.
/// </summary>
[BenchmarkTest("ShellSectionSwitchLeak", Description = "Verifies TabBar tab switching releases old section handler resources")]
public class ShellSectionSwitchLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();

        await RunShellLifecycle(window, this, trackedObjects, logger, cancellationToken);

        // Allow Dispatcher callbacks to drain
        await Task.Delay(200, cancellationToken);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        await Task.Delay(100, cancellationToken);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var memAfter = MemorySnapshot.Capture(forceGC: false);
        var memoryDelta = memAfter.Compare(memBefore);

        var leaked = new List<string>();
        foreach (var (name, weakRef) in trackedObjects)
        {
            if (weakRef.TryGetTarget(out _))
            {
                leaked.Add(name);
            }
        }

        var metrics = new Dictionary<string, object>
        {
            ["TotalObjectsTracked"] = trackedObjects.Count,
            ["ObjectsLeaked"] = leaked.Count,
            ["LeakedObjects"] = leaked.Count > 0 ? string.Join(", ", leaked) : "none",
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        if (leaked.Count > 0)
        {
            var leakedNames = string.Join(", ", leaked);
            logger.LogWarning("TabBar navigation leak detected: {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Objects leaked: {leakedNames}", metrics);
        }

        logger.LogInformation(
            "All {Count} objects collected after TabBar navigation",
            trackedObjects.Count);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static async Task RunShellLifecycle(
        Window window,
        Page restorePage,
        Dictionary<string, WeakReference<object>> trackedObjects,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var shell = CreateTabBarShell(trackedObjects);
        window.Page = shell;

        await Task.Delay(150, cancellationToken);

        var tabBar = (TabBar)shell.Items[0];

        // Switch through all tabs multiple times
        for (int cycle = 0; cycle < 3; cycle++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            for (int i = 0; i < tabBar.Items.Count; i++)
            {
                shell.CurrentItem = tabBar.Items[i];
                await Task.Delay(50, cancellationToken);
            }

            shell.CurrentItem = tabBar.Items[0];
            await Task.Delay(50, cancellationToken);
        }

        // Track handlers before teardown
        if (shell.Handler is object h)
            trackedObjects["Shell.Handler"] = new WeakReference<object>(h);

        window.Page = restorePage;

        shell = null!;
        tabBar = null!;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Shell CreateTabBarShell(Dictionary<string, WeakReference<object>> trackedObjects)
    {
        var shell = new Shell { FlyoutBehavior = FlyoutBehavior.Disabled };
        trackedObjects["Shell"] = new WeakReference<object>(shell);

        var tabBar = new TabBar();
        trackedObjects["TabBar"] = new WeakReference<object>(tabBar);

        string[] tabNames = ["Home", "Search", "Profile", "Settings"];

        for (int i = 0; i < tabNames.Length; i++)
        {
            var page = new ContentPage
            {
                Title = tabNames[i],
                Content = new VerticalStackLayout
                {
                    Spacing = 8,
                    Padding = new Thickness(16),
                    Children =
                    {
                        new Label { Text = $"{tabNames[i]} Page", FontSize = 20, FontAttributes = FontAttributes.Bold },
                        new Button { Text = $"{tabNames[i]} Action" },
                        new Entry { Placeholder = $"Search {tabNames[i]}..." },
                        new Image { Source = "dotnet_bot.png", WidthRequest = 64, HeightRequest = 64 },
                        new Border
                        {
                            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
                            Padding = new Thickness(10),
                            Content = new Label { Text = $"{tabNames[i]} card content" },
                        },
                    },
                },
            };
            trackedObjects[$"Page[{tabNames[i]}]"] = new WeakReference<object>(page);

            var content = new ShellContent
            {
                Title = tabNames[i],
                ContentTemplate = new DataTemplate(() => page),
                Route = tabNames[i].ToLowerInvariant(),
            };
            tabBar.Items.Add(content);
            trackedObjects[$"ShellContent[{tabNames[i]}]"] = new WeakReference<object>(content);
        }

        shell.Items.Add(tabBar);
        return shell;
    }
}
