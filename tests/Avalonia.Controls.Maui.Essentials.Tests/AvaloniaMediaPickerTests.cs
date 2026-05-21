using System.Text;
using Avalonia.Controls;
using Avalonia.Controls.Maui.Essentials;
using Avalonia.Platform.Storage;
using Microsoft.Maui.Storage;
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

    [Fact]
    public async Task PickPhotoAsync_ReturnsAvaloniaFileResult_ForPathlessStorageFile()
    {
        var file = CreatePathlessStorageFile("photo.png");
        var picker = new StubMediaPicker(file);

        var result = await picker.PickPhotoAsync();

        var avaloniaResult = Assert.IsType<AvaloniaFileResult>(result);
        Assert.Same(file, avaloniaResult.StorageFile);
        Assert.Equal("photo.png", avaloniaResult.FileName);
        Assert.Equal("photo.png", avaloniaResult.FullPath);
        Assert.Equal("image/png", avaloniaResult.ContentType);
    }

    [Fact]
    public async Task PickPhotoAsync_OpenReadAsync_DelegatesToStorageFile()
    {
        var payload = "photo bytes"u8.ToArray();
        var file = CreatePathlessStorageFile("photo.png");
        file.OpenReadAsync().Returns(_ => Task.FromResult<Stream>(new MemoryStream(payload, writable: false)));
        var picker = new StubMediaPicker(file);

        var result = await picker.PickPhotoAsync();

        Assert.NotNull(result);
        await using var stream = await result.OpenReadAsync();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var text = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);

        Assert.Equal("photo bytes", text);
        await file.Received(1).OpenReadAsync();
    }

    [Fact]
    public async Task PickPhotoAsync_ReturnsNull_WhenStorageProviderReturnsNoResults()
    {
        var picker = new StubMediaPicker();

        var result = await picker.PickPhotoAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task PickPhotosAsync_WrapsEveryResultAsAvaloniaFileResult()
    {
        var first = CreatePathlessStorageFile("a.png");
        var second = CreatePathlessStorageFile("b.png");
        var picker = new StubMediaPicker(first, second);

        var results = await picker.PickPhotosAsync();

        Assert.Equal(2, results.Count);
        Assert.Same(first, Assert.IsType<AvaloniaFileResult>(results[0]).StorageFile);
        Assert.Same(second, Assert.IsType<AvaloniaFileResult>(results[1]).StorageFile);
    }

    [Fact]
    public async Task PickVideoAsync_ReturnsAvaloniaFileResult_ForPathlessStorageFile()
    {
        var file = CreatePathlessStorageFile("clip.mp4");
        var picker = new StubMediaPicker(file);

        var result = await picker.PickVideoAsync();

        var avaloniaResult = Assert.IsType<AvaloniaFileResult>(result);
        Assert.Same(file, avaloniaResult.StorageFile);
        Assert.Equal("video/mp4", avaloniaResult.ContentType);
    }

    [Fact]
    public async Task PickVideoAsync_OpenReadAsync_DelegatesToStorageFile()
    {
        var payload = new byte[] { 1, 2, 3, 4 };
        var file = CreatePathlessStorageFile("clip.mp4");
        var underlying = new MemoryStream(payload, writable: false);
        file.OpenReadAsync().Returns(Task.FromResult<Stream>(underlying));
        var picker = new StubMediaPicker(file);

        var result = await picker.PickVideoAsync();

        Assert.NotNull(result);
        await using var stream = await result.OpenReadAsync();
        Assert.Same(underlying, stream);
        await file.Received(1).OpenReadAsync();
    }

    [Fact]
    public async Task PickVideosAsync_WrapsEveryResultAsAvaloniaFileResult()
    {
        var first = CreatePathlessStorageFile("a.mp4");
        var second = CreatePathlessStorageFile("b.mp4");
        var picker = new StubMediaPicker(first, second);

        var results = await picker.PickVideosAsync();

        Assert.Equal(2, results.Count);
        Assert.Same(first, Assert.IsType<AvaloniaFileResult>(results[0]).StorageFile);
        Assert.Same(second, Assert.IsType<AvaloniaFileResult>(results[1]).StorageFile);
    }

    static IStorageFile CreatePathlessStorageFile(string name)
    {
        // Browser-style URI that has no local path representation.
        var file = Substitute.For<IStorageFile>();
        file.Name.Returns(name);
        file.Path.Returns(new Uri("blob:https://example.test/" + Guid.NewGuid()));
        return file;
    }

    sealed class StubMediaPicker : AvaloniaMediaPicker
    {
        readonly IReadOnlyList<IStorageFile> _results;

        public StubMediaPicker(params IStorageFile[] results)
            : base(Substitute.For<IAvaloniaEssentialsPlatformProvider>())
        {
            _results = results;
        }

        internal override Task<IReadOnlyList<IStorageFile>> OpenFilePickerAsync(FilePickerOpenOptions options)
            => Task.FromResult(_results);
    }
}
