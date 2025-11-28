using Microsoft.Maui;
using MauiGraphics = Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.Tests.Stubs;

public class PageStub : StubBase, IContentView, IPadding
{
    object? IContentView.Content => Content;

    public IView? Content { get; set; }

    public IView? PresentedContent => Content;

    public Microsoft.Maui.Thickness Padding { get; set; }

    public MauiGraphics.Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
    {
        // Return a simple size to avoid infinite recursion
        return new MauiGraphics.Size(
            double.IsNaN(widthConstraint) || double.IsInfinity(widthConstraint) ? 100 : widthConstraint,
            double.IsNaN(heightConstraint) || double.IsInfinity(heightConstraint) ? 100 : heightConstraint);
    }

    public MauiGraphics.Size CrossPlatformArrange(MauiGraphics.Rect bounds)
    {
        return new MauiGraphics.Size(bounds.Width, bounds.Height);
    }
}
