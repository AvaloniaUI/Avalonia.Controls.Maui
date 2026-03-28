using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp.Tests;

/// <summary>
/// Tests that menu and toolbar items (MenuBarItem, MenuFlyoutItem, MenuFlyoutSubItem,
/// MenuFlyoutSeparator, ToolbarItem) and their handlers are collected after the owning
/// page is replaced. Uses window.Page swapping to ensure items are no longer retained
/// by the active page's navigation infrastructure.
/// </summary>
[BenchmarkTest("MenuAndToolbarLeak", Description = "Verifies menu and toolbar items are collected after page replacement")]
public class MenuAndToolbarLeakBenchmark : BenchmarkTestPage
{
    /// <inheritdoc/>
    public override async Task<BenchmarkResult> RunAsync(Window window, ILogger logger, CancellationToken cancellationToken)
    {
        var memBefore = MemorySnapshot.Capture(forceGC: true);

        var trackedObjects = new Dictionary<string, WeakReference<object>>();

        await CreateAndDestroyMenuPage(window, trackedObjects, cancellationToken);

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

        var metrics = new Dictionary<string, object>
        {
            ["TotalObjectsTracked"] = trackedObjects.Count,
            ["ObjectsLeaked"] = leaked.Count,
            ["MenuBar.Leaked"] = leaked.Any(n => n.StartsWith("MenuBar")),
            ["MenuFlyout.Leaked"] = leaked.Any(n => n.StartsWith("MenuFlyout")),
            ["Toolbar.Leaked"] = leaked.Any(n => n.StartsWith("Toolbar")),
            ["Page.Leaked"] = leaked.Any(n => n == "MenuPage"),
            ["LeakedObjects"] = leaked.Count > 0 ? string.Join(", ", leaked) : "none",
        };

        foreach (var (key, value) in memoryDelta.ToMetrics())
        {
            metrics[key] = value;
        }

        if (leaked.Count > 0)
        {
            var leakedNames = string.Join(", ", leaked);
            logger.LogWarning("Menu/Toolbar leak detected: {LeakedObjects}", leakedNames);
            return BenchmarkResult.Fail($"Objects leaked: {leakedNames}", metrics);
        }

        if (memoryDelta.WorkingSetDelta > 50 * 1024 * 1024)
        {
            return BenchmarkResult.Warn(
                $"Native memory growth {memoryDelta.WorkingSetDelta / (1024.0 * 1024):F1} MB exceeds 50 MB threshold",
                metrics);
        }

        logger.LogInformation(
            "All {Count} menu/toolbar objects collected successfully",
            trackedObjects.Count);
        return BenchmarkResult.Pass(metrics);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task CreateAndDestroyMenuPage(
        Window window,
        Dictionary<string, WeakReference<object>> trackedObjects,
        CancellationToken cancellationToken)
    {
        // Create a separate page with menus and toolbars
        CreateMenuPage(window, trackedObjects);

        // Allow handlers to connect
        await Task.Delay(100, cancellationToken);

        // Tear down the menu page
        TearDownMenuPage(window, trackedObjects);

        // Restore this test page as the window's page
        window.Page = this;
        Content = new Label { Text = "Menu/Toolbar test complete" };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateMenuPage(
        Window window,
        Dictionary<string, WeakReference<object>> trackedObjects)
    {
        var menuPage = new ContentPage
        {
            Title = "Menu Test Page",
            Content = new Label { Text = "Page with menus and toolbars" },
        };
        trackedObjects["MenuPage"] = new WeakReference<object>(menuPage);

        // Build menu bar structure
        var fileMenu = new MenuBarItem { Text = "File" };
        trackedObjects["MenuBarItem.File"] = new WeakReference<object>(fileMenu);

        var newItem = new MenuFlyoutItem { Text = "New" };
        newItem.Clicked += (_, _) => { };
        fileMenu.Add(newItem);
        trackedObjects["MenuFlyoutItem.New"] = new WeakReference<object>(newItem);

        var openItem = new MenuFlyoutItem { Text = "Open" };
        openItem.Clicked += (_, _) => { };
        fileMenu.Add(openItem);
        trackedObjects["MenuFlyoutItem.Open"] = new WeakReference<object>(openItem);

        var separator = new MenuFlyoutSeparator();
        fileMenu.Add(separator);
        trackedObjects["MenuFlyoutSeparator"] = new WeakReference<object>(separator);

        var recentSubMenu = new MenuFlyoutSubItem { Text = "Recent" };
        trackedObjects["MenuFlyoutSubItem.Recent"] = new WeakReference<object>(recentSubMenu);

        var recentItem1 = new MenuFlyoutItem { Text = "File1.txt" };
        recentItem1.Clicked += (_, _) => { };
        recentSubMenu.Add(recentItem1);
        trackedObjects["MenuFlyoutItem.Recent1"] = new WeakReference<object>(recentItem1);

        var recentItem2 = new MenuFlyoutItem { Text = "File2.txt" };
        recentItem2.Clicked += (_, _) => { };
        recentSubMenu.Add(recentItem2);
        trackedObjects["MenuFlyoutItem.Recent2"] = new WeakReference<object>(recentItem2);

        fileMenu.Add(recentSubMenu);

        var exitItem = new MenuFlyoutItem { Text = "Exit" };
        exitItem.Clicked += (_, _) => { };
        fileMenu.Add(exitItem);
        trackedObjects["MenuFlyoutItem.Exit"] = new WeakReference<object>(exitItem);

        menuPage.MenuBarItems.Add(fileMenu);

        // Second menu
        var editMenu = new MenuBarItem { Text = "Edit" };
        trackedObjects["MenuBarItem.Edit"] = new WeakReference<object>(editMenu);

        var copyItem = new MenuFlyoutItem { Text = "Copy" };
        copyItem.Clicked += (_, _) => { };
        editMenu.Add(copyItem);
        trackedObjects["MenuFlyoutItem.Copy"] = new WeakReference<object>(copyItem);

        var pasteItem = new MenuFlyoutItem { Text = "Paste" };
        pasteItem.Clicked += (_, _) => { };
        editMenu.Add(pasteItem);
        trackedObjects["MenuFlyoutItem.Paste"] = new WeakReference<object>(pasteItem);

        menuPage.MenuBarItems.Add(editMenu);

        // Add toolbar items
        var toolbar1 = new ToolbarItem { Text = "Save", Order = ToolbarItemOrder.Primary };
        toolbar1.Clicked += (_, _) => { };
        menuPage.ToolbarItems.Add(toolbar1);
        trackedObjects["ToolbarItem.Save"] = new WeakReference<object>(toolbar1);

        var toolbar2 = new ToolbarItem { Text = "Settings", Order = ToolbarItemOrder.Secondary };
        toolbar2.Clicked += (_, _) => { };
        menuPage.ToolbarItems.Add(toolbar2);
        trackedObjects["ToolbarItem.Settings"] = new WeakReference<object>(toolbar2);

        window.Page = menuPage;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void TearDownMenuPage(
        Window window,
        Dictionary<string, WeakReference<object>> trackedObjects)
    {
        if (window.Page is not ContentPage menuPage)
            return;

        // Track page handler
        if (menuPage.Handler is object pageHandler)
        {
            trackedObjects["MenuPage.Handler"] = new WeakReference<object>(pageHandler);
        }

        // Disconnect toolbar items
        foreach (var item in menuPage.ToolbarItems)
        {
            item.Handler?.DisconnectHandler();
        }

        menuPage.ToolbarItems.Clear();

        // Clear menu bar items and disconnect handlers
        foreach (var menuBarItem in menuPage.MenuBarItems)
        {
            foreach (var child in menuBarItem)
            {
                if (child is MenuFlyoutSubItem subItem)
                {
                    foreach (var subChild in subItem)
                    {
                        if (subChild is IElement elem && elem.Handler is not null)
                        {
                            elem.Handler.DisconnectHandler();
                        }
                    }

                    subItem.Clear();
                }

                if (child is IElement element && element.Handler is not null)
                {
                    element.Handler.DisconnectHandler();
                }
            }

            menuBarItem.Clear();
            if (menuBarItem is IElement mbi && mbi.Handler is not null)
            {
                mbi.Handler.DisconnectHandler();
            }
        }

        menuPage.MenuBarItems.Clear();

        // Disconnect page handler
        menuPage.Handler?.DisconnectHandler();
    }
}
