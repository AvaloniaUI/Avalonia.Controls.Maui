using Avalonia.Controls;
using Avalonia.Input;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using System.Linq;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Platform touch graphics view that handles pointer interactions for GraphicsView
/// </summary>
internal class PlatformTouchGraphicsView : UserControl
{
    private IGraphicsView? _graphicsView;
    private readonly PlatformGraphicsView _platformGraphicsView;
    private bool _isTouching;
    private bool _isInBounds;

    public PlatformTouchGraphicsView()
    {
        Content = _platformGraphicsView = new PlatformGraphicsView();
    }

    public void UpdateDrawable(IGraphicsView graphicsView)
    {
        _platformGraphicsView.UpdateDrawable(graphicsView);
        _graphicsView = graphicsView;
    }

    public void Invalidate() => _platformGraphicsView.InvalidateVisual();

    private PointF[] GetViewPoints(PointerEventArgs e)
    {
        var point = e.GetPosition(this);
        return new[] { new PointF((float)point.X, (float)point.Y) };
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        _isInBounds = true;
        _graphicsView?.StartHoverInteraction(GetViewPoints(e));
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        _isInBounds = false;

        _graphicsView?.EndHoverInteraction();

        if (_isTouching)
        {
            _isTouching = false;
            _graphicsView?.EndInteraction(GetViewPoints(e), _isInBounds);
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        var points = GetViewPoints(e);

        _graphicsView?.MoveHoverInteraction(points);

        if (_isTouching)
            _graphicsView?.DragInteraction(points);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        var points = GetViewPoints(e);
        _isTouching = true;
        _graphicsView?.StartInteraction(points);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        var points = GetViewPoints(e);

        if (_isTouching)
        {
            _isTouching = false;
            _graphicsView?.EndInteraction(points, _isInBounds);
        }
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        if (_isTouching)
        {
            _isTouching = false;
            _graphicsView?.CancelInteraction();
        }
    }

    public void Connect(IGraphicsView graphicsView) => _graphicsView = graphicsView;

    public void Disconnect() => _graphicsView = null;
}
