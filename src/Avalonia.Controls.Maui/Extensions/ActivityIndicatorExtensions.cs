using Microsoft.Maui;

namespace Avalonia.Controls.Maui.Extensions;

/// <summary>
/// Extension methods for mapping IActivityIndicator properties to ProgressRing.
/// </summary>
public static class ActivityIndicatorExtensions
{
    /// <summary>
    /// Updates the IsActive state of the ProgressRing based on the ActivityIndicator's IsRunning property.
    /// </summary>
    /// <param name="control">The ProgressRing control to update.</param>
    /// <param name="activityIndicator">The .NET MAUI ActivityIndicator providing the state.</param>
    internal static void UpdateIsRunning(this ProgressRing control, IActivityIndicator activityIndicator)
    {
        control.IsActive = activityIndicator.IsRunning;
    }

    /// <summary>
    /// Updates the foreground color of the ProgressRing based on the ActivityIndicator's Color property.
    /// </summary>
    /// <param name="control">The ProgressRing control to update.</param>
    /// <param name="activityIndicator">The .NET MAUI ActivityIndicator providing the color.</param>
    internal static void UpdateColor(this ProgressRing control, IActivityIndicator activityIndicator)
    {
        if (activityIndicator.Color != null)
        {
            control.Foreground = activityIndicator.Color.ToPlatform();
        }
        else
        {
            control.ClearValue(ProgressRing.ForegroundProperty);
        }
    }
}