using Avalonia.Platform.Storage;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;
using AvaloniaFilePickerFileType = Avalonia.Platform.Storage.FilePickerFileType;

namespace Avalonia.Controls.Maui.Essentials;

public class AvaloniaMediaPicker : IMediaPicker
{
    readonly IAvaloniaEssentialsPlatformProvider _platformProvider;

    internal AvaloniaMediaPicker(IAvaloniaEssentialsPlatformProvider platformProvider)
    {
        _platformProvider = platformProvider;
    }

    public bool IsCaptureSupported => false;

    public Task<FileResult?> CapturePhotoAsync(MediaPickerOptions? options = null)
    {
        throw new NotSupportedException("Camera capture is not supported on Avalonia desktop platforms.");
    }

    public Task<FileResult?> CaptureVideoAsync(MediaPickerOptions? options = null)
    {
        throw new NotSupportedException("Camera capture is not supported on Avalonia desktop platforms.");
    }

    public async Task<FileResult?> PickPhotoAsync(MediaPickerOptions? options = null)
    {
        var topLevel = _platformProvider.GetTopLevel()
            ?? throw new InvalidOperationException("Unable to get Avalonia TopLevel. Ensure the application has been fully initialized.");

        var pickerOptions = CreatePhotoPickerOptions(options);
        var results = await topLevel.StorageProvider.OpenFilePickerAsync(pickerOptions).ConfigureAwait(false);

        if (results.Count == 0)
            return null;

        var path = results[0].TryGetLocalPath();
        return path is not null ? new FileResult(path) : null;
    }

    public async Task<List<FileResult>> PickPhotosAsync(MediaPickerOptions? options = null)
    {
        var topLevel = _platformProvider.GetTopLevel()
            ?? throw new InvalidOperationException("Unable to get Avalonia TopLevel. Ensure the application has been fully initialized.");

        var pickerOptions = CreatePhotoPickerOptions(options, allowMultiple: true);
        var results = await topLevel.StorageProvider.OpenFilePickerAsync(pickerOptions).ConfigureAwait(false);

        var fileResults = new List<FileResult>();
        foreach (var result in results)
        {
            var path = result.TryGetLocalPath();
            if (path is not null)
                fileResults.Add(new FileResult(path));
        }

        return fileResults;
    }

    public async Task<FileResult?> PickVideoAsync(MediaPickerOptions? options = null)
    {
        var topLevel = _platformProvider.GetTopLevel()
            ?? throw new InvalidOperationException("Unable to get Avalonia TopLevel. Ensure the application has been fully initialized.");

        var pickerOptions = CreateVideoPickerOptions(options);
        var results = await topLevel.StorageProvider.OpenFilePickerAsync(pickerOptions).ConfigureAwait(false);

        if (results.Count == 0)
            return null;

        var path = results[0].TryGetLocalPath();
        return path is not null ? new FileResult(path) : null;
    }

    public async Task<List<FileResult>> PickVideosAsync(MediaPickerOptions? options = null)
    {
        var topLevel = _platformProvider.GetTopLevel()
            ?? throw new InvalidOperationException("Unable to get Avalonia TopLevel. Ensure the application has been fully initialized.");

        var pickerOptions = CreateVideoPickerOptions(options, allowMultiple: true);
        var results = await topLevel.StorageProvider.OpenFilePickerAsync(pickerOptions).ConfigureAwait(false);

        var fileResults = new List<FileResult>();
        foreach (var result in results)
        {
            var path = result.TryGetLocalPath();
            if (path is not null)
                fileResults.Add(new FileResult(path));
        }

        return fileResults;
    }

    static FilePickerOpenOptions CreatePhotoPickerOptions(MediaPickerOptions? options, bool allowMultiple = false)
    {
        return new FilePickerOpenOptions
        {
            AllowMultiple = allowMultiple,
            Title = options?.Title,
            FileTypeFilter =
            [
                new AvaloniaFilePickerFileType("Images")
                {
                    Patterns = ["*.jpg", "*.jpeg", "*.png", "*.gif", "*.bmp", "*.webp"]
                }
            ]
        };
    }

    static FilePickerOpenOptions CreateVideoPickerOptions(MediaPickerOptions? options, bool allowMultiple = false)
    {
        return new FilePickerOpenOptions
        {
            AllowMultiple = allowMultiple,
            Title = options?.Title,
            FileTypeFilter =
            [
                new AvaloniaFilePickerFileType("Videos")
                {
                    Patterns = ["*.mp4", "*.mov", "*.avi", "*.wmv", "*.mkv", "*.webm"]
                }
            ]
        };
    }
}
