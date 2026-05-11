using Avalonia.Platform.Storage;
using Microsoft.Maui.Storage;
using AvaloniaFilePickerFileType = Avalonia.Platform.Storage.FilePickerFileType;
using MauiFilePickerFileType = Microsoft.Maui.Storage.FilePickerFileType;

namespace Avalonia.Controls.Maui.Essentials;

/// <summary>
/// Implements IFilePicker using Avalonia's StorageProvider API to present native file picker dialogs on desktop and browser platforms.
/// </summary>
public class AvaloniaFilePicker : IFilePicker
{
    readonly IAvaloniaEssentialsPlatformProvider _platformProvider;

    internal AvaloniaFilePicker(IAvaloniaEssentialsPlatformProvider platformProvider)
    {
        _platformProvider = platformProvider;
    }

    /// <summary>
    /// Displays a file picker dialog that allows the user to select a single file.
    /// </summary>
    /// <param name="options">The options that configure the file picker, including title and allowed file types. Can be <c>null</c> for default behavior.</param>
    /// <returns>A <see cref="FileResult"/> representing the selected file, or <c>null</c> if the user cancelled the dialog.</returns>
    public async Task<FileResult?> PickAsync(PickOptions? options)
    {
        var topLevel = _platformProvider.GetTopLevel()
            ?? throw new InvalidOperationException("Unable to get Avalonia TopLevel. Ensure the application has been fully initialized.");

        var avaloniaOptions = ConvertOptions(options, allowMultiple: false);
        var results = await topLevel.StorageProvider.OpenFilePickerAsync(avaloniaOptions).ConfigureAwait(false);

        if (results.Count == 0)
            return null;

        return new AvaloniaFileResult(results[0]);
    }

    /// <summary>
    /// Displays a file picker dialog that allows the user to select multiple files.
    /// </summary>
    /// <param name="options">The options that configure the file picker, including title and allowed file types. Can be <c>null</c> for default behavior.</param>
    /// <returns>A collection of <see cref="FileResult"/> objects representing the selected files. Returns an empty collection if the user cancels the dialog.</returns>
    public async Task<IEnumerable<FileResult>?> PickMultipleAsync(PickOptions? options)
    {
        var topLevel = _platformProvider.GetTopLevel()
            ?? throw new InvalidOperationException("Unable to get Avalonia TopLevel. Ensure the application has been fully initialized.");

        var avaloniaOptions = ConvertOptions(options, allowMultiple: true);
        var results = await topLevel.StorageProvider.OpenFilePickerAsync(avaloniaOptions).ConfigureAwait(false);

        if (results.Count == 0)
            return [];

        var fileResults = new List<FileResult>(results.Count);
        foreach (var result in results)
            fileResults.Add(new AvaloniaFileResult(result));

        return fileResults;
    }

    static FilePickerOpenOptions ConvertOptions(PickOptions? options, bool allowMultiple)
    {
        var avaloniaOptions = new FilePickerOpenOptions
        {
            AllowMultiple = allowMultiple,
            Title = options?.PickerTitle
        };

        if (options?.FileTypes is not null)
        {
            var fileTypes = ConvertFileTypes(options.FileTypes);
            if (fileTypes is not null)
                avaloniaOptions.FileTypeFilter = [fileTypes];
        }

        return avaloniaOptions;
    }

    static AvaloniaFilePickerFileType? ConvertFileTypes(MauiFilePickerFileType mauiFileTypes)
    {
        try
        {
            var values = mauiFileTypes.Value;
            if (values is null)
                return null;

            var patterns = new List<string>();
            foreach (var value in values)
            {
                if (value.StartsWith('.'))
                    patterns.Add($"*{value}");
                else if (!value.Contains('/'))
                    patterns.Add($"*.{value}");
            }

            if (patterns.Count == 0)
                return null;

            return new AvaloniaFilePickerFileType("Selected Files")
            {
                Patterns = patterns
            };
        }
        catch (PlatformNotSupportedException)
        {
            return null;
        }
    }
}
