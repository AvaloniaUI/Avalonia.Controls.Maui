using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using MauiSwipeDirection = Microsoft.Maui.SwipeDirection;

namespace Avalonia.Controls.Maui.Tests.Stubs;

public class SwipeViewStub : StubBase, ISwipeView
{
    public double Threshold { get; set; }
    public ISwipeItems LeftItems { get; set; }
    public ISwipeItems RightItems { get; set; }
    public ISwipeItems TopItems { get; set; }
    public ISwipeItems BottomItems { get; set; }
    
    public Microsoft.Maui.Graphics.Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
    {
        return new Microsoft.Maui.Graphics.Size(widthConstraint, heightConstraint);
    }
    
    public object? Content => PresentedContent;

    public Microsoft.Maui.Graphics.Size CrossPlatformArrange(Microsoft.Maui.Graphics.Rect bounds)
    {
        return bounds.Size;
    }

    public IView? PresentedContent { get; set; }
    
    // Tracking properties for test verification
    public bool SwipeStartedFired { get; private set; }
    public MauiSwipeDirection LastSwipeStartedDirection { get; private set; }
    
    public bool SwipeChangingFired { get; private set; }
    public MauiSwipeDirection LastSwipeChangingDirection { get; private set; }
    public double LastSwipeChangingOffset { get; private set; }
    
    public bool SwipeEndedFired { get; private set; }
    public MauiSwipeDirection LastSwipeEndedDirection { get; private set; }
    public bool LastSwipeEndedIsOpen { get; private set; }

    public bool IsOpen { get; set; }

    public SwipeViewStub()
    {
        LeftItems = new SwipeItemsStub();
        RightItems = new SwipeItemsStub();
        TopItems = new SwipeItemsStub();
        BottomItems = new SwipeItemsStub();
    }

    public void RequestOpen(SwipeViewOpenRequest swipeViewOpenRequest)
    {
        Handler?.Invoke(nameof(ISwipeView.RequestOpen), swipeViewOpenRequest);
        IsOpen = true; 
    }

    public void RequestClose(SwipeViewCloseRequest swipeViewCloseRequest)
    {
        Handler?.Invoke(nameof(ISwipeView.RequestClose), swipeViewCloseRequest);
        IsOpen = false; 
    }

    public void SwipeStarted(SwipeViewSwipeStarted swipeStarted)
    {
        SwipeStartedFired = true;
        LastSwipeStartedDirection = swipeStarted.SwipeDirection;
    }

    public void SwipeChanging(SwipeViewSwipeChanging swipeChanging)
    {
        SwipeChangingFired = true;
        LastSwipeChangingDirection = swipeChanging.SwipeDirection;
        LastSwipeChangingOffset = swipeChanging.Offset;
    }

    public void SwipeEnded(SwipeViewSwipeEnded swipeEnded)
    {
        SwipeEndedFired = true;
        LastSwipeEndedDirection = swipeEnded.SwipeDirection;
        LastSwipeEndedIsOpen = swipeEnded.IsOpen;
    }

    public Microsoft.Maui.Thickness Padding { get; set; }
}

public class SwipeItemsStub : List<ISwipeItem>, ISwipeItems
{
    public Microsoft.Maui.SwipeMode Mode { get; set; }
    public SwipeBehaviorOnInvoked SwipeBehaviorOnInvoked { get; set; }
}

public class SwipeItemStub : StubBase, ISwipeItem
{
    public string Text { get; set; } = string.Empty;
    public object CommandParameter { get; set; } = new object();
    public System.Windows.Input.ICommand Command { get; set; } = null!;
    public bool IsVisible { get; set; } = true;
    public int InvokedCount { get; private set; }

    public void OnInvoked()
    {
        InvokedCount++;
    }
}
