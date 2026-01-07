using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.Tests.Stubs;

#pragma warning disable CS0618 // Type or member is obsolete
public class FrameStub : Frame
{
    public FrameStub()
    {
        BorderColor = Colors.Transparent;
        CornerRadius = -1;
        HasShadow = true;
        Padding = new Microsoft.Maui.Thickness(20);
        BackgroundColor = Colors.White;
    }

    protected override Microsoft.Maui.Graphics.Size MeasureOverride(double widthConstraint, double heightConstraint)
    {
        return new Microsoft.Maui.Graphics.Size(0, 0);
    }
}
#pragma warning restore CS0618 // Type or member is obsolete
