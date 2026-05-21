using System.Text;
using Avalonia.Controls.Maui.Essentials;
using Avalonia.Platform.Storage;
using NSubstitute;

namespace Avalonia.Controls.Maui.Tests.Services;

public class AvaloniaFileResultTests
{
    [Fact]
    public void Constructor_WithLocalPath_UsesLocalPathAsFullPath()
    {
        var localPath = Path.Combine(Path.GetTempPath(), "sample.png");
        var file = CreateStorageFile("sample.png", new Uri(localPath));

        var result = new AvaloniaFileResult(file);

        Assert.Equal(localPath, result.FullPath);
        Assert.Equal("sample.png", result.FileName);
    }

    [Fact]
    public void Constructor_WithoutLocalPath_FallsBackToFileName()
    {
        // Browser-style URI that has no local path representation.
        var file = CreateStorageFile("photo.jpg", new Uri("blob:https://example.test/abc-123"));

        var result = new AvaloniaFileResult(file);

        Assert.Equal("photo.jpg", result.FullPath);
        Assert.Equal("photo.jpg", result.FileName);
    }

    [Fact]
    public void StorageFile_ExposesUnderlyingFile()
    {
        var file = CreateStorageFile("any.txt", new Uri("blob:test"));

        var result = new AvaloniaFileResult(file);

        Assert.Same(file, result.StorageFile);
    }

    [Theory]
    [InlineData("photo.png", "image/png")]
    [InlineData("photo.PNG", "image/png")]
    [InlineData("photo.jpg", "image/jpeg")]
    [InlineData("photo.jpeg", "image/jpeg")]
    [InlineData("photo.gif", "image/gif")]
    [InlineData("photo.webp", "image/webp")]
    [InlineData("doc.pdf", "application/pdf")]
    [InlineData("data.json", "application/json")]
    [InlineData("page.html", "text/html")]
    [InlineData("notes.txt", "text/plain")]
    [InlineData("readme.md", "text/markdown")]
    [InlineData("song.mp3", "audio/mpeg")]
    [InlineData("clip.mp4", "video/mp4")]
    [InlineData("archive.zip", "application/x-zip-compressed")]
    public void ContentType_ResolvedFromKnownExtension(string fileName, string expectedContentType)
    {
        var file = CreateStorageFile(fileName, new Uri("blob:test"));

        var result = new AvaloniaFileResult(file);

        Assert.Equal(expectedContentType, result.ContentType);
    }

    [Theory]
    [InlineData("file.unknownext")]
    [InlineData("filewithoutextension")]
    [InlineData(".dotfile")] // The mapping table treats ".dotfile" as the extension; not in the table.
    public void ContentType_FallsBackToOctetStream_ForUnknownOrMissingExtension(string fileName)
    {
        var file = CreateStorageFile(fileName, new Uri("blob:test"));

        var result = new AvaloniaFileResult(file);

        Assert.Equal("application/octet-stream", result.ContentType);
    }

    [Fact]
    public async Task OpenReadAsync_ReturnsStreamFromUnderlyingStorageFile()
    {
        var payload = "hello world"u8.ToArray();
        var file = CreateStorageFile("hello.txt", new Uri("blob:test"));
        file.OpenReadAsync().Returns(_ => Task.FromResult<Stream>(new MemoryStream(payload, writable: false)));

        var result = new AvaloniaFileResult(file);

        await using var stream = await result.OpenReadAsync();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var text = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);

        Assert.Equal("hello world", text);
        await file.Received(1).OpenReadAsync();
    }

    [Fact]
    public async Task PlatformOpenReadAsync_DelegatesToStorageFile()
    {
        var underlying = new MemoryStream([1, 2, 3]);
        var file = CreateStorageFile("a.bin", new Uri("blob:test"));
        file.OpenReadAsync().Returns(Task.FromResult<Stream>(underlying));

        var result = new AvaloniaFileResult(file);

        await using var stream = await result.PlatformOpenReadAsync();

        await file.Received(1).OpenReadAsync();
        Assert.Same(underlying, stream);
    }

    static IStorageFile CreateStorageFile(string name, Uri path)
    {
        var file = Substitute.For<IStorageFile>();
        file.Name.Returns(name);
        file.Path.Returns(path);
        return file;
    }
}
