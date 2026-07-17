using Microsoft.Maui;
using Microsoft.Maui.Controls.Platform;

namespace Avalonia.Controls.Maui.Platform;

internal sealed class AvaloniaGesturePlatformManagerFactory : IGesturePlatformManagerFactory
{
    public IGesturePlatformManager CreateGesturePlatformManager(IViewHandler handler) =>
        new AvaloniaGesturePlatformManager(handler);
}
