using Avalonia.Controls.Maui.Essentials;

namespace Avalonia.Controls.Maui.Tests.Services;

public class AvaloniaClipboardTests
{
    [Fact]
    public void HasText_Initially_False()
    {
        var provider = new NullPlatformProvider();
        var clipboard = new AvaloniaClipboard(provider);

        Assert.False(clipboard.HasText);
    }

    [Fact]
    public async Task GetTextAsync_Returns_Null_When_No_TopLevel()
    {
        var provider = new NullPlatformProvider();
        var clipboard = new AvaloniaClipboard(provider);

        var result = await clipboard.GetTextAsync();
        Assert.Null(result);
    }

    [Fact]
    public async Task SetTextAsync_Does_Not_Throw_When_No_TopLevel()
    {
        var provider = new NullPlatformProvider();
        var clipboard = new AvaloniaClipboard(provider);

        await clipboard.SetTextAsync("test");
    }

    [Fact]
    public void ClipboardContentChanged_Event_Can_Subscribe()
    {
        var provider = new NullPlatformProvider();
        var clipboard = new AvaloniaClipboard(provider);

        var eventRaised = false;
        clipboard.ClipboardContentChanged += (_, _) => eventRaised = true;

        Assert.False(eventRaised);
    }

    class NullPlatformProvider : IAvaloniaEssentialsPlatformProvider
    {
        public Avalonia.Controls.TopLevel? GetTopLevel() => null;
    }
}
