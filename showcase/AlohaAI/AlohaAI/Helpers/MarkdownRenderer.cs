namespace AlohaAI.Helpers;

/// <summary>
/// Converts Markdown text into MAUI views for display.
/// Supports headers, paragraphs, bold, italic, code blocks, inline code, and bullet lists.
/// </summary>
public static class MarkdownRenderer
{
    public static View Render(string markdown, bool darkMode = true)
    {
        var layout = new VerticalStackLayout { Spacing = 12 };
        if (string.IsNullOrWhiteSpace(markdown))
            return layout;

        var lines = markdown.Split('\n');
        var i = 0;

        while (i < lines.Length)
        {
            var line = lines[i];

            // Code block
            if (line.TrimStart().StartsWith("```"))
            {
                var codeLines = new List<string>();
                i++;
                while (i < lines.Length && !lines[i].TrimStart().StartsWith("```"))
                {
                    codeLines.Add(lines[i]);
                    i++;
                }
                i++; // skip closing ```
                layout.Add(CreateCodeBlock(string.Join('\n', codeLines), darkMode));
                continue;
            }

            // Table (lines starting with |)
            if (line.TrimStart().StartsWith('|'))
            {
                var tableLines = new List<string>();
                while (i < lines.Length && lines[i].TrimStart().StartsWith('|'))
                {
                    var stripped = lines[i].Trim().Trim('|').Replace(" ", "");
                    var isSeparator = stripped.Length > 0 && stripped.All(c => c == '-' || c == ':' || c == '|');
                    if (!isSeparator)
                        tableLines.Add(lines[i]);
                    i++;
                }
                layout.Add(CreateTable(tableLines, darkMode));
                continue;
            }

            // Empty line
            if (string.IsNullOrWhiteSpace(line))
            {
                i++;
                continue;
            }

            // Headers
            if (line.StartsWith("### "))
            {
                layout.Add(CreateHeader(line[4..], 18, darkMode));
                i++;
                continue;
            }
            if (line.StartsWith("## "))
            {
                layout.Add(CreateHeader(line[3..], 22, darkMode));
                i++;
                continue;
            }
            if (line.StartsWith("# "))
            {
                layout.Add(CreateHeader(line[2..], 26, darkMode));
                i++;
                continue;
            }

            // Bullet list
            if (line.TrimStart().StartsWith("- ") || line.TrimStart().StartsWith("* "))
            {
                var bulletText = line.TrimStart()[2..];
                layout.Add(CreateBullet(bulletText, darkMode));
                i++;
                continue;
            }

            // Numbered list (e.g., "1. text", "10. text")
            var trimmedForNum = line.TrimStart();
            var dotIdx = trimmedForNum.IndexOf(". ");
            if (dotIdx > 0 && dotIdx <= 3 && trimmedForNum[..dotIdx].All(char.IsDigit))
            {
                var num = trimmedForNum[..dotIdx];
                var numText = trimmedForNum[(dotIdx + 2)..];
                layout.Add(CreateNumberedItem(num, numText, darkMode));
                i++;
                continue;
            }

            // Blockquote
            if (line.TrimStart().StartsWith("> "))
            {
                layout.Add(CreateBlockquote(line.TrimStart()[2..], darkMode));
                i++;
                continue;
            }

            // Regular paragraph
            layout.Add(CreateParagraph(line, darkMode));
            i++;
        }

        return layout;
    }

    private static View CreateHeader(string text, int fontSize, bool darkMode)
    {
        return new Label
        {
            Text = text.Trim(),
            FontSize = fontSize,
            FontAttributes = FontAttributes.Bold,
            TextColor = darkMode ? Colors.White : Color.FromArgb("#1A1D21"),
            Margin = new Thickness(0, 8, 0, 4)
        };
    }

    private static View CreateParagraph(string text, bool darkMode)
    {
        var label = new Label
        {
            FontSize = 15,
            LineHeight = 1.5,
            TextColor = darkMode ? Color.FromArgb("#D6D0DC") : Color.FromArgb("#342D42")
        };

        label.FormattedText = ParseInlineFormatting(text, darkMode);
        return label;
    }

    private static View CreateBullet(string text, bool darkMode)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(20)),
                new ColumnDefinition(GridLength.Star)
            }
        };

        grid.Add(new Label
        {
            Text = "•",
            FontSize = 15,
            VerticalOptions = LayoutOptions.Start,
            TextColor = darkMode ? Color.FromArgb("#7BA3DB") : Color.FromArgb("#5B8FD4")
        }, 0);

        var contentLabel = new Label
        {
            FontSize = 15,
            LineHeight = 1.5,
            TextColor = darkMode ? Color.FromArgb("#D6D0DC") : Color.FromArgb("#342D42")
        };
        contentLabel.FormattedText = ParseInlineFormatting(text, darkMode);
        grid.Add(contentLabel, 1);

        return grid;
    }

    private static View CreateNumberedItem(string number, string text, bool darkMode)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(24)),
                new ColumnDefinition(GridLength.Star)
            }
        };

        grid.Add(new Label
        {
            Text = $"{number}.",
            FontSize = 15,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Start,
            TextColor = darkMode ? Color.FromArgb("#7BA3DB") : Color.FromArgb("#5B8FD4")
        }, 0);

        var contentLabel = new Label
        {
            FontSize = 15,
            LineHeight = 1.5,
            TextColor = darkMode ? Color.FromArgb("#D6D0DC") : Color.FromArgb("#342D42")
        };
        contentLabel.FormattedText = ParseInlineFormatting(text, darkMode);
        grid.Add(contentLabel, 1);

        return grid;
    }

    private static View CreateCodeBlock(string code, bool darkMode)
    {
        return new Border
        {
            BackgroundColor = darkMode ? Color.FromArgb("#0E0A1A") : Color.FromArgb("#F0EDF5"),
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
            Stroke = darkMode ? Color.FromArgb("#2D2538") : Color.FromArgb("#D6D0DC"),
            StrokeThickness = 1,
            Padding = new Thickness(16, 12),
            Content = new Label
            {
                Text = code,
                FontFamily = "CascadiaMono",
                FontSize = 13,
                TextColor = darkMode ? Color.FromArgb("#E6EDF3") : Color.FromArgb("#24292F"),
                LineHeight = 1.5
            }
        };
    }

    private static View CreateTable(List<string> rows, bool darkMode)
    {
        var parsedRows = rows
            .Select(r => r.Trim().Trim('|').Split('|').Select(c => c.Trim()).ToArray())
            .Where(r => r.Length > 0 && !r.All(c => c.Replace("-", "").Replace(":", "").Trim().Length == 0))
            .ToList();

        if (parsedRows.Count == 0)
            return new Label { Text = "" };

        var colCount = parsedRows.Max(r => r.Length);
        var grid = new Grid
        {
            ColumnSpacing = 0,
            RowSpacing = 0,
            Padding = new Thickness(0, 8)
        };

        for (var c = 0; c < colCount; c++)
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        for (var r = 0; r < parsedRows.Count; r++)
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        for (var r = 0; r < parsedRows.Count; r++)
        {
            for (var c = 0; c < parsedRows[r].Length; c++)
            {
                var isHeader = r == 0;
                var cellBorder = new Border
                {
                    BackgroundColor = isHeader
                        ? (darkMode ? Color.FromArgb("#1E2328") : Color.FromArgb("#E9ECEF"))
                        : Colors.Transparent,
                    Stroke = darkMode ? Color.FromArgb("#2D2538") : Color.FromArgb("#D6D0DC"),
                    StrokeThickness = 0.5,
                    Padding = new Thickness(10, 8),
                    Content = new Label
                    {
                        Text = parsedRows[r][c],
                        FontSize = 13,
                        FontAttributes = isHeader ? FontAttributes.Bold : FontAttributes.None,
                        TextColor = darkMode ? Colors.White : Color.FromArgb("#212529")
                    }
                };
                grid.SetColumn(cellBorder, c);
                grid.SetRow(cellBorder, r);
                grid.Children.Add(cellBorder);
            }
        }

        return new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 8 },
            Stroke = darkMode ? Color.FromArgb("#2D2538") : Color.FromArgb("#D6D0DC"),
            StrokeThickness = 1,
            Content = grid
        };
    }

    private static View CreateBlockquote(string text, bool darkMode)
    {
        var quoteGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(4)),
                new ColumnDefinition(GridLength.Star)
            }
        };

        quoteGrid.Add(new BoxView
        {
            Color = Color.FromArgb("#5B8FD4"),
            WidthRequest = 4,
            VerticalOptions = LayoutOptions.Fill
        }, 0);
        quoteGrid.Add(new Label
        {
            Text = text,
            FontSize = 14,
            FontAttributes = FontAttributes.Italic,
            TextColor = darkMode ? Color.FromArgb("#ADB5BD") : Color.FromArgb("#6C757D"),
            Margin = new Thickness(12, 0, 0, 0)
        }, 1);

        return new Border
        {
            BackgroundColor = darkMode ? Color.FromArgb("#1A1D21") : Color.FromArgb("#F8F9FA"),
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 4 },
            Stroke = Colors.Transparent,
            StrokeThickness = 0,
            Padding = new Thickness(16, 12),
            Content = quoteGrid
        };
    }

    private static FormattedString ParseInlineFormatting(string text, bool darkMode = true)
    {
        var formatted = new FormattedString();
        var i = 0;

        while (i < text.Length)
        {
            // Bold: **text**
            if (i + 1 < text.Length && text[i] == '*' && text[i + 1] == '*')
            {
                var end = text.IndexOf("**", i + 2, StringComparison.Ordinal);
                if (end > 0)
                {
                    formatted.Spans.Add(new Span
                    {
                        Text = text[(i + 2)..end],
                        FontAttributes = FontAttributes.Bold
                    });
                    i = end + 2;
                    continue;
                }
            }

            // Inline code: `text`
            if (text[i] == '`')
            {
                var end = text.IndexOf('`', i + 1);
                if (end > 0)
                {
                    formatted.Spans.Add(new Span
                    {
                        Text = text[(i + 1)..end],
                        FontFamily = "CascadiaMono",
                        BackgroundColor = darkMode ? Color.FromArgb("#231E30") : Color.FromArgb("#E9ECEF"),
                        TextColor = darkMode ? Color.FromArgb("#E6EDF3") : Color.FromArgb("#24292F"),
                        FontSize = 13
                    });
                    i = end + 1;
                    continue;
                }
            }

            // Italic: *text*
            if (text[i] == '*' && (i + 1 >= text.Length || text[i + 1] != '*'))
            {
                var end = text.IndexOf('*', i + 1);
                if (end > 0)
                {
                    formatted.Spans.Add(new Span
                    {
                        Text = text[(i + 1)..end],
                        FontAttributes = FontAttributes.Italic
                    });
                    i = end + 1;
                    continue;
                }
            }

            // Regular text - collect until next special char
            var nextSpecial = text.Length;
            for (var j = i + 1; j < text.Length; j++)
            {
                if (text[j] == '*' || text[j] == '`')
                {
                    nextSpecial = j;
                    break;
                }
            }

            formatted.Spans.Add(new Span { Text = text[i..nextSpecial] });
            i = nextSpecial;
        }

        return formatted;
    }
}
