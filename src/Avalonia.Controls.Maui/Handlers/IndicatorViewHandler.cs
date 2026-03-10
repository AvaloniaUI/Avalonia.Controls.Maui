using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
// When Avalonia.Controls.PipsPager ships, replace the line below with:
// using PlatformView = Avalonia.Controls.PipsPager;
using PlatformView = Avalonia.Controls.Maui.Controls.PipsPager;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="IIndicatorView"/>.</summary>
public partial class IndicatorViewHandler : ViewHandler<IIndicatorView, PlatformView>
{
    /// <summary>Property mapper for <see cref="IndicatorViewHandler"/>.</summary>
    public static readonly IPropertyMapper<IIndicatorView, IndicatorViewHandler> Mapper =
        new PropertyMapper<IIndicatorView, IndicatorViewHandler>(ViewHandler.ViewMapper)
        {
            [nameof(IIndicatorView.Count)] = MapCount,
            [nameof(IIndicatorView.Position)] = MapPosition,
            [nameof(IIndicatorView.HideSingle)] = MapHideSingle,
            [nameof(IIndicatorView.MaximumVisible)] = MapMaximumVisible,
            [nameof(IIndicatorView.IndicatorSize)] = MapIndicatorSize,
            [nameof(IIndicatorView.IndicatorColor)] = MapIndicatorColor,
            [nameof(IIndicatorView.SelectedIndicatorColor)] = MapSelectedIndicatorColor,
            [nameof(IIndicatorView.IndicatorsShape)] = MapIndicatorShape,
            [nameof(IndicatorView.IndicatorTemplate)] = MapIndicatorTemplate,
        };

    /// <summary>Command mapper for <see cref="IndicatorViewHandler"/>.</summary>
    public static readonly CommandMapper<IIndicatorView, IndicatorViewHandler> CommandMapper =
        new(ViewCommandMapper)
        {
        };

    /// <summary>Initializes a new instance of <see cref="IndicatorViewHandler"/>.</summary>
    public IndicatorViewHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="IndicatorViewHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    public IndicatorViewHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="IndicatorViewHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    /// <param name="commandMapper">The command mapper to use, or <c>null</c> to use the default command mapper.</param>
    public IndicatorViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override PlatformView CreatePlatformView() => new PlatformView
    {
        // MAUI's IndicatorView does not show navigation buttons
        IsPreviousButtonVisible = false,
        IsNextButtonVisible = false,
    };

    /// <inheritdoc/>
    protected override void ConnectHandler(PlatformView platformView)
    {
        base.ConnectHandler(platformView);
        platformView.SelectedIndexChanged += OnSelectedIndexChanged;
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(PlatformView platformView)
    {
        platformView.SelectedIndexChanged -= OnSelectedIndexChanged;
        base.DisconnectHandler(platformView);
    }

    private void OnSelectedIndexChanged(object? sender, Controls.PipsPagerSelectedIndexChangedEventArgs e)
    {
        if (VirtualView.Position != e.NewIndex)
            VirtualView.Position = e.NewIndex;
    }

    /// <summary>Maps <see cref="IIndicatorView.Count"/> to <see cref="PlatformView.NumberOfPages"/>.</summary>
    public static void MapCount(IndicatorViewHandler handler, IIndicatorView indicator)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateNumberOfPages(indicator);
    }

    /// <summary>Maps <see cref="IIndicatorView.Position"/> to <see cref="PlatformView.SelectedPageIndex"/>.</summary>
    public static void MapPosition(IndicatorViewHandler handler, IIndicatorView indicator)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateSelectedPageIndex(indicator);
    }

    /// <summary>Maps <see cref="IIndicatorView.HideSingle"/> to the platform view visibility.</summary>
    public static void MapHideSingle(IndicatorViewHandler handler, IIndicatorView indicator)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateHideSingle(indicator);
    }

    /// <summary>Maps <see cref="IIndicatorView.MaximumVisible"/> to <see cref="PlatformView.MaxVisiblePips"/>.</summary>
    public static void MapMaximumVisible(IndicatorViewHandler handler, IIndicatorView indicator)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateMaxVisiblePips(indicator);
    }

    /// <summary>Maps <see cref="IIndicatorView.IndicatorSize"/> to pip size resources.</summary>
    public static void MapIndicatorSize(IndicatorViewHandler handler, IIndicatorView indicator)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdatePipSize(indicator);
    }

    /// <summary>Maps <see cref="IIndicatorView.IndicatorColor"/> to the pip foreground resource brush.</summary>
    public static void MapIndicatorColor(IndicatorViewHandler handler, IIndicatorView indicator)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdatePipFill(indicator);
    }

    /// <summary>Maps <see cref="IIndicatorView.SelectedIndicatorColor"/> to the selected pip foreground resource brush.</summary>
    public static void MapSelectedIndicatorColor(IndicatorViewHandler handler, IIndicatorView indicator)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateSelectedPipFill(indicator);
    }

    /// <summary>Maps <see cref="IIndicatorView.IndicatorsShape"/> to the pip corner radius resource.</summary>
    public static void MapIndicatorShape(IndicatorViewHandler handler, IIndicatorView indicator)
    {
        if (handler.PlatformView is PlatformView platformView)
            platformView.UpdateIndicatorShape(indicator);
    }

    /// <summary>Maps <see cref="IndicatorView.IndicatorTemplate"/> to <see cref="PlatformView.IndicatorTemplate"/>.</summary>
    public static void MapIndicatorTemplate(IndicatorViewHandler handler, IIndicatorView indicator)
    {
        if (handler.PlatformView is PlatformView platformView && handler.MauiContext is not null)
            platformView.UpdateIndicatorTemplate(indicator, handler.MauiContext);
    }
}
