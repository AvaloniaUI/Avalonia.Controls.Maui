using System.Net;
using System.Text.RegularExpressions;
using Avalonia.Controls.Documents;
using Avalonia.Media;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Converts a subset of HTML markup into Avalonia <see cref="Inline"/> elements
/// for display in a <see cref="Avalonia.Controls.TextBlock"/>.
/// </summary>
internal static partial class HtmlToInlinesConverter
{
    private static readonly Regex TagRegex = CreateTagRegex();

    private readonly record struct StyleState(
        bool Bold,
        bool Italic,
        bool Underline,
        bool Strikethrough,
        bool Monospace,
        double? FontSize,
        IBrush? Foreground,
        IBrush? Background)
    {
        public static StyleState Default => new(false, false, false, false, false, null, null, null);

        public StyleState WithBold(bool value) => this with { Bold = value };
        public StyleState WithItalic(bool value) => this with { Italic = value };
        public StyleState WithUnderline(bool value) => this with { Underline = value };
        public StyleState WithStrikethrough(bool value) => this with { Strikethrough = value };
        public StyleState WithMonospace(bool value) => this with { Monospace = value };
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

        // List tracking
        var listTypeStack = new Stack<ListType>();
        var listCounterStack = new Stack<int>();

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

                case "u" or "ins":
                    if (isClosing)
                        PopStyle(styleStack);
                    else
                        PushStyle(styleStack, styleStack.Peek().WithUnderline(true));
                    break;

                case "s" or "strike" or "del":
                    if (isClosing)
                        PopStyle(styleStack);
                    else
                        PushStyle(styleStack, styleStack.Peek().WithStrikethrough(true));
                    break;

                case "code" or "tt":
                    if (isClosing)
                        PopStyle(styleStack);
                    else
                        PushStyle(styleStack, styleStack.Peek().WithMonospace(true));
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

                case "span":
                    if (isClosing)
                    {
                        PopStyle(styleStack);
                    }
                    else
                    {
                        var style = ApplyInlineStyle(styleStack.Peek(), attributes);
                        PushStyle(styleStack, style);
                    }
                    break;

                case "h1":
                    HandleHeaderTag(inlines, styleStack, isClosing, 24);
                    break;
                case "h2":
                    HandleHeaderTag(inlines, styleStack, isClosing, 20);
                    break;
                case "h3":
                    HandleHeaderTag(inlines, styleStack, isClosing, 18);
                    break;
                case "h4":
                    HandleHeaderTag(inlines, styleStack, isClosing, 16);
                    break;
                case "h5":
                    HandleHeaderTag(inlines, styleStack, isClosing, 14);
                    break;
                case "h6":
                    HandleHeaderTag(inlines, styleStack, isClosing, 13);
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

                case "div" or "blockquote":
                    if (isClosing)
                    {
                        inlines.Add(new LineBreak());
                    }
                    else if (inlines.Count > 0 && inlines[^1] is not LineBreak)
                    {
                        inlines.Add(new LineBreak());
                    }
                    break;

                case "ul":
                    if (isClosing)
                    {
                        PopList(listTypeStack, listCounterStack);
                        inlines.Add(new LineBreak());
                    }
                    else
                    {
                        if (inlines.Count > 0 && inlines[^1] is not LineBreak)
                            inlines.Add(new LineBreak());
                        listTypeStack.Push(ListType.Unordered);
                        listCounterStack.Push(0);
                    }
                    break;

                case "ol":
                    if (isClosing)
                    {
                        PopList(listTypeStack, listCounterStack);
                        inlines.Add(new LineBreak());
                    }
                    else
                    {
                        if (inlines.Count > 0 && inlines[^1] is not LineBreak)
                            inlines.Add(new LineBreak());

                        var start = 1;
                        var startAttr = GetAttributeValue(attributes, "start");
                        if (startAttr != null && int.TryParse(startAttr, out var s))
                            start = s;

                        listTypeStack.Push(ListType.Ordered);
                        listCounterStack.Push(start - 1); // will be incremented on first <li>
                    }
                    break;

                case "li":
                    if (!isClosing && listTypeStack.Count > 0)
                    {
                        if (inlines.Count > 0 && inlines[^1] is not LineBreak)
                            inlines.Add(new LineBreak());

                        var indent = new string(' ', (listTypeStack.Count - 1) * 4);
                        var listType = listTypeStack.Peek();

                        string bullet;
                        if (listType == ListType.Ordered)
                        {
                            var counter = listCounterStack.Pop() + 1;
                            listCounterStack.Push(counter);
                            bullet = $"{indent}{counter}. ";
                        }
                        else
                        {
                            bullet = $"{indent}\u2022 ";
                        }

                        AddTextRun(inlines, bullet, styleStack.Peek());
                    }
                    break;

                case "hr":
                    if (inlines.Count > 0 && inlines[^1] is not LineBreak)
                        inlines.Add(new LineBreak());
                    AddTextRun(inlines, "\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500", styleStack.Peek());
                    inlines.Add(new LineBreak());
                    break;

                case "sup":
                    if (isClosing)
                        PopStyle(styleStack);
                    else
                    {
                        var currentSize = styleStack.Peek().FontSize ?? 14.0;
                        PushStyle(styleStack, styleStack.Peek().WithFontSize(currentSize * 0.7));
                    }
                    break;

                case "sub":
                    if (isClosing)
                        PopStyle(styleStack);
                    else
                    {
                        var currentSize = styleStack.Peek().FontSize ?? 14.0;
                        PushStyle(styleStack, styleStack.Peek().WithFontSize(currentSize * 0.7));
                    }
                    break;

                case "mark":
                    if (isClosing)
                        PopStyle(styleStack);
                    else
                        PushStyle(styleStack, styleStack.Peek() with { Background = new SolidColorBrush(Colors.Yellow) });
                    break;

                case "pre":
                    if (isClosing)
                        PopStyle(styleStack);
                    else
                        PushStyle(styleStack, styleStack.Peek().WithMonospace(true));
                    break;

                // Self-closing tags or unknown: ignore
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

    private static void HandleHeaderTag(List<Inline> inlines, Stack<StyleState> styleStack, bool isClosing, double fontSize)
    {
        if (isClosing)
        {
            PopStyle(styleStack);
            inlines.Add(new LineBreak());
        }
        else
        {
            if (inlines.Count > 0 && inlines[^1] is not LineBreak)
                inlines.Add(new LineBreak());

            PushStyle(styleStack, styleStack.Peek().WithBold(true).WithFontSize(fontSize));
        }
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
        if (style.Monospace)
            run.FontFamily = new FontFamily("Cascadia Code, Consolas, Courier New, monospace");
        if (style.FontSize.HasValue)
            run.FontSize = style.FontSize.Value;
        if (style.Foreground != null)
            run.Foreground = style.Foreground;
        if (style.Background != null)
            run.Background = style.Background;

        if (style.Underline || style.Strikethrough)
        {
            var decorations = new TextDecorationCollection();
            if (style.Underline)
                decorations.Add(new TextDecoration { Location = TextDecorationLocation.Underline });
            if (style.Strikethrough)
                decorations.Add(new TextDecoration { Location = TextDecorationLocation.Strikethrough });
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

    private static void PopList(Stack<ListType> typeStack, Stack<int> counterStack)
    {
        if (typeStack.Count > 0)
            typeStack.Pop();
        if (counterStack.Count > 0)
            counterStack.Pop();
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
        // Match name="value" or name='value'
        var match = Regex.Match(attributes, $@"{name}\s*=\s*[""']([^""']*)[""']", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : null;
    }

    private static StyleState ApplyInlineStyle(StyleState style, string attributes)
    {
        var styleAttr = GetAttributeValue(attributes, "style");
        if (styleAttr == null)
            return style;

        foreach (var declaration in styleAttr.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = declaration.Split(':', 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2)
                continue;

            var prop = parts[0].ToLowerInvariant();
            var value = parts[1].Trim();

            switch (prop)
            {
                case "color":
                    if (TryParseColor(value, out var fg))
                        style = style.WithForeground(new SolidColorBrush(fg));
                    break;
                case "background-color" or "background":
                    if (TryParseColor(value, out var bg))
                        style = style with { Background = new SolidColorBrush(bg) };
                    break;
                case "font-weight":
                    style = value is "bold" or "bolder" or "700" or "800" or "900"
                        ? style.WithBold(true)
                        : style.WithBold(false);
                    break;
                case "font-style":
                    style = value == "italic" || value == "oblique"
                        ? style.WithItalic(true)
                        : style.WithItalic(false);
                    break;
                case "text-decoration":
                    if (value.Contains("underline"))
                        style = style.WithUnderline(true);
                    if (value.Contains("line-through"))
                        style = style.WithStrikethrough(true);
                    break;
                case "font-size":
                    if (TryParseCssFontSize(value, out var cssFontSize))
                        style = style.WithFontSize(cssFontSize);
                    break;
                case "font-family":
                    if (value.Contains("monospace") || value.Contains("courier", StringComparison.OrdinalIgnoreCase))
                        style = style.WithMonospace(true);
                    break;
            }
        }

        return style;
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

    private static bool TryParseCssFontSize(string value, out double fontSize)
    {
        fontSize = 14;
        value = value.Trim().ToLowerInvariant();

        // Named sizes
        switch (value)
        {
            case "xx-small": fontSize = 9; return true;
            case "x-small": fontSize = 10; return true;
            case "small": fontSize = 13; return true;
            case "medium": fontSize = 16; return true;
            case "large": fontSize = 18; return true;
            case "x-large": fontSize = 24; return true;
            case "xx-large": fontSize = 32; return true;
        }

        // px values
        if (value.EndsWith("px") && double.TryParse(value[..^2], out fontSize))
            return true;

        // pt values
        if (value.EndsWith("pt") && double.TryParse(value[..^2], out var pt))
        {
            fontSize = pt * 4.0 / 3.0; // pt to px
            return true;
        }

        // em values (relative to default 14px)
        if (value.EndsWith("em") && double.TryParse(value[..^2], out var em))
        {
            fontSize = em * 14.0;
            return true;
        }

        // Plain number
        if (double.TryParse(value, out fontSize))
            return true;

        return false;
    }

    private enum ListType
    {
        Unordered,
        Ordered
    }

    [GeneratedRegex(@"<(/?)(\w+)([^>]*)/?>")]
    private static partial Regex CreateTagRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex CollapseWhitespaceRegex();

    [GeneratedRegex(@"rgba?\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*(?:,\s*([\d.]+))?\s*\)")]
    private static partial Regex RgbRegex();
}
