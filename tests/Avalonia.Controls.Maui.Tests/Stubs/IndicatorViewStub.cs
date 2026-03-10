using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.Tests.Stubs;

public partial class IndicatorViewStub : StubBase, IIndicatorView
{
    public int Count { get; set; } = 0;
    public int Position { get; set; } = 0;
    public bool HideSingle { get; set; } = true;
    public int MaximumVisible { get; set; } = int.MaxValue;
    public double IndicatorSize { get; set; } = 6.0;
    public Paint IndicatorColor { get; set; } = null!;
    public Paint SelectedIndicatorColor { get; set; } = null!;
    public IShape IndicatorsShape { get; set; } = null!;
}
