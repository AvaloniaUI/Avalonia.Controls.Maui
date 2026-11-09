using System.Text.RegularExpressions;

namespace Avalonia.Controls.Maui.Tests.Services;

public class ImageSourceServiceCallTests
{
    // Image sources load through ImageSourceLoader, which follows the display scale; only the
    // loader and the services themselves call GetImageAsync.
    static readonly Regex AllowedFile = new(
        @"^src/[^/]+/Services/(ImageSourceLoader|IAvaloniaImageSourceService|Avalonia\w*ImageSourceService(\.\w+)?)\.cs$");

    [Fact(DisplayName = "Only the image loader calls the image source services")]
    public void OnlyTheLoaderCallsImageSourceServices()
    {
        var root = FindRepositoryRoot();
        var calls = Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            .Select(path => Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/'))
            .Where(path => !AllowedFile.IsMatch(path))
            .SelectMany(path => File.ReadLines(Path.Combine(root, path))
                .Select((line, index) => (path, line, number: index + 1)))
            .Where(x => Regex.IsMatch(x.line, @"\.GetImageAsync\s*\("))
            .Select(x => $"{x.path}:{x.number}: {x.line.Trim()}")
            .ToList();

        Assert.True(calls.Count == 0,
            "Load image sources with ImageSourceLoader instead of calling GetImageAsync:\n" + string.Join("\n", calls));
    }

    static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory != null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Avalonia.Controls.Maui.slnx")))
                return directory.FullName;
        }

        throw new InvalidOperationException("The repository root was not found.");
    }
}
