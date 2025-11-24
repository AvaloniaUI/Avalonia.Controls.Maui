# SwipeView Control

## Abstract
Swipe gestures reveal contextual actions behind content. This control allow left/right/top/bottom swipes with Reveal/Execute semantics, thresholds, programmatic open/close, and invoke behaviors. It is a temporary fork until Avalonia.Labs provides the official package.

Vision
* Support all four directions with Reveal and Execute modes.
* Support Threshold, SwipeItems per side, SwipeMode, SwipeBehaviorOnInvoked, Open/Close requests, SwipeStarted/Changing/Ended.

### Requirements

* **Execute and auto-close.** Execute mode invokes and closes; `SwipeBehaviorOnInvoked=Close` must collapse after invoke.
* **All directions.** Same rules for left/right/top/bottom.
* **Programmatic control.** Open/Close methods and cancellable OpenRequested/CloseRequested events.

## API

```csharp
public class Swipe : Grid
{
    public double Threshold { get; set; }
    public TimeSpan AnimationDuration { get; set; }

    public IDataTemplate? Left { get; set; }
    public IDataTemplate? Right { get; set; }
    public IDataTemplate? Top { get; set; }
    public IDataTemplate? Bottom { get; set; }

    public Control? Content { get; set; }
    public SwipeState SwipeState { get; set; }

    public SwipeMode LeftMode { get; set; }
    public SwipeMode RightMode { get; set; }
    public SwipeMode TopMode { get; set; }
    public SwipeMode BottomMode { get; set; }

    public event EventHandler<OpenRequestedEventArgs>? OpenRequested;
    public event EventHandler<CloseRequestedEventArgs>? CloseRequested;
    public event EventHandler<SwipeStartedEventArgs>? SwipeStarted;
    public event EventHandler<SwipeChangingEventArgs>? SwipeChanging;
    public event EventHandler<SwipeEndedEventArgs>? SwipeEnded;
    public event EventHandler<SwipeStartedEventArgs>? SwipeExecuted;

    public void Open(OpenSwipeItem openSwipeItem, bool animated = true);
    public void Close(bool animated = true);
}
```