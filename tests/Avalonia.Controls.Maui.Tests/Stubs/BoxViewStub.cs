using Microsoft.Maui;
using Microsoft.Maui.Controls;
using MauiGraphics = Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.Tests.Stubs;

public class BoxViewStub : BoxView
{
    protected override MauiGraphics.Size MeasureOverride(double widthConstraint, double heightConstraint)
    {
        var width = double.IsNaN(widthConstraint) || double.IsInfinity(widthConstraint) 
            ? 100.0  // Default width for BoxView
            : widthConstraint;
            
        var height = double.IsNaN(heightConstraint) || double.IsInfinity(heightConstraint) 
            ? 100.0  // Default height for BoxView
            : heightConstraint;

        // Return a reasonable size for testing
        var measuredWidth = Math.Min(width, WidthRequest >= 0 ? WidthRequest : width);
        var measuredHeight = Math.Min(height, HeightRequest >= 0 ? HeightRequest : height);
        
        return new MauiGraphics.Size(measuredWidth, measuredHeight);
    }

    protected override MauiGraphics.Size ArrangeOverride(MauiGraphics.Rect bounds)
    {
        return new MauiGraphics.Size(bounds.Width, bounds.Height);
    }
}