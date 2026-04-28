namespace Avalonia.Controls.Maui.Services;

/// <summary>
/// Configures downloads performed by <see cref="AvaloniaUriImageSourceService"/>.
/// </summary>
public sealed class AvaloniaUriImageSourceServiceOptions
{
    /// <summary>
    /// Gets or sets the optional HTTP User-Agent header used when downloading remote URI images.
    /// The default is <see langword="null"/>, which sends no User-Agent header unless the supplied
    /// <see cref="System.Net.Http.HttpClient"/> already has one configured.
    /// </summary>
    public string? UserAgent { get; set; }
}
