using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using PlatformView = Avalonia.Controls.RefreshContainer;

namespace Avalonia.Controls.Maui.Handlers;

public partial class RefreshViewHandler : ViewHandler<IRefreshView, PlatformView>
{
    private RefreshCompletionDeferral? _currentRefreshDeferral;
    private bool _isSettingRefreshingFromCode;

    public static IPropertyMapper<IRefreshView, RefreshViewHandler> Mapper = new PropertyMapper<IRefreshView, RefreshViewHandler>(ViewHandler.ViewMapper)
    {
        [nameof(IRefreshView.IsRefreshing)] = MapIsRefreshing,
        [nameof(IRefreshView.Content)] = MapContent,
        [nameof(IRefreshView.RefreshColor)] = MapRefreshColor,
        [nameof(IView.Background)] = MapBackground,
        [nameof(IView.IsEnabled)] = MapIsEnabled,
    };

    public static CommandMapper<IRefreshView, RefreshViewHandler> CommandMapper = new(ViewHandler.ViewCommandMapper)
    {
    };

    public RefreshViewHandler() : base(Mapper, CommandMapper)
    {
    }

    public RefreshViewHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }
    
    public RefreshViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    protected override PlatformView CreatePlatformView()
    {
        return new PlatformView();
    }

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

    public static void MapIsRefreshing(RefreshViewHandler handler, IRefreshView refreshView)
    {
        handler.UpdateIsRefreshingState();
    }
    
    public static void MapContent(RefreshViewHandler handler, IRefreshView refreshView)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateContent(refreshView, handler.MauiContext);
    }
    
    public static void MapRefreshColor(RefreshViewHandler handler, IRefreshView refreshView)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateRefreshColor(refreshView);
    }

    public static void MapBackground(RefreshViewHandler handler, IRefreshView refreshView)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateBackground(refreshView);
    }

    public static void MapIsEnabled(RefreshViewHandler handler, IRefreshView refreshView)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateIsEnabled(refreshView);
    }
}