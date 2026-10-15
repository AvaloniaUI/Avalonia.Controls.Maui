namespace ControlGallery.Pages;

public partial class LabelPage : ContentPage
{
    public LabelPage()
    {
        InitializeComponent();

        HtmlLabel.Text =
            "<b>Bold</b>, <i>Italic</i>, <u>Underline</u> formatting." +
            "<br/><br/>" +
            "<font color=\"red\">Red text</font>, " +
            "<font color=\"#0000FF\">Blue hex</font>, " +
            "<font size=\"5\">Large</font> and <font size=\"1\">small</font> text." +
            "<p>A paragraph with <b>bold</b> and <i>italic</i> content.</p>" +
            "<p><a href=\"https://learn.microsoft.com/dotnet/maui/\">Learn more about .NET MAUI</a></p>";
    }
}
