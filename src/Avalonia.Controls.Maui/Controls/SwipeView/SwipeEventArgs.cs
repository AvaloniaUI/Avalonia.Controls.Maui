using Avalonia.Interactivity;

namespace Avalonia.Controls.Maui;

/// <summary>
/// Provides data for the OpenRequested event
/// </summary>
public class OpenRequestedEventArgs : RoutedEventArgs
{
    /// <summary>
    /// Gets the direction in which the swipe is being opened
    /// </summary>
    public OpenSwipeItem OpenSwipeItem { get; }

    /// <summary>
    /// Gets or sets whether the open request is cancelled.
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Initializes a new instance of <see cref="OpenRequestedEventArgs"/> with the specified swipe direction.
    /// </summary>
    /// <param name="openSwipeItem">The direction in which the swipe is being opened.</param>
    public OpenRequestedEventArgs(OpenSwipeItem openSwipeItem)
    {
        OpenSwipeItem = openSwipeItem;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="OpenRequestedEventArgs"/> with the specified routed event and swipe direction.
    /// </summary>
    /// <param name="routedEvent">The routed event associated with this event args.</param>
    /// <param name="openSwipeItem">The direction in which the swipe is being opened.</param>
    public OpenRequestedEventArgs(RoutedEvent? routedEvent, OpenSwipeItem openSwipeItem) : base(routedEvent)
    {
        OpenSwipeItem = openSwipeItem;
    }
}

/// <summary>
/// Provides data for the CloseRequested event
/// </summary>
public class CloseRequestedEventArgs : RoutedEventArgs
{
    /// <summary>
    /// Gets or sets whether the close request is cancelled.
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Initializes a new instance of <see cref="CloseRequestedEventArgs"/>.
    /// </summary>
    public CloseRequestedEventArgs()
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CloseRequestedEventArgs"/> with the specified routed event.
    /// </summary>
    /// <param name="routedEvent">The routed event associated with this event args.</param>
    public CloseRequestedEventArgs(RoutedEvent? routedEvent) : base(routedEvent)
    {
    }
}

/// <summary>
/// Provides data for the SwipeStarted event
/// </summary>
public class SwipeStartedEventArgs : RoutedEventArgs
{
    /// <summary>
    /// Gets the direction of the swipe gesture
    /// </summary>
    public SwipeDirection SwipeDirection { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="SwipeStartedEventArgs"/> with the specified swipe direction.
    /// </summary>
    /// <param name="swipeDirection">The direction of the swipe gesture.</param>
    public SwipeStartedEventArgs(SwipeDirection swipeDirection)
    {
        SwipeDirection = swipeDirection;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SwipeStartedEventArgs"/> with the specified routed event and swipe direction.
    /// </summary>
    /// <param name="routedEvent">The routed event associated with this event args.</param>
    /// <param name="swipeDirection">The direction of the swipe gesture.</param>
    public SwipeStartedEventArgs(RoutedEvent? routedEvent, SwipeDirection swipeDirection) : base(routedEvent)
    {
        SwipeDirection = swipeDirection;
    }
}

/// <summary>
/// Provides data for the SwipeChanging event.
/// </summary>
public class SwipeChangingEventArgs : EventArgs
{
    /// <summary>
    /// Gets the direction of the swipe gesture.
    /// </summary>
    public SwipeDirection SwipeDirection { get; set; }

    /// <summary>
    /// Gets the current offset of the swipe in pixels.
    /// </summary>
    public double Offset { get; set; }

    /// <summary>
    /// Initializes a new instance of <see cref="SwipeChangingEventArgs"/> with the specified direction and offset.
    /// </summary>
    /// <param name="swipeDirection">The direction of the swipe gesture.</param>
    /// <param name="offset">The current offset of the swipe in pixels.</param>
    public SwipeChangingEventArgs(SwipeDirection swipeDirection, double offset)
    {
        SwipeDirection = swipeDirection;
        Offset = offset;
    }
}

/// <summary>
/// Provides data for the SwipeEnded event
/// </summary>
public class SwipeEndedEventArgs : RoutedEventArgs
{
    /// <summary>
    /// Gets the direction of the swipe gesture
    /// </summary>
    public SwipeDirection SwipeDirection { get; }

    /// <summary>
    /// Gets whether the swipe items remain visible after the gesture completes
    /// </summary>
    public bool IsOpen { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="SwipeEndedEventArgs"/> with the specified direction and open state.
    /// </summary>
    /// <param name="swipeDirection">The direction of the swipe gesture.</param>
    /// <param name="isOpen">Whether the swipe items remain visible after the gesture completes.</param>
    public SwipeEndedEventArgs(SwipeDirection swipeDirection, bool isOpen)
    {
        SwipeDirection = swipeDirection;
        IsOpen = isOpen;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="SwipeEndedEventArgs"/> with the specified routed event, direction, and open state.
    /// </summary>
    /// <param name="routedEvent">The routed event associated with this event args.</param>
    /// <param name="swipeDirection">The direction of the swipe gesture.</param>
    /// <param name="isOpen">Whether the swipe items remain visible after the gesture completes.</param>
    public SwipeEndedEventArgs(RoutedEvent? routedEvent, SwipeDirection swipeDirection, bool isOpen) : base(routedEvent)
    {
        SwipeDirection = swipeDirection;
        IsOpen = isOpen;
    }
}
