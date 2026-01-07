using Avalonia.Controls.Maui.Handlers;
using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Headless.XUnit;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public class ContentPresenterHandlerTests : HandlerTestBase<ContentPresenterHandler, ContentPresenterStub>
{
    [AvaloniaFact]
    public async Task MapContent_Updates_Platform_Children()
    {
        var content = new ButtonStub();
        var stub = new ContentPresenterStub
        {
            PresentedContent = content
        };

        var handler = await CreateHandlerAsync(stub);

        // Simulate mapping
        await InvokeOnMainThreadAsync(() =>
        {
            ContentPresenterHandler.MapContent(handler, stub);
        });
        
        var platformView = handler.PlatformView;
        
        Assert.Single(platformView.Children);
        Assert.IsAssignableFrom<Button>(platformView.Children[0]);
    }

    [AvaloniaFact]
    public async Task MapContent_Null_Clears_Children()
    {
        var content = new ButtonStub();
        var stub = new ContentPresenterStub
        {
            PresentedContent = content
        };

        var handler = await CreateHandlerAsync(stub);
        
        await InvokeOnMainThreadAsync(() =>
        {
            ContentPresenterHandler.MapContent(handler, stub);
        });
        
        // Clear content
        stub.PresentedContent = null;
        
        await InvokeOnMainThreadAsync(() =>
        {
            ContentPresenterHandler.MapContent(handler, stub);
        });

        var platformView = handler.PlatformView;
        Assert.Empty(platformView.Children);
    }

    [AvaloniaFact]
    public async Task MapContent_Updates_New_Content()
    {
        var stub = new ContentPresenterStub
        {
            PresentedContent = new ButtonStub()
        };

        var handler = await CreateHandlerAsync(stub);
        
        await InvokeOnMainThreadAsync(() =>
        {
            ContentPresenterHandler.MapContent(handler, stub);
        });
        
        // Update content
        stub.PresentedContent = new LabelStub();
        
        await InvokeOnMainThreadAsync(() =>
        {
            ContentPresenterHandler.MapContent(handler, stub);
        });

        var platformView = handler.PlatformView;
        Assert.Single(platformView.Children);
        Assert.IsAssignableFrom<TextBlock>(platformView.Children[0]);
    }
}
