using Microsoft.Maui;
using MauiGraphics = Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.Tests.Stubs;

public partial class ProgressBarStub : StubBase, IProgress
{
    public double Progress { get; set; } = 0.0;

    public MauiGraphics.Color ProgressColor { get; set; } = null!;
}
