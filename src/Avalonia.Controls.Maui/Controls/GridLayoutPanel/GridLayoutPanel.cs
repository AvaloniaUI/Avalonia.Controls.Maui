using Avalonia.Layout;
using Avalonia.VisualTree;

namespace Avalonia.Controls.Maui;

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

    private double GetWidthConstraint()
    {
        var collectionView = this.FindAncestorOfType<MauiCollectionView>();
        if (collectionView != null)
        {
            var width = collectionView.Width;
            if (!double.IsNaN(width) && width > 0)
                return width;
            var boundsWidth = collectionView.Bounds.Width;
            if (boundsWidth > 0 && !double.IsInfinity(boundsWidth) && !double.IsNaN(boundsWidth))
                return boundsWidth;
        }

        var scrollViewer = this.FindAncestorOfType<ScrollViewer>();
        if (scrollViewer != null)
        {
            var viewportWidth = scrollViewer.Viewport.Width;
            if (viewportWidth > 0 && !double.IsInfinity(viewportWidth) && !double.IsNaN(viewportWidth))
                return viewportWidth;
            var boundsWidth = scrollViewer.Bounds.Width;
            if (boundsWidth > 0 && !double.IsInfinity(boundsWidth) && !double.IsNaN(boundsWidth))
                return boundsWidth;
        }

        return double.PositiveInfinity;
    }

    private double GetHeightConstraint()
    {
        var collectionView = this.FindAncestorOfType<MauiCollectionView>();
        if (collectionView != null)
        {
            var height = collectionView.Height;
            if (!double.IsNaN(height) && height > 0)
                return height;
            var boundsHeight = collectionView.Bounds.Height;
            if (boundsHeight > 0 && !double.IsInfinity(boundsHeight) && !double.IsNaN(boundsHeight))
                return boundsHeight;
        }

        var scrollViewer = this.FindAncestorOfType<ScrollViewer>();
        if (scrollViewer != null)
        {
            var viewportHeight = scrollViewer.Viewport.Height;
            if (viewportHeight > 0 && !double.IsInfinity(viewportHeight) && !double.IsNaN(viewportHeight))
                return viewportHeight;
            var boundsHeight = scrollViewer.Bounds.Height;
            if (boundsHeight > 0 && !double.IsInfinity(boundsHeight) && !double.IsNaN(boundsHeight))
                return boundsHeight;
        }

        return double.PositiveInfinity;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var children = Children;
        if (children.Count == 0)
            return new Size(0, 0);

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

        var totalHorizontalSpacing = HorizontalSpacing * Math.Max(0, actualColumns - 1);
        var totalVerticalSpacing = VerticalSpacing * Math.Max(0, actualRows - 1);

        var effectiveWidth = availableSize.Width;
        if (double.IsInfinity(effectiveWidth))
            effectiveWidth = GetWidthConstraint();

        var effectiveHeight = availableSize.Height;
        if (double.IsInfinity(effectiveHeight))
            effectiveHeight = GetHeightConstraint();

        double cellWidth, cellHeight;

        if (Orientation == Orientation.Vertical)
        {
            cellWidth = double.IsInfinity(effectiveWidth) || effectiveWidth <= 0
                ? double.PositiveInfinity
                : Math.Max(0, (effectiveWidth - totalHorizontalSpacing) / actualColumns);
            cellHeight = double.PositiveInfinity;
        }
        else
        {
            cellHeight = double.IsInfinity(effectiveHeight) || effectiveHeight <= 0
                ? double.PositiveInfinity
                : Math.Max(0, (effectiveHeight - totalVerticalSpacing) / actualRows);
            cellWidth = double.PositiveInfinity;
        }

        var cellSize = new Size(cellWidth, cellHeight);

        double maxChildWidth = 0;
        double maxChildHeight = 0;

        foreach (var child in children)
        {
            child.Measure(cellSize);
            maxChildWidth = Math.Max(maxChildWidth, child.DesiredSize.Width);
            maxChildHeight = Math.Max(maxChildHeight, child.DesiredSize.Height);
        }

        double totalWidth, totalHeight;

        if (Orientation == Orientation.Vertical)
        {
            totalWidth = !double.IsInfinity(effectiveWidth) && effectiveWidth > 0
                ? effectiveWidth
                : (maxChildWidth * actualColumns) + totalHorizontalSpacing;
            totalHeight = (maxChildHeight * actualRows) + totalVerticalSpacing;
        }
        else
        {
            totalHeight = !double.IsInfinity(effectiveHeight) && effectiveHeight > 0
                ? effectiveHeight
                : (maxChildHeight * actualRows) + totalVerticalSpacing;
            totalWidth = (maxChildWidth * actualColumns) + totalHorizontalSpacing;
        }

        return new Size(totalWidth, totalHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var children = Children;
        if (children.Count == 0)
            return finalSize;

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

        var totalHorizontalSpacing = HorizontalSpacing * Math.Max(0, actualColumns - 1);
        var totalVerticalSpacing = VerticalSpacing * Math.Max(0, actualRows - 1);

        var effectiveWidth = finalSize.Width;
        if (double.IsInfinity(effectiveWidth) || effectiveWidth <= 0)
            effectiveWidth = GetWidthConstraint();

        var effectiveHeight = finalSize.Height;
        if (double.IsInfinity(effectiveHeight) || effectiveHeight <= 0)
            effectiveHeight = GetHeightConstraint();

        double maxChildWidth = 0;
        double maxChildHeight = 0;
        foreach (var child in children)
        {
            maxChildWidth = Math.Max(maxChildWidth, child.DesiredSize.Width);
            maxChildHeight = Math.Max(maxChildHeight, child.DesiredSize.Height);
        }

        double cellWidth, cellHeight;

        if (Orientation == Orientation.Vertical)
        {
            cellWidth = double.IsInfinity(effectiveWidth) || effectiveWidth <= 0
                ? maxChildWidth
                : Math.Max(0, (effectiveWidth - totalHorizontalSpacing) / actualColumns);
            cellHeight = maxChildHeight;
        }
        else
        {
            cellHeight = double.IsInfinity(effectiveHeight) || effectiveHeight <= 0
                ? maxChildHeight
                : Math.Max(0, (effectiveHeight - totalVerticalSpacing) / actualRows);
            cellWidth = maxChildWidth;
        }

        for (int i = 0; i < children.Count; i++)
        {
            int row, col;

            if (Orientation == Orientation.Vertical)
            {
                row = i / actualColumns;
                col = i % actualColumns;
            }
            else
            {
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
