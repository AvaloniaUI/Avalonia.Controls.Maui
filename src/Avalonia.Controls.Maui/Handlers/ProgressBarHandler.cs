using Avalonia.Media;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using AvaloniaProgressBar = global::Avalonia.Controls.ProgressBar;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>
/// Handler for MAUI IProgress to Avalonia ProgressBar mapping
/// </summary>
public class ProgressBarHandler : ViewHandler<Microsoft.Maui.IProgress, AvaloniaProgressBar>, IProgressBarHandler
{
    public static IPropertyMapper<Microsoft.Maui.IProgress, IProgressBarHandler> Mapper = new PropertyMapper<Microsoft.Maui.IProgress, IProgressBarHandler>(ViewHandler.ViewMapper)
    {
        [nameof(Microsoft.Maui.IProgress.Progress)] = MapProgress,
        [nameof(Microsoft.Maui.IProgress.ProgressColor)] = MapProgressColor
    };

    public static CommandMapper<Microsoft.Maui.IProgress, IProgressBarHandler> CommandMapper = new(ViewCommandMapper)
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

    Microsoft.Maui.IProgress IProgressBarHandler.VirtualView => VirtualView;

    System.Object IProgressBarHandler.PlatformView => PlatformView;

    protected override AvaloniaProgressBar CreatePlatformView()
    {
        return new AvaloniaProgressBar
        {
            Minimum = 0,
            Maximum = 1
        };
    }

    public override bool NeedsContainer => false;

    public static void MapProgress(IProgressBarHandler handler, Microsoft.Maui.IProgress progress)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        var platformView = (AvaloniaProgressBar)handler.PlatformView;

        // Clamp progress value between 0 and 1 as per IProgress contract
        var clampedProgress = Math.Max(0, Math.Min(1, progress.Progress));
        platformView.Value = clampedProgress;
    }

    public static void MapProgressColor(IProgressBarHandler handler, Microsoft.Maui.IProgress progress)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        var platformView = (AvaloniaProgressBar)handler.PlatformView;

        if (progress.ProgressColor != null)
        {
            platformView.Foreground = progress.ProgressColor.ToPlatform();
        }
    }
}
