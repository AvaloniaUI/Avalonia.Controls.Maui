namespace ControlGallery.Pages;

public partial class FilePickerPage : ContentPage
{
    public FilePickerPage()
    {
        InitializeComponent();
    }

    private async void OnPickSingleClicked(object? sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync();
            if (result is not null)
            {
                FileNameLabel.Text = $"File: {result.FileName}";
                FilePathLabel.Text = $"Path: {result.FullPath}";
            }
            else
            {
                FileNameLabel.Text = "File: (cancelled)";
                FilePathLabel.Text = "Path: --";
            }
        }
        catch (Exception ex)
        {
            FileNameLabel.Text = $"Error: {ex.Message}";
            FilePathLabel.Text = "Path: --";
        }
    }

    private async void OnPickMultipleClicked(object? sender, EventArgs e)
    {
        try
        {
            var results = await FilePicker.Default.PickMultipleAsync();
            var files = results?.Where(r => r is not null).ToList();
            if (files is not null && files.Count > 0)
            {
                var names = string.Join(", ", files.Select(f => f!.FileName));
                var paths = string.Join("\n", files.Select(f => f!.FullPath));
                FileNameLabel.Text = $"Files: {names}";
                FilePathLabel.Text = $"Paths:\n{paths}";
            }
            else
            {
                FileNameLabel.Text = "Files: (cancelled)";
                FilePathLabel.Text = "Path: --";
            }
        }
        catch (Exception ex)
        {
            FileNameLabel.Text = $"Error: {ex.Message}";
            FilePathLabel.Text = "Path: --";
        }
    }

    private async void OnPickImagesClicked(object? sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(PickOptions.Images);
            if (result is not null)
            {
                FileNameLabel.Text = $"File: {result.FileName}";
                FilePathLabel.Text = $"Path: {result.FullPath}";
            }
            else
            {
                FileNameLabel.Text = "File: (cancelled)";
                FilePathLabel.Text = "Path: --";
            }
        }
        catch (Exception ex)
        {
            FileNameLabel.Text = $"Error: {ex.Message}";
            FilePathLabel.Text = "Path: --";
        }
    }
}
