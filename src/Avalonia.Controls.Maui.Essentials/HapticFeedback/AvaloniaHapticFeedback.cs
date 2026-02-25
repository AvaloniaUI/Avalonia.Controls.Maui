
using Microsoft.Maui.Devices;

namespace Avalonia.Controls.Maui.Essentials;

public class AvaloniaHapticFeedback : IHapticFeedback
{
    public bool IsSupported => false;

    public void Perform(HapticFeedbackType type)
    {
        // No-op on Avalonia desktop platforms, as haptic feedback is not supported.
    }
}