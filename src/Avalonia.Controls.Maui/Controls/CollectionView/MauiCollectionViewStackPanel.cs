using Avalonia.Layout;
using Avalonia.VisualTree;

namespace Avalonia.Controls.Maui;

internal class MauiCollectionViewStackPanel : Panel
{
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<MauiCollectionViewStackPanel, Orientation>(nameof(Orientation), Orientation.Vertical);

    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<MauiCollectionViewStackPanel, double>(nameof(Spacing), 0.0);

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    static MauiCollectionViewStackPanel()
    {
        AffectsMeasure<MauiCollectionViewStackPanel>(OrientationProperty, SpacingProperty);
    }

    private double GetCrossAxisConstraint(bool isHorizontal)
    {
        var collectionView = this.FindAncestorOfType<MauiCollectionView>();
        if (collectionView != null)
        {
            if (isHorizontal)
            {
                var height = collectionView.Height;
                if (!double.IsNaN(height) && height > 0)
                    return height;
                var boundsHeight = collectionView.Bounds.Height;
                if (boundsHeight > 0 && !double.IsInfinity(boundsHeight) && !double.IsNaN(boundsHeight))
                    return boundsHeight;
            }
            else
            {
                var width = collectionView.Width;
                if (!double.IsNaN(width) && width > 0)
                    return width;
                var boundsWidth = collectionView.Bounds.Width;
                if (boundsWidth > 0 && !double.IsInfinity(boundsWidth) && !double.IsNaN(boundsWidth))
                    return boundsWidth;
            }
        }

        var scrollViewer = this.FindAncestorOfType<ScrollViewer>();
        if (scrollViewer != null)
        {
            if (isHorizontal)
            {
                var viewportHeight = scrollViewer.Viewport.Height;
                if (viewportHeight > 0 && !double.IsInfinity(viewportHeight) && !double.IsNaN(viewportHeight))
                    return viewportHeight;
                var boundsHeight = scrollViewer.Bounds.Height;
                if (boundsHeight > 0 && !double.IsInfinity(boundsHeight) && !double.IsNaN(boundsHeight))
                    return boundsHeight;
            }
            else
            {
                var viewportWidth = scrollViewer.Viewport.Width;
                if (viewportWidth > 0 && !double.IsInfinity(viewportWidth) && !double.IsNaN(viewportWidth))
                    return viewportWidth;
                var boundsWidth = scrollViewer.Bounds.Width;
                if (boundsWidth > 0 && !double.IsInfinity(boundsWidth) && !double.IsNaN(boundsWidth))
                    return boundsWidth;
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

        var crossAxisConstraint = isHorizontal ? availableSize.Height : availableSize.Width;

        if (double.IsInfinity(crossAxisConstraint))
        {
            crossAxisConstraint = GetCrossAxisConstraint(isHorizontal);
        }

        foreach (var child in children)
        {
            Size childConstraint;

            if (isHorizontal)
            {
                childConstraint = new Size(double.PositiveInfinity, crossAxisConstraint);
            }
            else
            {
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

        if (children.Count > 1)
        {
            totalMainAxis += spacing * (children.Count - 1);
        }

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

        var crossAxisSize = isHorizontal ? finalSize.Height : finalSize.Width;

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
                var childWidth = child.DesiredSize.Width;
                child.Arrange(new Rect(currentPosition, 0, childWidth, crossAxisSize));
                currentPosition += childWidth + spacing;
            }
            else
            {
                var childHeight = child.DesiredSize.Height;
                child.Arrange(new Rect(0, currentPosition, crossAxisSize, childHeight));
                currentPosition += childHeight + spacing;
            }
        }

        return finalSize;
    }
}
