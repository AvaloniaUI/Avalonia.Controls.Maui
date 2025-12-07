using Avalonia.Controls.Maui.Animations;
using Microsoft.Maui.Animations;
using Microsoft.Maui;
using MauiAnimation = Microsoft.Maui.Animations.Animation;

namespace Avalonia.Controls.Maui.Tests.Animations;

public class AvaloniaAnimationManagerTests : IDisposable
{
    private readonly MockTicker _ticker;
    private readonly AvaloniaAnimationManager _manager;

    public AvaloniaAnimationManagerTests()
    {
        _ticker = new MockTicker();
        _manager = new AvaloniaAnimationManager(_ticker);
    }

    public void Dispose()
    {
        _manager.Dispose();
    }

    [Fact(DisplayName = "Constructor sets default property values")]
    public void Constructor_SetsDefaultPropertyValues()
    {
        Assert.Equal(1, _manager.SpeedModifier);
        Assert.True(_manager.AutoStartTicker);
        Assert.Same(_ticker, _manager.Ticker);
    }

    [Fact(DisplayName = "Constructor with ticker hooks up Fire callback")]
    public void Constructor_HooksUpFireCallback()
    {
        Assert.NotNull(_ticker.Fire);
    }

    [Fact(DisplayName = "Add starts ticker when AutoStartTicker is true")]
    public void Add_StartsTickerWhenAutoStartTickerIsTrue()
    {
        var animation = CreateAnimation();

        _manager.Add(animation);

        Assert.True(_ticker.IsRunning);
    }

    [Fact(DisplayName = "Add does not start ticker when AutoStartTicker is false")]
    public void Add_DoesNotStartTickerWhenAutoStartTickerIsFalse()
    {
        _manager.AutoStartTicker = false;
        var animation = CreateAnimation();

        _manager.Add(animation);

        Assert.False(_ticker.IsRunning);
    }

    [Fact(DisplayName = "Add does nothing when ticker is system disabled")]
    public void Add_DoesNothingWhenSystemDisabled()
    {
        _ticker.SetSystemEnabled(false);
        var animation = CreateAnimation();

        _manager.Add(animation);

        Assert.False(_ticker.IsRunning);
    }

    [Fact(DisplayName = "Add does not add duplicate animations")]
    public void Add_DoesNotAddDuplicateAnimations()
    {
        var animation = CreateAnimation();

        _manager.Add(animation);
        _manager.Add(animation);

        // Remove should only need to be called once to stop the ticker
        _manager.Remove(animation);
        Assert.False(_ticker.IsRunning);
    }

    [Fact(DisplayName = "Add does not start ticker if already running")]
    public void Add_DoesNotRestartTickerIfAlreadyRunning()
    {
        var animation1 = CreateAnimation();
        var animation2 = CreateAnimation();

        _manager.Add(animation1);
        Assert.Equal(1, _ticker.StartCount);

        _manager.Add(animation2);
        Assert.Equal(1, _ticker.StartCount); // Should not have called Start again
    }

    [Fact(DisplayName = "Remove stops ticker when last animation removed")]
    public void Remove_StopsTickerWhenLastAnimationRemoved()
    {
        var animation = CreateAnimation();

        _manager.Add(animation);
        Assert.True(_ticker.IsRunning);

        _manager.Remove(animation);
        Assert.False(_ticker.IsRunning);
    }

    [Fact(DisplayName = "Remove does not stop ticker when other animations exist")]
    public void Remove_DoesNotStopTickerWhenOtherAnimationsExist()
    {
        var animation1 = CreateAnimation();
        var animation2 = CreateAnimation();

        _manager.Add(animation1);
        _manager.Add(animation2);
        Assert.True(_ticker.IsRunning);

        _manager.Remove(animation1);
        Assert.True(_ticker.IsRunning);

        _manager.Remove(animation2);
        Assert.False(_ticker.IsRunning);
    }

    [Fact(DisplayName = "Remove handles non-existent animation gracefully")]
    public void Remove_HandlesNonExistentAnimationGracefully()
    {
        var animation = CreateAnimation();

        // Should not throw
        _manager.Remove(animation);
    }

    [Fact(DisplayName = "SpeedModifier can be set and retrieved")]
    public void SpeedModifier_CanBeSetAndRetrieved()
    {
        _manager.SpeedModifier = 2.5;

        Assert.Equal(2.5, _manager.SpeedModifier);
    }

    [Fact(DisplayName = "SpeedModifier defaults to 1")]
    public void SpeedModifier_DefaultsToOne()
    {
        Assert.Equal(1, _manager.SpeedModifier);
    }

    [Fact(DisplayName = "AutoStartTicker can be set and retrieved")]
    public void AutoStartTicker_CanBeSetAndRetrieved()
    {
        _manager.AutoStartTicker = false;

        Assert.False(_manager.AutoStartTicker);
    }

    [Fact(DisplayName = "AutoStartTicker defaults to true")]
    public void AutoStartTicker_DefaultsToTrue()
    {
        Assert.True(_manager.AutoStartTicker);
    }

    [Fact(DisplayName = "OnFire invokes animation processing")]
    public void OnFire_InvokesAnimationProcessing()
    {
        var animation = CreateAnimation();
        _manager.Add(animation);

        // Simulating fire should not throw
        _ticker.SimulateFire();
    }

    [Fact(DisplayName = "OnFire stops ticker when animations are disabled mid-flight")]
    public void OnFire_StopsTickerWhenSystemDisabled()
    {
        var animation = CreateAnimation();
        _manager.Add(animation);
        Assert.True(_ticker.IsRunning);

        _ticker.SetSystemEnabled(false);
        _ticker.SimulateFire();

        Assert.False(_ticker.IsRunning);
    }

    [Fact(DisplayName = "Dispose disposes ticker if disposable")]
    public void Dispose_DisposesTickerIfDisposable()
    {
        var disposableTicker = new MockTicker();
        using var manager = new AvaloniaAnimationManager(disposableTicker);

        manager.Dispose();

        Assert.True(disposableTicker.IsDisposed);
    }

    [Fact(DisplayName = "Dispose can be called multiple times")]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        _manager.Dispose();
        _manager.Dispose(); // Should not throw
    }

    [Fact(DisplayName = "Dispose stops running ticker")]
    public void Dispose_StopsRunningTicker()
    {
        var animation = CreateAnimation();
        _manager.Add(animation);
        Assert.True(_ticker.IsRunning);

        _manager.Dispose();

        Assert.True(_ticker.IsDisposed);
    }

    [Fact(DisplayName = "Default constructor creates AvaloniaTicker")]
    public void DefaultConstructor_CreatesAvaloniaTicker()
    {
        using var manager = new AvaloniaAnimationManager();

        Assert.IsType<AvaloniaTicker>(manager.Ticker);
    }

    [Fact(DisplayName = "Ticker property returns the injected ticker")]
    public void Ticker_ReturnsInjectedTicker()
    {
        Assert.Same(_ticker, _manager.Ticker);
    }

    [Theory(DisplayName = "Multiple animations can be added")]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void Add_MultipleAnimationsCanBeAdded(int count)
    {
        for (int i = 0; i < count; i++)
        {
            _manager.Add(CreateAnimation());
        }

        Assert.True(_ticker.IsRunning);
    }

    [Fact(DisplayName = "Fire callback is set during construction")]
    public void FireCallback_IsSetDuringConstruction()
    {
        var ticker = new MockTicker();

        using var manager = new AvaloniaAnimationManager(ticker);

        Assert.NotNull(ticker.Fire);
    }

    private static MauiAnimation CreateAnimation()
    {
        return new MauiAnimation
        {
            Duration = 1,
            Easing = Easing.Linear
        };
    }

    /// <summary>
    /// A mock ticker for testing the animation manager without relying on real timers.
    /// </summary>
    private class MockTicker : ITicker, IDisposable
    {
        private bool _systemEnabled = true;
        private bool _isRunning;

        public bool IsRunning => _isRunning;
        public bool SystemEnabled => _systemEnabled;
        public int MaxFps { get; set; } = 60;
        public Action? Fire { get; set; }
        public bool IsDisposed { get; private set; }
        public int StartCount { get; private set; }
        public int StopCount { get; private set; }

        public void Start()
        {
            if (!_isRunning)
            {
                _isRunning = true;
                StartCount++;
            }
        }

        public void Stop()
        {
            if (_isRunning)
            {
                _isRunning = false;
                StopCount++;
            }
        }

        public void SetSystemEnabled(bool enabled)
        {
            _systemEnabled = enabled;
        }

        public void SimulateFire()
        {
            Fire?.Invoke();
        }

        public void Dispose()
        {
            IsDisposed = true;
            _isRunning = false;
        }
    }
}
