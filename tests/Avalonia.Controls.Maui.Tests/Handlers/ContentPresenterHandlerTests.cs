using Avalonia.Controls.Maui.Handlers;
using Avalonia.Controls.Maui.Platform;
using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Headless;
using Microsoft.Maui;
using Xunit;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public class ContentPresenterHandlerTests : HandlerTestBase<ContentPresenterHandler, ContentPresenterStub>
{
    [Fact]
    public void MapContent_Updates_Platform_Children()
    {
        var content = new ButtonStub();
        var stub = new ContentPresenterStub
        {
            PresentedContent = content
        };

        var handler = CreateHandler<ContentPresenterHandler>(stub);

        // Simulate mapping
        ContentPresenterHandler.MapContent(handler, stub);
        
        var platformView = (MauiContentPresenter)handler.PlatformView;
        
        Assert.Single(platformView.Children);
        Assert.IsAssignableFrom<global::Avalonia.Controls.Button>(platformView.Children[0]);
    }

    [Fact]
    public void MapContent_Null_Clears_Children()
    {
        var content = new ButtonStub();
        var stub = new ContentPresenterStub
        {
            PresentedContent = content
        };

        var handler = CreateHandler<ContentPresenterHandler>(stub);
        ContentPresenterHandler.MapContent(handler, stub);
        
        // Clear content
        stub.PresentedContent = null;
        ContentPresenterHandler.MapContent(handler, stub);

        var platformView = (MauiContentPresenter)handler.PlatformView;
        Assert.Empty(platformView.Children);
    }

    [Fact]
    public void MapContent_Updates_New_Content()
    {
        var stub = new ContentPresenterStub
        {
            PresentedContent = new ButtonStub()
        };

        var handler = CreateHandler<ContentPresenterHandler>(stub);
        ContentPresenterHandler.MapContent(handler, stub);
        
        // Update content
        stub.PresentedContent = new LabelStub();
        ContentPresenterHandler.MapContent(handler, stub);

        var platformView = (MauiContentPresenter)handler.PlatformView;
        Assert.Single(platformView.Children);
        Assert.IsAssignableFrom<global::Avalonia.Controls.TextBlock>(platformView.Children[0]);
    }
}
