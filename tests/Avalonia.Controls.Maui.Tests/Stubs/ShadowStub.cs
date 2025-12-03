using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.Tests.Stubs;

public class ShadowStub : IShadow
{
    public float Radius { get; set; }

    public float Opacity { get; set; } = 1f;

    public Microsoft.Maui.Graphics.Point Offset { get; set; }

    public Paint Paint { get; set; } = new SolidPaint(Colors.Black);
}
