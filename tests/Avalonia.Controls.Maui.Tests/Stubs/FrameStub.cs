using Microsoft.Maui;
using MauiGraphics = Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.Tests.Stubs;

public class FrameStub : StubBase, IContentView, IPadding
{
    public MauiGraphics.Color? BorderColor { get; set; }

    public float CornerRadius { get; set; } = -1f;

    public bool HasShadow { get; set; } = true;

    object? IContentView.Content => Content;

    public IView? Content { get; set; }

    public IView? PresentedContent => Content;

    public Microsoft.Maui.Thickness Padding { get; set; }

    public MauiGraphics.Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
    {
        return Measure(widthConstraint, heightConstraint);
    }

    public MauiGraphics.Size CrossPlatformArrange(MauiGraphics.Rect bounds)
    {
        return Arrange(bounds);
    }
}