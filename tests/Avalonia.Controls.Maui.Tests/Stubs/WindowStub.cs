using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using System.Collections.Generic;
using MauiRect = Microsoft.Maui.Graphics.Rect;

namespace Avalonia.Controls.Maui.Tests.Stubs;

public class WindowStub : ElementStub, IWindow
{
    private readonly List<IWindowOverlay> _overlays = new();

    public IView? Content { get; set; }

    public IVisualDiagnosticsOverlay VisualDiagnosticsOverlay => null!;

    public IReadOnlyCollection<IWindowOverlay> Overlays => _overlays.AsReadOnly();

    public double X { get; set; }

    public double Y { get; set; }

    public double Width { get; set; } = 800;

    public double MinimumWidth { get; set; }

    public double MaximumWidth { get; set; } = double.PositiveInfinity;

    public double Height { get; set; } = 600;

    public double MinimumHeight { get; set; }

    public double MaximumHeight { get; set; } = double.PositiveInfinity;

    public string Title { get; set; } = string.Empty;

    public FlowDirection FlowDirection { get; set; } = FlowDirection.LeftToRight;

    public bool AddOverlay(IWindowOverlay overlay)
    {
        _overlays.Add(overlay);
        return true;
    }

    public bool RemoveOverlay(IWindowOverlay overlay)
    {
        return _overlays.Remove(overlay);
    }

    public void Created() { }

    public void Resumed() { }

    public void Activated() { }

    public void Deactivated() { }

    public void Stopped() { }

    public void Destroying() { }

    public void Backgrounding(IPersistedState state) { }

    public bool BackButtonClicked() => false;

    public void DisplayDensityChanged(float displayDensity) { }

    public void FrameChanged(MauiRect frame) { }

    public float RequestDisplayDensity() => 1.0f;
}
