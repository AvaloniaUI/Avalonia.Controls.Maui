using Microsoft.Maui.Controls.Shapes;

namespace ControlGallery.Pages;

public partial class ClipPage : ContentPage
{
    public ClipPage()
    {
        InitializeComponent();
    }

    private void OnToggleClipClicked(object? sender, EventArgs e)
    {
        var add = BorderHostedContent.Clip == null;
        foreach (var content in new View[] { BorderHostedContent, ScrollHostedContent })
            content.Clip = add ? new RoundRectangleGeometry(new CornerRadius(50), new Rect(0, 0, 300, 100)) : null;

        ToggleClipButton.Text = add ? "Remove Clip" : "Add Clip";
    }

    private void OnToggleShadowClicked(object? sender, EventArgs e)
    {
        var add = BorderHostedContent.Shadow == null;
        foreach (var content in new View[] { BorderHostedContent, ScrollHostedContent })
            content.Shadow = add ? new Shadow { Brush = Brush.Black, Offset = new Point(4, 4), Radius = 8, Opacity = 0.6f } : null!;

        ToggleShadowButton.Text = add ? "Remove Shadow" : "Add Shadow";
    }
}
