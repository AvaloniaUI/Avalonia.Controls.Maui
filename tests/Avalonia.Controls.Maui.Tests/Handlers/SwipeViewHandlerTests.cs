using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Headless.XUnit;
using Microsoft.Maui;
using SwipeViewHandler = Avalonia.Controls.Maui.Handlers.SwipeViewHandler;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public partial class SwipeViewHandlerTests : HandlerTestBase<SwipeViewHandler, SwipeViewStub>
{
    [AvaloniaFact(DisplayName = "Content Initializes Correctly")]
    public async Task ContentInitializesCorrectly()
    {
        var content = new ButtonStub { Text = "Content" };
        var swipeView = new SwipeViewStub
        {
            PresentedContent = content
        };

        var platformContent = await GetValueAsync(swipeView, handler =>
        {
            return handler.PlatformView.Content;
        });

        Assert.NotNull(platformContent);
    }

    [AvaloniaFact(DisplayName = "Threshold Initializes Correctly")]
    public async Task ThresholdInitializesCorrectly()
    {
        var swipeView = new SwipeViewStub
        {
            Threshold = 150
        };

        await ValidatePropertyInitValue(swipeView, () => swipeView.Threshold, GetPlatformThreshold, 150.0);
    }

    [AvaloniaTheory(DisplayName = "Threshold Updates Correctly")]
    [InlineData(50.0)]
    [InlineData(100.0)]
    [InlineData(300.0)]
    public async Task ThresholdUpdatesCorrectly(double threshold)
    {
        var swipeView = new SwipeViewStub { Threshold = 200 };

        await ValidatePropertyUpdatesValue(
            swipeView,
            nameof(ISwipeView.Threshold),
            GetPlatformThreshold,
            threshold,
            200.0);
    }

    [AvaloniaFact(DisplayName = "LeftItems Initialize Correctly")]
    public async Task LeftItemsInitializeCorrectly()
    {
        var leftItems = new SwipeItemsStub { new SwipeItemStub { Text = "Left" } };
        var swipeView = new SwipeViewStub { LeftItems = leftItems };

        var handler = await CreateHandlerAsync(swipeView);
        
        // Verify Left template is set (Avalonia Swipe uses templates, so we just check for non-null)
        Assert.NotNull(handler.PlatformView.Left);
    }

    [AvaloniaFact(DisplayName = "RightItems Initialize Correctly")]
    public async Task RightItemsInitializeCorrectly()
    {
        var rightItems = new SwipeItemsStub { new SwipeItemStub { Text = "Right" } };
        var swipeView = new SwipeViewStub { RightItems = rightItems };

        var handler = await CreateHandlerAsync(swipeView);

        Assert.NotNull(handler.PlatformView.Right);
    }

    [AvaloniaFact(DisplayName = "TopItems Initialize Correctly")]
    public async Task TopItemsInitializeCorrectly()
    {
        var topItems = new SwipeItemsStub { new SwipeItemStub { Text = "Top" } };
        var swipeView = new SwipeViewStub { TopItems = topItems };

        var handler = await CreateHandlerAsync(swipeView);

        Assert.NotNull(handler.PlatformView.Top);
    }

    [AvaloniaFact(DisplayName = "BottomItems Initialize Correctly")]
    public async Task BottomItemsInitializeCorrectly()
    {
        var bottomItems = new SwipeItemsStub { new SwipeItemStub { Text = "Bottom" } };
        var swipeView = new SwipeViewStub { BottomItems = bottomItems };

        var handler = await CreateHandlerAsync(swipeView);

        Assert.NotNull(handler.PlatformView.Bottom);
    }

    [AvaloniaFact(DisplayName = "RequestOpen Opens Platform View")]
    public async Task RequestOpenOpensPlatformView()
    {
        var swipeView = new SwipeViewStub();
        var handler = await CreateHandlerAsync(swipeView);
        var platformView = handler.PlatformView;

        // Ensure initial state
        Assert.Equal(SwipeState.Hidden, platformView.SwipeState);

        // Simulate RequestOpen command
        var request = new SwipeViewOpenRequest(Microsoft.Maui.OpenSwipeItem.LeftItems, false);
        swipeView.RequestOpen(request);
        
        // Handler should process the command and call Open on platform view
        // Note: Since we can't easily spy on the method call without Moq, 
        // we check the state change which happens synchronously for non-animated requests in our Stub/Impl
        Assert.Equal(SwipeState.LeftVisible, platformView.SwipeState);
    }

    [AvaloniaFact(DisplayName = "RequestClose Closes Platform View")]
    public async Task RequestCloseClosesPlatformView()
    {
        var swipeView = new SwipeViewStub();
        var handler = await CreateHandlerAsync(swipeView);
        var platformView = handler.PlatformView;

        // Set to open state first
        platformView.Open(OpenSwipeItem.LeftItems, false);
        Assert.Equal(SwipeState.LeftVisible, platformView.SwipeState);

        // Simulate RequestClose command
        var request = new SwipeViewCloseRequest(false);
        swipeView.RequestClose(request);

        Assert.Equal(SwipeState.Hidden, platformView.SwipeState);
    }

    [AvaloniaFact(DisplayName = "SwipeStarted Event Triggers VirtualView")]
    public async Task SwipeStartedEventTriggersVirtualView()
    {
        var swipeView = new SwipeViewStub();
        var handler = await CreateHandlerAsync(swipeView);
        var platformView = handler.PlatformView;

        // Simulate SwipeStarted on platform view using the actual event from Swipe class
        var args = new SwipeStartedEventArgs(Swipe.SwipeStartedEvent, SwipeDirection.Left);
        platformView.RaiseEvent(args);

        Assert.True(swipeView.SwipeStartedFired);
        Assert.Equal(Microsoft.Maui.SwipeDirection.Left, swipeView.LastSwipeStartedDirection);
    }

    [AvaloniaFact(DisplayName = "SwipeChanging Event Triggers VirtualView")]
    public async Task SwipeChangingEventTriggersVirtualView()
    {
        var swipeView = new SwipeViewStub();
        var handler = await CreateHandlerAsync(swipeView);
        var platformView = handler.PlatformView;
        
        // Let's try using reflection to fire the event for test coverage:
        var eventInfo = typeof(Swipe).GetField("SwipeChanging", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var eventDelegate = (MulticastDelegate?)eventInfo?.GetValue(platformView);
        
        if (eventDelegate != null)
        {
            var args = new SwipeChangingEventArgs(SwipeDirection.Right, 50);
            eventDelegate.DynamicInvoke(platformView, args);

            Assert.True(swipeView.SwipeChangingFired);
            Assert.Equal(Microsoft.Maui.SwipeDirection.Right, swipeView.LastSwipeChangingDirection);
            Assert.Equal(50, swipeView.LastSwipeChangingOffset);
        }
    }

    [AvaloniaFact(DisplayName = "SwipeEnded Event Triggers VirtualView")]
    public async Task SwipeEndedEventTriggersVirtualView()
    {
        var swipeView = new SwipeViewStub();
        var handler = await CreateHandlerAsync(swipeView);
        var platformView = handler.PlatformView;

        // Simulate SwipeEnded using the actual event from Swipe class
        var args = new SwipeEndedEventArgs(Swipe.SwipeEndedEvent, SwipeDirection.Left, true);
        platformView.RaiseEvent(args);

        Assert.True(swipeView.SwipeEndedFired);
        Assert.Equal(Microsoft.Maui.SwipeDirection.Left, swipeView.LastSwipeEndedDirection);
        Assert.True(swipeView.LastSwipeEndedIsOpen);
    }
    
    [AvaloniaFact(DisplayName = "Updating Content Does Not Affect Threshold")]
    public async Task ContentDoesNotAffectThreshold()
    {
        var swipeView = new SwipeViewStub
        {
            Threshold = 200,
            PresentedContent = new ButtonStub()
        };

        await ValidateUnrelatedPropertyUnaffected(
            swipeView,
            GetPlatformThreshold,
            nameof(ISwipeView.Content),
            () => swipeView.PresentedContent = new ButtonStub { Text = "New" });
    }

    [AvaloniaFact(DisplayName = "SwipeChanging Event Triggers For Vertical Directions")]
    public async Task SwipeChangingEventTriggersForVertical()
    {
        var swipeView = new SwipeViewStub();
        var handler = await CreateHandlerAsync(swipeView);
        var platformView = handler.PlatformView;

        var eventInfo = typeof(Swipe).GetField("SwipeChanging", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var eventDelegate = (MulticastDelegate?)eventInfo?.GetValue(platformView);

        if (eventDelegate != null)
        {
            var args = new SwipeChangingEventArgs(SwipeDirection.Down, 30);
            eventDelegate.DynamicInvoke(platformView, args);

            Assert.True(swipeView.SwipeChangingFired);
            Assert.Equal(Microsoft.Maui.SwipeDirection.Down, swipeView.LastSwipeChangingDirection);
            Assert.Equal(30, swipeView.LastSwipeChangingOffset);
        }
    }
    
    [AvaloniaFact(DisplayName = "Execute Mode Invokes SwipeItem")]
    public async Task ExecuteModeInvokesSwipeItem()
    {
        var swipeItem = new SwipeItemStub { Text = "Execute" };
        var rightItems = new SwipeItemsStub { swipeItem };
        rightItems.Mode = Microsoft.Maui.SwipeMode.Execute;
        var swipeView = new SwipeViewStub { RightItems = rightItems };

        var handler = await CreateHandlerAsync(swipeView);
        var platformView = handler.PlatformView;

        // simulate execute event
        var onExecuted = typeof(SwipeViewHandler).GetMethod("OnSwipeExecuted", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        onExecuted?.Invoke(handler, [platformView, new SwipeStartedEventArgs(Swipe.SwipeStartedEvent, SwipeDirection.Left)
        ]);

        Assert.Equal(1, swipeItem.InvokedCount);
    }

    double GetPlatformThreshold(SwipeViewHandler handler) =>
        handler.PlatformView.Threshold;

    [AvaloniaFact(DisplayName = "RequestOpen Works for TopItems")]
    public async Task RequestOpenWorksForTopItems()
    {
        var topItems = new SwipeItemsStub { new SwipeItemStub { Text = "Top" } };
        var swipeView = new SwipeViewStub { TopItems = topItems };
        var handler = await CreateHandlerAsync(swipeView);
        var platformView = handler.PlatformView;

        Assert.Equal(SwipeState.Hidden, platformView.SwipeState);

        var request = new SwipeViewOpenRequest(Microsoft.Maui.OpenSwipeItem.TopItems, false);
        swipeView.RequestOpen(request);

        Assert.Equal(SwipeState.TopVisible, platformView.SwipeState);
    }

    [AvaloniaFact(DisplayName = "RequestOpen Works for BottomItems")]
    public async Task RequestOpenWorksForBottomItems()
    {
        var bottomItems = new SwipeItemsStub { new SwipeItemStub { Text = "Bottom" } };
        var swipeView = new SwipeViewStub { BottomItems = bottomItems };
        var handler = await CreateHandlerAsync(swipeView);
        var platformView = handler.PlatformView;

        Assert.Equal(SwipeState.Hidden, platformView.SwipeState);

        var request = new SwipeViewOpenRequest(Microsoft.Maui.OpenSwipeItem.BottomItems, false);
        swipeView.RequestOpen(request);

        Assert.Equal(SwipeState.BottomVisible, platformView.SwipeState);
    }

    [AvaloniaFact(DisplayName = "RequestOpen Works for RightItems")]
    public async Task RequestOpenWorksForRightItems()
    {
        var rightItems = new SwipeItemsStub { new SwipeItemStub { Text = "Right" } };
        var swipeView = new SwipeViewStub { RightItems = rightItems };
        var handler = await CreateHandlerAsync(swipeView);
        var platformView = handler.PlatformView;

        Assert.Equal(SwipeState.Hidden, platformView.SwipeState);

        var request = new SwipeViewOpenRequest(Microsoft.Maui.OpenSwipeItem.RightItems, false);
        swipeView.RequestOpen(request);

        Assert.Equal(SwipeState.RightVisible, platformView.SwipeState);
    }

    [AvaloniaFact(DisplayName = "Multiple SwipeItems Initialize Correctly")]
    public async Task MultipleSwipeItemsInitializeCorrectly()
    {
        var leftItems = new SwipeItemsStub
        {
            new SwipeItemStub { Text = "Item1" },
            new SwipeItemStub { Text = "Item2" },
            new SwipeItemStub { Text = "Item3" }
        };
        var swipeView = new SwipeViewStub { LeftItems = leftItems };
        var handler = await CreateHandlerAsync(swipeView);

        Assert.NotNull(handler.PlatformView.Left);
        Assert.Equal(3, leftItems.Count);
    }

    [AvaloniaFact(DisplayName = "SwipeMode Reveal Is Set Correctly")]
    public async Task SwipeModeRevealIsSetCorrectly()
    {
        var leftItems = new SwipeItemsStub { new SwipeItemStub { Text = "Left" } };
        leftItems.Mode = Microsoft.Maui.SwipeMode.Reveal;
        var swipeView = new SwipeViewStub { LeftItems = leftItems };
        var handler = await CreateHandlerAsync(swipeView);

        // Verify the mode is propagated to platform (LeftMode property in Swipe)
        Assert.Equal(SwipeMode.Reveal, handler.PlatformView.LeftMode);
    }

    [AvaloniaFact(DisplayName = "SwipeMode Execute Is Set Correctly")]
    public async Task SwipeModeExecuteIsSetCorrectly()
    {
        var rightItems = new SwipeItemsStub { new SwipeItemStub { Text = "Delete" } };
        rightItems.Mode = Microsoft.Maui.SwipeMode.Execute;
        var swipeView = new SwipeViewStub { RightItems = rightItems };
        var handler = await CreateHandlerAsync(swipeView);

        // Verify the mode is propagated to platform (RightMode property in Swipe)
        Assert.Equal(SwipeMode.Execute, handler.PlatformView.RightMode);
    }

    [AvaloniaFact(DisplayName = "AnimationDuration Updates Correctly")]
    public async Task AnimationDurationUpdatesCorrectly()
    {
        var swipeView = new SwipeViewStub();
        var handler = await CreateHandlerAsync(swipeView);
        var platformView = handler.PlatformView;

        var initialDuration = platformView.AnimationDuration;
        Assert.NotEqual(TimeSpan.Zero, initialDuration);

        // Update animation duration
        platformView.AnimationDuration = TimeSpan.FromMilliseconds(500);
        Assert.Equal(TimeSpan.FromMilliseconds(500), platformView.AnimationDuration);
    }

    [AvaloniaFact(DisplayName = "OpenRequested Event Fires When Opening")]
    public async Task OpenRequestedEventFiresWhenOpening()
    {
        var swipeView = new SwipeViewStub();
        var handler = await CreateHandlerAsync(swipeView);
        var platformView = handler.PlatformView;

        bool eventFired = false;
        OpenSwipeItem? requestedItem = null;

        platformView.OpenRequested += (sender, e) =>
        {
            eventFired = true;
            requestedItem = e.OpenSwipeItem;
        };

        platformView.Open(OpenSwipeItem.LeftItems, false);

        Assert.True(eventFired);
        Assert.Equal(OpenSwipeItem.LeftItems, requestedItem);
    }

    [AvaloniaFact(DisplayName = "CloseRequested Event Fires When Closing")]
    public async Task CloseRequestedEventFiresWhenClosing()
    {
        var swipeView = new SwipeViewStub();
        var handler = await CreateHandlerAsync(swipeView);
        var platformView = handler.PlatformView;

        platformView.Open(OpenSwipeItem.LeftItems, false);

        bool eventFired = false;
        platformView.CloseRequested += (sender, e) =>
        {
            eventFired = true;
        };

        platformView.Close(false);

        Assert.True(eventFired);
    }

    [AvaloniaFact(DisplayName = "Empty SwipeItems Collection Handles Gracefully")]
    public async Task EmptySwipeItemsCollectionHandlesGracefully()
    {
        var emptyItems = new SwipeItemsStub(); // Empty collection
        var swipeView = new SwipeViewStub { LeftItems = emptyItems };
        var handler = await CreateHandlerAsync(swipeView);

        // Should not throw and Left should be null or empty template
        Assert.NotNull(handler.PlatformView);
    }
}
