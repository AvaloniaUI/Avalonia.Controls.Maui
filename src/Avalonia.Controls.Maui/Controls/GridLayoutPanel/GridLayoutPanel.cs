using System;
using System.Globalization;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace Avalonia.Controls.Maui;

/// <summary>
/// A panel that arranges items in a grid with fixed columns/rows and spacing.
/// Items stretch to fill their cells, matching MAUI's GridItemsLayout behavior.
/// </summary>
internal class GridLayoutPanel : Panel
{
    public static readonly StyledProperty<int> ColumnsProperty =
        AvaloniaProperty.Register<GridLayoutPanel, int>(nameof(Columns), 1);

    public static readonly StyledProperty<int> RowsProperty =
        AvaloniaProperty.Register<GridLayoutPanel, int>(nameof(Rows), 0);

    public static readonly StyledProperty<double> HorizontalSpacingProperty =
        AvaloniaProperty.Register<GridLayoutPanel, double>(nameof(HorizontalSpacing), 0.0);

    public static readonly StyledProperty<double> VerticalSpacingProperty =
        AvaloniaProperty.Register<GridLayoutPanel, double>(nameof(VerticalSpacing), 0.0);

    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<GridLayoutPanel, Orientation>(nameof(Orientation), Orientation.Vertical);

    public int Columns
    {
        get => GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    public int Rows
    {
        get => GetValue(RowsProperty);
        set => SetValue(RowsProperty, value);
    }

    public double HorizontalSpacing
    {
        get => GetValue(HorizontalSpacingProperty);
        set => SetValue(HorizontalSpacingProperty, value);
    }

    public double VerticalSpacing
    {
        get => GetValue(VerticalSpacingProperty);
        set => SetValue(VerticalSpacingProperty, value);
    }

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    static GridLayoutPanel()
    {
        AffectsMeasure<GridLayoutPanel>(ColumnsProperty, RowsProperty, HorizontalSpacingProperty, VerticalSpacingProperty, OrientationProperty);
    }

    /// <summary>
    /// Gets the width constraint from the CollectionView or ScrollViewer ancestor.
    /// This is necessary because when inside a ScrollViewer, the available width can be infinite,
    /// but we need to constrain cells to the actual viewport width.
    /// </summary>
    private double GetWidthConstraint()
    {
        // First, try to find the CollectionView ancestor
        var collectionView = this.FindAncestorOfType<CollectionView>();
        if (collectionView != null)
        {
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

        // Fallback: try the ScrollViewer's viewport
        var scrollViewer = this.FindAncestorOfType<ScrollViewer>();
        if (scrollViewer != null)
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

        return double.PositiveInfinity;
    }

    /// <summary>
    /// Gets the height constraint from the CollectionView or ScrollViewer ancestor.
    /// </summary>
    private double GetHeightConstraint()
    {
        // First, try to find the CollectionView ancestor
        var collectionView = this.FindAncestorOfType<CollectionView>();
        if (collectionView != null)
        {
            var height = collectionView.Height;
            if (!double.IsNaN(height) && height > 0)
            {
                return height;
            }
            var boundsHeight = collectionView.Bounds.Height;
            if (boundsHeight > 0 && !double.IsInfinity(boundsHeight) && !double.IsNaN(boundsHeight))
            {
                return boundsHeight;
            }
        }

        // Fallback: try the ScrollViewer's viewport
        var scrollViewer = this.FindAncestorOfType<ScrollViewer>();
        if (scrollViewer != null)
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

        return double.PositiveInfinity;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var children = Children;
        if (children.Count == 0)
            return new Size(0, 0);

        // Determine actual columns and rows based on orientation and child count
        int actualColumns = Columns;
        int actualRows = Rows;

        if (Orientation == Orientation.Vertical)
        {
            // Vertical: fixed columns, rows calculated
            if (actualColumns <= 0) actualColumns = 1;
            actualRows = (int)Math.Ceiling((double)children.Count / actualColumns);
        }
        else
        {
            // Horizontal: fixed rows, columns calculated
            if (actualRows <= 0) actualRows = 1;
            actualColumns = (int)Math.Ceiling((double)children.Count / actualRows);
        }

        // Calculate spacing
        var totalHorizontalSpacing = HorizontalSpacing * Math.Max(0, actualColumns - 1);
        var totalVerticalSpacing = VerticalSpacing * Math.Max(0, actualRows - 1);

        // If available width is infinite (inside ScrollViewer), get constraint from ancestor
        var effectiveWidth = availableSize.Width;
        if (double.IsInfinity(effectiveWidth))
        {
            effectiveWidth = GetWidthConstraint();
        }

        // If available height is infinite (inside ScrollViewer), get constraint from ancestor
        var effectiveHeight = availableSize.Height;
        if (double.IsInfinity(effectiveHeight))
        {
            effectiveHeight = GetHeightConstraint();
        }

        // Calculate cell size - if we have a constraint, use it; otherwise use infinite
        var cellWidth = double.IsInfinity(effectiveWidth) || effectiveWidth <= 0
            ? double.PositiveInfinity
            : Math.Max(0, (effectiveWidth - totalHorizontalSpacing) / actualColumns);

        var cellHeight = double.IsInfinity(effectiveHeight) || effectiveHeight <= 0
            ? double.PositiveInfinity
            : Math.Max(0, (effectiveHeight - totalVerticalSpacing) / actualRows);

        var cellSize = new Size(cellWidth, cellHeight);

        // Measure all children with the cell size
        double maxChildWidth = 0;
        double maxChildHeight = 0;

        foreach (var child in children)
        {
            child.Measure(cellSize);
            maxChildWidth = Math.Max(maxChildWidth, child.DesiredSize.Width);
            maxChildHeight = Math.Max(maxChildHeight, child.DesiredSize.Height);
        }

        // Calculate total size
        double totalWidth, totalHeight;

        if (Orientation == Orientation.Vertical)
        {
            // For vertical grid: width is constrained, height grows with content
            if (!double.IsInfinity(effectiveWidth) && effectiveWidth > 0)
            {
                totalWidth = effectiveWidth;
            }
            else
            {
                // Fallback: use children's desired widths
                totalWidth = (maxChildWidth * actualColumns) + totalHorizontalSpacing;
            }
            totalHeight = (maxChildHeight * actualRows) + totalVerticalSpacing;
        }
        else
        {
            // For horizontal grid: height is constrained, width grows with content
            if (!double.IsInfinity(effectiveHeight) && effectiveHeight > 0)
            {
                totalHeight = effectiveHeight;
            }
            else
            {
                // Fallback: use children's desired heights
                totalHeight = (maxChildHeight * actualRows) + totalVerticalSpacing;
            }
            totalWidth = (maxChildWidth * actualColumns) + totalHorizontalSpacing;
        }

        return new Size(totalWidth, totalHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var children = Children;
        if (children.Count == 0)
            return finalSize;

        // Determine actual columns and rows
        int actualColumns = Columns;
        int actualRows = Rows;

        if (Orientation == Orientation.Vertical)
        {
            if (actualColumns <= 0) actualColumns = 1;
            actualRows = (int)Math.Ceiling((double)children.Count / actualColumns);
        }
        else
        {
            if (actualRows <= 0) actualRows = 1;
            actualColumns = (int)Math.Ceiling((double)children.Count / actualRows);
        }

        // Calculate cell size from finalSize to ensure items fill the available space
        // This matches MAUI's behavior where grid items stretch to fill their cells
        var totalHorizontalSpacing = HorizontalSpacing * Math.Max(0, actualColumns - 1);
        var totalVerticalSpacing = VerticalSpacing * Math.Max(0, actualRows - 1);

        // Get effective dimensions, falling back to ancestor constraints if finalSize is problematic
        var effectiveWidth = finalSize.Width;
        if (double.IsInfinity(effectiveWidth) || effectiveWidth <= 0)
        {
            effectiveWidth = GetWidthConstraint();
        }

        var effectiveHeight = finalSize.Height;
        if (double.IsInfinity(effectiveHeight) || effectiveHeight <= 0)
        {
            effectiveHeight = GetHeightConstraint();
        }

        var cellWidth = double.IsInfinity(effectiveWidth) || effectiveWidth <= 0
            ? children.Count > 0 ? children[0].DesiredSize.Width : 0
            : Math.Max(0, (effectiveWidth - totalHorizontalSpacing) / actualColumns);

        var cellHeight = double.IsInfinity(effectiveHeight) || effectiveHeight <= 0
            ? children.Count > 0 ? children[0].DesiredSize.Height : 0
            : Math.Max(0, (effectiveHeight - totalVerticalSpacing) / actualRows);

        // Arrange children in grid
        for (int i = 0; i < children.Count; i++)
        {
            int row, col;

            if (Orientation == Orientation.Vertical)
            {
                // Vertical orientation: fill across rows first, then down (left to right, then top to bottom)
                // This matches MAUI's GridItemsLayout with Vertical orientation
                row = i / actualColumns;
                col = i % actualColumns;
            }
            else
            {
                // Horizontal orientation: fill down columns first, then across (top to bottom, then left to right)
                col = i / actualRows;
                row = i % actualRows;
            }

            var x = col * (cellWidth + HorizontalSpacing);
            var y = row * (cellHeight + VerticalSpacing);

            children[i].Arrange(new Rect(x, y, cellWidth, cellHeight));
        }

        return finalSize;
    }
}