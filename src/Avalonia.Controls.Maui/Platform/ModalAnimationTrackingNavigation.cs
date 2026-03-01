

using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// An <see cref="INavigation"/> wrapper that intercepts modal push/pop calls to track
/// the <c>animated</c> parameter. This is necessary because MAUI's <see cref="ModalPushedEventArgs"/>
/// and <see cref="ModalPoppedEventArgs"/> do not carry the animated flag, so by the time
/// the event fires the information is lost. The tracked value is consumed by
/// <see cref="Handlers.WindowHandler"/> to decide whether to animate the modal transition.
/// </summary>
internal sealed class ModalAnimationTrackingNavigation : INavigation
{
    private static readonly ConditionalWeakTable<Page, StrongBox<bool>> s_animatedFlags = new();

    private readonly INavigation _inner;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModalAnimationTrackingNavigation"/> class.
    /// </summary>
    /// <param name="inner">The inner <see cref="INavigation"/> to delegate to.</param>
    public ModalAnimationTrackingNavigation(INavigation inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    /// <summary>
    /// Gets the inner <see cref="INavigation"/> being wrapped.
    /// </summary>
    internal INavigation Inner => _inner;

    /// <summary>
    /// Retrieves and consumes the tracked animated flag for the given page.
    /// Falls back to <see cref="Shell.GetPresentationMode"/> if no tracked value exists
    /// (handles Shell modals and other bypass scenarios).
    /// </summary>
    /// <param name="page">The modal page to look up.</param>
    /// <returns><c>true</c> if the modal transition should be animated; otherwise <c>false</c>.</returns>
    internal static bool GetAnimated(Page page)
    {
        if (s_animatedFlags.TryGetValue(page, out var box))
        {
            s_animatedFlags.Remove(page);
            return box.Value;
        }

        // Fallback for Shell modals or cases where the tracker wasn't installed
        var mode = Shell.GetPresentationMode(page);
        return !mode.HasFlag(PresentationMode.NotAnimated);
    }

    /// <inheritdoc/>
    public IReadOnlyList<Page> ModalStack => _inner.ModalStack;

    /// <inheritdoc/>
    public IReadOnlyList<Page> NavigationStack => _inner.NavigationStack;

    /// <inheritdoc/>
    public Task PushModalAsync(Page page)
    {
        return PushModalAsync(page, true);
    }

    /// <inheritdoc/>
    public Task PushModalAsync(Page page, bool animated)
    {
        s_animatedFlags.AddOrUpdate(page, new StrongBox<bool>(animated));
        return _inner.PushModalAsync(page, animated);
    }

    /// <inheritdoc/>
    public Task<Page> PopModalAsync()
    {
        return PopModalAsync(true);
    }

    /// <inheritdoc/>
    public Task<Page> PopModalAsync(bool animated)
    {
        // Store the animated flag on the top modal page before popping
        var modalStack = _inner.ModalStack;
        if (modalStack.Count > 0)
        {
            var topModal = modalStack[modalStack.Count - 1];
            s_animatedFlags.AddOrUpdate(topModal, new StrongBox<bool>(animated));
        }

        return _inner.PopModalAsync(animated);
    }

    // All non-modal methods delegate directly to inner

    /// <inheritdoc/>
    public Task PushAsync(Page page) => _inner.PushAsync(page);

    /// <inheritdoc/>
    public Task PushAsync(Page page, bool animated) => _inner.PushAsync(page, animated);

    /// <inheritdoc/>
    public Task<Page> PopAsync() => _inner.PopAsync();

    /// <inheritdoc/>
    public Task<Page> PopAsync(bool animated) => _inner.PopAsync(animated);

    /// <inheritdoc/>
    public Task PopToRootAsync() => _inner.PopToRootAsync();

    /// <inheritdoc/>
    public Task PopToRootAsync(bool animated) => _inner.PopToRootAsync(animated);

    /// <inheritdoc/>
    public void InsertPageBefore(Page page, Page before) => _inner.InsertPageBefore(page, before);

    /// <inheritdoc/>
    public void RemovePage(Page page) => _inner.RemovePage(page);
}
