using Avalonia.Controls.Maui.Platform;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;

namespace Avalonia.Controls.Maui.Tests.Platform;

public class PlatformViewMeasureTests
{
    [AvaloniaFact(DisplayName = "PlatformGraphicsView With Start Alignment Keeps Arranged Frame Size")]
    public void PlatformGraphicsView_With_Start_Alignment_Keeps_Arranged_Frame_Size()
    {
        var view = new PlatformGraphicsView
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
        };

        view.Measure(new Size(100, 48));
        view.Arrange(new Rect(0, 0, 100, 48));

        Assert.Equal(100, view.Bounds.Width);
        Assert.Equal(48, view.Bounds.Height);
    }

    [AvaloniaFact(DisplayName = "PlatformTouchGraphicsView With Start Alignment Keeps Arranged Frame Size")]
    public void PlatformTouchGraphicsView_With_Start_Alignment_Keeps_Arranged_Frame_Size()
    {
        var view = new PlatformTouchGraphicsView
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
        };

        view.Measure(new Size(100, 48));
        view.Arrange(new Rect(0, 0, 100, 48));

        Assert.Equal(100, view.Bounds.Width);
        Assert.Equal(48, view.Bounds.Height);
    }

    [AvaloniaFact(DisplayName = "ProgressRingVisual With Center Alignment Keeps Arranged Frame Size")]
    public void ProgressRingVisual_With_Center_Alignment_Keeps_Arranged_Frame_Size()
    {
        var view = new ProgressRingVisual
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };

        view.Measure(new Size(32, 32));
        view.Arrange(new Rect(0, 0, 32, 32));

        Assert.Equal(32, view.Bounds.Width);
        Assert.Equal(32, view.Bounds.Height);
    }

    [AvaloniaFact(DisplayName = "PlatformGraphicsView Measures Zero Under Unbounded Constraint")]
    public void PlatformGraphicsView_Measures_Zero_Under_Unbounded_Constraint()
    {
        var view = new PlatformGraphicsView();

        view.Measure(Size.Infinity);

        Assert.Equal(default, view.DesiredSize);
    }
}
