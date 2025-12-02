using Avalonia.Controls.Maui.Handlers;
using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using MauiColors = Microsoft.Maui.Graphics.Colors;
using MauiEllipseGeometry = Microsoft.Maui.Controls.Shapes.EllipseGeometry;
using MauiPoint = Microsoft.Maui.Graphics.Point;
using MauiSolidPaint = Microsoft.Maui.Graphics.SolidPaint;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public class ViewHandlerTests
{
    [AvaloniaFact(DisplayName = "UpdateClip applies geometry to control")]
    public void UpdateClipAppliesGeometry()
    {
        var view = new StubBase
        {
            Clip = new MauiEllipseGeometry
            {
                Center = new MauiPoint(50, 40),
                RadiusX = 50,
                RadiusY = 40
            },
            Width = 100,
            Height = 80
        };

        var control = new global::Avalonia.Controls.Control();
        control.UpdateClip(view);

        var clip = Assert.IsType<global::Avalonia.Media.EllipseGeometry>(control.Clip);
        Assert.Equal(new global::Avalonia.Rect(0, 0, 100, 80), clip.Rect);
    }

    [AvaloniaFact(DisplayName = "UpdateShadow applies DropShadowEffect")]
    public void UpdateShadowAppliesDropShadowEffect()
    {
        var view = new StubBase
        {
            Shadow = new ShadowStub
            {
                Paint = new MauiSolidPaint(MauiColors.Red),
                Offset = new MauiPoint(6, 8),
                Opacity = 0.5f,
                Radius = 12f
            }
        };

        var control = new global::Avalonia.Controls.Control();

        control.UpdateShadow(view);

        var effect = Assert.IsType<DropShadowEffect>(control.Effect);

        Assert.Equal((byte)(255 * 0.5f), effect.Color.A);
        Assert.Equal((byte)(MauiColors.Red.Red * 255), effect.Color.R);
        Assert.Equal((byte)(MauiColors.Red.Green * 255), effect.Color.G);
        Assert.Equal((byte)(MauiColors.Red.Blue * 255), effect.Color.B);
        Assert.Equal(6, effect.OffsetX, 2);
        Assert.Equal(8, effect.OffsetY, 2);
        Assert.Equal(12, effect.BlurRadius, 2);
    }

    [AvaloniaFact(DisplayName = "UpdateShadow clears effect when null")]
    public void UpdateShadowClearsWhenNull()
    {
        var view = new StubBase
        {
            Shadow = new ShadowStub
            {
                Paint = new MauiSolidPaint(MauiColors.Red),
                Offset = new MauiPoint(2, 2),
                Opacity = 1f,
                Radius = 6f
            }
        };

        var control = new global::Avalonia.Controls.Control();
        control.UpdateShadow(view);

        view.Shadow = null;
        control.UpdateShadow(view);

        Assert.Null(control.Effect);
    }
}
