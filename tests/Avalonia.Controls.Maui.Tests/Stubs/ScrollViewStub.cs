using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Primitives;

namespace Avalonia.Controls.Maui.Tests.Stubs;

public class ScrollViewStub : StubBase, IScrollView, ICrossPlatformLayout
{
    public IView? Content { get; set; }

    public IView? PresentedContent => Content;

    public ScrollBarVisibility HorizontalScrollBarVisibility { get; set; } = ScrollBarVisibility.Default;

    public ScrollBarVisibility VerticalScrollBarVisibility { get; set; } = ScrollBarVisibility.Default;

    public ScrollOrientation Orientation { get; set; } = ScrollOrientation.Vertical;

    public Microsoft.Maui.Thickness Padding { get; set; }

    public double HorizontalOffset { get; set; }

    public double VerticalOffset { get; set; }

    public Microsoft.Maui.Graphics.Size ContentSize { get; set; }

    private double _scrollX;
    private double _scrollY;

    public double ScrollX
    {
        get => _scrollX;
        set => SetProperty(ref _scrollX, value);
    }

    public double ScrollY
    {
        get => _scrollY;
        set => SetProperty(ref _scrollY, value);
    }

    public ScrollToRequest? LastScrollToRequest { get; private set; }

    public bool ScrollFinishedInvoked { get; private set; }

    object? IContentView.Content => Content;

    public void RequestScrollTo(ScrollToRequest request)
    {
        LastScrollToRequest = request;
        Handler?.Invoke(nameof(IScrollView.RequestScrollTo), request);
    }

    public void RequestScrollTo(double horizontalOffset, double verticalOffset, bool instant)
    {
        RequestScrollTo(new ScrollToRequest(horizontalOffset, verticalOffset, instant));
    }

    public void ScrollFinished()
    {
        ScrollFinishedInvoked = true;
    }

    public Microsoft.Maui.Graphics.Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
    {
        return new Microsoft.Maui.Graphics.Size(widthConstraint, heightConstraint);
    }

    public Microsoft.Maui.Graphics.Size CrossPlatformArrange(Microsoft.Maui.Graphics.Rect bounds)
    {
        return bounds.Size;
    }
}
