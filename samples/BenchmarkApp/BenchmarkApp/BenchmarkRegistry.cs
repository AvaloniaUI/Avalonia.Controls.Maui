

using System.Reflection;
using BenchmarkApp.Tests;

namespace BenchmarkApp;

/// <summary>
/// Registry of benchmark test pages. Tests are registered explicitly to support trimming and AOT.
/// </summary>
public static class BenchmarkRegistry
{
    private static readonly Dictionary<string, (string? Description, Func<BenchmarkTestPage> Factory)> Tests;

    static BenchmarkRegistry()
    {
        Tests = new Dictionary<string, (string?, Func<BenchmarkTestPage>)>(StringComparer.OrdinalIgnoreCase);
        Register<ButtonCreationBenchmark>();
        Register<HandlerDisconnectLeakBenchmark>();
        Register<PageNavigationLeakBenchmark>();
        Register<RepeatedCreationLeakBenchmark>();
        Register<ShellNavigationLeakBenchmark>();
        Register<ImageSourceLeakBenchmark>();
        Register<BindableLayoutLeakBenchmark>();
        Register<ScrollViewLeakBenchmark>();
        Register<GestureRecognizerLeakBenchmark>();
        Register<InputControlLeakBenchmark>();
        Register<CollectionViewLeakBenchmark>();
        Register<TabbedPageLeakBenchmark>();
        Register<ControlCreationPerformanceBenchmark>();
        Register<AlohaTabBarNavigationLeakBenchmark>();
        Register<PropertyChangedSubscriptionLeakBenchmark>();
        Register<RichCardBindableLayoutLeakBenchmark>();
        Register<DynamicChildCreationLeakBenchmark>();
        Register<GradientAndShadowLeakBenchmark>();
        Register<ShellRouteNavigationLeakBenchmark>();
        Register<DateTimePickerLeakBenchmark>();
        Register<RadioButtonStepperLeakBenchmark>();
        Register<ActivityIndicatorProgressBarLeakBenchmark>();
        Register<ContentViewLeakBenchmark>();
        Register<ShapeHandlerLeakBenchmark>();
        Register<ImageButtonLeakBenchmark>();
        Register<CompositeViewLeakBenchmark>();
        Register<WebViewMiscLeakBenchmark>();
        Register<MenuAndToolbarLeakBenchmark>();
        Register<CompatibilityHandlerLeakBenchmark>();
        Register<FlyoutPageLeakBenchmark>();
        Register<ShellFlyoutItemChurnLeakBenchmark>();
        Register<NavigationStackDepthLeakBenchmark>();
        Register<ShellSearchHandlerLeakBenchmark>();
        Register<DataTemplateRecyclingLeakBenchmark>();
        Register<ModalPageLeakBenchmark>();
        Register<CarouselViewSourceChurnLeakBenchmark>();
        Register<SwipeViewLeakBenchmark>();
        Register<RefreshViewLeakBenchmark>();
        Register<BorderIndicatorViewLeakBenchmark>();
        Register<AlertDialogLeakBenchmark>();
        Register<DispatcherTimerLeakBenchmark>();
        Register<HandlerReconnectionLeakBenchmark>();
        Register<BindingContextChurnLeakBenchmark>();
        Register<GraphicsViewLeakBenchmark>();
        Register<PropertyMappingPerformanceBenchmark>();
        Register<LayoutPerformanceBenchmark>();
        Register<NavigationPerformanceBenchmark>();
        Register<IdleRenderMemoryGrowthBenchmark>();
        Register<ShellTabSwitchMemoryGrowthBenchmark>();
        Register<CompositionVisualLeakBenchmark>();
        Register<NavigationWithToolbarChurnBenchmark>();
        Register<GestureDoubleTapCancellationLeakBenchmark>();
        Register<ShellTabSwitchWithNavigationAndToolbarBenchmark>();
        Register<ModalWithGesturesAndBindingsLeakBenchmark>();
        Register<RefreshViewWithNavigationAndBindingChurnBenchmark>();
        Register<CompoundPageLifecycleStressLeakBenchmark>();
        Register<ShellNavigationWithModalsAndFlyoutBenchmark>();
        Register<PlatformViewLeakBenchmark>();
        Register<ShellItemSwitchLeakBenchmark>();
        Register<ShellContentNavigationBenchmark>();
        Register<ShellSectionSwitchLeakBenchmark>();
        Register<TransformCleanupLeakBenchmark>();
        Register<BorderShapeSubscriptionLeakBenchmark>();
        Register<LayoutChildRemovalLeakBenchmark>();
        Register<BorderStrokeShapeLeakBenchmark>();

        // Phase 3: Enhanced leak detection tests
        Register<FinalizerQueueGrowthBenchmark>();

        // Phase 4: Soak/stress tests
        Register<HandlerCreateDestroySoakBenchmark>();
        Register<NavigationSoakBenchmark>();
        Register<PropertyUpdateSoakBenchmark>();

        // Phase 7: Render performance & event auditing
        Register<LayoutThrashingBenchmark>();
        Register<EventSubscriptionBalanceBenchmark>();

        // Phase 8: Targeted event subscription leak tests
        Register<ShellHandlerFlyoutContentLeakBenchmark>();
        Register<ShellFlyoutButtonClickLeakBenchmark>();
        Register<NavigationViewDetachLeakBenchmark>();
    }

    /// <summary>
    /// Returns the names and descriptions of all registered benchmark tests.
    /// </summary>
    public static IReadOnlyDictionary<string, (string? Description, Func<BenchmarkTestPage> Factory)> GetTests() => Tests;

    /// <summary>
    /// Creates an instance of the benchmark test page with the given name.
    /// </summary>
    /// <param name="name">The benchmark test name (case-insensitive).</param>
    /// <returns>The test page instance, or <c>null</c> if the name is not found.</returns>
    public static BenchmarkTestPage? CreateTest(string name)
    {
        if (Tests.TryGetValue(name, out var entry))
        {
            return entry.Factory();
        }

        return null;
    }

    /// <summary>
    /// Registers a benchmark test type. The type must have <see cref="BenchmarkTestAttribute"/>
    /// and a parameterless constructor.
    /// </summary>
    private static void Register<T>()
        where T : BenchmarkTestPage, new()
    {
        var attr = typeof(T).GetCustomAttribute<BenchmarkTestAttribute>()
            ?? throw new InvalidOperationException($"{typeof(T).Name} is missing [BenchmarkTest] attribute.");
        Tests[attr.Name] = (attr.Description, static () => new T());
    }
}
