using System.ComponentModel;
using Microsoft.Maui;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// A utilities class for text transformations.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class TextTransformUtilities
{
    /// <summary>
    /// Applies the <paramref name="textTransform"/> to <paramref name="source"/>.
    /// </summary>
    /// <param name="source">The text to transform.</param>
    /// <param name="textTransform">The transform to apply to <paramref name="source"/>.</param>
    /// <returns>The transformed text.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static string GetTransformedText(string source, TextTransform textTransform)
    {
        if (string.IsNullOrEmpty(source))
        {
            return string.Empty;
        }

        switch (textTransform)
        {
            case TextTransform.None:
            default:
                return source;
            case TextTransform.Lowercase:
                return source.ToLowerInvariant();
            case TextTransform.Uppercase:
                return source.ToUpperInvariant();
        }
    }
}