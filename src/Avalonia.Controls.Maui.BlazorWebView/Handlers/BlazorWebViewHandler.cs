using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Avalonia.Controls.Maui.BlazorWebView.Compatibility;
using Avalonia.Controls.Maui.BlazorWebView.Hosting;
using Avalonia.Controls.Maui.BlazorWebView.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Handlers;
using AvaloniaControl = Avalonia.Controls.Control;
using AvaloniaDispatcher = Avalonia.Threading.Dispatcher;
using AvaloniaRect = Avalonia.Rect;
using AvaloniaSize = Avalonia.Size;
using NativeWebView = Avalonia.Controls.NativeWebView;
using MauiBlazorWebViewHandler = Microsoft.AspNetCore.Components.WebView.Maui.BlazorWebViewHandler;
using MauiRect = Microsoft.Maui.Graphics.Rect;
using MauiSize = Microsoft.Maui.Graphics.Size;

namespace Avalonia.Controls.Maui.BlazorWebView.Handlers;

/// <summary>
/// Avalonia handler for the official MAUI <see cref="Microsoft.AspNetCore.Components.WebView.Maui.BlazorWebView"/>.
/// </summary>
public class BlazorWebViewHandler : MauiBlazorWebViewHandler
{
    private const int InitializationScriptMaxAttempts = 5;
    private static readonly TimeSpan InitializationScriptRetryDelay = TimeSpan.FromMilliseconds(50);

    /// <summary>
    /// Property mapper for the Avalonia Blazor WebView handler.
    /// </summary>
    public static PropertyMapper<IBlazorWebView, BlazorWebViewHandler> Mapper =
        new(Avalonia.Controls.Maui.Handlers.ViewHandler.ViewMapper)
        {
            [nameof(IBlazorWebView.HostPage)] = MapAvaloniaHostPage,
            [nameof(IBlazorWebView.RootComponents)] = MapAvaloniaRootComponents,
        };

    private string? _hostPage;
    private RootComponentsCollection? _rootComponents;
    private AvaloniaBlazorWebViewManager? _webViewManager;
    private readonly HashSet<string> _attachedRootComponentSelectors = new(StringComparer.Ordinal);
    private readonly object _rootComponentUpdateLock = new();
    private readonly object _documentTrustLock = new();
    private Task _rootComponentUpdateTask = Task.CompletedTask;
    private int _rootComponentUpdateGeneration;
    private IDisposable? _gestureManager;
    private ILogger _logger = NullLogger.Instance;
    private Uri? _trustedDocumentUri;
    private string? _documentMessageToken;
    private int _navigationGeneration;
    private bool _isDisconnecting;
    private bool _isAdapterReady;
    private bool _isLoaded;
    private bool _developerToolsEnabled;

    /// <summary>
    /// Initializes a new instance of <see cref="BlazorWebViewHandler"/>.
    /// </summary>
    public BlazorWebViewHandler()
        : base(Mapper)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="BlazorWebViewHandler"/> with custom mappings.
    /// </summary>
    /// <param name="mapper">The property mapper to use.</param>
    public BlazorWebViewHandler(PropertyMapper? mapper)
        : base(mapper ?? Mapper)
    {
    }

    private NativeWebView? NativeWebView =>
        ((IElementHandler)this).PlatformView as NativeWebView;

    /// <inheritdoc />
    protected override object CreatePlatformView()
    {
        var nativeWebView = new NativeWebView();
        nativeWebView.EnvironmentRequested += OnEnvironmentRequested;

        if (((IElementHandler)this).VirtualView is IBlazorWebView virtualView)
            virtualView.BlazorWebViewInitializing(new AvaloniaBlazorWebViewInitializingEventArgs(nativeWebView));

        return nativeWebView;
    }

    /// <inheritdoc />
    protected override void ConnectHandler(object platformView)
    {
        base.ConnectHandler(platformView);

        if (platformView is NativeWebView nativeWebView)
        {
            nativeWebView.AdapterCreated += OnAdapterCreated;
            nativeWebView.AdapterDestroyed += OnAdapterDestroyed;
            nativeWebView.NavigationStarted += OnNavigationStarted;
            nativeWebView.NavigationCompleted += OnNavigationCompleted;
            nativeWebView.WebMessageReceived += OnWebMessageReceived;
            nativeWebView.NewWindowRequested += OnNewWindowRequested;

            AttachPlatformViewEvents(nativeWebView);
        }

        _isDisconnecting = false;
        if (MauiContext?.Services is { } services)
        {
            _logger = services.GetService<ILogger<BlazorWebViewHandler>>() ?? NullLogger<BlazorWebViewHandler>.Instance;
            _developerToolsEnabled = MauiBlazorWebViewCompatibility.AreDeveloperToolsEnabled(services, _logger);
        }

        if (((IElementHandler)this).VirtualView is IView controlsView && _gestureManager is null)
            _gestureManager = Avalonia.Controls.Maui.Platform.GestureManager.CreateIfNeeded(controlsView);

        if (platformView is NativeWebView connectedWebView && connectedWebView.AdapterInfo is not null)
            HandleAdapterReady(connectedWebView);

        StartWebViewCoreIfPossible();
    }

    /// <inheritdoc />
    protected override void DisconnectHandler(object platformView)
    {
        _isDisconnecting = true;

        if (platformView is NativeWebView nativeWebView)
        {
            if (_isLoaded)
            {
                _isLoaded = false;
                TrySendUnloaded();
            }

            DetachPlatformViewEvents(nativeWebView);
            nativeWebView.EnvironmentRequested -= OnEnvironmentRequested;
            nativeWebView.AdapterCreated -= OnAdapterCreated;
            nativeWebView.AdapterDestroyed -= OnAdapterDestroyed;
            nativeWebView.NavigationStarted -= OnNavigationStarted;
            nativeWebView.NavigationCompleted -= OnNavigationCompleted;
            nativeWebView.WebMessageReceived -= OnWebMessageReceived;
            nativeWebView.NewWindowRequested -= OnNewWindowRequested;
            nativeWebView.Stop();
            Avalonia.Controls.Maui.Extensions.ViewExtensions.DisposeClipSubscription(nativeWebView);
            nativeWebView.RenderTransform = null;
        }

        _gestureManager?.Dispose();
        _gestureManager = null;
        _isAdapterReady = false;
        InvalidateDocumentTrust();

        if (_rootComponents is not null)
            _rootComponents.CollectionChanged -= OnRootComponentsCollectionChanged;

        AvaloniaBlazorWebViewManager? manager;
        lock (_rootComponentUpdateLock)
        {
            manager = _webViewManager;
            _webViewManager = null;
            _rootComponentUpdateGeneration++;
            _rootComponentUpdateTask = Task.CompletedTask;
            _attachedRootComponentSelectors.Clear();
        }

        if (manager is not null)
            _ = DisposeWebViewManagerAsync(manager, _logger);

        base.DisconnectHandler(platformView);
    }

    /// <inheritdoc />
    public override IFileProvider CreateFileProvider(string contentRootDir)
    {
        return new AvaloniaResourceFileProvider(contentRootDir);
    }

    /// <inheritdoc />
    public override Task<bool> TryDispatchAsync(Action<IServiceProvider> workItem)
    {
        ArgumentNullException.ThrowIfNull(workItem);
        return _webViewManager?.TryDispatchAsync(workItem) ?? Task.FromResult(false);
    }

    /// <inheritdoc />
    public override MauiSize GetDesiredSize(double widthConstraint, double heightConstraint)
    {
        if (GetLayoutView() is not { } platformView)
            return MauiSize.Zero;

        if (AvaloniaDispatcher.UIThread.CheckAccess())
            return MeasureCore(platformView, widthConstraint, heightConstraint);

        return AvaloniaDispatcher.UIThread.InvokeAsync(() =>
            MeasureCore(platformView, widthConstraint, heightConstraint)).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public override void PlatformArrange(MauiRect frame)
    {
        if (GetLayoutView() is not { } platformView)
            return;

        if (AvaloniaDispatcher.UIThread.CheckAccess())
            Arrange(platformView, frame);
        else
            AvaloniaDispatcher.UIThread.Invoke(() => Arrange(platformView, frame));
    }

    /// <summary>
    /// Maps the Blazor host page.
    /// </summary>
    /// <param name="handler">The handler.</param>
    /// <param name="webView">The virtual view.</param>
    public static void MapAvaloniaHostPage(BlazorWebViewHandler handler, IBlazorWebView webView)
    {
        handler._hostPage = webView.HostPage;
        handler.StartWebViewCoreIfPossible();
    }

    /// <summary>
    /// Maps the Blazor root components collection.
    /// </summary>
    /// <param name="handler">The handler.</param>
    /// <param name="webView">The virtual view.</param>
    public static void MapAvaloniaRootComponents(BlazorWebViewHandler handler, IBlazorWebView webView)
    {
        if (handler._rootComponents is not null)
            handler._rootComponents.CollectionChanged -= handler.OnRootComponentsCollectionChanged;

        handler._rootComponents = webView.RootComponents;
        handler._rootComponents.CollectionChanged += handler.OnRootComponentsCollectionChanged;

        if (handler._webViewManager is not null)
        {
            var rootComponents = handler._rootComponents.ToArray();
            handler.QueueRootComponentUpdate((manager, generation) =>
                handler.ReplaceRootComponentsAsync(manager, generation, rootComponents));
        }
        else
            handler.StartWebViewCoreIfPossible();
    }

    private AvaloniaControl? GetLayoutView() =>
        ((IViewHandler)this).ContainerView as AvaloniaControl ?? NativeWebView;

    private static MauiSize MeasureCore(AvaloniaControl platformView, double widthConstraint, double heightConstraint)
    {
        var avaloniaConstraint = new AvaloniaSize(
            double.IsNaN(widthConstraint) ? double.PositiveInfinity : widthConstraint,
            double.IsNaN(heightConstraint) ? double.PositiveInfinity : heightConstraint);

        platformView.Measure(avaloniaConstraint);

        var contentSize = platformView.DesiredSize.Deflate(platformView.Margin);
        return new MauiSize(contentSize.Width, contentSize.Height);
    }

    private static void Arrange(AvaloniaControl platformView, MauiRect frame)
    {
        var arrangeRect = new AvaloniaRect(frame.X, frame.Y, frame.Width, frame.Height)
            .Inflate(platformView.Margin);

        if (!platformView.IsMeasureValid)
            platformView.Measure(arrangeRect.Size);

        platformView.Arrange(arrangeRect);
    }

    // The neutral MAUI handler cannot inherit the repository's generic Avalonia
    // handler, so it preserves the same container and visual lifecycle behavior here.
    /// <inheritdoc />
    protected override void SetupContainer()
    {
        if (NativeWebView is not { } platformView)
            return;

        var parentPanel = platformView.Parent as Avalonia.Controls.Panel;
        var index = parentPanel?.Children.IndexOf(platformView) ?? -1;
        using var reparenting = platformView.BeginReparenting(yieldOnLayoutBeforeExiting: false);

        if (parentPanel is not null && index >= 0)
            parentPanel.Children.RemoveAt(index);

        var containerView = new Avalonia.Controls.Maui.Platform.ContentView();
        containerView.Children.Add(platformView);
        MauiBlazorWebViewCompatibility.SetContainerView(this, containerView);

        if (platformView.Margin != default)
        {
            containerView.Margin = platformView.Margin;
            platformView.Margin = default;
        }

        if (parentPanel is not null && index >= 0)
            parentPanel.Children.Insert(Math.Min(index, parentPanel.Children.Count), containerView);
    }

    /// <inheritdoc />
    protected override void RemoveContainer()
    {
        if (ContainerView is Avalonia.Controls.Maui.Platform.ContentView container &&
            NativeWebView is { } platformView)
        {
            using var reparenting = platformView.BeginReparenting(yieldOnLayoutBeforeExiting: false);

            if (container.Margin != default)
            {
                platformView.Margin = container.Margin;
                container.Margin = default;
            }

            var parentPanel = container.Parent as Avalonia.Controls.Panel;
            var index = parentPanel?.Children.IndexOf(container) ?? -1;

            if (parentPanel is not null && index >= 0)
                parentPanel.Children.RemoveAt(index);

            container.Children.Remove(platformView);

            if (parentPanel is not null && index >= 0)
                parentPanel.Children.Insert(Math.Min(index, parentPanel.Children.Count), platformView);
        }

        MauiBlazorWebViewCompatibility.SetContainerView(this, null);
    }

    private void OnEnvironmentRequested(object? sender, Avalonia.Controls.WebViewEnvironmentRequestedEventArgs e)
    {
        e.EnableDevTools = _developerToolsEnabled;
    }

    private void OnAdapterCreated(object? sender, EventArgs e)
    {
        if (sender is NativeWebView nativeWebView)
            HandleAdapterReady(nativeWebView);
    }

    private void HandleAdapterReady(NativeWebView nativeWebView)
    {
        if (_isAdapterReady || _isDisconnecting)
            return;

        _isAdapterReady = true;

        if (((IElementHandler)this).VirtualView is IBlazorWebView virtualView)
            virtualView.BlazorWebViewInitialized(new AvaloniaBlazorWebViewInitializedEventArgs(nativeWebView));

        if (_webViewManager is { } webViewManager &&
            ((IElementHandler)this).VirtualView is IBlazorWebView existingVirtualView)
        {
            webViewManager.Navigate(existingVirtualView.StartPath);
        }
        else
        {
            StartWebViewCoreIfPossible();
        }
    }

    private void OnAdapterDestroyed(object? sender, EventArgs e)
    {
        _isAdapterReady = false;
        InvalidateDocumentTrust();
    }

    private void AttachPlatformViewEvents(NativeWebView platformView)
    {
        platformView.AttachedToVisualTree += OnPlatformViewAttachedToVisualTree;
        platformView.DetachedFromVisualTree += OnPlatformViewDetachedFromVisualTree;
        platformView.GotFocus += OnPlatformViewGotFocus;
        platformView.LostFocus += OnPlatformViewLostFocus;
        platformView.PropertyChanged += OnPlatformViewPropertyChanged;

        if (platformView.Parent is not null)
        {
            _isLoaded = true;
            TrySendLoaded();
        }

        if (platformView.IsFocused)
            SetFocused(true);
    }

    private void DetachPlatformViewEvents(NativeWebView platformView)
    {
        platformView.AttachedToVisualTree -= OnPlatformViewAttachedToVisualTree;
        platformView.DetachedFromVisualTree -= OnPlatformViewDetachedFromVisualTree;
        platformView.GotFocus -= OnPlatformViewGotFocus;
        platformView.LostFocus -= OnPlatformViewLostFocus;
        platformView.PropertyChanged -= OnPlatformViewPropertyChanged;
    }

    private void OnPlatformViewAttachedToVisualTree(object? sender, Avalonia.VisualTreeAttachmentEventArgs e)
    {
        if (_isLoaded)
            return;

        _isLoaded = true;
        TrySendLoaded();
    }

    private void OnPlatformViewDetachedFromVisualTree(object? sender, Avalonia.VisualTreeAttachmentEventArgs e)
    {
        if (!_isLoaded)
            return;

        _isLoaded = false;
        TrySendUnloaded();
        SetFocused(false);
    }

    private void OnPlatformViewGotFocus(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        SetFocused(true);

    private void OnPlatformViewLostFocus(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        SetFocused(false);

    private void OnPlatformViewPropertyChanged(object? sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != Avalonia.Visual.BoundsProperty || e.NewValue is not AvaloniaRect newBounds)
            return;

        var oldBounds = e.OldValue is AvaloniaRect oldRect ? oldRect : default;
        if (newBounds.Width.Equals(oldBounds.Width) && newBounds.Height.Equals(oldBounds.Height))
            return;

        if (((IElementHandler)this).VirtualView is IVisualElementController controller)
            controller.PlatformSizeChanged();
    }

    private void SetFocused(bool isFocused)
    {
        if (((IElementHandler)this).VirtualView is not IView virtualView || virtualView.IsFocused == isFocused)
            return;

        virtualView.IsFocused = isFocused;
    }

    private void TrySendLoaded()
    {
        if (((IElementHandler)this).VirtualView is VisualElement visualElement)
            VisualElementLifecycle.SendLoaded(visualElement, false);
    }

    private void TrySendUnloaded()
    {
        if (((IElementHandler)this).VirtualView is VisualElement visualElement)
            VisualElementLifecycle.SendUnloaded(visualElement, false);
    }

    private static class VisualElementLifecycle
    {
        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "SendLoaded")]
        public static extern void SendLoaded(VisualElement element, bool updateWiring);

        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "SendUnloaded")]
        public static extern void SendUnloaded(VisualElement element, bool updateWiring);
    }

    private void StartWebViewCoreIfPossible()
    {
        if (_isDisconnecting ||
            !_isAdapterReady ||
            _webViewManager is not null ||
            NativeWebView is not { } nativeWebView ||
            string.IsNullOrWhiteSpace(_hostPage) ||
            ((IElementHandler)this).VirtualView is not IBlazorWebView virtualView ||
            MauiContext?.Services is not { } services)
        {
            return;
        }

        var (contentRootDir, hostPageRelativePath) = GetHostPagePaths(_hostPage);

        var fileProvider = virtualView.CreateFileProvider(contentRootDir);
        var dispatcher = new AvaloniaBlazorDispatcher(services.GetRequiredService<IDispatcher>());
        BlazorWebViewLoopbackHost? host = null;
        AvaloniaBlazorWebViewManager? webViewManager = null;

        try
        {
            host = BlazorWebViewLoopbackHost.Create(_logger);
            webViewManager = new AvaloniaBlazorWebViewManager(
                nativeWebView,
                services,
                dispatcher,
                host,
                fileProvider,
                virtualView.JSComponents,
                contentRootDir,
                hostPageRelativePath,
                _logger);
            host = null;

            MauiBlazorWebViewCompatibility.AttachStaticContentHotReload(webViewManager, _logger);

            if (_rootComponents is not null)
                AddInitialRootComponents(webViewManager, _rootComponents.ToArray());

            lock (_rootComponentUpdateLock)
                _webViewManager = webViewManager;

            webViewManager.Navigate(virtualView.StartPath);
            webViewManager = null;
        }
        finally
        {
            if (webViewManager is not null)
            {
                lock (_rootComponentUpdateLock)
                {
                    if (ReferenceEquals(_webViewManager, webViewManager))
                        _webViewManager = null;

                    _attachedRootComponentSelectors.Clear();
                }

                DisposeWebViewManagerSynchronously(webViewManager, _logger);
            }

            if (host is not null)
                DisposeLoopbackHostSynchronously(host, _logger);
        }
    }

    private async Task AddRootComponentsAsync(
        AvaloniaBlazorWebViewManager webViewManager,
        int generation,
        IEnumerable<RootComponent> rootComponents)
    {
        foreach (var rootComponent in rootComponents)
        {
            if (!IsRootComponentUpdateCurrent(webViewManager, generation))
                return;

            await AddRootComponentAsync(webViewManager, rootComponent);

            if (!TryUpdateAttachedRootComponentSelector(
                webViewManager,
                generation,
                rootComponent.Selector!,
                attach: true))
            {
                return;
            }
        }
    }

    private async Task RemoveRootComponentsAsync(
        AvaloniaBlazorWebViewManager webViewManager,
        int generation,
        IEnumerable<RootComponent> rootComponents)
    {
        foreach (var rootComponent in rootComponents)
        {
            if (!IsRootComponentUpdateCurrent(webViewManager, generation))
                return;

            await RemoveRootComponentAsync(webViewManager, rootComponent);

            if (!TryUpdateAttachedRootComponentSelector(
                webViewManager,
                generation,
                rootComponent.Selector!,
                attach: false))
            {
                return;
            }
        }
    }

    private void OnRootComponentsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var newItems = e.NewItems?.Cast<RootComponent>().ToArray();
        var oldItems = e.OldItems?.Cast<RootComponent>().ToArray();
        var resetItems = e.Action == NotifyCollectionChangedAction.Reset
            ? _rootComponents?.ToArray()
            : null;

        QueueRootComponentUpdate((manager, generation) =>
            OnRootComponentsCollectionChangedAsync(manager, generation, e.Action, newItems, oldItems, resetItems));
    }

    private async Task OnRootComponentsCollectionChangedAsync(
        AvaloniaBlazorWebViewManager webViewManager,
        int generation,
        NotifyCollectionChangedAction action,
        RootComponent[]? newItems,
        RootComponent[]? oldItems,
        RootComponent[]? resetItems)
    {
        if (!IsRootComponentUpdateCurrent(webViewManager, generation))
            return;

        switch (action)
        {
            case NotifyCollectionChangedAction.Add:
                await AddRootComponentsAsync(webViewManager, generation, newItems!);
                break;
            case NotifyCollectionChangedAction.Remove:
                await RemoveRootComponentsAsync(webViewManager, generation, oldItems!);
                break;
            case NotifyCollectionChangedAction.Replace:
                await RemoveRootComponentsAsync(webViewManager, generation, oldItems!);
                await AddRootComponentsAsync(webViewManager, generation, newItems!);
                break;
            case NotifyCollectionChangedAction.Reset:
                await RemoveAllRootComponentsAsync(webViewManager, generation);
                if (resetItems is not null)
                    await AddRootComponentsAsync(webViewManager, generation, resetItems);
                break;
            case NotifyCollectionChangedAction.Move:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action));
        }
    }

    private async Task ReplaceRootComponentsAsync(
        AvaloniaBlazorWebViewManager webViewManager,
        int generation,
        IEnumerable<RootComponent> rootComponents)
    {
        await RemoveAllRootComponentsAsync(webViewManager, generation);
        await AddRootComponentsAsync(webViewManager, generation, rootComponents);
    }

    private void AddInitialRootComponents(WebViewManager webViewManager, IEnumerable<RootComponent> rootComponents)
    {
        foreach (var rootComponent in rootComponents)
        {
            AddRootComponentAsync(webViewManager, rootComponent).GetAwaiter().GetResult();
            lock (_rootComponentUpdateLock)
                _attachedRootComponentSelectors.Add(rootComponent.Selector!);
        }
    }

    private async Task RemoveAllRootComponentsAsync(
        AvaloniaBlazorWebViewManager webViewManager,
        int generation)
    {
        string[] attachedSelectors;
        lock (_rootComponentUpdateLock)
        {
            if (!IsRootComponentUpdateCurrentCore(webViewManager, generation))
                return;

            attachedSelectors = _attachedRootComponentSelectors.ToArray();
        }

        foreach (var selector in attachedSelectors)
        {
            if (!IsRootComponentUpdateCurrent(webViewManager, generation))
                return;

            await webViewManager.RemoveRootComponentAsync(selector);

            if (!TryUpdateAttachedRootComponentSelector(
                webViewManager,
                generation,
                selector,
                attach: false))
            {
                return;
            }
        }
    }

    private static Task AddRootComponentAsync(WebViewManager webViewManager, RootComponent rootComponent)
    {
        if (string.IsNullOrWhiteSpace(rootComponent.Selector))
            throw new InvalidOperationException("RootComponent requires a value for its Selector property, but no value was set.");

        if (rootComponent.ComponentType is null)
            throw new InvalidOperationException("RootComponent requires a value for its ComponentType property, but no value was set.");

        var parameters = rootComponent.Parameters is null
            ? ParameterView.Empty
            : ParameterView.FromDictionary(rootComponent.Parameters);

        return webViewManager.AddRootComponentAsync(rootComponent.ComponentType, rootComponent.Selector, parameters);
    }

    private static Task RemoveRootComponentAsync(WebViewManager webViewManager, RootComponent rootComponent)
    {
        if (string.IsNullOrWhiteSpace(rootComponent.Selector))
            throw new InvalidOperationException("RootComponent requires a value for its Selector property, but no value was set.");

        return webViewManager.RemoveRootComponentAsync(rootComponent.Selector);
    }

    private async void OnNavigationCompleted(object? sender, Avalonia.Controls.WebViewNavigationCompletedEventArgs e)
    {
        var webViewManager = _webViewManager;

        if (!e.IsSuccess ||
            NativeWebView is not { } nativeWebView ||
            webViewManager is null ||
            e.Request is not { } request ||
            !webViewManager.AppBaseUri.IsBaseOf(request))
        {
            return;
        }

        string documentToken;
        int navigationGeneration;

        // Avalonia's message event does not expose the sending frame URI. A token
        // generated for each completed app document provides that trust boundary.
        lock (_documentTrustLock)
        {
            documentToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            _documentMessageToken = documentToken;
            _trustedDocumentUri = request;
            navigationGeneration = _navigationGeneration;
        }

        await InjectInitializationScriptAsync(
            nativeWebView,
            webViewManager,
            documentToken,
            navigationGeneration);
    }

    private async Task InjectInitializationScriptAsync(
        NativeWebView nativeWebView,
        AvaloniaBlazorWebViewManager webViewManager,
        string documentToken,
        int navigationGeneration)
    {
        for (var attempt = 0; attempt < InitializationScriptMaxAttempts; attempt++)
        {
            if (!IsDocumentCurrent(webViewManager, documentToken, navigationGeneration))
                return;

            try
            {
                await InvokeInitializationScriptAsync(nativeWebView, documentToken);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Unable to inject the Avalonia BlazorWebView bridge (attempt {Attempt}).", attempt + 1);

                if (attempt == InitializationScriptMaxAttempts - 1)
                {
                    InvalidateDocumentTrust(documentToken, navigationGeneration);
                    break;
                }
            }

            await Task.Delay(InitializationScriptRetryDelay).ConfigureAwait(false);
        }
    }

    private static Task InvokeInitializationScriptAsync(NativeWebView nativeWebView, string documentToken)
    {
        var script = AvaloniaBlazorWebViewScripts.CreateInitializationScript(documentToken);

        if (AvaloniaDispatcher.UIThread.CheckAccess())
            return nativeWebView.InvokeScript(script);

        return AvaloniaDispatcher.UIThread.InvokeAsync(() =>
            nativeWebView.InvokeScript(script));
    }

    private bool IsDocumentCurrent(
        AvaloniaBlazorWebViewManager webViewManager,
        string documentToken,
        int navigationGeneration)
    {
        lock (_documentTrustLock)
        {
            return !_isDisconnecting &&
                ReferenceEquals(_webViewManager, webViewManager) &&
                navigationGeneration == _navigationGeneration &&
                string.Equals(_documentMessageToken, documentToken, StringComparison.Ordinal);
        }
    }

    private static async Task DisposeWebViewManagerAsync(AvaloniaBlazorWebViewManager manager, ILogger logger)
    {
        try
        {
            await manager.DisposeAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to dispose the Avalonia BlazorWebView manager.");
        }
    }

    private static void DisposeWebViewManagerSynchronously(AvaloniaBlazorWebViewManager manager, ILogger logger)
    {
        try
        {
            manager.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to dispose the Avalonia BlazorWebView manager.");
        }
    }

    private static void DisposeLoopbackHostSynchronously(BlazorWebViewLoopbackHost host, ILogger logger)
    {
        try
        {
            host.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to dispose the Avalonia BlazorWebView content host.");
        }
    }

    private void OnNavigationStarted(object? sender, Avalonia.Controls.WebViewNavigationStartingEventArgs e)
    {
        if (e.Request is not { } request)
            return;

        if (_webViewManager is not { } webViewManager ||
            ((IElementHandler)this).VirtualView is not IBlazorWebView virtualView)
        {
            InvalidateDocumentTrust();
            return;
        }

        var args = CreateUrlLoadingEventArgs(request, webViewManager.AppBaseUri);

        virtualView.UrlLoading(args);

        if (args.UrlLoadingStrategy == UrlLoadingStrategy.CancelLoad)
        {
            e.Cancel = true;
        }
        else if (args.UrlLoadingStrategy == UrlLoadingStrategy.OpenExternally)
        {
            e.Cancel = true;
            _ = OpenExternalAsync(args.Url, _logger);
        }
        else
        {
            InvalidateDocumentTrust();
        }
    }

    private void OnNewWindowRequested(object? sender, Avalonia.Controls.WebViewNewWindowRequestedEventArgs e)
    {
        if (e.Request is null)
            return;

        e.Handled = true;
        _ = OpenExternalAsync(e.Request, _logger);
    }

    private void OnWebMessageReceived(object? sender, Avalonia.Controls.WebMessageReceivedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Body))
            return;

        AvaloniaBlazorWebViewManager? webViewManager;
        Uri? sourceUri;
        string? documentToken;

        lock (_documentTrustLock)
        {
            webViewManager = _webViewManager;
            sourceUri = _trustedDocumentUri;
            documentToken = _documentMessageToken;
        }

        if (webViewManager is null ||
            sourceUri is null ||
            documentToken is null ||
            !webViewManager.AppBaseUri.IsBaseOf(sourceUri) ||
            !AvaloniaBlazorWebViewScripts.TryUnwrapMessage(documentToken, e.Body, out var message))
        {
            return;
        }

        webViewManager.DispatchMessageReceived(sourceUri, message);
    }

    internal static UrlLoadingEventArgs CreateUrlLoadingEventArgs(Uri url, Uri appOriginUri)
    {
        return MauiBlazorWebViewCompatibility.CreateUrlLoadingEventArgs(url, appOriginUri);
    }

    private void QueueRootComponentUpdate(Func<AvaloniaBlazorWebViewManager, int, Task> update)
    {
        if (_webViewManager is not { } webViewManager)
            return;

        Task queuedTask;
        int generation;

        lock (_rootComponentUpdateLock)
        {
            generation = _rootComponentUpdateGeneration;
            _rootComponentUpdateTask = _rootComponentUpdateTask.ContinueWith(
                static async (previousTask, state) =>
                {
                    var (handler, webViewManager, update, generation) =
                        ((BlazorWebViewHandler Handler, AvaloniaBlazorWebViewManager WebViewManager, Func<AvaloniaBlazorWebViewManager, int, Task> Update, int Generation))state!;

                    try
                    {
                        await previousTask.ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        handler._logger.LogDebug(ex, "A previous Blazor root-component update failed.");
                    }

                    if (!handler.IsRootComponentUpdateCurrent(webViewManager, generation))
                        return;

                    await webViewManager.Dispatcher.InvokeAsync(async () =>
                    {
                        if (handler.IsRootComponentUpdateCurrent(webViewManager, generation))
                            await update(webViewManager, generation);
                    }).ConfigureAwait(false);
                },
                (this, webViewManager, update, generation),
                CancellationToken.None,
                TaskContinuationOptions.None,
                TaskScheduler.Default).Unwrap();

            queuedTask = _rootComponentUpdateTask;
        }

        _ = ObserveRootComponentUpdateAsync(queuedTask, _logger);
    }

    private bool IsRootComponentUpdateCurrent(AvaloniaBlazorWebViewManager webViewManager, int generation)
    {
        lock (_rootComponentUpdateLock)
            return IsRootComponentUpdateCurrentCore(webViewManager, generation);
    }

    private bool TryUpdateAttachedRootComponentSelector(
        AvaloniaBlazorWebViewManager webViewManager,
        int generation,
        string selector,
        bool attach)
    {
        lock (_rootComponentUpdateLock)
        {
            if (!IsRootComponentUpdateCurrentCore(webViewManager, generation))
                return false;

            if (attach)
                _attachedRootComponentSelectors.Add(selector);
            else
                _attachedRootComponentSelectors.Remove(selector);

            return true;
        }
    }

    private bool IsRootComponentUpdateCurrentCore(
        AvaloniaBlazorWebViewManager webViewManager,
        int generation)
    {
        return !_isDisconnecting &&
            generation == _rootComponentUpdateGeneration &&
            ReferenceEquals(_webViewManager, webViewManager);
    }

    private static async Task ObserveRootComponentUpdateAsync(Task task, ILogger logger)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "A Blazor root-component update failed.");
        }
    }

    private void InvalidateDocumentTrust()
    {
        lock (_documentTrustLock)
        {
            _navigationGeneration++;
            _trustedDocumentUri = null;
            _documentMessageToken = null;
        }
    }

    private void InvalidateDocumentTrust(string documentToken, int navigationGeneration)
    {
        lock (_documentTrustLock)
        {
            if (navigationGeneration != _navigationGeneration ||
                !string.Equals(_documentMessageToken, documentToken, StringComparison.Ordinal))
            {
                return;
            }

            _navigationGeneration++;
            _trustedDocumentUri = null;
            _documentMessageToken = null;
        }
    }

    private static async Task OpenExternalAsync(Uri uri, ILogger logger)
    {
        try
        {
            if (await Launcher.OpenAsync(uri))
                return;

            logger.LogDebug("MAUI Launcher declined to open {Uri}; falling back to the operating-system shell.", uri);
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "MAUI Launcher could not open {Uri}; falling back to the operating-system shell.", uri);
        }

        OpenExternalWithShell(uri, logger);
    }

    private static void OpenExternalWithShell(Uri uri, ILogger logger)
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
            }
            else if (OperatingSystem.IsMacOS())
            {
                Process.Start("open", uri.AbsoluteUri);
            }
            else
            {
                Process.Start("xdg-open", uri.AbsoluteUri);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Unable to open external URI {Uri}.", uri);
        }
    }

    internal static (string ContentRootDir, string HostPageRelativePath) GetHostPagePaths(string hostPage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hostPage);

        var platformPath = hostPage
            .Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar);
        var contentRootDir = Path.GetDirectoryName(platformPath) ?? string.Empty;
        var hostPageRelativePath = string.IsNullOrEmpty(contentRootDir)
            ? platformPath
            : Path.GetRelativePath(contentRootDir, platformPath);

        return (contentRootDir.Replace('\\', '/'), NormalizePath(hostPageRelativePath));
    }

    private static string NormalizePath(string path)
    {
        return path.Replace('\\', '/').TrimStart('/');
    }
}
