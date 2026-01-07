using Avalonia.Controls.Maui.Extensions;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Microsoft.Maui.Graphics;
using MauiColors = Microsoft.Maui.Graphics.Colors;
using MauiSolidPaint = Microsoft.Maui.Graphics.SolidPaint;
using MauiLinearGradientPaint = Microsoft.Maui.Graphics.LinearGradientPaint;
using MauiRadialGradientPaint = Microsoft.Maui.Graphics.RadialGradientPaint;
using MauiGradientStop = Microsoft.Maui.Graphics.PaintGradientStop;
using MauiControlsSolidColorBrush = Microsoft.Maui.Controls.SolidColorBrush;
using MauiControlsLinearGradientBrush = Microsoft.Maui.Controls.LinearGradientBrush;
using MauiControlsRadialGradientBrush = Microsoft.Maui.Controls.RadialGradientBrush;
using MauiControlsGradientStop = Microsoft.Maui.Controls.GradientStop;
using MauiPoint = Microsoft.Maui.Graphics.Point;

namespace Avalonia.Controls.Maui.Tests.Core;

public class BrushesTests
{
    [AvaloniaFact(DisplayName = "ToPlatform maps MauiColor to Avalonia SolidColorBrush")]
    public void ToPlatform_Maps_MauiColor_To_Avalonia_SolidColorBrush()
    {
        var mauiColor = MauiColors.Red;
        var brush = mauiColor.ToPlatform();

        Assert.Equal(global::Avalonia.Media.Colors.Red, brush.Color);
    }

    [AvaloniaFact(DisplayName = "ToPlatform maps MauiSolidPaint to Avalonia SolidColorBrush")]
    public void ToPlatform_Maps_MauiSolidPaint_To_Avalonia_SolidColorBrush()
    {
        var paint = new MauiSolidPaint(MauiColors.Blue);
        var brush = paint.ToPlatform() as ISolidColorBrush;

        Assert.NotNull(brush);
        Assert.Equal(global::Avalonia.Media.Colors.Blue, brush!.Color);
    }

    [AvaloniaFact(DisplayName = "ToPlatform maps MauiLinearGradientPaint to Avalonia LinearGradientBrush")]
    public void ToPlatform_Maps_MauiLinearGradientPaint_To_Avalonia_LinearGradientBrush()
    {
        var paint = new MauiLinearGradientPaint
        {
            StartPoint = new MauiPoint(0, 0),
            EndPoint = new MauiPoint(1, 1),
            GradientStops = new[]
            {
                new MauiGradientStop(0, MauiColors.Red),
                new MauiGradientStop(1, MauiColors.Blue)
            }
        };

        var brush = paint.ToPlatform() as LinearGradientBrush;

        Assert.NotNull(brush);
        Assert.Equal(new RelativePoint(0, 0, RelativeUnit.Relative), brush!.StartPoint);
        Assert.Equal(new RelativePoint(1, 1, RelativeUnit.Relative), brush.EndPoint);
        Assert.Equal(2, brush.GradientStops.Count);
        Assert.Equal(global::Avalonia.Media.Colors.Red, brush.GradientStops[0].Color);
        Assert.Equal(global::Avalonia.Media.Colors.Blue, brush.GradientStops[1].Color);
    }

    [AvaloniaFact(DisplayName = "ToPlatform maps MauiRadialGradientPaint to Avalonia RadialGradientBrush")]
    public void ToPlatform_Maps_MauiRadialGradientPaint_To_Avalonia_RadialGradientBrush()
    {
        var paint = new MauiRadialGradientPaint
        {
            Center = new MauiPoint(0.5, 0.5),
            Radius = 0.5,
            GradientStops = new[]
            {
                new MauiGradientStop(0, MauiColors.White),
                new MauiGradientStop(1, MauiColors.Black)
            }
        };

        var brush = paint.ToPlatform() as RadialGradientBrush;

        Assert.NotNull(brush);
        Assert.Equal(new RelativePoint(0.5, 0.5, RelativeUnit.Relative), brush!.Center);
        Assert.Equal(new RelativePoint(0.5, 0.5, RelativeUnit.Relative), brush.GradientOrigin);
        // RelativeScalar struct equality check
        Assert.Equal(new RelativeScalar(0.5, RelativeUnit.Relative), brush.RadiusX);
        Assert.Equal(new RelativeScalar(0.5, RelativeUnit.Relative), brush.RadiusY);
        Assert.Equal(2, brush.GradientStops.Count);
    }

    [AvaloniaFact(DisplayName = "ToPlatform maps MauiControlsSolidColorBrush to Avalonia Brush")]
    public void ToPlatform_Maps_MauiControlsSolidColorBrush_To_Avalonia_Brush()
    {
        var mauiBrush = new MauiControlsSolidColorBrush(MauiColors.Green);
        var brush = mauiBrush.ToPlatform() as ISolidColorBrush;

        Assert.NotNull(brush);
        Assert.Equal(global::Avalonia.Media.Colors.Green, brush!.Color);
    }

    [AvaloniaFact(DisplayName = "ToPlatform maps MauiControlsLinearGradientBrush to Avalonia LinearGradientBrush")]
    public void ToPlatform_Maps_MauiControlsLinearGradientBrush_To_Avalonia_LinearGradientBrush()
    {
        var mauiBrush = new MauiControlsLinearGradientBrush
        {
            StartPoint = new MauiPoint(0, 0.5),
            EndPoint = new MauiPoint(1, 0.5),
            GradientStops = new Microsoft.Maui.Controls.GradientStopCollection
            {
                new MauiControlsGradientStop(MauiColors.Yellow, 0),
                new MauiControlsGradientStop(MauiColors.Cyan, 1)
            }
        };

        var brush = mauiBrush.ToPlatform() as LinearGradientBrush;

        Assert.NotNull(brush);
        Assert.Equal(new RelativePoint(0, 0.5, RelativeUnit.Relative), brush!.StartPoint);
        Assert.Equal(new RelativePoint(1, 0.5, RelativeUnit.Relative), brush.EndPoint);
        Assert.Equal(global::Avalonia.Media.Colors.Yellow, brush.GradientStops[0].Color);
        Assert.Equal(global::Avalonia.Media.Colors.Cyan, brush.GradientStops[1].Color);
    }

    [AvaloniaFact(DisplayName = "ToPlatform maps MauiControlsRadialGradientBrush to Avalonia RadialGradientBrush")]
    public void ToPlatform_Maps_MauiControlsRadialGradientBrush_To_Avalonia_RadialGradientBrush()
    {
        var mauiBrush = new MauiControlsRadialGradientBrush
        {
            Center = new MauiPoint(0.5, 0.5),
            Radius = 0.25,
            GradientStops = new Microsoft.Maui.Controls.GradientStopCollection
            {
                new MauiControlsGradientStop(MauiColors.Red, 0),
                new MauiControlsGradientStop(MauiColors.Blue, 1)
            }
        };

        var brush = mauiBrush.ToPlatform() as RadialGradientBrush;

        Assert.NotNull(brush);
        Assert.Equal(new RelativePoint(0.5, 0.5, RelativeUnit.Relative), brush!.Center);
        Assert.Equal(new RelativeScalar(0.25, RelativeUnit.Relative), brush.RadiusX);
        Assert.Equal(global::Avalonia.Media.Colors.Red, brush.GradientStops[0].Color);
        Assert.Equal(global::Avalonia.Media.Colors.Blue, brush.GradientStops[1].Color);
    }
}
