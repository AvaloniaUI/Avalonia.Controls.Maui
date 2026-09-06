using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.VisualTree;
using Microsoft.Maui;
using MauiSwipeMode = Microsoft.Maui.SwipeMode;
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
        platformView.SetCurrentValue(Swipe.SwipeStateProperty, SwipeState.LeftVisible);
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

        var onExecute = typeof(SwipeViewHandler).GetMethod("OnExecuteRequested", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        onExecute?.Invoke(handler, new object?[] { platformView, SwipeDirection.Left });

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

        platformView.RaiseEvent(new OpenRequestedEventArgs(Swipe.OpenRequestedEvent, OpenSwipeItem.LeftItems));
        platformView.SetCurrentValue(Swipe.SwipeStateProperty, SwipeState.LeftVisible);

        Assert.True(eventFired);
        Assert.Equal(OpenSwipeItem.LeftItems, requestedItem);
    }

    [AvaloniaFact(DisplayName = "CloseRequested Event Fires When Closing")]
    public async Task CloseRequestedEventFiresWhenClosing()
    {
        var swipeView = new SwipeViewStub();
        var handler = await CreateHandlerAsync(swipeView);
        var platformView = handler.PlatformView;

        platformView.SetCurrentValue(Swipe.SwipeStateProperty, SwipeState.LeftVisible);

        bool eventFired = false;
        platformView.CloseRequested += (sender, e) =>
        {
            eventFired = true;
        };

        platformView.RaiseEvent(new CloseRequestedEventArgs(Swipe.CloseRequestedEvent));
        platformView.SetCurrentValue(Swipe.SwipeStateProperty, SwipeState.Hidden);

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

    [AvaloniaFact(DisplayName = "Horizontal wheel pan opens right items")]
    public async Task HorizontalWheelPanOpensRightItems()
    {
        var swipeView = new SwipeViewStub
        {
            RightItems = new SwipeItemsStub { new Microsoft.Maui.Controls.SwipeItem { Text = "Delete" } }
        };
        var handler = await CreateHandlerAsync(swipeView);

        await InvokeOnMainThreadAsync(() =>
        {
            var platformView = handler.PlatformView;
            platformView.Width = 300;
            platformView.Height = 100;
            // Swipe materializes side items with the inherited DataContext, as pages
            // with a binding context do at runtime.
            platformView.DataContext = swipeView;
            var window = new Avalonia.Controls.Window { Content = platformView, Width = 300, Height = 100 };
            window.Show();

            try
            {
                Threading.Dispatcher.UIThread.RunJobs();

                // Layout runs between deltas, like frames during a real gesture.
                for (var i = 0; i < 3; i++)
                {
                    platformView.RaiseEvent(CreateWheelEventArgs(platformView, new Vector(-1, 0)));
                    Threading.Dispatcher.UIThread.RunJobs();
                }

                platformView.CompleteWheelPan();
                Threading.Dispatcher.UIThread.RunJobs();

                Assert.Equal(SwipeState.RightVisible, platformView.SwipeState);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [AvaloniaFact(DisplayName = "Horizontal wheel pan closes open items")]
    public async Task HorizontalWheelPanClosesOpenItems()
    {
        var swipeView = new SwipeViewStub
        {
            RightItems = new SwipeItemsStub { new Microsoft.Maui.Controls.SwipeItem { Text = "Delete" } }
        };
        var handler = await CreateHandlerAsync(swipeView);

        await InvokeOnMainThreadAsync(() =>
        {
            var platformView = handler.PlatformView;
            platformView.Width = 300;
            platformView.Height = 100;
            platformView.DataContext = swipeView;
            var window = new Avalonia.Controls.Window { Content = platformView, Width = 300, Height = 100 };
            window.Show();

            try
            {
                Threading.Dispatcher.UIThread.RunJobs();
                platformView.SetSwipeState(SwipeState.RightVisible, animated: false);
                Threading.Dispatcher.UIThread.RunJobs();

                for (var i = 0; i < 3; i++)
                    platformView.RaiseEvent(CreateWheelEventArgs(platformView, new Vector(1, 0)));

                platformView.CompleteWheelPan();
                Threading.Dispatcher.UIThread.RunJobs();

                Assert.Equal(SwipeState.Hidden, platformView.SwipeState);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [AvaloniaFact(DisplayName = "Vertical wheel is not consumed without vertical items")]
    public async Task VerticalWheelNotConsumedWithoutVerticalItems()
    {
        var swipeView = new SwipeViewStub
        {
            RightItems = new SwipeItemsStub { new Microsoft.Maui.Controls.SwipeItem { Text = "Delete" } }
        };
        var handler = await CreateHandlerAsync(swipeView);

        await InvokeOnMainThreadAsync(() =>
        {
            var platformView = handler.PlatformView;
            platformView.Width = 300;
            platformView.Height = 100;
            platformView.DataContext = swipeView;
            var window = new Avalonia.Controls.Window { Content = platformView, Width = 300, Height = 100 };
            window.Show();

            try
            {
                Threading.Dispatcher.UIThread.RunJobs();

                var args = CreateWheelEventArgs(platformView, new Vector(0, -1));
                platformView.RaiseEvent(args);

                // Ancestor scroll containers must keep receiving vertical scrolling.
                Assert.False(args.Handled);
                Assert.Equal(SwipeState.Hidden, platformView.SwipeState);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [AvaloniaFact(DisplayName = "Wheel pan below threshold snaps back to hidden")]
    public async Task WheelPanBelowThresholdSnapsBack()
    {
        var swipeView = new SwipeViewStub
        {
            RightItems = new SwipeItemsStub { new Microsoft.Maui.Controls.SwipeItem { Text = "Delete" } }
        };
        var handler = await CreateHandlerAsync(swipeView);

        await InvokeOnMainThreadAsync(() =>
        {
            var platformView = handler.PlatformView;
            platformView.Width = 300;
            platformView.Height = 100;
            platformView.DataContext = swipeView;
            var window = new Avalonia.Controls.Window { Content = platformView, Width = 300, Height = 100 };
            window.Show();

            try
            {
                Threading.Dispatcher.UIThread.RunJobs();

                platformView.RaiseEvent(CreateWheelEventArgs(platformView, new Vector(-0.5, 0)));

                platformView.CompleteWheelPan();
                Threading.Dispatcher.UIThread.RunJobs();

                Assert.Equal(SwipeState.Hidden, platformView.SwipeState);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [AvaloniaFact(DisplayName = "Wheel input is ignored while a pointer drag is active")]
    public async Task WheelIgnoredWhilePointerDragActive()
    {
        var swipeView = new SwipeViewStub
        {
            RightItems = new SwipeItemsStub { new Microsoft.Maui.Controls.SwipeItem { Text = "Delete" } }
        };
        var handler = await CreateHandlerAsync(swipeView);

        await InvokeOnMainThreadAsync(() =>
        {
            var platformView = handler.PlatformView;
            platformView.Width = 300;
            platformView.Height = 100;
            platformView.DataContext = swipeView;
            var window = new Avalonia.Controls.Window { Content = platformView, Width = 300, Height = 100 };
            window.Show();

            try
            {
                Threading.Dispatcher.UIThread.RunJobs();

                // Start a pointer drag on the body container (it owns the recognizer).
                var body = (Avalonia.Controls.Control)platformView.Children[^1];
                var pointer = new Pointer(1, PointerType.Mouse, true);
                body.RaiseEvent(new PointerPressedEventArgs(
                    body, pointer, body, new Point(150, 50), 0,
                    new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.LeftButtonPressed),
                    KeyModifiers.None));
                body.RaiseEvent(new Avalonia.Input.PointerEventArgs(
                    InputElement.PointerMovedEvent, body, pointer, body, new Point(120, 50), 0,
                    new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.Other),
                    KeyModifiers.None));

                var args = CreateWheelEventArgs(platformView, new Vector(-1, 0));
                platformView.RaiseEvent(args);

                Assert.False(args.Handled);

                body.RaiseEvent(new PointerReleasedEventArgs(
                    body, pointer, body, new Point(120, 50), 0,
                    new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.LeftButtonReleased),
                    KeyModifiers.None, MouseButton.Left));
            }
            finally
            {
                window.Close();
            }
        });
    }

    [AvaloniaFact(DisplayName = "Programmatic close during a wheel pan is not overridden by the settle")]
    public async Task ProgrammaticCloseDuringWheelPanWins()
    {
        var swipeView = new SwipeViewStub
        {
            RightItems = new SwipeItemsStub { new Microsoft.Maui.Controls.SwipeItem { Text = "Delete" } }
        };
        var handler = await CreateHandlerAsync(swipeView);

        await InvokeOnMainThreadAsync(() =>
        {
            var platformView = handler.PlatformView;
            platformView.Width = 300;
            platformView.Height = 100;
            platformView.DataContext = swipeView;
            var window = new Avalonia.Controls.Window { Content = platformView, Width = 300, Height = 100 };
            window.Show();

            try
            {
                Threading.Dispatcher.UIThread.RunJobs();

                for (var i = 0; i < 3; i++)
                {
                    platformView.RaiseEvent(CreateWheelEventArgs(platformView, new Vector(-1, 0)));
                    Threading.Dispatcher.UIThread.RunJobs();
                }

                // Explicit close (Esc, MAUI Close(), item invocation) discards the stream.
                platformView.SetSwipeState(SwipeState.Hidden, animated: false);
                platformView.CompleteWheelPan();
                Threading.Dispatcher.UIThread.RunJobs();

                Assert.Equal(SwipeState.Hidden, platformView.SwipeState);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [AvaloniaFact(DisplayName = "Vertical wheel does not open vertical items")]
    public async Task VerticalWheelDoesNotOpenVerticalItems()
    {
        var swipeView = new SwipeViewStub
        {
            TopItems = new SwipeItemsStub { new Microsoft.Maui.Controls.SwipeItem { Text = "Pin" } }
        };
        var handler = await CreateHandlerAsync(swipeView);

        await InvokeOnMainThreadAsync(() =>
        {
            var platformView = handler.PlatformView;
            platformView.Width = 300;
            platformView.Height = 100;
            platformView.DataContext = swipeView;
            var window = new Avalonia.Controls.Window { Content = platformView, Width = 300, Height = 100 };
            window.Show();

            try
            {
                Threading.Dispatcher.UIThread.RunJobs();

                var args = CreateWheelEventArgs(platformView, new Vector(0, 1));
                platformView.RaiseEvent(args);

                // Plain mouse wheels must keep scrolling lists that host swipes.
                Assert.False(args.Handled);
                Assert.Equal(SwipeState.Hidden, platformView.SwipeState);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [AvaloniaFact(DisplayName = "Vertical wheel closes an open vertical panel")]
    public async Task VerticalWheelClosesOpenVerticalPanel()
    {
        var swipeView = new SwipeViewStub
        {
            TopItems = new SwipeItemsStub { new Microsoft.Maui.Controls.SwipeItem { Text = "Pin" } }
        };
        var handler = await CreateHandlerAsync(swipeView);

        await InvokeOnMainThreadAsync(() =>
        {
            var platformView = handler.PlatformView;
            platformView.Width = 300;
            platformView.Height = 100;
            platformView.DataContext = swipeView;
            var window = new Avalonia.Controls.Window { Content = platformView, Width = 300, Height = 100 };
            window.Show();

            try
            {
                Threading.Dispatcher.UIThread.RunJobs();
                platformView.SetSwipeState(SwipeState.TopVisible, animated: false);
                Threading.Dispatcher.UIThread.RunJobs();

                for (var i = 0; i < 3; i++)
                {
                    platformView.RaiseEvent(CreateWheelEventArgs(platformView, new Vector(0, -1)));
                    Threading.Dispatcher.UIThread.RunJobs();
                }

                platformView.CompleteWheelPan();
                Threading.Dispatcher.UIThread.RunJobs();

                Assert.Equal(SwipeState.Hidden, platformView.SwipeState);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [AvaloniaFact(DisplayName = "Zero-delta wheel events are not consumed")]
    public async Task ZeroDeltaWheelNotConsumed()
    {
        var swipeView = new SwipeViewStub
        {
            RightItems = new SwipeItemsStub { new Microsoft.Maui.Controls.SwipeItem { Text = "Delete" } }
        };
        var handler = await CreateHandlerAsync(swipeView);

        await InvokeOnMainThreadAsync(() =>
        {
            var platformView = handler.PlatformView;
            platformView.Width = 300;
            platformView.Height = 100;
            platformView.DataContext = swipeView;
            var window = new Avalonia.Controls.Window { Content = platformView, Width = 300, Height = 100 };
            window.Show();

            try
            {
                Threading.Dispatcher.UIThread.RunJobs();

                var args = CreateWheelEventArgs(platformView, new Vector(0, 0));
                platformView.RaiseEvent(args);

                Assert.False(args.Handled);
                Assert.Equal(SwipeState.Hidden, platformView.SwipeState);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [AvaloniaFact(DisplayName = "Direct SwipeState set during a wheel pan is not overridden by the settle")]
    public async Task DirectStateSetDuringWheelPanWins()
    {
        var swipeView = new SwipeViewStub
        {
            LeftItems = new SwipeItemsStub { new Microsoft.Maui.Controls.SwipeItem { Text = "Pin" } },
            RightItems = new SwipeItemsStub { new Microsoft.Maui.Controls.SwipeItem { Text = "Delete" } }
        };
        var handler = await CreateHandlerAsync(swipeView);

        await InvokeOnMainThreadAsync(() =>
        {
            var platformView = handler.PlatformView;
            platformView.Width = 300;
            platformView.Height = 100;
            platformView.DataContext = swipeView;
            var window = new Avalonia.Controls.Window { Content = platformView, Width = 300, Height = 100 };
            window.Show();

            try
            {
                Threading.Dispatcher.UIThread.RunJobs();

                for (var i = 0; i < 3; i++)
                {
                    platformView.RaiseEvent(CreateWheelEventArgs(platformView, new Vector(-1, 0)));
                    Threading.Dispatcher.UIThread.RunJobs();
                }

                platformView.SwipeState = SwipeState.LeftVisible;
                platformView.CompleteWheelPan();
                Threading.Dispatcher.UIThread.RunJobs();

                Assert.Equal(SwipeState.LeftVisible, platformView.SwipeState);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [AvaloniaFact(DisplayName = "Vertical scrolling after a tiny horizontal delta is released to ancestors")]
    public async Task VerticalScrollAfterTinyHorizontalDeltaIsReleased()
    {
        var swipeView = new SwipeViewStub
        {
            RightItems = new SwipeItemsStub { new Microsoft.Maui.Controls.SwipeItem { Text = "Delete" } }
        };
        var handler = await CreateHandlerAsync(swipeView);

        await InvokeOnMainThreadAsync(() =>
        {
            var platformView = handler.PlatformView;
            platformView.Width = 300;
            platformView.Height = 100;
            platformView.DataContext = swipeView;
            var window = new Avalonia.Controls.Window { Content = platformView, Width = 300, Height = 100 };
            window.Show();

            try
            {
                Threading.Dispatcher.UIThread.RunJobs();

                var started = 0;
                var ended = 0;
                platformView.AddHandler(Swipe.SwipeStartedEvent, (_, _) => started++);
                platformView.AddHandler(Swipe.SwipeEndedEvent, (_, _) => ended++);

                // Below the direction-lock threshold, so the axis is still undecided.
                platformView.RaiseEvent(CreateWheelEventArgs(platformView, new Vector(-0.05, 0)));
                Threading.Dispatcher.UIThread.RunJobs();

                // Locks the axis vertical with no vertical action available.
                var vertical = CreateWheelEventArgs(platformView, new Vector(0, 1));
                platformView.RaiseEvent(vertical);
                Assert.False(vertical.Handled);

                var next = CreateWheelEventArgs(platformView, new Vector(0, 1));
                platformView.RaiseEvent(next);
                Assert.False(next.Handled);

                platformView.CompleteWheelPan();
                Threading.Dispatcher.UIThread.RunJobs();
                Assert.Equal(SwipeState.Hidden, platformView.SwipeState);

                // Cancellation must keep the event pair balanced.
                Assert.Equal(started, ended);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [AvaloniaFact(DisplayName = "Tapping the content of an open swipe closes it")]
    public async Task TappingContentOfOpenSwipeClosesIt()
    {
        var swipeView = new SwipeViewStub
        {
            RightItems = new SwipeItemsStub { new Microsoft.Maui.Controls.SwipeItem { Text = "Delete" } }
        };
        var handler = await CreateHandlerAsync(swipeView);

        await InvokeOnMainThreadAsync(() =>
        {
            var platformView = handler.PlatformView;
            platformView.Width = 300;
            platformView.Height = 100;
            platformView.DataContext = swipeView;
            var window = new Avalonia.Controls.Window { Content = platformView, Width = 300, Height = 100 };
            window.Show();

            try
            {
                Threading.Dispatcher.UIThread.RunJobs();
                platformView.SetSwipeState(SwipeState.RightVisible, animated: false);
                Threading.Dispatcher.UIThread.RunJobs();

                RaiseTap(platformView, new Point(50, 50));

                Assert.Equal(SwipeState.Hidden, platformView.SwipeState);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [AvaloniaFact(DisplayName = "Tapping the exposed items area does not close the swipe")]
    public async Task TappingExposedItemsAreaDoesNotClose()
    {
        var swipeView = new SwipeViewStub
        {
            RightItems = new SwipeItemsStub { new Microsoft.Maui.Controls.SwipeItem { Text = "Delete" } }
        };
        var handler = await CreateHandlerAsync(swipeView);

        await InvokeOnMainThreadAsync(() =>
        {
            var platformView = handler.PlatformView;
            platformView.Width = 300;
            platformView.Height = 100;
            platformView.DataContext = swipeView;
            var window = new Avalonia.Controls.Window { Content = platformView, Width = 300, Height = 100 };
            window.Show();

            try
            {
                Threading.Dispatcher.UIThread.RunJobs();
                platformView.SetSwipeState(SwipeState.RightVisible, animated: false);
                Threading.Dispatcher.UIThread.RunJobs();

                RaiseTap(platformView, new Point(295, 50));

                Assert.Equal(SwipeState.RightVisible, platformView.SwipeState);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [AvaloniaFact(DisplayName = "A pan release is not treated as a content tap")]
    public async Task PanReleaseIsNotTreatedAsContentTap()
    {
        var swipeView = new SwipeViewStub
        {
            RightItems = new SwipeItemsStub { new Microsoft.Maui.Controls.SwipeItem { Text = "Delete" } }
        };
        var handler = await CreateHandlerAsync(swipeView);

        await InvokeOnMainThreadAsync(() =>
        {
            var platformView = handler.PlatformView;
            platformView.Width = 300;
            platformView.Height = 100;
            platformView.DataContext = swipeView;
            var window = new Avalonia.Controls.Window { Content = platformView, Width = 300, Height = 100 };
            window.Show();

            try
            {
                Threading.Dispatcher.UIThread.RunJobs();
                platformView.SetSwipeState(SwipeState.RightVisible, animated: false);
                Threading.Dispatcher.UIThread.RunJobs();

                // Press and release far apart, as a pan's own release does.
                var pointer = new Pointer(1, PointerType.Mouse, true);
                platformView.RaiseEvent(new PointerPressedEventArgs(
                    platformView, pointer, platformView, new Point(200, 50), 0,
                    new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.LeftButtonPressed),
                    KeyModifiers.None));
                platformView.RaiseEvent(new PointerReleasedEventArgs(
                    platformView, pointer, platformView, new Point(120, 50), 0,
                    new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.LeftButtonReleased),
                    KeyModifiers.None, MouseButton.Left));

                Assert.Equal(SwipeState.RightVisible, platformView.SwipeState);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [AvaloniaTheory(DisplayName = "Invoking a swipe item honors SwipeBehaviorOnInvoked")]
    [InlineData(SwipeBehaviorOnInvoked.Auto, MauiSwipeMode.Reveal, true)]
    [InlineData(SwipeBehaviorOnInvoked.Auto, MauiSwipeMode.Execute, false)]
    [InlineData(SwipeBehaviorOnInvoked.Close, MauiSwipeMode.Execute, true)]
    [InlineData(SwipeBehaviorOnInvoked.RemainOpen, MauiSwipeMode.Reveal, false)]
    public async Task InvokingSwipeItemHonorsSwipeBehaviorOnInvoked(
        SwipeBehaviorOnInvoked behavior, MauiSwipeMode mode, bool expectClosed)
    {
        var invokedCount = 0;
        var swipeItem = new Microsoft.Maui.Controls.SwipeItem { Text = "Delete" };
        swipeItem.Invoked += (_, _) => invokedCount++;
        var swipeView = new SwipeViewStub
        {
            RightItems = new SwipeItemsStub { swipeItem }
        };
        ((SwipeItemsStub)swipeView.RightItems).SwipeBehaviorOnInvoked = behavior;
        ((SwipeItemsStub)swipeView.RightItems).Mode = mode;
        var handler = await CreateHandlerAsync(swipeView);

        await InvokeOnMainThreadAsync(() =>
        {
            var platformView = handler.PlatformView;
            platformView.Width = 300;
            platformView.Height = 100;
            platformView.DataContext = swipeView;
            var window = new Avalonia.Controls.Window { Content = platformView, Width = 300, Height = 100 };
            window.Show();

            try
            {
                Threading.Dispatcher.UIThread.RunJobs();
                platformView.SetSwipeState(SwipeState.RightVisible, animated: false);
                Threading.Dispatcher.UIThread.RunJobs();

                var button = platformView.GetVisualDescendants()
                    .OfType<Avalonia.Controls.Button>()
                    .First(b => b.Tag is ValueTuple<SwipeBehaviorOnInvoked, MauiSwipeMode, Swipe>);
                button.RaiseEvent(new Interactivity.RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
                Threading.Dispatcher.UIThread.RunJobs();

                Assert.Equal(1, invokedCount);
                Assert.Equal(expectClosed ? SwipeState.Hidden : SwipeState.RightVisible, platformView.SwipeState);
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static void RaiseTap(Swipe platformView, Point point)
    {
        var pointer = new Pointer(1, PointerType.Mouse, true);
        platformView.RaiseEvent(new PointerPressedEventArgs(
            platformView, pointer, platformView, point, 0,
            new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.LeftButtonPressed),
            KeyModifiers.None));
        platformView.RaiseEvent(new PointerReleasedEventArgs(
            platformView, pointer, platformView, point, 0,
            new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.LeftButtonReleased),
            KeyModifiers.None, MouseButton.Left));
    }

    private static PointerWheelEventArgs CreateWheelEventArgs(Visual target, Vector delta)
    {
        var pointer = new Pointer(1, PointerType.Mouse, true);
        return new PointerWheelEventArgs(
            target,
            pointer,
            target,
            new Point(50, 50),
            0,
            new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.Other),
            KeyModifiers.None,
            delta);
    }
}
