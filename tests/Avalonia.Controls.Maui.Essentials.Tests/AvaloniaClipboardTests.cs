using Avalonia.Controls;
using Avalonia.Controls.Maui.Essentials;
using Avalonia.Headless.XUnit;
using NSubstitute;

namespace Avalonia.Controls.Maui.Tests.Services;

public class AvaloniaClipboardTests
{
    [Fact]
    public void HasText_Initially_False()
    {
        var provider = Substitute.For<IAvaloniaEssentialsPlatformProvider>();
        provider.GetTopLevel().Returns((TopLevel?)null);

        var clipboard = new AvaloniaClipboard(provider);

        Assert.False(clipboard.HasText);
    }

    [Fact]
    public async Task GetTextAsync_Returns_Null_When_No_TopLevel()
    {
        var provider = Substitute.For<IAvaloniaEssentialsPlatformProvider>();
        provider.GetTopLevel().Returns((TopLevel?)null);

        var clipboard = new AvaloniaClipboard(provider);

        var result = await clipboard.GetTextAsync();
        Assert.Null(result);
        Assert.False(clipboard.HasText);
    }

    [Fact]
    public async Task SetTextAsync_Does_Not_Throw_When_No_TopLevel()
    {
        var provider = Substitute.For<IAvaloniaEssentialsPlatformProvider>();
        provider.GetTopLevel().Returns((TopLevel?)null);

        var clipboard = new AvaloniaClipboard(provider);

        await clipboard.SetTextAsync("test");
    }

    [Fact]
    public void ClipboardContentChanged_Not_Raised_Without_SetText()
    {
        var provider = Substitute.For<IAvaloniaEssentialsPlatformProvider>();
        provider.GetTopLevel().Returns((TopLevel?)null);

        var clipboard = new AvaloniaClipboard(provider);
        var eventRaised = false;
        clipboard.ClipboardContentChanged += (_, _) => eventRaised = true;

        Assert.False(eventRaised);
    }

    [AvaloniaFact]
    public async Task SetTextAsync_And_GetTextAsync_RoundTrip()
    {
        var window = new Window { Width = 100, Height = 100 };
        window.Show();

        var provider = Substitute.For<IAvaloniaEssentialsPlatformProvider>();
        provider.GetTopLevel().Returns(window);

        var clipboard = new AvaloniaClipboard(provider);
        await clipboard.SetTextAsync("hello");

        var result = await clipboard.GetTextAsync();
        Assert.Equal("hello", result);
        Assert.True(clipboard.HasText);
    }

    [AvaloniaFact]
    public async Task SetTextAsync_Raises_ClipboardContentChanged()
    {
        var window = new Window { Width = 100, Height = 100 };
        window.Show();

        var provider = Substitute.For<IAvaloniaEssentialsPlatformProvider>();
        provider.GetTopLevel().Returns(window);

        var clipboard = new AvaloniaClipboard(provider);
        var eventCount = 0;
        clipboard.ClipboardContentChanged += (_, _) => eventCount++;

        await clipboard.SetTextAsync("test");

        Assert.Equal(1, eventCount);
    }

    [AvaloniaFact]
    public async Task SetTextAsync_Null_Clears_HasText()
    {
        var window = new Window { Width = 100, Height = 100 };
        window.Show();

        var provider = Substitute.For<IAvaloniaEssentialsPlatformProvider>();
        provider.GetTopLevel().Returns(window);

        var clipboard = new AvaloniaClipboard(provider);
        await clipboard.SetTextAsync("text");
        Assert.True(clipboard.HasText);

        await clipboard.SetTextAsync(null);
        Assert.False(clipboard.HasText);
    }

    [AvaloniaFact]
    public async Task GetTextAsync_Updates_HasText()
    {
        var window = new Window { Width = 100, Height = 100 };
        window.Show();

        var provider = Substitute.For<IAvaloniaEssentialsPlatformProvider>();
        provider.GetTopLevel().Returns(window);

        var clipboard = new AvaloniaClipboard(provider);

        Assert.False(clipboard.HasText);
        await clipboard.SetTextAsync("something");

        var text = await clipboard.GetTextAsync();
        Assert.NotNull(text);
        Assert.True(clipboard.HasText);
    }
}
