using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using MauiLayout = Microsoft.Maui.Controls.Layout;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class CustomLayoutRenderTests : RenderTestBase
{
    /// <summary>
    /// Verifies that a custom layout positions children correctly even when
    /// ArrangeChildren returns a size smaller than the allocated space.
    /// This is a regression test for an issue where LayoutPanel.ArrangeOverride
    /// returned the cross-platform arrange result, causing Avalonia's ArrangeCore
    /// to center the zero-sized panel and shift all children.
    /// Expected: Red bar at top, green bar at bottom, blue bar at left, yellow bar at right.
    /// </summary>
    [AvaloniaFact]
    public async Task Render_CustomLayout_DockPositions()
    {
        var layout = new TestDockLayout
        {
            BackgroundColor = Colors.White,
            WidthRequest = 200,
            HeightRequest = 200,
            Children =
            {
                new BoxView { Color = Colors.Red, HeightRequest = 30 },
                new BoxView { Color = Colors.Green, HeightRequest = 30 },
                new BoxView { Color = Colors.Blue, WidthRequest = 30 },
                new BoxView { Color = Colors.Yellow, WidthRequest = 30 },
            }
        };

        await RenderToFile(layout);
        CompareImages();
    }
}

/// <summary>
/// A custom dock-style layout that places children at the top, bottom, left, and right edges.
/// Intentionally returns Size.Zero from ArrangeChildren to exercise the LayoutPanel fix
/// (matching the pattern from the Controls.Sample DockLayout).
/// </summary>
file class TestDockLayout : MauiLayout
{
    protected override ILayoutManager CreateLayoutManager()
    {
        return new TestDockLayoutManager(this);
    }

    private class TestDockLayoutManager : LayoutManager
    {
        private readonly TestDockLayout _layout;

        public TestDockLayoutManager(TestDockLayout layout) : base(layout)
        {
            _layout = layout;
        }

        public override Microsoft.Maui.Graphics.Size Measure(double widthConstraint, double heightConstraint)
        {
            foreach (IView child in _layout)
            {
                child.Measure(widthConstraint, heightConstraint);
            }

            return new Microsoft.Maui.Graphics.Size(widthConstraint, heightConstraint);
        }

        public override Microsoft.Maui.Graphics.Size ArrangeChildren(Microsoft.Maui.Graphics.Rect bounds)
        {
            var x = bounds.X;
            var y = bounds.Y;
            var width = bounds.Width;
            var height = bounds.Height;

            // Child 0: Top (red) - full width bar at top
            if (_layout.Count > 0)
            {
                var child = (IView)_layout[0];
                var childHeight = child.DesiredSize.Height;
                child.Arrange(new Microsoft.Maui.Graphics.Rect(x, y, width, childHeight));
                y += childHeight;
                height -= childHeight;
            }

            // Child 1: Bottom (green) - full width bar at bottom
            if (_layout.Count > 1)
            {
                var child = (IView)_layout[1];
                var childHeight = child.DesiredSize.Height;
                child.Arrange(new Microsoft.Maui.Graphics.Rect(x, y + height - childHeight, width, childHeight));
                height -= childHeight;
            }

            // Child 2: Left (blue) - remaining height bar at left
            if (_layout.Count > 2)
            {
                var child = (IView)_layout[2];
                var childWidth = child.DesiredSize.Width;
                child.Arrange(new Microsoft.Maui.Graphics.Rect(x, y, childWidth, height));
                x += childWidth;
                width -= childWidth;
            }

            // Child 3: Right (yellow) - remaining height bar at right
            if (_layout.Count > 3)
            {
                var child = (IView)_layout[3];
                var childWidth = child.DesiredSize.Width;
                child.Arrange(new Microsoft.Maui.Graphics.Rect(x + width - childWidth, y, childWidth, height));
            }

            // Intentionally return Size.Zero, mimicking the buggy DockLayout sample.
            // Before the fix, this caused LayoutPanel to report a zero size to Avalonia,
            // which then centered the panel and shifted all children to the middle.
            return Microsoft.Maui.Graphics.Size.Zero;
        }
    }
}
