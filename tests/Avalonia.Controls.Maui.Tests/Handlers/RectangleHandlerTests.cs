using Avalonia.Headless.XUnit;
using RectangleStub = Avalonia.Controls.Maui.Tests.Stubs.RectangleStub;
using MauiRectangleHandler = Avalonia.Controls.Maui.Tests.Stubs.RectangleStubHandler;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public class RectangleHandlerTests : HandlerTestBase<MauiRectangleHandler, RectangleStub>
{
    [AvaloniaFact(DisplayName = "Radius Maps Correctly")]
    public async Task RadiusMapsCorrectly()
    {
        var rectangle = new RectangleStub
        {
            RadiusX = 12,
            RadiusY = 6
        };

        var handler = await CreateHandlerAsync(rectangle);

        Assert.Equal(rectangle.RadiusX, handler.PlatformView.RadiusX);
        Assert.Equal(rectangle.RadiusY, handler.PlatformView.RadiusY);
    }
}
