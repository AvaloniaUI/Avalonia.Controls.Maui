using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Tests.Stubs;

/// <summary>
/// Minimal stub based on CarouselView for handler testing.
/// </summary>
public class CarouselViewStub : CarouselView
{
    public CarouselViewStub()
    {
        WidthRequest = 320;
        HeightRequest = 200;
    }
}
