using Avalonia.Media;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using AvaloniaProgressBar = global::Avalonia.Controls.ProgressBar;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>
/// Handler for MAUI IProgress to Avalonia ProgressBar mapping
/// </summary>
public class ProgressBarHandler : ViewHandler<Microsoft.Maui.IProgress, AvaloniaProgressBar>
{
    public static IPropertyMapper<Microsoft.Maui.IProgress, ProgressBarHandler> Mapper = new PropertyMapper<Microsoft.Maui.IProgress, ProgressBarHandler>(ViewHandler.ViewMapper)
    {
        [nameof(Microsoft.Maui.IProgress.Progress)] = MapProgress,
        [nameof(Microsoft.Maui.IProgress.ProgressColor)] = MapProgressColor
    };

    public static CommandMapper<Microsoft.Maui.IProgress, ProgressBarHandler> CommandMapper = new(ViewCommandMapper)
    {
    };

    public ProgressBarHandler() : base(Mapper, CommandMapper)
    {
    }

    public ProgressBarHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public ProgressBarHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    protected override AvaloniaProgressBar CreatePlatformView()
    {
        return new AvaloniaProgressBar
        {
            Minimum = 0,
            Maximum = 1
        };
    }

    public override bool NeedsContainer => false;

    public static void MapProgress(ProgressBarHandler handler, Microsoft.Maui.IProgress progress)
    {
        if (handler.PlatformView is AvaloniaProgressBar platformView)
        {
            platformView.UpdateProgress(progress);
        }
    }

    public static void MapProgressColor(ProgressBarHandler handler, Microsoft.Maui.IProgress progress)
    {
        if (handler.PlatformView is AvaloniaProgressBar platformView)
        {
            platformView.UpdateProgressColor(progress);
        }
    }
}
