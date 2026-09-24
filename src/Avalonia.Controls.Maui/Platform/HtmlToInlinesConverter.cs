using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using TextTransform = Microsoft.Maui.TextTransform;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Converts the HTML subset supported by MAUI Label (b, strong, i, em, u, br, p, a, font) into Avalonia <see cref="Inline"/> elements.
/// </summary>
internal static partial class HtmlToInlinesConverter
{
    private readonly record struct StyleState(
        bool Bold,
        bool Italic,
        bool Underline,
        double? FontSize,
        FontFamily? FontFamily,
        IBrush? Foreground);

    /// <summary>
    /// Parses an HTML string and returns a list of <see cref="Inline"/> elements.
    /// </summary>
    public static IList<Inline> Convert(string html, TextTransform textTransform = TextTransform.None)
    {
        var inlines = new List<Inline>();
        var styleStack = new Stack<(string Tag, StyleState Style)>();
        styleStack.Push((string.Empty, default));

        var pos = 0;

        foreach (Match match in TagRegex().Matches(html))
        {
            if (match.Index > pos)
                AddText(inlines, html[pos..match.Index], styleStack.Peek().Style, textTransform);

            pos = match.Index + match.Length;

            var tagName = match.Groups[2].Value.ToLowerInvariant();
            if (tagName.Length == 0)
                continue;

            var attributes = match.Groups[3].Value;
            var isClosing = match.Groups[1].Value == "/";
            var isSelfClosing = attributes.EndsWith('/');

            switch (tagName)
            {
                case "br":
                    TrimTrailingSpace(inlines);
                    inlines.Add(new LineBreak());
                    break;

                case "p":
                    AddParagraphBreak(inlines);
                    break;

                case "b" or "strong" or "i" or "em" or "u" or "a" or "font":
                    if (isClosing)
                        PopStyle(styleStack, tagName);
                    else if (!isSelfClosing)
                        styleStack.Push((tagName, ApplyTag(styleStack.Peek().Style, tagName, attributes)));
                    break;
            }
        }

        if (pos < html.Length)
            AddText(inlines, html[pos..], styleStack.Peek().Style, textTransform);

        while (inlines.Count > 0 && inlines[^1] is LineBreak)
            inlines.RemoveAt(inlines.Count - 1);

        TrimTrailingSpace(inlines);

        return inlines;
    }

    private static StyleState ApplyTag(StyleState style, string tagName, string attributes)
    {
        switch (tagName)
        {
            case "b" or "strong":
                return style with { Bold = true };

            case "i" or "em":
                return style with { Italic = true };

            case "u":
                return style with { Underline = true };

            case "a":
                return string.IsNullOrWhiteSpace(GetAttributeValue(attributes, "href"))
                    ? style
                    : style with { Underline = true, Foreground = new SolidColorBrush(Colors.RoyalBlue) };

            case "font":
                if (GetAttributeValue(attributes, "color") is { } color && TryParseColor(color, out var fontColor))
                    style = style with { Foreground = new SolidColorBrush(fontColor) };

                if (GetAttributeValue(attributes, "size") is { } size && TryParseFontSize(size, out var fontSize))
                    style = style with { FontSize = fontSize };

                if (GetAttributeValue(attributes, "face") is { } face && !string.IsNullOrWhiteSpace(face))
                    style = style with { FontFamily = new FontFamily(face.Trim()) };

                return style;

            default:
                return style;
        }
    }

    private static void PopStyle(Stack<(string Tag, StyleState Style)> stack, string tagName)
    {
        // Unwind to the matching open tag so mis-nested markup does not leak styles.
        if (!stack.Any(entry => entry.Tag == tagName))
            return;

        while (stack.Count > 1)
        {
            if (stack.Pop().Tag == tagName)
                break;
        }
    }

    private static void AddText(List<Inline> inlines, string rawText, StyleState style, TextTransform textTransform)
    {
        var text = CollapseWhitespaceRegex().Replace(rawText, " ");

        if (text.StartsWith(' ') && EndsWithSpaceOrLineStart(inlines))
            text = text.TrimStart(' ');

        if (text.Length == 0)
            return;

        text = TextTransformUtilities.GetTransformedText(WebUtility.HtmlDecode(text), textTransform);

        var run = new Run(text);

        if (style.Bold)
            run.FontWeight = FontWeight.Bold;
        if (style.Italic)
            run.FontStyle = FontStyle.Italic;
        if (style.FontSize.HasValue)
            run.FontSize = style.FontSize.Value;
        if (style.FontFamily != null)
            run.FontFamily = style.FontFamily;
        if (style.Foreground != null)
            run.Foreground = style.Foreground;
        if (style.Underline)
            run.TextDecorations = Avalonia.Media.TextDecorations.Underline;

        inlines.Add(run);
    }

    private static bool EndsWithSpaceOrLineStart(List<Inline> inlines) =>
        inlines.Count == 0 ||
        inlines[^1] is LineBreak ||
        inlines[^1] is Run { Text: { } text } && text.EndsWith(' ');

    private static void TrimTrailingSpace(List<Inline> inlines)
    {
        if (inlines.Count > 0 && inlines[^1] is Run { Text: { } text } run && text.EndsWith(' '))
            run.Text = text.TrimEnd(' ');
    }

    private static void AddParagraphBreak(List<Inline> inlines)
    {
        if (inlines.Count == 0)
            return;

        TrimTrailingSpace(inlines);

        var trailingBreaks = 0;
        for (var i = inlines.Count - 1; i >= 0 && inlines[i] is LineBreak; i--)
            trailingBreaks++;

        for (; trailingBreaks < 2; trailingBreaks++)
            inlines.Add(new LineBreak());
    }

    private static string? GetAttributeValue(string attributes, string name)
    {
        foreach (Match match in AttributeRegex().Matches(attributes))
        {
            if (string.Equals(match.Groups[1].Value, name, StringComparison.OrdinalIgnoreCase))
            {
                var value = match.Groups[2].Success ? match.Groups[2].Value
                    : match.Groups[3].Success ? match.Groups[3].Value
                    : match.Groups[4].Value;
                return WebUtility.HtmlDecode(value);
            }
        }

        return null;
    }

    private static bool TryParseColor(string value, out Color color)
    {
        color = default;
        value = value.Trim();

        var rgbMatch = RgbRegex().Match(value);
        if (rgbMatch.Success)
        {
            if (byte.TryParse(rgbMatch.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r) &&
                byte.TryParse(rgbMatch.Groups[2].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var g) &&
                byte.TryParse(rgbMatch.Groups[3].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var b))
            {
                byte a = 255;
                if (rgbMatch.Groups[4].Success &&
                    double.TryParse(rgbMatch.Groups[4].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var alpha))
                {
                    a = (byte)Math.Round(Math.Clamp(alpha, 0d, 1d) * 255);
                }

                color = Color.FromArgb(a, r, g, b);
                return true;
            }

            return false;
        }

        return Color.TryParse(value, out color);
    }

    private static bool TryParseFontSize(string value, out double fontSize)
    {
        fontSize = 0;
        value = value.Trim();

        if (!int.TryParse(value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var htmlSize))
            return false;

        // "+n" / "-n" are relative to the default size 3; the result is clamped to the HTML range 1-7.
        if (value.StartsWith('+') || value.StartsWith('-'))
            htmlSize += 3;

        fontSize = Math.Clamp(htmlSize, 1, 7) switch
        {
            1 => 10,
            2 => 13,
            3 => 16,
            4 => 18,
            5 => 24,
            6 => 32,
            _ => 48,
        };
        return true;
    }

    [GeneratedRegex("""<!--[\s\S]*?-->|<(/?)([a-zA-Z][a-zA-Z0-9]*)((?:[^>"']|"[^"]*"|'[^']*')*)>""")]
    private static partial Regex TagRegex();

    [GeneratedRegex("""([a-zA-Z_:][-a-zA-Z0-9_:.]*)\s*=\s*(?:"([^"]*)"|'([^']*)'|([^\s"'>/]+))""")]
    private static partial Regex AttributeRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex CollapseWhitespaceRegex();

    [GeneratedRegex(@"^rgba?\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*(?:,\s*([\d.]+))?\s*\)$")]
    private static partial Regex RgbRegex();
}
