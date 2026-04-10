using System.Net;
using System.Text.RegularExpressions;
using Avalonia.Controls.Documents;
using Avalonia.Media;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Converts a subset of HTML markup into Avalonia <see cref="Inline"/> elements
/// for display in a <see cref="Avalonia.Controls.TextBlock"/>.
/// Only supports the tags that MAUI Native supports: b, i, u, br, p, a, font.
/// </summary>
internal static partial class HtmlToInlinesConverter
{
    private static readonly Regex TagRegex = CreateTagRegex();

    private readonly record struct StyleState(
        bool Bold,
        bool Italic,
        bool Underline,
        double? FontSize,
        IBrush? Foreground)
    {
        public static StyleState Default => new(false, false, false, null, null);

        public StyleState WithBold(bool value) => this with { Bold = value };
        public StyleState WithItalic(bool value) => this with { Italic = value };
        public StyleState WithUnderline(bool value) => this with { Underline = value };
        public StyleState WithFontSize(double? value) => this with { FontSize = value };
        public StyleState WithForeground(IBrush? value) => this with { Foreground = value };
    }

    /// <summary>
    /// Parses an HTML string and returns a list of <see cref="Inline"/> elements.
    /// </summary>
    public static IList<Inline> Convert(string html)
    {
        var inlines = new List<Inline>();
        var styleStack = new Stack<StyleState>();
        styleStack.Push(StyleState.Default);

        // Link tracking
        Uri? pendingHref = null;
        int linkStartIndex = -1;

        var pos = 0;

        foreach (Match match in TagRegex.Matches(html))
        {
            // Add text before this tag
            if (match.Index > pos)
            {
                var text = html[pos..match.Index];
                AddTextRun(inlines, DecodeHtml(text), styleStack.Peek());
            }

            pos = match.Index + match.Length;

            var isClosing = match.Groups[1].Value == "/";
            var tagName = match.Groups[2].Value.ToLowerInvariant();
            var attributes = match.Groups[3].Value;

            switch (tagName)
            {
                case "b" or "strong":
                    if (isClosing)
                        PopStyle(styleStack);
                    else
                        PushStyle(styleStack, styleStack.Peek().WithBold(true));
                    break;

                case "i" or "em":
                    if (isClosing)
                        PopStyle(styleStack);
                    else
                        PushStyle(styleStack, styleStack.Peek().WithItalic(true));
                    break;

                case "u":
                    if (isClosing)
                        PopStyle(styleStack);
                    else
                        PushStyle(styleStack, styleStack.Peek().WithUnderline(true));
                    break;

                case "a":
                    if (isClosing)
                    {
                        PopStyle(styleStack);
                        if (pendingHref != null && linkStartIndex >= 0)
                        {
                            var linkText = string.Join("", inlines.Skip(linkStartIndex).OfType<Run>().Select(r => r.Text));
                            inlines.RemoveRange(linkStartIndex, inlines.Count - linkStartIndex);
                            var button = new HyperlinkButton
                            {
                                Content = linkText,
                                NavigateUri = pendingHref,
                                Padding = new Thickness(0),
                                Margin = new Thickness(0),
                            };
                            inlines.Add(new InlineUIContainer(button));
                        }
                        pendingHref = null;
                        linkStartIndex = -1;
                    }
                    else
                    {
                        var href = GetAttributeValue(attributes, "href");
                        if (href != null && Uri.TryCreate(href, UriKind.Absolute, out var uri))
                        {
                            pendingHref = uri;
                            linkStartIndex = inlines.Count;
                        }
                        var style = styleStack.Peek()
                            .WithUnderline(true)
                            .WithForeground(new SolidColorBrush(Colors.RoyalBlue));
                        PushStyle(styleStack, style);
                    }
                    break;

                case "font":
                    if (isClosing)
                    {
                        PopStyle(styleStack);
                    }
                    else
                    {
                        var style = styleStack.Peek();
                        var color = GetAttributeValue(attributes, "color");
                        if (color != null && TryParseColor(color, out var fontColor))
                            style = style.WithForeground(new SolidColorBrush(fontColor));

                        var size = GetAttributeValue(attributes, "size");
                        if (size != null && TryParseFontSize(size, out var fontSize))
                            style = style.WithFontSize(fontSize);

                        PushStyle(styleStack, style);
                    }
                    break;

                case "br":
                    inlines.Add(new LineBreak());
                    break;

                case "p":
                    if (isClosing)
                    {
                        inlines.Add(new LineBreak());
                        inlines.Add(new LineBreak());
                    }
                    else if (inlines.Count > 0 && inlines[^1] is not LineBreak)
                    {
                        inlines.Add(new LineBreak());
                        inlines.Add(new LineBreak());
                    }
                    break;

                // Unknown tags: ignore
            }
        }

        // Add remaining text after the last tag
        if (pos < html.Length)
        {
            var remaining = html[pos..];
            AddTextRun(inlines, DecodeHtml(remaining), styleStack.Peek());
        }

        // Trim trailing line breaks
        while (inlines.Count > 0 && inlines[^1] is LineBreak)
            inlines.RemoveAt(inlines.Count - 1);

        return inlines;
    }

    private static void AddTextRun(List<Inline> inlines, string text, StyleState style)
    {
        if (string.IsNullOrEmpty(text))
            return;

        var run = new Run(text);

        if (style.Bold)
            run.FontWeight = FontWeight.Bold;
        if (style.Italic)
            run.FontStyle = FontStyle.Italic;
        if (style.FontSize.HasValue)
            run.FontSize = style.FontSize.Value;
        if (style.Foreground != null)
            run.Foreground = style.Foreground;

        if (style.Underline)
        {
            var decorations = new TextDecorationCollection();
            decorations.Add(new TextDecoration { Location = TextDecorationLocation.Underline });
            run.TextDecorations = decorations;
        }

        inlines.Add(run);
    }

    private static void PushStyle(Stack<StyleState> stack, StyleState style) => stack.Push(style);

    private static void PopStyle(Stack<StyleState> stack)
    {
        if (stack.Count > 1) // Always keep the default style
            stack.Pop();
    }

    private static string DecodeHtml(string text)
    {
        // Collapse whitespace (HTML behavior)
        text = CollapseWhitespaceRegex().Replace(text, " ");
        // Decode HTML entities
        return WebUtility.HtmlDecode(text);
    }

    private static string? GetAttributeValue(string attributes, string name)
    {
        var match = Regex.Match(attributes, $@"{name}\s*=\s*[""']([^""']*)[""']", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : null;
    }

    private static bool TryParseColor(string value, out Color color)
    {
        color = default;
        value = value.Trim();

        // Handle named colors and hex via Avalonia's Color.TryParse
        if (Color.TryParse(value, out color))
            return true;

        // Handle rgb(r, g, b) and rgba(r, g, b, a)
        var rgbMatch = RgbRegex().Match(value);
        if (rgbMatch.Success)
        {
            if (byte.TryParse(rgbMatch.Groups[1].Value, out var r) &&
                byte.TryParse(rgbMatch.Groups[2].Value, out var g) &&
                byte.TryParse(rgbMatch.Groups[3].Value, out var b))
            {
                byte a = 255;
                if (rgbMatch.Groups[4].Success && double.TryParse(rgbMatch.Groups[4].Value, out var alpha))
                    a = (byte)(alpha * 255);

                color = Color.FromArgb(a, r, g, b);
                return true;
            }
        }

        return false;
    }

    private static bool TryParseFontSize(string value, out double fontSize)
    {
        fontSize = 14;

        // HTML font size attribute: 1-7
        if (int.TryParse(value, out var htmlSize))
        {
            fontSize = htmlSize switch
            {
                1 => 10,
                2 => 13,
                3 => 16,
                4 => 18,
                5 => 24,
                6 => 32,
                7 => 48,
                _ => 16
            };
            return true;
        }

        return false;
    }

    [GeneratedRegex(@"<(/?)(\w+)([^>]*)/?>")]
    private static partial Regex CreateTagRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex CollapseWhitespaceRegex();

    [GeneratedRegex(@"rgba?\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*(?:,\s*([\d.]+))?\s*\)")]
    private static partial Regex RgbRegex();
}
