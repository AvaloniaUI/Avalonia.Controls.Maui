using Avalonia.Controls.Maui.Handlers;
using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Headless.XUnit;
using Microsoft.Maui.Hosting;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public class ContentViewHandlerTests : HandlerTestBase<ContentViewHandler, ContentViewStub>
{
    protected override MauiAppBuilder ConfigureBuilder(MauiAppBuilder mauiAppBuilder)
    {
        return base.ConfigureBuilder(mauiAppBuilder)
            .ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler<ButtonStub, ButtonHandler>();
            });
    }

    [AvaloniaFact(DisplayName = "Content Initializes Correctly")]
    public async Task ContentInitializesCorrectly()
    {
        var button = new ButtonStub { Text = "Content" };
        var contentView = new ContentViewStub
        {
            PresentedContent = button
        };

        var platformView = await GetValueAsync(contentView, handler => handler.PlatformView);
        var content = platformView.Children.FirstOrDefault();

        Assert.NotNull(content);
        Assert.IsAssignableFrom<Button>(content);
    }

    [AvaloniaFact(DisplayName = "Content Updates Correctly")]
    public async Task ContentUpdatesCorrectly()
    {
        var button1 = new ButtonStub { Text = "Button 1" };
        var button2 = new ButtonStub { Text = "Button 2" };
        
        var contentView = new ContentViewStub
        {
            PresentedContent = button1
        };

        var handler = await CreateHandlerAsync(contentView);

        var platformContent1 = handler.PlatformView.Children.FirstOrDefault();
        Assert.NotNull(platformContent1);
        Assert.IsAssignableFrom<Button>(platformContent1);

        // Update content
        contentView.PresentedContent = button2;
        ContentViewHandler.MapContent(handler, contentView);

        var platformContent2 = handler.PlatformView.Children.FirstOrDefault();
        Assert.NotNull(platformContent2);
        Assert.IsAssignableFrom<Button>(platformContent2);
        
        Assert.NotEqual(platformContent1, platformContent2);
    }
}