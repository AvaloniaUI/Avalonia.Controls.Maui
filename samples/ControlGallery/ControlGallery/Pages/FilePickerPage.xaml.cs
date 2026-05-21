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

    private static async Task<List<FileResult>?> PickSingleAsListAsync(PickOptions? options)
    {
        var single = await FilePicker.Default.PickAsync(options);
        return single is null ? null : [single];
    }
}
