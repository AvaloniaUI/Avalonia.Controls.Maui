using Avalonia.Controls;

namespace Avalonia.Controls.Maui.Essentials;

public interface IAvaloniaEssentialsPlatformProvider
{
    TopLevel? GetTopLevel();
}
