namespace ControlGallery.Pages;

public partial class LabelPage : ContentPage
{
    public LabelPage()
    {
        InitializeComponent();

        HtmlLabel.Text =
            "<b>Bold</b>, <i>Italic</i>, <u>Underline</u>, <s>Strikethrough</s>, " +
            "and <code>inline code</code> formatting." +
            "<h1>Heading 1</h1>" +
            "<h2>Heading 2</h2>" +
            "<h3>Heading 3</h3>" +
            "<p>Regular paragraph text below the headings.</p>" +
            "<b>Unordered list:</b>" +
            "<ul>" +
            "<li>First item</li>" +
            "<li>Second item</li>" +
            "<li>Third item</li>" +
            "</ul>" +
            "<b>Ordered list:</b>" +
            "<ol>" +
            "<li>Step one</li>" +
            "<li>Step two</li>" +
            "<li>Step three</li>" +
            "</ol>" +
            "<b>Nested list:</b>" +
            "<ul>" +
            "<li>Parent item" +
            "<ul><li>Child A</li><li>Child B</li></ul>" +
            "</li>" +
            "<li>Another parent</li>" +
            "</ul>" +
            "<font color=\"red\">Red text</font>, " +
            "<font color=\"#0000FF\">Blue hex</font>, " +
            "<span style=\"color: green; font-weight: bold;\">Green bold via style</span>, " +
            "<span style=\"background-color: yellow;\">Highlighted text</span>, " +
            "<font size=\"5\">Large</font> and <font size=\"1\">small</font> text." +
            "<h2>Recipe: Pancakes</h2>" +
            "<p><i>A simple recipe for fluffy pancakes.</i></p>" +
            "<b>Ingredients:</b>" +
            "<ul>" +
            "<li>1 cup flour</li>" +
            "<li>2 tbsp <mark>sugar</mark></li>" +
            "<li>1 egg</li>" +
            "<li>3/4 cup milk</li>" +
            "</ul>" +
            "<b>Instructions:</b>" +
            "<ol>" +
            "<li>Mix <b>dry ingredients</b> together</li>" +
            "<li>Add <i>wet ingredients</i> and stir</li>" +
            "<li>Cook on a <span style=\"color: orangered;\">hot griddle</span></li>" +
            "</ol>" +
            "<hr/>" +
            "<p><a href=\"https://learn.microsoft.com/dotnet/maui/\">Learn more about .NET MAUI</a></p>";
    }
}
