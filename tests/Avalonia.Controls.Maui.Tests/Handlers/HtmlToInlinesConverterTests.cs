using System.Globalization;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Maui.Platform;
using Avalonia.Headless.XUnit;
using Avalonia.Media;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public class HtmlToInlinesConverterTests
{
    [AvaloniaFact(DisplayName = "Plain text returns single Run")]
    public void PlainText_Returns_Single_Run()
    {
        var inlines = HtmlToInlinesConverter.Convert("Hello World");

        Assert.Single(inlines);
        var run = Assert.IsType<Run>(inlines[0]);
        Assert.Equal("Hello World", run.Text);
    }

    [AvaloniaFact(DisplayName = "Bold tag creates bold Run")]
    public void Bold_Tag_Creates_Bold_Run()
    {
        var inlines = HtmlToInlinesConverter.Convert("<b>Bold</b>");

        Assert.Single(inlines);
        var run = Assert.IsType<Run>(inlines[0]);
        Assert.Equal("Bold", run.Text);
        Assert.Equal(FontWeight.Bold, run.FontWeight);
    }

    [AvaloniaFact(DisplayName = "Strong tag creates bold Run")]
    public void Strong_Tag_Creates_Bold_Run()
    {
        var inlines = HtmlToInlinesConverter.Convert("<strong>Bold</strong>");

        Assert.Single(inlines);
        var run = Assert.IsType<Run>(inlines[0]);
        Assert.Equal(FontWeight.Bold, run.FontWeight);
    }

    [AvaloniaFact(DisplayName = "Italic tag creates italic Run")]
    public void Italic_Tag_Creates_Italic_Run()
    {
        var inlines = HtmlToInlinesConverter.Convert("<i>Italic</i>");

        Assert.Single(inlines);
        var run = Assert.IsType<Run>(inlines[0]);
        Assert.Equal(FontStyle.Italic, run.FontStyle);
    }

    [AvaloniaFact(DisplayName = "Underline tag creates underlined Run")]
    public void Underline_Tag_Creates_Underlined_Run()
    {
        var inlines = HtmlToInlinesConverter.Convert("<u>Underline</u>");

        Assert.Single(inlines);
        var run = Assert.IsType<Run>(inlines[0]);
        Assert.NotNull(run.TextDecorations);
        Assert.Contains(run.TextDecorations, d => d.Location == TextDecorationLocation.Underline);
    }

    [AvaloniaFact(DisplayName = "Br tag inserts LineBreak")]
    public void Br_Tag_Inserts_LineBreak()
    {
        var inlines = HtmlToInlinesConverter.Convert("Line1<br/>Line2");

        Assert.Equal(3, inlines.Count);
        Assert.IsType<Run>(inlines[0]);
        Assert.IsType<LineBreak>(inlines[1]);
        Assert.IsType<Run>(inlines[2]);
    }

    [AvaloniaFact(DisplayName = "Nested bold and italic")]
    public void Nested_Bold_And_Italic()
    {
        var inlines = HtmlToInlinesConverter.Convert("<b><i>BoldItalic</i></b>");

        Assert.Single(inlines);
        var run = Assert.IsType<Run>(inlines[0]);
        Assert.Equal(FontWeight.Bold, run.FontWeight);
        Assert.Equal(FontStyle.Italic, run.FontStyle);
    }

    [AvaloniaFact(DisplayName = "Font color attribute sets foreground")]
    public void Font_Color_Sets_Foreground()
    {
        var inlines = HtmlToInlinesConverter.Convert("<font color=\"red\">Red</font>");

        var run = Assert.IsType<Run>(inlines[0]);
        Assert.NotNull(run.Foreground);
        var brush = Assert.IsType<SolidColorBrush>(run.Foreground);
        Assert.Equal(Colors.Red, brush.Color);
    }

    [AvaloniaFact(DisplayName = "Font size attribute sets font size")]
    public void Font_Size_Sets_FontSize()
    {
        var inlines = HtmlToInlinesConverter.Convert("<font size=\"5\">Large</font>");

        var run = Assert.IsType<Run>(inlines[0]);
        Assert.Equal(24, run.FontSize);
    }

    [AvaloniaFact(DisplayName = "HTML entities are decoded")]
    public void Html_Entities_Are_Decoded()
    {
        var inlines = HtmlToInlinesConverter.Convert("&amp; &lt; &gt; &quot;");

        var run = Assert.IsType<Run>(inlines[0]);
        Assert.Equal("& < > \"", run.Text);
    }

    [AvaloniaFact(DisplayName = "Empty input returns no inlines")]
    public void Empty_Input_Returns_No_Inlines()
    {
        var inlines = HtmlToInlinesConverter.Convert("");
        Assert.Empty(inlines);
    }

    [AvaloniaFact(DisplayName = "Mixed text and tags")]
    public void Mixed_Text_And_Tags()
    {
        var inlines = HtmlToInlinesConverter.Convert("Hello <b>World</b>!");

        Assert.Equal(3, inlines.Count);
        var hello = Assert.IsType<Run>(inlines[0]);
        Assert.Equal("Hello ", hello.Text);

        var world = Assert.IsType<Run>(inlines[1]);
        Assert.Equal("World", world.Text);
        Assert.Equal(FontWeight.Bold, world.FontWeight);

        var excl = Assert.IsType<Run>(inlines[2]);
        Assert.Equal("!", excl.Text);
    }

    [AvaloniaFact(DisplayName = "Paragraph tag adds line breaks")]
    public void Paragraph_Tag_Adds_Line_Breaks()
    {
        var inlines = HtmlToInlinesConverter.Convert("<p>First</p><p>Second</p>");

        Assert.Equal(4, inlines.Count);
        Assert.Equal("First", Assert.IsType<Run>(inlines[0]).Text);
        Assert.IsType<LineBreak>(inlines[1]);
        Assert.IsType<LineBreak>(inlines[2]);
        Assert.Equal("Second", Assert.IsType<Run>(inlines[3]).Text);
    }

    [AvaloniaFact(DisplayName = "Anchor tag renders styled text like MAUI native")]
    public void Anchor_Tag_Renders_Styled_Text()
    {
        var inlines = HtmlToInlinesConverter.Convert("<a href=\"https://example.com\">Link <b>here</b></a> after");

        Assert.Equal(3, inlines.Count);
        var link = Assert.IsType<Run>(inlines[0]);
        Assert.Equal("Link ", link.Text);
        Assert.Contains(link.TextDecorations!, d => d.Location == TextDecorationLocation.Underline);
        Assert.IsType<SolidColorBrush>(link.Foreground);

        var bold = Assert.IsType<Run>(inlines[1]);
        Assert.Equal(FontWeight.Bold, bold.FontWeight);
        Assert.NotNull(bold.TextDecorations);

        var after = Assert.IsType<Run>(inlines[2]);
        Assert.Null(after.TextDecorations);
        Assert.False(after.IsSet(TextElement.ForegroundProperty));
    }

    [AvaloniaFact(DisplayName = "Anchor without href renders plain text")]
    public void Anchor_Without_Href_Renders_Plain_Text()
    {
        var inlines = HtmlToInlinesConverter.Convert("<a name=\"top\">Anchor</a>");

        var run = Assert.IsType<Run>(Assert.Single(inlines));
        Assert.Null(run.TextDecorations);
        Assert.False(run.IsSet(TextElement.ForegroundProperty));
    }

    [AvaloniaFact(DisplayName = "Mis-nested closing tag unwinds to the matching open tag")]
    public void Misnested_Closing_Tag_Unwinds_To_Matching_Tag()
    {
        var inlines = HtmlToInlinesConverter.Convert("<b>a<i>b</b>c");

        Assert.Equal(3, inlines.Count);
        var c = Assert.IsType<Run>(inlines[2]);
        Assert.Equal("c", c.Text);
        Assert.NotEqual(FontWeight.Bold, c.FontWeight);
        Assert.NotEqual(FontStyle.Italic, c.FontStyle);
    }

    [AvaloniaFact(DisplayName = "Unmatched closing tag is ignored")]
    public void Unmatched_Closing_Tag_Is_Ignored()
    {
        var inlines = HtmlToInlinesConverter.Convert("<b>a</i>b</b>");

        Assert.Equal(2, inlines.Count);
        Assert.All(inlines.Cast<Run>(), r => Assert.Equal(FontWeight.Bold, r.FontWeight));
    }

    [AvaloniaTheory(DisplayName = "Font size supports absolute and relative values")]
    [InlineData("1", 10)]
    [InlineData("7", 48)]
    [InlineData("9", 48)]
    [InlineData("+1", 18)]
    [InlineData("-2", 10)]
    public void Font_Size_Supports_Relative_Values(string size, double expected)
    {
        var inlines = HtmlToInlinesConverter.Convert($"<font size=\"{size}\">Text</font>");

        Assert.Equal(expected, Assert.IsType<Run>(inlines[0]).FontSize);
    }

    [AvaloniaFact(DisplayName = "Font attributes accept unquoted values and face")]
    public void Font_Attributes_Accept_Unquoted_Values_And_Face()
    {
        var inlines = HtmlToInlinesConverter.Convert("<font color=red face='Courier New'>Text</font>");

        var run = Assert.IsType<Run>(inlines[0]);
        Assert.Equal(Colors.Red, Assert.IsType<SolidColorBrush>(run.Foreground).Color);
        Assert.Equal("Courier New", run.FontFamily.Name);
    }

    [AvaloniaFact(DisplayName = "Attribute names match whole words only")]
    public void Attribute_Names_Match_Whole_Words_Only()
    {
        var inlines = HtmlToInlinesConverter.Convert("<font bgcolor=\"red\">Text</font>");

        Assert.False(Assert.IsType<Run>(inlines[0]).IsSet(TextElement.ForegroundProperty));
    }

    [AvaloniaFact(DisplayName = "Whitespace collapses across tags and line starts")]
    public void Whitespace_Collapses_Across_Tags()
    {
        var inlines = HtmlToInlinesConverter.Convert("\n    Hello   <b> World </b> !<br/>   Next  ");

        var texts = inlines.Select(i => i is Run r ? r.Text : "\n");
        Assert.Equal(new[] { "Hello ", "World ", "!", "\n", "Next" }, texts);
    }

    [AvaloniaFact(DisplayName = "Paragraph after line break adds a single blank line")]
    public void Paragraph_After_Line_Break_Adds_Single_Blank_Line()
    {
        var inlines = HtmlToInlinesConverter.Convert("Text<br><p>Next</p>");

        Assert.Equal(2, inlines.OfType<LineBreak>().Count());
    }

    [AvaloniaFact(DisplayName = "Comments are ignored")]
    public void Comments_Are_Ignored()
    {
        var inlines = HtmlToInlinesConverter.Convert("A<!-- <b>hidden</b> -->B");

        Assert.Equal("AB", string.Concat(inlines.OfType<Run>().Select(r => r.Text)));
        Assert.All(inlines.OfType<Run>(), r => Assert.NotEqual(FontWeight.Bold, r.FontWeight));
    }

    [AvaloniaFact(DisplayName = "TextTransform applies to text but not markup")]
    public void TextTransform_Applies_To_Text_Only()
    {
        var inlines = HtmlToInlinesConverter.Convert("<font color=\"red\">red &amp; blue</font>", Microsoft.Maui.TextTransform.Uppercase);

        var run = Assert.IsType<Run>(Assert.Single(inlines));
        Assert.Equal("RED & BLUE", run.Text);
        Assert.Equal(Colors.Red, Assert.IsType<SolidColorBrush>(run.Foreground).Color);
    }

    [AvaloniaFact(DisplayName = "RGBA alpha parses using invariant culture")]
    public void Rgba_Alpha_Parses_Using_Invariant_Culture()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("fr-FR");

            var inlines = HtmlToInlinesConverter.Convert("<font color=\"rgba(10, 20, 30, 0.5)\">Half</font>");

            var run = Assert.IsType<Run>(inlines[0]);
            var brush = Assert.IsType<SolidColorBrush>(run.Foreground);
            Assert.Equal(Color.FromArgb(128, 10, 20, 30), brush.Color);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }
}
