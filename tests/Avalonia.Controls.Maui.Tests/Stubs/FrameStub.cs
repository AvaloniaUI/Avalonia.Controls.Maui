using Microsoft.Maui.Controls;
using MauiGraphics = Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.Tests.Stubs;

public class FrameStub : Frame
{
    public FrameStub()
    {
        WidthRequest = 50;
        HeightRequest = 50;
    }

    protected override MauiGraphics.Size MeasureOverride(double widthConstraint, double heightConstraint)
    {
        var width = double.IsNaN(widthConstraint) || double.IsInfinity(widthConstraint)
            ? WidthRequest
            : widthConstraint;

        var height = double.IsNaN(heightConstraint) || double.IsInfinity(heightConstraint)
            ? HeightRequest
            : heightConstraint;

        return new MauiGraphics.Size(width, height);
    }

    protected override MauiGraphics.Size ArrangeOverride(MauiGraphics.Rect bounds)
    {
        return new MauiGraphics.Size(bounds.Width, bounds.Height);
    }
}
