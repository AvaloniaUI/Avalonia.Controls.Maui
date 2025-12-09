using System;
using System.Collections.Generic;
using Microsoft.Maui.Animations;
using MauiAnimation = Microsoft.Maui.Animations.Animation;

namespace Avalonia.Controls.Maui.Animations;

/// <summary>
/// An Avalonia-based implementation of <see cref="IAnimationManager"/>
/// </summary>
public class AvaloniaAnimationManager : IAnimationManager, IDisposable
{
    private readonly List<MauiAnimation> _animations = new();
    private long _lastUpdate;
    private bool _disposed;

    /// <summary>
    /// Creates a new <see cref="AvaloniaAnimationManager"/> with an <see cref="AvaloniaTicker"/>.
    /// </summary>
    public AvaloniaAnimationManager()
        : this(new AvaloniaTicker())
    {
    }

    /// <summary>
    /// Creates a new <see cref="AvaloniaAnimationManager"/> with the specified ticker.
    /// </summary>
    /// <param name="ticker">The ticker to use for timing animations.</param>
    public AvaloniaAnimationManager(ITicker ticker)
    {
        _lastUpdate = GetCurrentTick();
        Ticker = ticker;
        Ticker.Fire = OnFire;
    }

    /// <inheritdoc/>
    public ITicker Ticker { get; }

    /// <inheritdoc/>
    public double SpeedModifier { get; set; } = 1;

    /// <inheritdoc/>
    public bool AutoStartTicker { get; set; } = true;

    /// <inheritdoc/>
    public void Add(MauiAnimation animation)
    {
        // If animations are disabled, don't do anything
        if (!Ticker.SystemEnabled)
        {
            return;
        }

        if (!_animations.Contains(animation))
            _animations.Add(animation);

        if (!Ticker.IsRunning && AutoStartTicker)
            Start();
    }

    /// <inheritdoc/>
    public void Remove(MauiAnimation animation)
    {
        _animations.Remove(animation);

        if (_animations.Count == 0)
            End();
    }

    private void Start()
    {
        _lastUpdate = GetCurrentTick();
        Ticker.Start();
    }

    private void End() =>
        Ticker?.Stop();

    private static long GetCurrentTick() =>
        Environment.TickCount & int.MaxValue;

    private void OnFire()
    {
        if (!Ticker.SystemEnabled)
        {
            // Animations are disabled - force all running animations to their finished state
            ForceFinishAnimations();
            return;
        }

        var now = GetCurrentTick();
        var milliseconds = TimeSpan.FromMilliseconds(now - _lastUpdate).TotalMilliseconds;
        _lastUpdate = now;

        // Create a copy to iterate safely
        var animations = new List<MauiAnimation>(_animations);
        foreach (var animation in animations)
        {
            OnAnimationTick(animation, milliseconds);
        }

        if (_animations.Count == 0)
            End();
    }

    private void OnAnimationTick(MauiAnimation animation, double milliseconds)
    {
        if (animation.HasFinished)
        {
            _animations.Remove(animation);
            animation.RemoveFromParent();
            return;
        }

        animation.Tick(AdjustSpeed(milliseconds));

        if (animation.HasFinished)
        {
            _animations.Remove(animation);
            animation.RemoveFromParent();
        }
    }

    private void ForceFinishAnimations()
    {
        var animations = new List<MauiAnimation>(_animations);
        foreach (var animation in animations)
        {
            animation.ForceFinish();
            _animations.Remove(animation);
            animation.RemoveFromParent();
        }
        End();
    }

    internal virtual double AdjustSpeed(double elapsedMilliseconds)
    {
        return elapsedMilliseconds * SpeedModifier;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing && Ticker is IDisposable disposable)
                disposable.Dispose();

            _disposed = true;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
