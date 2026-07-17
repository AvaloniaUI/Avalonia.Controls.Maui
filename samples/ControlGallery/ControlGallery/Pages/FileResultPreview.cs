using System.Text;
using Avalonia.Controls.Maui.Essentials;

namespace ControlGallery.Pages;

/// <summary>
/// Shared helpers used by the FilePicker / MediaPicker sample pages to render
/// <see cref="FileResult"/> details and produce image-preview byte buffers.
/// </summary>
internal static class FileResultPreview
{
    /// <summary>
    /// Opens the file contents, routing through <see cref="AvaloniaFileResult"/> when available.
    /// The base <see cref="FileResult.OpenReadAsync"/> is not functional on the portable
    /// Microsoft.Maui.Essentials build, so the Avalonia storage-backed method must be used.
    /// </summary>
    static Task<Stream> OpenReadAsync(FileResult file) =>
        file is AvaloniaFileResult avaloniaFile
            ? avaloniaFile.OpenReadAsync()
            : file.OpenReadAsync();

    public static async Task AppendFileDetailsAsync(StringBuilder sb, FileResult file, int index, int total)
    {
        sb.AppendLine($"[{index}/{total}] {file.FileName}");
        sb.AppendLine($"  Type:        {file.GetType().Name}");
        sb.AppendLine($"  FileName:    {file.FileName}");
        sb.AppendLine($"  FullPath:    {file.FullPath}");
        sb.AppendLine($"  ContentType: {file.ContentType}");

        try
        {
            await using var stream = await OpenReadAsync(file);
            long length;
            try
            {
                length = stream.Length;
            }
            catch (NotSupportedException)
            {
                using var counter = new CountingStream();
                await stream.CopyToAsync(counter);
                length = counter.BytesWritten;
            }

            sb.AppendLine($"  StreamType:  {stream.GetType().Name}");
            sb.AppendLine($"  Size:        {length:N0} bytes");
            sb.AppendLine($"  CanRead:     {stream.CanRead}");
            sb.AppendLine($"  CanSeek:     {stream.CanSeek}");
        }
        catch (Exception ex)
        {
            sb.AppendLine($"  OpenReadAsync FAILED: {ex.GetType().Name}: {ex.Message}");
        }
    }

    public static async Task<ImageSource?> TryLoadImagePreviewAsync(FileResult file)
    {
        if (!IsImageContentType(file.ContentType))
            return null;

        try
        {
            // Buffer the contents so the ImageSource has its own copy of the data.
            using var source = await OpenReadAsync(file);
            using var ms = new MemoryStream();
            await source.CopyToAsync(ms);
            var bytes = ms.ToArray();
            return ImageSource.FromStream(() => new MemoryStream(bytes, writable: false));
        }
        catch
        {
            return null;
        }
    }

    static bool IsImageContentType(string? contentType)
        => contentType is not null && contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);

    sealed class CountingStream : Stream
    {
        public long BytesWritten { get; private set; }
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => BytesWritten;
        public override long Position { get => BytesWritten; set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => BytesWritten += count;
        public override void Write(ReadOnlySpan<byte> buffer) => BytesWritten += buffer.Length;
    }
}
