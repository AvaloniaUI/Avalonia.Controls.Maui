using Microsoft.Maui.Controls.Shapes;

namespace Avalonia.Controls.Maui.Tests.Stubs;

public class PathStub : ShapeViewStubBase
{
    public Geometry? Data { get; set; }

    public Transform? RenderTransform { get; set; }
}
