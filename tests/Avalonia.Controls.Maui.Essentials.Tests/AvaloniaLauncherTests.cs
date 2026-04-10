using Avalonia.Controls.Maui.Essentials;

namespace Avalonia.Controls.Maui.Tests.Services;

public class AvaloniaLauncherTests
{
    [Theory]
    [InlineData("https://example.com", true)]
    [InlineData("http://example.com", true)]
    [InlineData("mailto:test@example.com", true)]
    [InlineData("tel:+1234567890", true)]
    [InlineData("file:///tmp/test.txt", true)]
    [InlineData("custom://something", false)]
    public async Task CanOpenAsync_Returns_Expected_For_Scheme(string url, bool expected)
    {
        var launcher = new AvaloniaLauncher();
        var result = await launcher.CanOpenAsync(new Uri(url));
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task TryOpenAsync_Returns_False_For_Unsupported_Scheme()
    {
        var launcher = new AvaloniaLauncher();
        var result = await launcher.TryOpenAsync(new Uri("custom://unsupported"));
        Assert.False(result);
    }

    [Fact]
    public async Task OpenAsync_FileRequest_Returns_False_For_Null_Request()
    {
        var launcher = new AvaloniaLauncher();
        var result = await launcher.OpenAsync((Microsoft.Maui.ApplicationModel.OpenFileRequest)null!);
        Assert.False(result);
    }
}
