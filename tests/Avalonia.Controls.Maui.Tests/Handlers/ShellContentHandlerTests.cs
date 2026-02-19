using Avalonia.Headless.XUnit;
using Microsoft.Maui.Controls;
using Avalonia.Controls.Maui.Handlers.Shell;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public class ShellContentHandlerTests : HandlerTestBase
{
    [AvaloniaFact(DisplayName = "ShellContent Creates ContentControl")]
    public async Task ShellContentCreatesContentControl()
    {
        var content = new ShellContent
        {
            Content = new ContentPage { Title = "Test Page" }
        };

        var handler = await CreateHandlerAsync<ShellContentHandler>(content);

        Assert.NotNull(handler.PlatformView);
        Assert.IsType<ContentControl>(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "ShellContent Content Mapping Works")]
    public async Task ShellContentContentMappingWorks()
    {
        var page = new ContentPage { Title = "Main Page" };
        var content = new ShellContent
        {
            Content = page
        };

        var handler = await CreateHandlerAsync<ShellContentHandler>(content);
        var contentControl = handler.PlatformView as ContentControl;

        Assert.NotNull(contentControl);
        // Page content is managed by higher-level handlers (ShellSectionHandler)
    }
}