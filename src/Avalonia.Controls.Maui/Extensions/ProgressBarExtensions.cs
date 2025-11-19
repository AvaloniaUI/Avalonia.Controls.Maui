using Microsoft.Maui;
using AvaloniaProgressBar = global::Avalonia.Controls.ProgressBar;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Extension methods for mapping IProgress properties.
/// </summary>
public static class ProgressBarExtensions
{
    /// <summary>
    /// Updates the ProgressBar's value based on the IProgress progress.
    /// </summary>
    /// <param name="progressBar">The ProgressBar control to update.</param>
    /// <param name="progress">The .NET MAUI IProgress providing the progress value.</param>
    public static void UpdateProgress(this AvaloniaProgressBar progressBar, IProgress progress)
    {
        // Clamp progress value between 0 and 1 as per IProgress contract
        var clampedProgress = Math.Max(0, Math.Min(1, progress.Progress));
        progressBar.Value = clampedProgress;
    }

    /// <summary>
    /// Updates the ProgressBar's foreground color based on the IProgress progress color.
    /// </summary>
    /// <param name="progressBar">The ProgressBar control to update.</param>
    /// <param name="progress">The .NET MAUI IProgress providing the progress color.</param>
    public static void UpdateProgressColor(this AvaloniaProgressBar progressBar, IProgress progress)
    {
        if (progress.ProgressColor != null)
        {
            progressBar.Foreground = progress.ProgressColor.ToPlatform();
        }
        else
        {
            progressBar.ClearValue(AvaloniaProgressBar.ForegroundProperty);
        }
    }
}
