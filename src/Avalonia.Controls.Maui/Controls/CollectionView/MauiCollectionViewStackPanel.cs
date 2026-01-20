using Avalonia.Layout;
using Avalonia.VisualTree;

namespace Avalonia.Controls.Maui;

/// <summary>
/// A StackPanel variant for CollectionView that constrains children to the cross-axis dimension.
/// In a horizontal orientation, children are constrained to the panel's height.
/// In a vertical orientation, children are constrained to the panel's width.
/// This matches MAUI's CollectionView behavior where items fill the cross-axis.
/// </summary>
internal class MauiCollectionViewStackPanel : Panel
{
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<MauiCollectionViewStackPanel, Orientation>(nameof(Orientation), Orientation.Vertical);

    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<MauiCollectionViewStackPanel, double>(nameof(Spacing), 0.0);

    /// <summary>
    /// Gets or sets the orientation of the panel.
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// Gets or sets the spacing between items.
    /// </summary>
    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    static MauiCollectionViewStackPanel()
    {
        AffectsMeasure<MauiCollectionViewStackPanel>(OrientationProperty, SpacingProperty);
    }

    /// <summary>
    /// Gets the cross-axis constraint by looking up the visual tree for the CollectionView
    /// and using its explicit Height/Width or Bounds.
    /// </summary>
    private double GetCrossAxisConstraint(bool isHorizontal)
    {
        // First, try to find the CollectionView ancestor (our custom Avalonia control)
        var collectionView = this.FindAncestorOfType<MauiCollectionView>();
        if (collectionView != null)
        {
            if (isHorizontal)
            {
                // For horizontal layout, use CollectionView's height
                // Check explicit Height first (set via HeightRequest mapping)
                var height = collectionView.Height;
                if (!double.IsNaN(height) && height > 0)
                {
                    return height;
                }
                // Then check Bounds
                var boundsHeight = collectionView.Bounds.Height;
                if (boundsHeight > 0 && !double.IsInfinity(boundsHeight) && !double.IsNaN(boundsHeight))
                {
                    return boundsHeight;
                }
            }
            else
            {
                // For vertical layout, use CollectionView's width
                var width = collectionView.Width;
                if (!double.IsNaN(width) && width > 0)
                {
                    return width;
                }
                var boundsWidth = collectionView.Bounds.Width;
                if (boundsWidth > 0 && !double.IsInfinity(boundsWidth) && !double.IsNaN(boundsWidth))
                {
                    return boundsWidth;
                }
            }
        }

        // Fallback: try the ScrollViewer's viewport
        var scrollViewer = this.FindAncestorOfType<ScrollViewer>();
        if (scrollViewer != null)
        {
            if (isHorizontal)
            {
                var viewportHeight = scrollViewer.Viewport.Height;
                if (viewportHeight > 0 && !double.IsInfinity(viewportHeight) && !double.IsNaN(viewportHeight))
                {
                    return viewportHeight;
                }
                var boundsHeight = scrollViewer.Bounds.Height;
                if (boundsHeight > 0 && !double.IsInfinity(boundsHeight) && !double.IsNaN(boundsHeight))
                {
                    return boundsHeight;
                }
            }
            else
            {
                var viewportWidth = scrollViewer.Viewport.Width;
                if (viewportWidth > 0 && !double.IsInfinity(viewportWidth) && !double.IsNaN(viewportWidth))
                {
                    return viewportWidth;
                }
                var boundsWidth = scrollViewer.Bounds.Width;
                if (boundsWidth > 0 && !double.IsInfinity(boundsWidth) && !double.IsNaN(boundsWidth))
                {
                    return boundsWidth;
                }
            }
        }

        return double.PositiveInfinity;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var children = Children;
        if (children.Count == 0)
            return new Size(0, 0);

        var isHorizontal = Orientation == Orientation.Horizontal;
        var spacing = Spacing;

        double totalMainAxis = 0;
        double maxCrossAxis = 0;

        // For horizontal: cross-axis is height, main-axis is width
        // For vertical: cross-axis is width, main-axis is height
        var crossAxisConstraint = isHorizontal ? availableSize.Height : availableSize.Width;

        // If the cross-axis constraint is infinite (we're inside a ScrollViewer),
        // try to get the constraint from the CollectionView or ScrollViewer
        if (double.IsInfinity(crossAxisConstraint))
        {
            crossAxisConstraint = GetCrossAxisConstraint(isHorizontal);
        }

        foreach (var child in children)
        {
            // Measure child with the cross-axis constrained to the available size
            // but main-axis unconstrained (or constrained to remaining space)
            Size childConstraint;
            
            if (isHorizontal)
            {
                // Horizontal: constrain height to available, width unconstrained
                childConstraint = new Size(double.PositiveInfinity, crossAxisConstraint);
            }
            else
            {
                // Vertical: constrain width to available, height unconstrained
                childConstraint = new Size(crossAxisConstraint, double.PositiveInfinity);
            }

            child.Measure(childConstraint);

            if (isHorizontal)
            {
                totalMainAxis += child.DesiredSize.Width;
                maxCrossAxis = Math.Max(maxCrossAxis, child.DesiredSize.Height);
            }
            else
            {
                totalMainAxis += child.DesiredSize.Height;
                maxCrossAxis = Math.Max(maxCrossAxis, child.DesiredSize.Width);
            }
        }

        // Add spacing between items
        if (children.Count > 1)
        {
            totalMainAxis += spacing * (children.Count - 1);
        }

        // Return desired size
        if (isHorizontal)
        {
            return new Size(totalMainAxis, maxCrossAxis);
        }
        else
        {
            return new Size(maxCrossAxis, totalMainAxis);
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var children = Children;
        if (children.Count == 0)
            return finalSize;

        var isHorizontal = Orientation == Orientation.Horizontal;
        var spacing = Spacing;

        double currentPosition = 0;

        // The cross-axis size is the full available size in that dimension
        var crossAxisSize = isHorizontal ? finalSize.Height : finalSize.Width;

        // If cross-axis size is still not determined, try to get it from CollectionView or ScrollViewer
        if (crossAxisSize <= 0 || double.IsInfinity(crossAxisSize))
        {
            var constraint = GetCrossAxisConstraint(isHorizontal);
            if (!double.IsInfinity(constraint) && constraint > 0)
            {
                crossAxisSize = constraint;
            }
        }

        foreach (var child in children)
        {
            if (isHorizontal)
            {
                // Horizontal: arrange at current X position, full height
                var childWidth = child.DesiredSize.Width;
                child.Arrange(new Rect(currentPosition, 0, childWidth, crossAxisSize));
                currentPosition += childWidth + spacing;
            }
            else
            {
                // Vertical: arrange at current Y position, full width
                var childHeight = child.DesiredSize.Height;
                child.Arrange(new Rect(0, currentPosition, crossAxisSize, childHeight));
                currentPosition += childHeight + spacing;
            }
        }

        return finalSize;
    }
}
