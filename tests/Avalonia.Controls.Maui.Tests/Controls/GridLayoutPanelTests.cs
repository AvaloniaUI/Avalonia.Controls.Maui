using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;

namespace Avalonia.Controls.Maui.Tests.Controls;

/// <summary>
/// Tests for GridLayoutPanel to ensure proper cell sizing and layout behavior.
/// These tests verify that GridLayoutPanel divides available space correctly among cells,
/// matching MAUI's GridItemsLayout behavior where items stretch to fill their cells.
/// </summary>
public class GridLayoutPanelTests
{
    /// <summary>
    /// Helper class that records the arrange rect it receives.
    /// </summary>
    private class ArrangeTrackingBorder : Border
    {
        public Rect LastArrangeRect { get; private set; }

        protected override Size ArrangeOverride(Size finalSize)
        {
            LastArrangeRect = new Rect(Bounds.Position, finalSize);
            return base.ArrangeOverride(finalSize);
        }
    }

    [AvaloniaFact(DisplayName = "GridLayoutPanel arranges items to fill available width with 2 columns")]
    public void GridLayoutPanel_ArrangesItemsToFillAvailableWidth_With2Columns()
    {
        // Arrange
        var panel = new GridLayoutPanel
        {
            Columns = 2,
            Orientation = Orientation.Vertical,
            HorizontalSpacing = 10,
            VerticalSpacing = 10
        };

        var children = new ArrangeTrackingBorder[4];
        for (int i = 0; i < 4; i++)
        {
            children[i] = new ArrangeTrackingBorder();
            panel.Children.Add(children[i]);
        }

        // Act - Measure with constrained width
        var availableSize = new Size(400, 400);
        panel.Measure(availableSize);
        panel.Arrange(new Rect(0, 0, 400, 400));

        // Assert - Each cell should be (400 - 10) / 2 = 195 wide
        var expectedCellWidth = (400 - 10) / 2.0; // 195

        // Verify children receive the correct cell width during arrange
        Assert.Equal(expectedCellWidth, children[0].LastArrangeRect.Width, 1);
        Assert.Equal(expectedCellWidth, children[1].LastArrangeRect.Width, 1);
    }

    [AvaloniaFact(DisplayName = "GridLayoutPanel cell width scales with container width")]
    public void GridLayoutPanel_CellWidthScalesWithContainerWidth()
    {
        // Arrange
        var panel = new GridLayoutPanel
        {
            Columns = 2,
            Orientation = Orientation.Vertical,
            HorizontalSpacing = 0,
            VerticalSpacing = 0
        };

        var child1 = new ArrangeTrackingBorder();
        var child2 = new ArrangeTrackingBorder();
        panel.Children.Add(child1);
        panel.Children.Add(child2);

        // Act - First layout at 200 width
        panel.Measure(new Size(200, 200));
        panel.Arrange(new Rect(0, 0, 200, 200));

        var cellWidthAt200 = child1.LastArrangeRect.Width;

        // Act - Second layout at 400 width
        panel.Measure(new Size(400, 200));
        panel.Arrange(new Rect(0, 0, 400, 200));

        var cellWidthAt400 = child1.LastArrangeRect.Width;

        // Assert - Cell width should double when container width doubles
        Assert.Equal(100, cellWidthAt200, 1); // 200 / 2 columns
        Assert.Equal(200, cellWidthAt400, 1); // 400 / 2 columns
    }

    [AvaloniaFact(DisplayName = "GridLayoutPanel respects horizontal spacing in cell width calculation")]
    public void GridLayoutPanel_RespectsHorizontalSpacingInCellWidth()
    {
        // Arrange
        var panel = new GridLayoutPanel
        {
            Columns = 2,
            Orientation = Orientation.Vertical,
            HorizontalSpacing = 20,
            VerticalSpacing = 0
        };

        var child1 = new ArrangeTrackingBorder();
        var child2 = new ArrangeTrackingBorder();
        panel.Children.Add(child1);
        panel.Children.Add(child2);

        // Act
        panel.Measure(new Size(200, 200));
        panel.Arrange(new Rect(0, 0, 200, 200));

        // Assert - Cell width should be (200 - 20 spacing) / 2 = 90
        var expectedCellWidth = (200 - 20) / 2.0;
        Assert.Equal(expectedCellWidth, child1.LastArrangeRect.Width, 1);
    }

    [AvaloniaFact(DisplayName = "GridLayoutPanel with 3 columns divides width correctly")]
    public void GridLayoutPanel_With3Columns_DividesWidthCorrectly()
    {
        // Arrange
        var panel = new GridLayoutPanel
        {
            Columns = 3,
            Orientation = Orientation.Vertical,
            HorizontalSpacing = 10,
            VerticalSpacing = 0
        };

        var children = new ArrangeTrackingBorder[3];
        for (int i = 0; i < 3; i++)
        {
            children[i] = new ArrangeTrackingBorder();
            panel.Children.Add(children[i]);
        }

        // Act
        var totalWidth = 320.0; // 320 = 3*100 + 2*10 spacing, so each cell = 100
        panel.Measure(new Size(totalWidth, 200));
        panel.Arrange(new Rect(0, 0, totalWidth, 200));

        // Assert - Cell width should be (320 - 20 total spacing) / 3 = 100
        var expectedCellWidth = (totalWidth - 20) / 3.0;

        foreach (var child in children)
        {
            Assert.Equal(expectedCellWidth, child.LastArrangeRect.Width, 1);
        }
    }

    [AvaloniaFact(DisplayName = "GridLayoutPanel horizontal orientation divides height correctly")]
    public void GridLayoutPanel_HorizontalOrientation_DividesHeightCorrectly()
    {
        // Arrange
        var panel = new GridLayoutPanel
        {
            Rows = 2,
            Orientation = Orientation.Horizontal,
            HorizontalSpacing = 0,
            VerticalSpacing = 10
        };

        var child1 = new ArrangeTrackingBorder();
        var child2 = new ArrangeTrackingBorder();
        panel.Children.Add(child1);
        panel.Children.Add(child2);

        // Act
        panel.Measure(new Size(200, 200));
        panel.Arrange(new Rect(0, 0, 200, 200));

        // Assert - Cell height should be (200 - 10 spacing) / 2 = 95
        var expectedCellHeight = (200 - 10) / 2.0;
        Assert.Equal(expectedCellHeight, child1.LastArrangeRect.Height, 1);
    }

    [AvaloniaFact(DisplayName = "GridLayoutPanel handles window resize by recalculating cell sizes")]
    public void GridLayoutPanel_HandlesResize_ByRecalculatingCellSizes()
    {
        // Arrange
        var panel = new GridLayoutPanel
        {
            Columns = 2,
            Orientation = Orientation.Vertical,
            HorizontalSpacing = 0,
            VerticalSpacing = 0
        };

        var children = new ArrangeTrackingBorder[4];
        for (int i = 0; i < 4; i++)
        {
            children[i] = new ArrangeTrackingBorder();
            panel.Children.Add(children[i]);
        }

        // Act - Initial layout at 300 width
        panel.Measure(new Size(300, 300));
        panel.Arrange(new Rect(0, 0, 300, 300));

        var initialCellWidth = children[0].LastArrangeRect.Width;
        Assert.Equal(150, initialCellWidth, 1); // 300 / 2

        // Act - Resize to 500 width
        panel.Measure(new Size(500, 300));
        panel.Arrange(new Rect(0, 0, 500, 300));

        var resizedCellWidth = children[0].LastArrangeRect.Width;

        // Assert - Cell width should update to reflect new container size
        Assert.Equal(250, resizedCellWidth, 1); // 500 / 2
    }

    [AvaloniaFact(DisplayName = "GridLayoutPanel children fill their allocated cell bounds")]
    public void GridLayoutPanel_ChildrenFillTheirAllocatedCellBounds()
    {
        // Arrange - This tests that children are given the full cell size to arrange into
        var panel = new GridLayoutPanel
        {
            Columns = 2,
            Orientation = Orientation.Vertical,
            HorizontalSpacing = 0,
            VerticalSpacing = 0
        };

        // Add children that can stretch
        var child1 = new Border { HorizontalAlignment = HorizontalAlignment.Stretch };
        var child2 = new Border { HorizontalAlignment = HorizontalAlignment.Stretch };

        panel.Children.Add(child1);
        panel.Children.Add(child2);

        // Act
        panel.Measure(new Size(400, 200));
        panel.Arrange(new Rect(0, 0, 400, 200));

        // Assert - Each child should be arranged with width = 200 (400 / 2)
        Assert.Equal(200, child1.Bounds.Width, 1);
        Assert.Equal(200, child2.Bounds.Width, 1);
    }
}
