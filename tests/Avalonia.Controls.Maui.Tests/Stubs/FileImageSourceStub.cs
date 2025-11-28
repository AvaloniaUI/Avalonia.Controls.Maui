using Microsoft.Maui;

namespace Avalonia.Controls.Maui.Tests.Stubs;

/// <summary>
/// Stub implementation of IFileImageSource for testing.
/// </summary>
public class FileImageSourceStub : IFileImageSource
{
    public string File { get; set; } = string.Empty;

    public bool IsEmpty => string.IsNullOrEmpty(File);
}
