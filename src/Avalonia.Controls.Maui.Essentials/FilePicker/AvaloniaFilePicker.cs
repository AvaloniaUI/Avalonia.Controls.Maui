using Avalonia.Platform.Storage;
using Microsoft.Maui.Storage;
using AvaloniaFilePickerFileType = Avalonia.Platform.Storage.FilePickerFileType;
using MauiFilePickerFileType = Microsoft.Maui.Storage.FilePickerFileType;

namespace Avalonia.Controls.Maui.Essentials;

public class AvaloniaFilePicker : IFilePicker
{
    readonly IAvaloniaEssentialsPlatformProvider _platformProvider;

    internal AvaloniaFilePicker(IAvaloniaEssentialsPlatformProvider platformProvider)
    {
        _platformProvider = platformProvider;
    }

    public async Task<FileResult?> PickAsync(PickOptions? options)
    {
        var topLevel = _platformProvider.GetTopLevel()
            ?? throw new InvalidOperationException("Unable to get Avalonia TopLevel. Ensure the application has been fully initialized.");

        var avaloniaOptions = ConvertOptions(options, allowMultiple: false);
        var results = await topLevel.StorageProvider.OpenFilePickerAsync(avaloniaOptions).ConfigureAwait(false);

        if (results.Count == 0)
            return null;

        var path = results[0].TryGetLocalPath();
        return path is not null ? new FileResult(path) : null;
    }

    public async Task<IEnumerable<FileResult?>> PickMultipleAsync(PickOptions? options)
    {
        var topLevel = _platformProvider.GetTopLevel()
            ?? throw new InvalidOperationException("Unable to get Avalonia TopLevel. Ensure the application has been fully initialized.");

        var avaloniaOptions = ConvertOptions(options, allowMultiple: true);
        var results = await topLevel.StorageProvider.OpenFilePickerAsync(avaloniaOptions).ConfigureAwait(false);

        if (results.Count == 0)
            return [];

        var fileResults = new List<FileResult?>();
        foreach (var result in results)
        {
            var path = result.TryGetLocalPath();
            if (path is not null)
                fileResults.Add(new FileResult(path));
        }

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
                // Values may be extensions (.png) or MIME types (image/png)
                // Avalonia FilePickerFileType.Patterns expects glob patterns (*.png)
                if (value.StartsWith('.'))
                    patterns.Add($"*{value}");
                else if (!value.Contains('/'))
                    patterns.Add($"*.{value}");
                // Skip MIME types and UTIs — Avalonia uses glob patterns
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
