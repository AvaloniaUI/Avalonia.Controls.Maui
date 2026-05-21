using System.Text;

namespace ControlGallery.Pages;

public partial class MediaPickerPage : ContentPage
{
    public MediaPickerPage()
    {
        InitializeComponent();
        CaptureLabel.Text = $"IsCaptureSupported: {MediaPicker.Default.IsCaptureSupported}";
    }

    private async void OnPickPhotoClicked(object? sender, EventArgs e)
        => await PickAndDescribeAsync(() => PickPhotoAsListAsync(), kind: "photo");

    private async void OnPickPhotosClicked(object? sender, EventArgs e)
        => await PickAndDescribeAsync(() => PickPhotosAsListAsync(), kind: "photos");

    private async void OnPickVideoClicked(object? sender, EventArgs e)
        => await PickAndDescribeAsync(() => PickVideoAsListAsync(), kind: "video");

    private async void OnPickVideosClicked(object? sender, EventArgs e)
        => await PickAndDescribeAsync(() => PickVideosAsListAsync(), kind: "videos");

    private async Task PickAndDescribeAsync(Func<Task<List<FileResult>?>> pick, string kind)
    {
        try
        {
            StatusLabel.Text = $"Status: showing {kind} picker…";
            PreviewImage.Source = null;

            var files = await pick();

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
                await FileResultPreview.AppendFileDetailsAsync(sb, files[i], index: i + 1, total: files.Count);
            }

            DetailsLabel.Text = sb.ToString();
            StatusLabel.Text = $"Status: {files.Count} file(s) read successfully";

            PreviewImage.Source = await FileResultPreview.TryLoadImagePreviewAsync(files[0]);
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Status: error — {ex.GetType().Name}";
            DetailsLabel.Text = ex.ToString();
        }
    }

    private static async Task<List<FileResult>?> PickPhotoAsListAsync()
    {
        var single = await MediaPicker.Default.PickPhotoAsync();
        return single is null ? null : [single];
    }

    private static async Task<List<FileResult>?> PickVideoAsListAsync()
    {
        var single = await MediaPicker.Default.PickVideoAsync();
        return single is null ? null : [single];
    }

    private static async Task<List<FileResult>?> PickPhotosAsListAsync()
    {
        var picker = MediaPicker.Default as Avalonia.Controls.Maui.Essentials.AvaloniaMediaPicker;
        if (picker is null)
            throw new NotSupportedException("Multi-photo picking requires AvaloniaMediaPicker.");
        return await picker.PickPhotosAsync();
    }

    private static async Task<List<FileResult>?> PickVideosAsListAsync()
    {
        var picker = MediaPicker.Default as Avalonia.Controls.Maui.Essentials.AvaloniaMediaPicker;
        if (picker is null)
            throw new NotSupportedException("Multi-video picking requires AvaloniaMediaPicker.");
        return await picker.PickVideosAsync();
    }
}
