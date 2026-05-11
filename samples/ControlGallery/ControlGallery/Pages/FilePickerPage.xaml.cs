using System.Text;

namespace ControlGallery.Pages;

public partial class FilePickerPage : ContentPage
{
    public FilePickerPage()
    {
        InitializeComponent();
    }

    private async void OnPickSingleClicked(object? sender, EventArgs e)
        => await PickAndDescribeAsync(options: null, allowMultiple: false);

    private async void OnPickMultipleClicked(object? sender, EventArgs e)
        => await PickAndDescribeAsync(options: null, allowMultiple: true);

    private async Task PickAndDescribeAsync(PickOptions? options, bool allowMultiple)
    {
        try
        {
            StatusLabel.Text = "Status: showing picker…";
            PreviewImage.Source = null;

            var files = allowMultiple
                ? (await FilePicker.Default.PickMultipleAsync(options))?.ToList()
                : await PickSingleAsListAsync(options);

            if (files is null || files.Count == 0)
            {
                StatusLabel.Text = "Status: cancelled";
                DetailsLabel.Text = "No file selected.";
                return;
            }

            var sb = new StringBuilder();
            for (var i = 0; i < files.Count; i++)
            {
                if (i > 0)
                    sb.AppendLine().AppendLine("---");
                await AppendFileDetailsAsync(sb, files[i], index: i + 1, total: files.Count);
            }

            DetailsLabel.Text = sb.ToString();
            StatusLabel.Text = $"Status: {files.Count} file(s) read successfully";

            await TryShowPreviewAsync(files[0]);
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Status: error — {ex.GetType().Name}";
            DetailsLabel.Text = ex.ToString();
        }
    }

    private static async Task<List<FileResult>?> PickSingleAsListAsync(PickOptions? options)
    {
        var single = await FilePicker.Default.PickAsync(options);
        return single is null ? null : [single];
    }

    private static async Task AppendFileDetailsAsync(StringBuilder sb, FileResult file, int index, int total)
    {
        sb.AppendLine($"[{index}/{total}] {file.FileName}");
        sb.AppendLine($"  Type:        {file.GetType().Name}");
        sb.AppendLine($"  FileName:    {file.FileName}");
        sb.AppendLine($"  FullPath:    {file.FullPath}");
        sb.AppendLine($"  ContentType: {file.ContentType}");

        try
        {
            await using var stream = await file.OpenReadAsync();
            long length;
            try
            {
                length = stream.Length;
            }
            catch (NotSupportedException)
            {
                // Some streams (e.g. browser-backed) may not report Length; copy to count.
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

    private async Task TryShowPreviewAsync(FileResult file)
    {
        if (!IsImageContentType(file.ContentType))
            return;

        try
        {
            // Buffer the contents so the ImageSource has its own copy of the data.
            using var source = await file.OpenReadAsync();
            var ms = new MemoryStream();
            await source.CopyToAsync(ms);
            ms.Position = 0;
            var bytes = ms.ToArray();
            PreviewImage.Source = ImageSource.FromStream(() => new MemoryStream(bytes, writable: false));
        }
        catch
        {
            PreviewImage.Source = null;
        }
    }

    private static bool IsImageContentType(string? contentType)
        => contentType is not null && contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);

    private sealed class CountingStream : Stream
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
