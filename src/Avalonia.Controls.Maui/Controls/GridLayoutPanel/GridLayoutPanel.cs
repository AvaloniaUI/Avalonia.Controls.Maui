using System;
using System.Globalization;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;

namespace Avalonia.Controls.Maui;

/// <summary>
/// A panel that arranges items in a grid with fixed columns/rows and spacing,
/// without stretching items to fill available space.
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

        // Calculate available size per cell
        var totalHorizontalSpacing = HorizontalSpacing * Math.Max(0, actualColumns - 1);
        var totalVerticalSpacing = VerticalSpacing * Math.Max(0, actualRows - 1);

        var cellWidth = double.IsInfinity(availableSize.Width)
            ? double.PositiveInfinity
            : Math.Max(0, (availableSize.Width - totalHorizontalSpacing) / actualColumns);

        var cellHeight = double.IsInfinity(availableSize.Height)
            ? double.PositiveInfinity
            : Math.Max(0, (availableSize.Height - totalVerticalSpacing) / actualRows);

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

        // Calculate total size needed
        var totalWidth = (maxChildWidth * actualColumns) + totalHorizontalSpacing;
        var totalHeight = (maxChildHeight * actualRows) + totalVerticalSpacing;

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

        // Calculate cell size from children's desired sizes
        double maxChildWidth = 0;
        double maxChildHeight = 0;

        foreach (var child in children)
        {
            maxChildWidth = Math.Max(maxChildWidth, child.DesiredSize.Width);
            maxChildHeight = Math.Max(maxChildHeight, child.DesiredSize.Height);
        }

        var cellWidth = maxChildWidth;
        var cellHeight = maxChildHeight;

        // Arrange children in grid
        for (int i = 0; i < children.Count; i++)
        {
            int row, col;

            if (Orientation == Orientation.Vertical)
            {
                // Vertical orientation: fill down columns first (top to bottom, then left to right)
                col = i / actualRows;
                row = i % actualRows;
            }
            else
            {
                // Horizontal orientation: fill across rows first (left to right, then top to bottom)
                row = i / actualColumns;
                col = i % actualColumns;
            }

            var x = col * (cellWidth + HorizontalSpacing);
            var y = row * (cellHeight + VerticalSpacing);

            children[i].Arrange(new Rect(x, y, cellWidth, cellHeight));
        }

        return finalSize;
    }
}