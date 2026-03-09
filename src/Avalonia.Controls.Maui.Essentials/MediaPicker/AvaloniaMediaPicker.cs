using Avalonia.Platform.Storage;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;
using AvaloniaFilePickerFileType = Avalonia.Platform.Storage.FilePickerFileType;

namespace Avalonia.Controls.Maui.Essentials;

/// <summary>
/// Avalonia implementation of IMediaPicker that uses the StorageProvider file picker for photo and video selection on desktop platforms.
/// </summary>
public class AvaloniaMediaPicker : IMediaPicker
{
    readonly IAvaloniaEssentialsPlatformProvider _platformProvider;

    internal AvaloniaMediaPicker(IAvaloniaEssentialsPlatformProvider platformProvider)
    {
        _platformProvider = platformProvider;
    }

    /// <summary>
    /// Gets a value indicating whether camera capture is supported; always returns <see langword="false"/> because Avalonia desktop platforms do not support camera capture.
    /// </summary>
    public bool IsCaptureSupported => false;

    /// <summary>
    /// Captures a photo using the device camera. This operation is not supported on Avalonia desktop platforms and always throws NotSupportedException.
    /// </summary>
    /// <param name="options">Optional media picker options such as a title for the picker dialog.</param>
    /// <returns>This method never returns; it always throws.</returns>
    /// <exception cref="NotSupportedException">Always thrown because camera capture is not available on desktop.</exception>
    public Task<FileResult?> CapturePhotoAsync(MediaPickerOptions? options = null)
    {
        throw new NotSupportedException("Camera capture is not supported on Avalonia desktop platforms.");
    }

    /// <summary>
    /// Captures a video using the device camera. This operation is not supported on Avalonia desktop platforms and always throws NotSupportedException.
    /// </summary>
    /// <param name="options">Optional media picker options such as a title for the picker dialog.</param>
    /// <returns>This method never returns; it always throws.</returns>
    /// <exception cref="NotSupportedException">Always thrown because camera capture is not available on desktop.</exception>
    public Task<FileResult?> CaptureVideoAsync(MediaPickerOptions? options = null)
    {
        throw new NotSupportedException("Camera capture is not supported on Avalonia desktop platforms.");
    }

    /// <summary>
    /// Opens a file picker dialog filtered to image file types and returns the selected photo.
    /// </summary>
    /// <param name="options">Optional media picker options such as a title for the picker dialog.</param>
    /// <returns>A FileResult representing the selected photo, or <see langword="null"/> if the user cancelled the dialog.</returns>
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

    /// <summary>
    /// Opens a file picker dialog filtered to image file types and allows the user to select multiple photos.
    /// </summary>
    /// <param name="options">Optional media picker options such as a title for the picker dialog.</param>
    /// <returns>A list of FileResult objects representing the selected photos, or an empty list if the user cancelled the dialog.</returns>
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

    /// <summary>
    /// Opens a file picker dialog filtered to video file types and returns the selected video.
    /// </summary>
    /// <param name="options">Optional media picker options such as a title for the picker dialog.</param>
    /// <returns>A FileResult representing the selected video, or <see langword="null"/> if the user cancelled the dialog.</returns>
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

    /// <summary>
    /// Opens a file picker dialog filtered to video file types and allows the user to select multiple videos.
    /// </summary>
    /// <param name="options">Optional media picker options such as a title for the picker dialog.</param>
    /// <returns>A list of FileResult objects representing the selected videos, or an empty list if the user cancelled the dialog.</returns>
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
