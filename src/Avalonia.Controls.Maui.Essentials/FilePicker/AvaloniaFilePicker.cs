using Avalonia.Platform.Storage;
using Microsoft.Maui.Storage;
using AvaloniaFilePickerFileType = Avalonia.Platform.Storage.FilePickerFileType;
using MauiFilePickerFileType = Microsoft.Maui.Storage.FilePickerFileType;

namespace Avalonia.Controls.Maui.Essentials;

/// <summary>
/// Implements <see cref="IFilePicker"/> using Avalonia's <c>StorageProvider</c> API to present native file
/// picker dialogs on desktop and browser platforms.
/// </summary>
/// <remarks>
/// <para>
/// The returned <see cref="FileResult"/> instances are <see cref="AvaloniaFileResult"/> wrappers around the
/// underlying Avalonia <see cref="IStorageFile"/>. This is intentional: on platforms such as
/// Avalonia.Browser the picked file does not have a real filesystem path, and the default
/// <see cref="FileResult"/> would not be able to open the stream.
/// </para>
/// <para>
/// <b>Reading file contents:</b> always prefer <c>OpenReadAsync()</c> over reading <c>FullPath</c> with
/// <c>System.IO.File</c> APIs. When the underlying storage item has no local path (Browser, sandboxed
/// providers, network shares), <c>FullPath</c> falls back to the bare file name, which would cause
/// <c>File.OpenRead(result.FullPath)</c> to throw a <see cref="FileNotFoundException"/> resolved against
/// the current working directory. <c>await result.OpenReadAsync()</c> routes through Avalonia's storage
/// provider and works uniformly across platforms.
/// </para>
/// <para>
/// Consumers needing richer Avalonia storage APIs (write streams, properties, bookmarks) can cast the
/// result to <see cref="AvaloniaFileResult"/> and access <see cref="AvaloniaFileResult.StorageFile"/>.
/// </para>
/// </remarks>
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
