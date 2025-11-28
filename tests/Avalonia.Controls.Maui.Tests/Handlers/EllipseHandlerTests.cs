using Avalonia.Headless.XUnit;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using EllipseStub = Avalonia.Controls.Maui.Tests.Stubs.EllipseStub;
using MauiEllipseHandler = Avalonia.Controls.Maui.Tests.Stubs.EllipseStubHandler;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public class EllipseHandlerTests : HandlerTestBase<MauiEllipseHandler, EllipseStub>
{
    [AvaloniaFact(DisplayName = "Ellipse Fill Maps Correctly")]
    public async Task EllipseFillMapsCorrectly()
    {
        var ellipse = new EllipseStub
        {
            Fill = new SolidColorBrush(Colors.Orange)
        };

        var handler = await CreateHandlerAsync(ellipse);

        Assert.NotNull(handler.PlatformView.Fill);
    }
}
