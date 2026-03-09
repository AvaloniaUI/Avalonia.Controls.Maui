
using Microsoft.Maui.Devices;

namespace Avalonia.Controls.Maui.Essentials;

/// <summary>
/// Provides a no-op implementation of IHapticFeedback for Avalonia desktop platforms where haptic feedback hardware is not available.
/// </summary>
public class AvaloniaHapticFeedback : IHapticFeedback
{
    /// <summary>
    /// Gets a value indicating whether haptic feedback is supported on the current platform. Always returns <c>false</c> for Avalonia desktop platforms.
    /// </summary>
    public bool IsSupported => false;

    /// <summary>
    /// Performs a haptic feedback action. This method is a no-op on Avalonia desktop platforms because haptic feedback is not supported.
    /// </summary>
    /// <param name="type">The type of haptic feedback to perform.</param>
    public void Perform(HapticFeedbackType type)
    {
        // No-op on Avalonia desktop platforms, as haptic feedback is not supported.
    }
}