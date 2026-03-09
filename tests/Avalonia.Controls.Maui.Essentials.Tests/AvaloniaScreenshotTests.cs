using Avalonia.Controls;
using Avalonia.Controls.Maui.Essentials;
using Avalonia.Headless.XUnit;
using NSubstitute;

namespace Avalonia.Controls.Maui.Tests.Services;

public class AvaloniaScreenshotTests
{
    [Fact]
    public void IsCaptureSupported_ReturnsTrue()
    {
        var provider = Substitute.For<IAvaloniaEssentialsPlatformProvider>();
        var screenshot = new AvaloniaScreenshot(provider);

        Assert.True(screenshot.IsCaptureSupported);
    }

    [Fact]
    public async Task CaptureAsync_NullTopLevel_ThrowsInvalidOperationException()
    {
        var provider = Substitute.For<IAvaloniaEssentialsPlatformProvider>();
        provider.GetTopLevel().Returns((TopLevel?)null);

        var screenshot = new AvaloniaScreenshot(provider);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => screenshot.CaptureAsync());
    }

    [AvaloniaFact]
    public async Task CaptureAsync_ReturnsValidResult()
    {
        var window = new Window
        {
            Width = 200,
            Height = 100
        };
        window.Show();

        var provider = Substitute.For<IAvaloniaEssentialsPlatformProvider>();
        provider.GetTopLevel().Returns(window);

        var screenshot = new AvaloniaScreenshot(provider);
        var result = await screenshot.CaptureAsync();

        Assert.NotNull(result);
        Assert.True(result.Width > 0);
        Assert.True(result.Height > 0);
    }
}