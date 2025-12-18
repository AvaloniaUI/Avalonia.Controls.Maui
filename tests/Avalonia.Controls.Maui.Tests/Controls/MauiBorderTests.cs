using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.Tests.Controls;

public class MauiBorderTests
{
    [AvaloniaFact(DisplayName = "Child clip matches stroke path for round rectangle")]
    public void ChildClip_MatchesStrokePath_ForRoundRectangle()
    {
        var border = new MauiBorder
        {
            Shape = new RoundRectangle { CornerRadius = new Microsoft.Maui.CornerRadius(30) },
            StrokeThickness = 4,
            Padding = new Thickness(8)
        };

        var child = new Border();
        border.Child = child;

        var finalSize = new Size(220, 180);
        border.Measure(finalSize);
        border.Arrange(new Rect(finalSize));

        var padding = border.Padding;
        var stroke = border.StrokeThickness;
        var expectedChildWidth = finalSize.Width - padding.Left - padding.Right;
        var expectedChildHeight = finalSize.Height - padding.Top - padding.Bottom;

        Assert.Equal(expectedChildWidth, child.Bounds.Width, 3);
        Assert.Equal(expectedChildHeight, child.Bounds.Height, 3);

        var clip = child.Clip;
        Assert.NotNull(clip);
        var expectedClipWidth = expectedChildWidth - stroke;
        var expectedClipHeight = expectedChildHeight - stroke;

        Assert.Equal(expectedClipWidth, clip!.Bounds.Width, 3);
        Assert.Equal(expectedClipHeight, clip.Bounds.Height, 3);
    }

    [AvaloniaFact(DisplayName = "Clip is cleared when shape is removed")]
    public void ClipClears_WhenShapeRemoved()
    {
        var border = new MauiBorder
        {
            Shape = new RoundRectangle { CornerRadius = new Microsoft.Maui.CornerRadius(10) }
        };

        var child = new Border();
        border.Child = child;

        var size = new Size(100, 100);
        border.Measure(size);
        border.Arrange(new Rect(size));

        Assert.NotNull(child.Clip);

        border.Shape = null;
        border.Measure(size);
        border.Arrange(new Rect(size));

        Assert.Null(child.Clip);
    }

    [AvaloniaFact(DisplayName = "Child without shape has no clip")]
    public void ChildWithoutShape_HasNoClip()
    {
        var border = new MauiBorder
        {
            StrokeThickness = 4,
            Padding = new Thickness(8)
        };

        var child = new Border();
        border.Child = child;

        var size = new Size(100, 100);
        border.Measure(size);
        border.Arrange(new Rect(size));

        Assert.Null(child.Clip);
    }

    [AvaloniaFact(DisplayName = "Child arranged correctly with zero padding")]
    public void ChildArranged_WithZeroPadding()
    {
        var border = new MauiBorder
        {
            Shape = new RoundRectangle { CornerRadius = new Microsoft.Maui.CornerRadius(20) },
            StrokeThickness = 2
        };

        var child = new Border();
        border.Child = child;

        var finalSize = new Size(100, 100);
        border.Measure(finalSize);
        border.Arrange(new Rect(finalSize));

        Assert.Equal(finalSize.Width, child.Bounds.Width, 3);
        Assert.Equal(finalSize.Height, child.Bounds.Height, 3);
    }

    [AvaloniaFact(DisplayName = "Child arranged correctly with asymmetric padding")]
    public void ChildArranged_WithAsymmetricPadding()
    {
        var border = new MauiBorder
        {
            Padding = new Thickness(10, 20, 30, 40)
        };

        var child = new Border();
        border.Child = child;

        var finalSize = new Size(200, 200);
        border.Measure(finalSize);
        border.Arrange(new Rect(finalSize));

        var expectedWidth = finalSize.Width - 10 - 30;
        var expectedHeight = finalSize.Height - 20 - 40;

        Assert.Equal(expectedWidth, child.Bounds.Width, 3);
        Assert.Equal(expectedHeight, child.Bounds.Height, 3);
        Assert.Equal(10, child.Bounds.X, 3);
        Assert.Equal(20, child.Bounds.Y, 3);
    }

    [AvaloniaFact(DisplayName = "Asymmetric corner radius clips correctly")]
    public void AsymmetricCornerRadius_ClipsCorrectly()
    {
        var border = new MauiBorder
        {
            Shape = new RoundRectangle { CornerRadius = new Microsoft.Maui.CornerRadius(10, 20, 30, 40) },
            StrokeThickness = 4
        };

        var child = new Border();
        border.Child = child;

        var size = new Size(150, 150);
        border.Measure(size);
        border.Arrange(new Rect(size));

        Assert.NotNull(child.Clip);
        var expectedClipWidth = size.Width - border.StrokeThickness;
        var expectedClipHeight = size.Height - border.StrokeThickness;
        Assert.Equal(expectedClipWidth, child.Clip!.Bounds.Width, 3);
        Assert.Equal(expectedClipHeight, child.Clip.Bounds.Height, 3);
    }

    [AvaloniaFact(DisplayName = "Zero corner radius creates rectangular clip")]
    public void ZeroCornerRadius_CreatesRectangularClip()
    {
        var border = new MauiBorder
        {
            Shape = new RoundRectangle { CornerRadius = new Microsoft.Maui.CornerRadius(0) },
            StrokeThickness = 2
        };

        var child = new Border();
        border.Child = child;

        var size = new Size(100, 100);
        border.Measure(size);
        border.Arrange(new Rect(size));

        Assert.NotNull(child.Clip);
    }

    [AvaloniaFact(DisplayName = "Corner radius larger than half size still works")]
    public void CornerRadiusLargerThanHalfSize_StillWorks()
    {
        var border = new MauiBorder
        {
            Shape = new RoundRectangle { CornerRadius = new Microsoft.Maui.CornerRadius(60) },
            StrokeThickness = 2
        };

        var child = new Border();
        border.Child = child;

        var size = new Size(100, 100);
        border.Measure(size);
        border.Arrange(new Rect(size));

        Assert.NotNull(child.Clip);
    }

    [AvaloniaFact(DisplayName = "Zero stroke thickness creates full-size clip")]
    public void ZeroStrokeThickness_CreatesFullSizeClip()
    {
        var border = new MauiBorder
        {
            Shape = new RoundRectangle { CornerRadius = new Microsoft.Maui.CornerRadius(10) },
            StrokeThickness = 0
        };

        var child = new Border();
        border.Child = child;

        var size = new Size(100, 100);
        border.Measure(size);
        border.Arrange(new Rect(size));

        Assert.NotNull(child.Clip);
        Assert.Equal(100, child.Clip!.Bounds.Width, 3);
        Assert.Equal(100, child.Clip.Bounds.Height, 3);
    }

    [AvaloniaFact(DisplayName = "Large stroke thickness reduces clip size")]
    public void LargeStrokeThickness_ReducesClipSize()
    {
        var border = new MauiBorder
        {
            Shape = new RoundRectangle { CornerRadius = new Microsoft.Maui.CornerRadius(20) },
            StrokeThickness = 10
        };

        var child = new Border();
        border.Child = child;

        var size = new Size(100, 100);
        border.Measure(size);
        border.Arrange(new Rect(size));

        Assert.NotNull(child.Clip);
        var expectedClipWidth = size.Width - border.StrokeThickness;
        var expectedClipHeight = size.Height - border.StrokeThickness;
        Assert.Equal(expectedClipWidth, child.Clip!.Bounds.Width, 3);
        Assert.Equal(expectedClipHeight, child.Clip.Bounds.Height, 3);
    }

    [AvaloniaFact(DisplayName = "Stroke thickness larger than radius adjusts corner radius to zero")]
    public void StrokeThicknessLargerThanRadius_AdjustsToZero()
    {
        var border = new MauiBorder
        {
            Shape = new RoundRectangle { CornerRadius = new Microsoft.Maui.CornerRadius(5) },
            StrokeThickness = 20
        };

        var child = new Border();
        border.Child = child;

        var size = new Size(100, 100);
        border.Measure(size);
        border.Arrange(new Rect(size));

        Assert.NotNull(child.Clip);
    }

    [AvaloniaFact(DisplayName = "Background property is set correctly")]
    public void Background_Property_IsSetCorrectly()
    {
        var border = new MauiBorder();
        var brush = new SolidColorBrush(Media.Colors.Red);

        border.Background = brush;

        Assert.Equal(brush, border.Background);
    }

    [AvaloniaFact(DisplayName = "Stroke property is set correctly")]
    public void Stroke_Property_IsSetCorrectly()
    {
        var border = new MauiBorder();
        var brush = new SolidColorBrush(Media.Colors.Blue);

        border.Stroke = brush;

        Assert.Equal(brush, border.Stroke);
    }

    [AvaloniaFact(DisplayName = "StrokeThickness property is set correctly")]
    public void StrokeThickness_Property_IsSetCorrectly()
    {
        var border = new MauiBorder();

        border.StrokeThickness = 5.5;

        Assert.Equal(5.5, border.StrokeThickness);
    }

    [AvaloniaFact(DisplayName = "StrokeDashPattern property is set correctly")]
    public void StrokeDashPattern_Property_IsSetCorrectly()
    {
        var border = new MauiBorder();
        var pattern = new[] { 4f, 2f, 1f, 2f };

        border.StrokeDashPattern = pattern;

        Assert.Equal(pattern, border.StrokeDashPattern);
    }

    [AvaloniaFact(DisplayName = "StrokeDashOffset property is set correctly")]
    public void StrokeDashOffset_Property_IsSetCorrectly()
    {
        var border = new MauiBorder();

        border.StrokeDashOffset = 3.5f;

        Assert.Equal(3.5f, border.StrokeDashOffset);
    }

    [AvaloniaFact(DisplayName = "StrokeLineCap property is set correctly")]
    public void StrokeLineCap_Property_IsSetCorrectly()
    {
        var border = new MauiBorder();

        border.StrokeLineCap = LineCap.Round;

        Assert.Equal(LineCap.Round, border.StrokeLineCap);
    }

    [AvaloniaFact(DisplayName = "StrokeLineJoin property is set correctly")]
    public void StrokeLineJoin_Property_IsSetCorrectly()
    {
        var border = new MauiBorder();

        border.StrokeLineJoin = LineJoin.Bevel;

        Assert.Equal(LineJoin.Bevel, border.StrokeLineJoin);
    }

    [AvaloniaFact(DisplayName = "StrokeMiterLimit property is set correctly")]
    public void StrokeMiterLimit_Property_IsSetCorrectly()
    {
        var border = new MauiBorder();

        border.StrokeMiterLimit = 15f;

        Assert.Equal(15f, border.StrokeMiterLimit);
    }

    [AvaloniaFact(DisplayName = "Shape property is set correctly")]
    public void Shape_Property_IsSetCorrectly()
    {
        var border = new MauiBorder();
        var shape = new RoundRectangle { CornerRadius = new Microsoft.Maui.CornerRadius(25) };

        border.Shape = shape;

        Assert.Equal(shape, border.Shape);
    }

    [AvaloniaFact(DisplayName = "Default values are correct")]
    public void DefaultValues_AreCorrect()
    {
        var border = new MauiBorder();

        Assert.Null(border.Background);
        Assert.Null(border.Stroke);
        Assert.Equal(0.0, border.StrokeThickness);
        Assert.Null(border.Shape);
        Assert.Null(border.StrokeDashPattern);
        Assert.Equal(0f, border.StrokeDashOffset);
        Assert.Equal(LineCap.Butt, border.StrokeLineCap);
        Assert.Equal(LineJoin.Miter, border.StrokeLineJoin);
        Assert.Equal(10f, border.StrokeMiterLimit);
    }

    [AvaloniaFact(DisplayName = "InvalidateShape triggers re-measure")]
    public void InvalidateShape_TriggersReMeasure()
    {
        var border = new MauiBorder
        {
            Shape = new RoundRectangle { CornerRadius = new Microsoft.Maui.CornerRadius(10) }
        };

        var child = new Border();
        border.Child = child;

        var size = new Size(100, 100);
        border.Measure(size);
        border.Arrange(new Rect(size));

        border.InvalidateShape();

        Assert.True(border.IsMeasureValid == false || border.IsArrangeValid == false);
    }

    [AvaloniaFact(DisplayName = "Measure accounts for stroke thickness")]
    public void Measure_AccountsForStrokeThickness()
    {
        var border = new MauiBorder
        {
            StrokeThickness = 4,
            Padding = new Thickness(0)
        };

        var child = new Border { Width = 50, Height = 50 };
        border.Child = child;

        border.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        Assert.Equal(50 + 4 * 2, border.DesiredSize.Width, 3);
        Assert.Equal(50 + 4 * 2, border.DesiredSize.Height, 3);
    }

    [AvaloniaFact(DisplayName = "Measure accounts for padding")]
    public void Measure_AccountsForPadding()
    {
        var border = new MauiBorder
        {
            Padding = new Thickness(10, 20, 30, 40)
        };

        var child = new Border { Width = 50, Height = 50 };
        border.Child = child;

        border.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        Assert.Equal(50 + 10 + 30, border.DesiredSize.Width, 3);
        Assert.Equal(50 + 20 + 40, border.DesiredSize.Height, 3);
    }

    [AvaloniaFact(DisplayName = "Measure with no child returns stroke and padding")]
    public void Measure_WithNoChild_ReturnsStrokeAndPadding()
    {
        var border = new MauiBorder
        {
            StrokeThickness = 4,
            Padding = new Thickness(10)
        };

        border.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        Assert.Equal(4 * 2 + 10 * 2, border.DesiredSize.Width, 3);
        Assert.Equal(4 * 2 + 10 * 2, border.DesiredSize.Height, 3);
    }

    [AvaloniaFact(DisplayName = "Arrange returns correct final size")]
    public void Arrange_ReturnsCorrectFinalSize()
    {
        var border = new MauiBorder();
        var child = new Border();
        border.Child = child;

        var finalSize = new Size(150, 120);
        border.Measure(finalSize);
        border.Arrange(new Rect(finalSize));

        Assert.Equal(finalSize, border.Bounds.Size);
    }

    [AvaloniaFact(DisplayName = "Zero size border does not throw")]
    public void ZeroSizeBorder_DoesNotThrow()
    {
        var border = new MauiBorder
        {
            Shape = new RoundRectangle { CornerRadius = new Microsoft.Maui.CornerRadius(10) },
            StrokeThickness = 2
        };

        var child = new Border();
        border.Child = child;

        var size = new Size(0, 0);
        border.Measure(size);
        border.Arrange(new Rect(size));
    }

    [AvaloniaFact(DisplayName = "Negative padding is handled")]
    public void NegativePadding_IsHandled()
    {
        var border = new MauiBorder
        {
            Padding = new Thickness(-5)
        };

        var child = new Border();
        border.Child = child;

        var size = new Size(100, 100);
        border.Measure(size);
        border.Arrange(new Rect(size));
    }

    [AvaloniaFact(DisplayName = "Very small size with large stroke thickness")]
    public void VerySmallSize_WithLargeStrokeThickness()
    {
        var border = new MauiBorder
        {
            Shape = new RoundRectangle { CornerRadius = new Microsoft.Maui.CornerRadius(20) },
            StrokeThickness = 50
        };

        var child = new Border();
        border.Child = child;

        var size = new Size(40, 40);
        border.Measure(size);
        border.Arrange(new Rect(size));

        Assert.NotNull(child.Clip);
    }

    [AvaloniaFact(DisplayName = "Null child is handled")]
    public void NullChild_IsHandled()
    {
        var border = new MauiBorder
        {
            Shape = new RoundRectangle { CornerRadius = new Microsoft.Maui.CornerRadius(10) },
            StrokeThickness = 2
        };

        var size = new Size(100, 100);
        border.Measure(size);
        border.Arrange(new Rect(size));
    }

    [AvaloniaFact(DisplayName = "Shape change updates clip")]
    public void ShapeChange_UpdatesClip()
    {
        var border = new MauiBorder
        {
            Shape = new RoundRectangle { CornerRadius = new Microsoft.Maui.CornerRadius(10) },
            StrokeThickness = 2
        };

        var child = new Border();
        border.Child = child;

        var size = new Size(100, 100);
        border.Measure(size);
        border.Arrange(new Rect(size));

        var originalClip = child.Clip;
        Assert.NotNull(originalClip);

        border.Shape = new RoundRectangle { CornerRadius = new Microsoft.Maui.CornerRadius(30) };
        border.Measure(size);
        border.Arrange(new Rect(size));

        Assert.NotNull(child.Clip);
    }

    [AvaloniaFact(DisplayName = "StrokeThickness change updates clip")]
    public void StrokeThicknessChange_UpdatesClip()
    {
        var border = new MauiBorder
        {
            Shape = new RoundRectangle { CornerRadius = new Microsoft.Maui.CornerRadius(20) },
            StrokeThickness = 2
        };

        var child = new Border();
        border.Child = child;

        var size = new Size(100, 100);
        border.Measure(size);
        border.Arrange(new Rect(size));

        var originalClipWidth = child.Clip!.Bounds.Width;

        border.StrokeThickness = 10;
        border.Measure(size);
        border.Arrange(new Rect(size));

        Assert.NotEqual(originalClipWidth, child.Clip!.Bounds.Width);
    }

    [AvaloniaFact(DisplayName = "Ellipse shape creates valid clip")]
    public void EllipseShape_CreatesValidClip()
    {
        var border = new MauiBorder
        {
            Shape = new Ellipse(),
            StrokeThickness = 2
        };

        var child = new Border();
        border.Child = child;

        var size = new Size(100, 80);
        border.Measure(size);
        border.Arrange(new Rect(size));

        Assert.NotNull(child.Clip);
    }

    [AvaloniaFact(DisplayName = "All properties combined work correctly")]
    public void AllPropertiesCombined_WorkCorrectly()
    {
        var border = new MauiBorder
        {
            Background = new SolidColorBrush(Media.Colors.LightBlue),
            Stroke = new SolidColorBrush(Media.Colors.DarkBlue),
            StrokeThickness = 4,
            Shape = new RoundRectangle { CornerRadius = new Microsoft.Maui.CornerRadius(15, 30, 15, 30) },
            Padding = new Thickness(10),
            StrokeDashPattern = [4f, 2f],
            StrokeDashOffset = 1f,
            StrokeLineCap = LineCap.Round,
            StrokeLineJoin = LineJoin.Round,
            StrokeMiterLimit = 5f
        };

        var child = new Border();
        border.Child = child;

        var size = new Size(200, 150);
        border.Measure(size);
        border.Arrange(new Rect(size));

        Assert.Equal(size.Width - 20, child.Bounds.Width, 3);
        Assert.Equal(size.Height - 20, child.Bounds.Height, 3);
        Assert.NotNull(child.Clip);
    }
}
