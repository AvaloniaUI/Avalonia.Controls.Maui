using Microsoft.Maui;
using MauiGraphics = Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.Tests.Stubs;

public class RefreshViewStub : StubBase, IRefreshView, IContentView
{
    private bool _isRefreshing;
    private MauiGraphics.Paint? _refreshColor;
    private IView? _content;
    
    public int RefreshingCount { get; private set; }
    
    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            if (value && !_isRefreshing)
            {
                RefreshingCount++;
            }
            SetProperty(ref _isRefreshing, value);
        }
    }
    
    public MauiGraphics.Paint? RefreshColor
    {
        get => _refreshColor;
        set => SetProperty(ref _refreshColor, value);
    }
    
    public IView? Content
    {
        get => _content;
        set => SetProperty(ref _content, value);
    }
    
    IView IRefreshView.Content => _content!;
    
    object? IContentView.Content => Content;
    
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