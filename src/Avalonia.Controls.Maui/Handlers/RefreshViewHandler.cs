using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using PlatformView = Avalonia.Controls.RefreshContainer;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="IRefreshView"/>.</summary>
public partial class RefreshViewHandler : ViewHandler<IRefreshView, PlatformView>
{
    private RefreshCompletionDeferral? _currentRefreshDeferral;
    private bool _isSettingRefreshingFromCode;

    /// <summary>Property mapper for <see cref="RefreshViewHandler"/>.</summary>
    public static IPropertyMapper<IRefreshView, RefreshViewHandler> Mapper = new PropertyMapper<IRefreshView, RefreshViewHandler>(ViewHandler.ViewMapper)
    {
        [nameof(IRefreshView.IsRefreshing)] = MapIsRefreshing,
        [nameof(IRefreshView.Content)] = MapContent,
        [nameof(IRefreshView.RefreshColor)] = MapRefreshColor,
        [nameof(IView.Background)] = MapBackground,
        [nameof(IView.IsEnabled)] = MapIsEnabled,
    };

    /// <summary>Command mapper for <see cref="RefreshViewHandler"/>.</summary>
    public static CommandMapper<IRefreshView, RefreshViewHandler> CommandMapper = new(ViewHandler.ViewCommandMapper)
    {
    };

    /// <summary>Initializes a new instance of <see cref="RefreshViewHandler"/>.</summary>
    public RefreshViewHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="RefreshViewHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    public RefreshViewHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="RefreshViewHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    /// <param name="commandMapper">The command mapper to use, or <c>null</c> to use the default command mapper.</param>
    public RefreshViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override PlatformView CreatePlatformView()
    {
        return new PlatformView();
    }

    /// <inheritdoc/>
    protected override void ConnectHandler(PlatformView platformView)
    {
        base.ConnectHandler(platformView);
        platformView.RefreshRequested += OnRefreshRequested;
        platformView.PropertyChanged += OnPlatformViewPropertyChanged;

        if (platformView.Visualizer != null)
        {
            platformView.Visualizer.TemplateApplied += OnVisualizerTemplateApplied;
        }
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(PlatformView platformView)
    {
        platformView.RefreshRequested -= OnRefreshRequested;
        platformView.PropertyChanged -= OnPlatformViewPropertyChanged;

        if (platformView.Visualizer != null)
        {
            platformView.Visualizer.TemplateApplied -= OnVisualizerTemplateApplied;
        }

        if (_currentRefreshDeferral != null)
        {
            _currentRefreshDeferral.Complete();
            _currentRefreshDeferral = null;
        }

        base.DisconnectHandler(platformView);
    }

    private void OnVisualizerTemplateApplied(object? sender, global::Avalonia.Controls.Primitives.TemplateAppliedEventArgs e)
    {
        UpdateValue(nameof(IRefreshView.RefreshColor));
    }

    private void OnPlatformViewPropertyChanged(object? sender, global::Avalonia.AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name == "Visualizer")
        {
            if (e.OldValue is global::Avalonia.Controls.Primitives.TemplatedControl oldTemplated)
            {
                oldTemplated.TemplateApplied -= OnVisualizerTemplateApplied;
            }
            if (e.NewValue is global::Avalonia.Controls.Primitives.TemplatedControl newTemplated)
            {
                newTemplated.TemplateApplied += OnVisualizerTemplateApplied;
            }

            UpdateValue(nameof(IRefreshView.RefreshColor));
        }
    }

    private void OnRefreshRequested(object? sender, RefreshRequestedEventArgs e)
    {
        if (VirtualView == null)
            return;

        _currentRefreshDeferral = e.GetDeferral();

        if (_isSettingRefreshingFromCode)
            return;

        VirtualView.IsRefreshing = true;
    }

    internal void UpdateIsRefreshingState()
    {
        if (PlatformView == null || VirtualView == null)
            return;

        if (VirtualView.IsRefreshing)
        {
            if (_currentRefreshDeferral == null)
            {
                _isSettingRefreshingFromCode = true;
                PlatformView.RequestRefresh();
                _isSettingRefreshingFromCode = false;
            }

            PlatformView.UpdateRefreshColor(VirtualView);
        }
        else
        {
            if (_currentRefreshDeferral != null)
            {
                _currentRefreshDeferral.Complete();
                _currentRefreshDeferral = null;
            }
        }
    }

    /// <summary>Maps the IsRefreshing property to the platform view.</summary>
    /// <param name="handler">The handler for the refresh view.</param>
    /// <param name="refreshView">The virtual refresh view.</param>
    public static void MapIsRefreshing(RefreshViewHandler handler, IRefreshView refreshView)
    {
        handler.UpdateIsRefreshingState();
    }

    /// <summary>Maps the Content property to the platform view.</summary>
    /// <param name="handler">The handler for the refresh view.</param>
    /// <param name="refreshView">The virtual refresh view.</param>
    public static void MapContent(RefreshViewHandler handler, IRefreshView refreshView)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateContent(refreshView, handler.MauiContext);
    }

    /// <summary>Maps the RefreshColor property to the platform view.</summary>
    /// <param name="handler">The handler for the refresh view.</param>
    /// <param name="refreshView">The virtual refresh view.</param>
    public static void MapRefreshColor(RefreshViewHandler handler, IRefreshView refreshView)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateRefreshColor(refreshView);
    }

    /// <summary>Maps the Background property to the platform view.</summary>
    /// <param name="handler">The handler for the refresh view.</param>
    /// <param name="refreshView">The virtual refresh view.</param>
    public static void MapBackground(RefreshViewHandler handler, IRefreshView refreshView)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateBackground(refreshView);
    }

    /// <summary>Maps the IsEnabled property to the platform view.</summary>
    /// <param name="handler">The handler for the refresh view.</param>
    /// <param name="refreshView">The virtual refresh view.</param>
    public static void MapIsEnabled(RefreshViewHandler handler, IRefreshView refreshView)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateIsEnabled(refreshView);
    }
}
