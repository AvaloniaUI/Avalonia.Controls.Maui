using Microsoft.Maui;
using System;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Shared state machine that forwards platform lifecycle transitions to a MAUI <see cref="IWindow"/>.
/// </summary>
/// <remarks>
/// MAUI's window lifecycle methods drive both the MAUI window events and the MAUI
/// <see cref="Microsoft.Maui.Controls.Application"/> lifecycle: <see cref="IWindow.Created"/> calls
/// <c>OnStart</c>, <see cref="IWindow.Resumed"/> calls <c>OnResume</c> and <see cref="IWindow.Stopped"/>
/// calls <c>OnSleep</c>. Those methods throw when invoked in an invalid order (for example activating an
/// already-activated window), so this base tracks state to keep the calls well-ordered and idempotent.
/// Subclasses subscribe to a concrete platform view and translate its events into the <c>Notify*</c> calls.
/// </remarks>
internal abstract class MauiWindowLifecycleDispatcher : IDisposable
{
    private readonly IWindow _virtualView;
    private bool _isCreated;
    private bool _isActivated;
    private bool _isDestroyed;

    /// <summary>
    /// Initializes a new instance of the <see cref="MauiWindowLifecycleDispatcher"/> class.
    /// </summary>
    /// <param name="virtualView">The MAUI window to forward lifecycle calls to.</param>
    protected MauiWindowLifecycleDispatcher(IWindow virtualView)
    {
        _virtualView = virtualView;
    }

    /// <summary>
    /// Gets a value indicating whether <see cref="IWindow.Created"/> has been raised.
    /// </summary>
    protected bool IsCreated => _isCreated;

    /// <summary>
    /// Raises <see cref="IWindow.Created"/> once, which also starts the MAUI application.
    /// </summary>
    internal void NotifyCreated()
    {
        if (_isCreated)
        {
            return;
        }

        _isCreated = true;
        _virtualView.Created();
    }

    /// <summary>
    /// Raises <see cref="IWindow.Activated"/>, ensuring the window has been created first.
    /// </summary>
    internal void NotifyActivated()
    {
        if (_isActivated || _isDestroyed)
        {
            return;
        }

        NotifyCreated();
        _isActivated = true;
        _virtualView.Activated();
    }

    /// <summary>
    /// Raises <see cref="IWindow.Deactivated"/> when the window is currently activated.
    /// </summary>
    internal void NotifyDeactivated()
    {
        if (!_isActivated)
        {
            return;
        }

        _isActivated = false;
        _virtualView.Deactivated();
    }

    /// <summary>
    /// Raises <see cref="IWindow.Stopped"/>, which puts the MAUI application to sleep.
    /// </summary>
    internal void NotifyStopped()
    {
        if (!_isCreated || _isDestroyed)
        {
            return;
        }

        _virtualView.Stopped();
    }

    /// <summary>
    /// Raises <see cref="IWindow.Resumed"/>, which resumes the MAUI application.
    /// </summary>
    internal void NotifyResumed()
    {
        if (!_isCreated || _isDestroyed)
        {
            return;
        }

        _virtualView.Resumed();
    }

    /// <summary>
    /// Raises <see cref="IWindow.Destroying"/> once, deactivating the window first if needed.
    /// </summary>
    internal void NotifyDestroying()
    {
        if (_isDestroyed)
        {
            return;
        }

        _isDestroyed = true;

        if (_isActivated)
        {
            _isActivated = false;
            _virtualView.Deactivated();
        }

        _virtualView.Destroying();
    }

    /// <summary>
    /// Unsubscribes from the platform view's lifecycle events.
    /// </summary>
    public abstract void Dispose();
}
