using Avalonia.Controls;
using Avalonia.Controls.Maui.Essentials;
using NSubstitute;

namespace Avalonia.Controls.Maui.Tests.Services;

public class AvaloniaMediaPickerTests
{
    [Fact]
    public void IsCaptureSupported_ReturnsFalse()
    {
        var provider = Substitute.For<IAvaloniaEssentialsPlatformProvider>();
        var picker = new AvaloniaMediaPicker(provider);

        Assert.False(picker.IsCaptureSupported);
    }

    [Fact]
    public async Task CapturePhotoAsync_ThrowsNotSupportedException()
    {
        var provider = Substitute.For<IAvaloniaEssentialsPlatformProvider>();
        var picker = new AvaloniaMediaPicker(provider);

        await Assert.ThrowsAsync<NotSupportedException>(
            () => picker.CapturePhotoAsync());
    }

    [Fact]
    public async Task CaptureVideoAsync_ThrowsNotSupportedException()
    {
        var provider = Substitute.For<IAvaloniaEssentialsPlatformProvider>();
        var picker = new AvaloniaMediaPicker(provider);

        await Assert.ThrowsAsync<NotSupportedException>(
            () => picker.CaptureVideoAsync());
    }

    [Fact]
    public async Task PickPhotoAsync_NullTopLevel_ThrowsInvalidOperationException()
    {
        var provider = Substitute.For<IAvaloniaEssentialsPlatformProvider>();
        provider.GetTopLevel().Returns((TopLevel?)null);

        var picker = new AvaloniaMediaPicker(provider);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => picker.PickPhotoAsync());
    }

    [Fact]
    public async Task PickPhotosAsync_NullTopLevel_ThrowsInvalidOperationException()
    {
        var provider = Substitute.For<IAvaloniaEssentialsPlatformProvider>();
        provider.GetTopLevel().Returns((TopLevel?)null);

        var picker = new AvaloniaMediaPicker(provider);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => picker.PickPhotosAsync());
    }

    [Fact]
    public async Task PickVideoAsync_NullTopLevel_ThrowsInvalidOperationException()
    {
        var provider = Substitute.For<IAvaloniaEssentialsPlatformProvider>();
        provider.GetTopLevel().Returns((TopLevel?)null);

        var picker = new AvaloniaMediaPicker(provider);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => picker.PickVideoAsync());
    }

    [Fact]
    public async Task PickVideosAsync_NullTopLevel_ThrowsInvalidOperationException()
    {
        var provider = Substitute.For<IAvaloniaEssentialsPlatformProvider>();
        provider.GetTopLevel().Returns((TopLevel?)null);

        var picker = new AvaloniaMediaPicker(provider);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => picker.PickVideosAsync());
    }
}
