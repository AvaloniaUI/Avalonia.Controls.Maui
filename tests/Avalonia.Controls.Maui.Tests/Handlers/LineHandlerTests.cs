using Avalonia.Headless.XUnit;
using LineStub = Avalonia.Controls.Maui.Tests.Stubs.LineStub;
using MauiLineHandler = Avalonia.Controls.Maui.Tests.Stubs.LineStubHandler;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public class LineHandlerTests : HandlerTestBase<MauiLineHandler, LineStub>
{
    [AvaloniaFact(DisplayName = "Line Points Map Correctly")]
    public async Task LinePointsMapCorrectly()
    {
        var line = new LineStub
        {
            X1 = 5,
            Y1 = 10,
            X2 = 25,
            Y2 = 40
        };

        var handler = await CreateHandlerAsync(line);

        Assert.Equal(line.X1, handler.PlatformView.StartPoint.X);
        Assert.Equal(line.Y1, handler.PlatformView.StartPoint.Y);
        Assert.Equal(line.X2, handler.PlatformView.EndPoint.X);
        Assert.Equal(line.Y2, handler.PlatformView.EndPoint.Y);
    }
}
