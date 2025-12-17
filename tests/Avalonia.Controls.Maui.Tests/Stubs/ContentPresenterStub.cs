using Avalonia.Controls.Maui.Tests.Stubs;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.Tests.Stubs;

public class ContentPresenterStub : StubBase, IContentView
{
    public object? Content { get; set; }
    public IView? PresentedContent { get; set; }
    public Microsoft.Maui.Thickness Padding { get; set; }
    
    public Microsoft.Maui.Graphics.Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
    {
        if (PresentedContent != null)
        {
            return PresentedContent.Measure(widthConstraint, heightConstraint);
        }
        return Microsoft.Maui.Graphics.Size.Zero;
    }

    public Microsoft.Maui.Graphics.Size CrossPlatformArrange(Microsoft.Maui.Graphics.Rect bounds)
    {
        Frame = bounds;
        if (PresentedContent != null)
        {
            PresentedContent.Arrange(bounds);
        }
        return bounds.Size;
    }
}
