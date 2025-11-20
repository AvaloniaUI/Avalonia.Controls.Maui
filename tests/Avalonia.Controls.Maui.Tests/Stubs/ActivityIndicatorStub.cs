using Microsoft.Maui;
using MauiGraphics = Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.Tests.Stubs;

public class ActivityIndicatorStub : StubBase, IActivityIndicator
{
    public bool IsRunning { get; set; }

    public MauiGraphics.Color Color { get; set; } = null!;
}