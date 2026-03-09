using Avalonia.Controls;
using Microsoft.Maui;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Abstract base panel that bridges MAUI's cross-platform layout system with Avalonia's layout infrastructure.
/// </summary>
public abstract partial class MauiView : Panel, ICrossPlatformLayoutBacking, IVisualTreeElementProvidable
{
    bool _invalidateParentWhenMovedToWindow;
    double _lastMeasureHeight = double.NaN;
    double _lastMeasureWidth = double.NaN;

    WeakReference<IView>? _reference;
    WeakReference<ICrossPlatformLayout>? _crossPlatformLayoutReference;

    /// <summary>
    /// Gets or sets the MAUI view associated with this panel.
    /// </summary>
    public IView? View
    {
        get => _reference != null && _reference.TryGetTarget(out var v) ? v : null;
        set => _reference = value == null ? null : new(value);
    }

    bool HasFixedConstraints => CrossPlatformLayout is IConstrainedView { HasFixedConstraints: true };

    /// <summary>
    /// Gets or sets the cross-platform layout delegate used for measure and arrange passes.
    /// </summary>
    public ICrossPlatformLayout? CrossPlatformLayout
    {
        get => _crossPlatformLayoutReference != null && _crossPlatformLayoutReference.TryGetTarget(out var v) ? v : null;
        set => _crossPlatformLayoutReference = value == null ? null : new WeakReference<ICrossPlatformLayout>(value);
    }

    Microsoft.Maui.Graphics.Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
    {
        return CrossPlatformLayout?.CrossPlatformMeasure(widthConstraint, heightConstraint) ?? Microsoft.Maui.Graphics.Size.Zero;
    }

    Microsoft.Maui.Graphics.Size CrossPlatformArrange(Microsoft.Maui.Graphics.Rect bounds)
    {
        return CrossPlatformLayout?.CrossPlatformArrange(bounds) ?? Microsoft.Maui.Graphics.Size.Zero;
    }

    /// <summary>
    /// Checks whether the cached measure constraints match the specified width and height constraints.
    /// </summary>
    /// <param name="widthConstraint">The width constraint to compare against the cached value.</param>
    /// <param name="heightConstraint">The height constraint to compare against the cached value.</param>
    /// <returns><see langword="true"/> if the cached constraints match the specified values; otherwise, <see langword="false"/>.</returns>
    protected new bool IsMeasureValid(double widthConstraint, double heightConstraint)
    {
        return heightConstraint == _lastMeasureHeight && widthConstraint == _lastMeasureWidth;
    }

    bool HasBeenMeasured()
    {
        return !double.IsNaN(_lastMeasureWidth) && !double.IsNaN(_lastMeasureHeight);
    }

    /// <summary>
    /// Clears the cached measure constraints by resetting them to <see cref="double.NaN"/>.
    /// </summary>
    protected void InvalidateConstraintsCache()
    {
        _lastMeasureWidth = double.NaN;
        _lastMeasureHeight = double.NaN;
    }

    /// <summary>
    /// Stores the specified measure constraints for subsequent cache validation via <see cref="IsMeasureValid"/>.
    /// </summary>
    /// <param name="widthConstraint">The width constraint to cache.</param>
    /// <param name="heightConstraint">The height constraint to cache.</param>
    protected void CacheMeasureConstraints(double widthConstraint, double heightConstraint)
    {
        _lastMeasureWidth = widthConstraint;
        _lastMeasureHeight = heightConstraint;
    }

    /// <summary>
    /// Measures the view by delegating to the cross-platform layout system when available.
    /// </summary>
    /// <param name="availableSize">The available size that the parent element can allocate to this view.</param>
    /// <returns>The desired size computed by the cross-platform layout, or the base measurement if no cross-platform layout is set.</returns>
    protected override Size MeasureOverride(Size availableSize)
    {
        if (_crossPlatformLayoutReference == null)
        {
            return base.MeasureOverride(availableSize);
        }

        var widthConstraint = availableSize.Width;
        var heightConstraint = availableSize.Height;

        var crossPlatformSize = CrossPlatformMeasure(widthConstraint, heightConstraint);

        CacheMeasureConstraints(widthConstraint, heightConstraint);

        return new Size(crossPlatformSize.Width, crossPlatformSize.Height);
    }

    /// <summary>
    /// Arranges the view by delegating to the cross-platform layout system, re-measuring if constraints have changed.
    /// </summary>
    /// <param name="finalSize">The final area within the parent that this view should use to arrange itself and its children.</param>
    /// <returns>The actual size used after arrangement by the cross-platform layout, or the base arrangement if no cross-platform layout is set.</returns>
    protected override Size ArrangeOverride(Size finalSize)
    {
        if (_crossPlatformLayoutReference == null)
        {
            return base.ArrangeOverride(finalSize);
        }

        var bounds = new Microsoft.Maui.Graphics.Rect(0, 0, finalSize.Width, finalSize.Height);

        var widthConstraint = bounds.Width;
        var heightConstraint = bounds.Height;

        if (!IsMeasureValid(widthConstraint, heightConstraint) && !HasFixedConstraints ||
            !HasBeenMeasured() && HasFixedConstraints)
        {
            CrossPlatformMeasure(widthConstraint, heightConstraint);
            CacheMeasureConstraints(widthConstraint, heightConstraint);
        }

        var arrangedSize = CrossPlatformArrange(bounds);

        return new Size(arrangedSize.Width, arrangedSize.Height);
    }

    /// <summary>
    /// Gets the <see cref="IVisualTreeElement"/> associated with this view by matching against the current <see cref="View"/> or <see cref="CrossPlatformLayout"/>.
    /// </summary>
    /// <returns>The matching <see cref="IVisualTreeElement"/>, or <see langword="null"/> if no match is found.</returns>
    public IVisualTreeElement? GetElement()
    {
#if IOS || MACCATALYST || ANDROID || WINDOWS
        // On platform builds, we can't use IsThisMyPlatformView because it expects native views.
        // Instead, we check if the handler's platform view matches this MauiView.
        if (View is IVisualTreeElement viewElement)
        {
            if (IsAvaloniaViewMatch(viewElement))
            {
                return viewElement;
            }
        }

        if (CrossPlatformLayout is IVisualTreeElement layoutElement)
        {
            if (IsAvaloniaViewMatch(layoutElement))
            {
                return layoutElement;
            }
        }

        return null;
#else
        if (View is IVisualTreeElement viewElement &&
            viewElement.IsThisMyPlatformView(this))
        {
            return viewElement;
        }

        if (CrossPlatformLayout is IVisualTreeElement layoutElement &&
            layoutElement.IsThisMyPlatformView(this))
        {
            return layoutElement;
        }

        return null;
#endif
    }

#if IOS || MACCATALYST || ANDROID || WINDOWS
    /// <summary>
    /// Checks if this MauiView is the platform view for the given visual tree element.
    /// </summary>
    private bool IsAvaloniaViewMatch(IVisualTreeElement element)
    {
        // Check if the element has a handler with this as the platform view
        if (element is IView view && view.Handler?.PlatformView == this)
        {
            return true;
        }
        return false;
    }
#endif

    /// <summary>
    /// Schedules invalidation of ancestor layout measures when this view is next attached to the visual tree.
    /// </summary>
    public void InvalidateAncestorsMeasuresWhenMovedToWindow()
    {
        _invalidateParentWhenMovedToWindow = true;
    }

    /// <summary>
    /// Invalidates the measure of this view, clearing the constraints cache and optionally stopping propagation for views with fixed constraints.
    /// </summary>
    /// <param name="isPropagating">When <see langword="true"/>, indicates the invalidation is propagating up the tree and may be stopped if this view has fixed constraints.</param>
    /// <returns><see langword="true"/> if invalidation should continue propagating to ancestors; <see langword="false"/> if propagation was stopped due to fixed constraints.</returns>
    public bool InvalidateMeasure(bool isPropagating = false)
    {
        InvalidateConstraintsCache();
        base.InvalidateMeasure();

        if (isPropagating && HasFixedConstraints)
        {
            return false;
        }

        return true;
    }

    [UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Event used for lifecycle management")]
    EventHandler? _lifecycleEvent;

    /// <summary>
    /// Occurs when this view is attached to the visual tree. Shadows the base event to provide lifecycle notification.
    /// </summary>
    public new event EventHandler? AttachedToVisualTree
    {
        add => _lifecycleEvent += value;
        remove => _lifecycleEvent -= value;
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _lifecycleEvent?.Invoke(this, EventArgs.Empty);

        // Clear the measurement cache so the next ArrangeOverride will re-measure
        // via CrossPlatformMeasure instead of using stale cached constraints.
        // This is critical for NavigationPage pop transitions where the MauiView
        // is detached from the visual tree and re-attached.
        InvalidateConstraintsCache();

        if (_invalidateParentWhenMovedToWindow)
        {
            _invalidateParentWhenMovedToWindow = false;
            InvalidateMeasure();
        }
    }
}