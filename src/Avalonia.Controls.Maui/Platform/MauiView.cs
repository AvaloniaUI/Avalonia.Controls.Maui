using Avalonia.Controls;
using Microsoft.Maui;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Avalonia.Controls.Maui.Platform;

public abstract partial class MauiView : Panel, ICrossPlatformLayoutBacking, IVisualTreeElementProvidable
{
    bool _invalidateParentWhenMovedToWindow;
    double _lastMeasureHeight = double.NaN;
    double _lastMeasureWidth = double.NaN;

    WeakReference<IView>? _reference;
    WeakReference<ICrossPlatformLayout>? _crossPlatformLayoutReference;

    public IView? View
    {
        get => _reference != null && _reference.TryGetTarget(out var v) ? v : null;
        set => _reference = value == null ? null : new(value);
    }

    bool HasFixedConstraints => CrossPlatformLayout is IConstrainedView { HasFixedConstraints: true };

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

    protected new bool IsMeasureValid(double widthConstraint, double heightConstraint)
    {
        return heightConstraint == _lastMeasureHeight && widthConstraint == _lastMeasureWidth;
    }

    bool HasBeenMeasured()
    {
        return !double.IsNaN(_lastMeasureWidth) && !double.IsNaN(_lastMeasureHeight);
    }

    protected void InvalidateConstraintsCache()
    {
        _lastMeasureWidth = double.NaN;
        _lastMeasureHeight = double.NaN;
    }

    protected void CacheMeasureConstraints(double widthConstraint, double heightConstraint)
    {
        _lastMeasureWidth = widthConstraint;
        _lastMeasureHeight = heightConstraint;
    }

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

    public IVisualTreeElement? GetElement()
    {
#if IOS || MACCATALYST || ANDROID
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

#if IOS || MACCATALYST || ANDROID
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

    public void InvalidateAncestorsMeasuresWhenMovedToWindow()
    {
        _invalidateParentWhenMovedToWindow = true;
    }

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
    public new event EventHandler? AttachedToVisualTree
    {
        add => _lifecycleEvent += value;
        remove => _lifecycleEvent -= value;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _lifecycleEvent?.Invoke(this, EventArgs.Empty);

        if (_invalidateParentWhenMovedToWindow)
        {
            _invalidateParentWhenMovedToWindow = false;
            InvalidateMeasure();
        }
    }
}