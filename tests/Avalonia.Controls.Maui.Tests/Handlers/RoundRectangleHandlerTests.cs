using Avalonia.Headless.XUnit;
using RoundRectangleStub = Avalonia.Controls.Maui.Tests.Stubs.RoundRectangleStub;
using MauiRoundRectangleHandler = Avalonia.Controls.Maui.Tests.Stubs.RoundRectangleStubHandler;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public class RoundRectangleHandlerTests : HandlerTestBase<MauiRoundRectangleHandler, RoundRectangleStub>
{
    [AvaloniaFact(DisplayName = "CornerRadius Maps Correctly")]
    public async Task CornerRadiusMapsCorrectly()
    {
        var roundRectangle = new RoundRectangleStub
        {
            CornerRadius = new CornerRadius(10, 20, 30, 40)
        };

        var handler = await CreateHandlerAsync(roundRectangle);

        Assert.Equal(roundRectangle.CornerRadius.TopLeft, handler.PlatformView.CornerRadius.TopLeft);
        Assert.Equal(roundRectangle.CornerRadius.TopRight, handler.PlatformView.CornerRadius.TopRight);
        Assert.Equal(roundRectangle.CornerRadius.BottomRight, handler.PlatformView.CornerRadius.BottomRight);
        Assert.Equal(roundRectangle.CornerRadius.BottomLeft, handler.PlatformView.CornerRadius.BottomLeft);
    }
}
