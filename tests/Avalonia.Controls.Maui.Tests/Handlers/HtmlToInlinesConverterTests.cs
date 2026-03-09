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

    [AvaloniaFact(DisplayName = "Strikethrough tag creates strikethrough Run")]
    public void Strikethrough_Tag_Creates_Strikethrough_Run()
    {
        var inlines = HtmlToInlinesConverter.Convert("<s>Strike</s>");

        Assert.Single(inlines);
        var run = Assert.IsType<Run>(inlines[0]);
        Assert.NotNull(run.TextDecorations);
        Assert.Contains(run.TextDecorations, d => d.Location == TextDecorationLocation.Strikethrough);
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

    [AvaloniaFact(DisplayName = "Unordered list creates bullet items")]
    public void Unordered_List_Creates_Bullet_Items()
    {
        var inlines = HtmlToInlinesConverter.Convert("<ul><li>A</li><li>B</li></ul>");

        // Should contain bullet markers and text runs
        var texts = inlines.OfType<Run>().Select(r => r.Text).ToList();
        Assert.Contains(texts, t => t != null && t.Contains("\u2022")); // bullet char
        Assert.Contains(texts, t => t == "A");
        Assert.Contains(texts, t => t == "B");
    }

    [AvaloniaFact(DisplayName = "Ordered list creates numbered items")]
    public void Ordered_List_Creates_Numbered_Items()
    {
        var inlines = HtmlToInlinesConverter.Convert("<ol><li>First</li><li>Second</li></ol>");

        var texts = inlines.OfType<Run>().Select(r => r.Text).ToList();
        Assert.Contains(texts, t => t != null && t.Contains("1."));
        Assert.Contains(texts, t => t != null && t.Contains("2."));
        Assert.Contains(texts, t => t == "First");
        Assert.Contains(texts, t => t == "Second");
    }

    [AvaloniaFact(DisplayName = "H1 tag creates large bold text")]
    public void H1_Tag_Creates_Large_Bold_Text()
    {
        var inlines = HtmlToInlinesConverter.Convert("<h1>Title</h1>");

        var run = inlines.OfType<Run>().First();
        Assert.Equal("Title", run.Text);
        Assert.Equal(FontWeight.Bold, run.FontWeight);
        Assert.Equal(24, run.FontSize);
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

    [AvaloniaFact(DisplayName = "Span style color sets foreground")]
    public void Span_Style_Color_Sets_Foreground()
    {
        var inlines = HtmlToInlinesConverter.Convert("<span style=\"color: blue;\">Blue</span>");

        var run = Assert.IsType<Run>(inlines[0]);
        Assert.NotNull(run.Foreground);
        var brush = Assert.IsType<SolidColorBrush>(run.Foreground);
        Assert.Equal(Colors.Blue, brush.Color);
    }

    [AvaloniaFact(DisplayName = "HTML entities are decoded")]
    public void Html_Entities_Are_Decoded()
    {
        var inlines = HtmlToInlinesConverter.Convert("&amp; &lt; &gt; &quot;");

        var run = Assert.IsType<Run>(inlines[0]);
        Assert.Equal("& < > \"", run.Text);
    }

    [AvaloniaFact(DisplayName = "Mark tag applies yellow background")]
    public void Mark_Tag_Applies_Yellow_Background()
    {
        var inlines = HtmlToInlinesConverter.Convert("<mark>Highlighted</mark>");

        var run = Assert.IsType<Run>(inlines[0]);
        Assert.NotNull(run.Background);
        var brush = Assert.IsType<SolidColorBrush>(run.Background);
        Assert.Equal(Colors.Yellow, brush.Color);
    }

    [AvaloniaFact(DisplayName = "Code tag uses monospace font")]
    public void Code_Tag_Uses_Monospace_Font()
    {
        var inlines = HtmlToInlinesConverter.Convert("<code>monospace</code>");

        var run = Assert.IsType<Run>(inlines[0]);
        Assert.NotNull(run.FontFamily);
        Assert.Contains("Cascadia Code", run.FontFamily.Name, StringComparison.OrdinalIgnoreCase);
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

    [AvaloniaFact(DisplayName = "Ordered list with start attribute")]
    public void Ordered_List_With_Start_Attribute()
    {
        var inlines = HtmlToInlinesConverter.Convert("<ol start=\"5\"><li>Five</li><li>Six</li></ol>");

        var texts = inlines.OfType<Run>().Select(r => r.Text).ToList();
        Assert.Contains(texts, t => t != null && t.Contains("5."));
        Assert.Contains(texts, t => t != null && t.Contains("6."));
    }

    [AvaloniaFact(DisplayName = "Paragraph tag adds line breaks")]
    public void Paragraph_Tag_Adds_Line_Breaks()
    {
        var inlines = HtmlToInlinesConverter.Convert("<p>First</p><p>Second</p>");

        var lineBreaks = inlines.OfType<LineBreak>().Count();
        Assert.True(lineBreaks >= 2);
    }
}
