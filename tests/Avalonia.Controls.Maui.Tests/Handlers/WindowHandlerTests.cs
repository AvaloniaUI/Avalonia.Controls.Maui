using Avalonia.Headless.XUnit;
using MauiWindowHandler = Avalonia.Controls.Maui.Handlers.WindowHandler;
using MauiWindow = Microsoft.Maui.Controls.Window;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public class WindowHandlerTests : HandlerTestBase
{
    [AvaloniaFact(DisplayName = "Scaling Change Raises DisplayDensityChanged")]
    public async Task ScalingChangeRaisesDisplayDensityChanged()
    {
        var window = new MauiWindow(new Microsoft.Maui.Controls.ContentPage());
        var densities = new List<float>();
        window.DisplayDensityChanged += (_, e) => densities.Add(e.DisplayDensity);

        var handler = await CreateHandlerAsync<MauiWindowHandler>(window);

        await InvokeOnMainThreadAsync(() =>
        {
            handler.PlatformView.Show();
            try
            {
                Avalonia.Headless.HeadlessWindowExtensions.SetRenderScaling(handler.PlatformView, 1.5);

                Assert.Equal(new[] { 1.5f }, densities);
                Assert.Equal(1.5f, window.DisplayDensity);
            }
            finally
            {
                handler.PlatformView.Close();
            }
        });
    }

    [AvaloniaFact(DisplayName = "MaximumWidth Uses MaxValue When Width Is NaN")]
    public async Task MaximumWidthUsesMaxValueWhenWidthIsNaN()
    {
        var window = new MauiWindow(new Microsoft.Maui.Controls.ContentPage())
        {
            MaximumWidth = double.NaN,
            MinimumHeight = 120
        };

        var handler = await CreateHandlerAsync<MauiWindowHandler>(window);

        Assert.Equal(double.MaxValue, handler.PlatformView.MaxWidth);
    }

    [AvaloniaFact(DisplayName = "MaximumWidth Maps Explicit Value")]
    public async Task MaximumWidthMapsExplicitValue()
    {
        var window = new MauiWindow(new Microsoft.Maui.Controls.ContentPage())
        {
            MaximumWidth = 640,
            MinimumHeight = 120
        };

        var handler = await CreateHandlerAsync<MauiWindowHandler>(window);

        Assert.Equal(640, handler.PlatformView.MaxWidth);
    }
}
