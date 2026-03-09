using Avalonia.Media;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using AvaloniaProgressBar = global::Avalonia.Controls.ProgressBar;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="Microsoft.Maui.IProgress"/>.</summary>
public class ProgressBarHandler : ViewHandler<Microsoft.Maui.IProgress, AvaloniaProgressBar>
{
    /// <summary>Property mapper for <see cref="ProgressBarHandler"/>.</summary>
    public static IPropertyMapper<Microsoft.Maui.IProgress, ProgressBarHandler> Mapper = new PropertyMapper<Microsoft.Maui.IProgress, ProgressBarHandler>(ViewHandler.ViewMapper)
    {
        [nameof(Microsoft.Maui.IProgress.Progress)] = MapProgress,
        [nameof(Microsoft.Maui.IProgress.ProgressColor)] = MapProgressColor
    };

    /// <summary>Command mapper for <see cref="ProgressBarHandler"/>.</summary>
    public static CommandMapper<Microsoft.Maui.IProgress, ProgressBarHandler> CommandMapper = new(ViewCommandMapper)
    {
    };

    /// <summary>Initializes a new instance of <see cref="ProgressBarHandler"/>.</summary>
    public ProgressBarHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="ProgressBarHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    public ProgressBarHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="ProgressBarHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    /// <param name="commandMapper">The command mapper to use, or <c>null</c> to use the default command mapper.</param>
    public ProgressBarHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override AvaloniaProgressBar CreatePlatformView()
    {
        return new AvaloniaProgressBar
        {
            Minimum = 0,
            Maximum = 1
        };
    }

    /// <summary>Gets a value indicating whether this handler requires a container view.</summary>
    public override bool NeedsContainer => false;

    /// <summary>Maps the Progress property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="progress">The virtual view.</param>
    public static void MapProgress(ProgressBarHandler handler, Microsoft.Maui.IProgress progress)
    {
        if (handler.PlatformView is AvaloniaProgressBar platformView)
        {
            platformView.UpdateProgress(progress);
        }
    }

    /// <summary>Maps the ProgressColor property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="progress">The virtual view.</param>
    public static void MapProgressColor(ProgressBarHandler handler, Microsoft.Maui.IProgress progress)
    {
        if (handler.PlatformView is AvaloniaProgressBar platformView)
        {
            platformView.UpdateProgressColor(progress);
        }
    }
}
