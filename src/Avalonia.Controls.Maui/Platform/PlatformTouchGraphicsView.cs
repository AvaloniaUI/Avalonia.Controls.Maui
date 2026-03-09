using Avalonia.Input;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Platform touch graphics view that handles pointer interactions for GraphicsView
/// </summary>
public class PlatformTouchGraphicsView : UserControl
{
    private IGraphicsView? _graphicsView;
    private readonly PlatformGraphicsView _platformGraphicsView;
    private bool _isTouching;
    private bool _isInBounds;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlatformTouchGraphicsView"/> class with an embedded <see cref="PlatformGraphicsView"/> for rendering.
    /// </summary>
    public PlatformTouchGraphicsView()
    {
        // Allow content (e.g. shadows drawn by IDrawable) to extend beyond
        // this control's bounds without being clipped.  TemplatedControl sets
        // ClipToBounds = true by default; native MAUI explicitly sets
        // ClipsToBounds = false on its wrapper views for the same reason.
        ClipToBounds = false;
        Content = _platformGraphicsView = new PlatformGraphicsView();
    }

    /// <summary>
    /// Updates the drawable from the specified MAUI graphics view, triggering a re-render.
    /// </summary>
    /// <param name="graphicsView">The MAUI graphics view whose drawable should be rendered.</param>
    public void UpdateDrawable(IGraphicsView graphicsView)
    {
        _platformGraphicsView.UpdateDrawable(graphicsView);
        _graphicsView = graphicsView;
    }

    /// <summary>
    /// Triggers a re-render of the underlying graphics view.
    /// </summary>
    public void InvalidateDrawable() => _platformGraphicsView.InvalidateVisual();

    private PointF[] GetViewPoints(PointerEventArgs e)
    {
        var point = e.GetPosition(this);
        return [new PointF((float)point.X, (float)point.Y)];
    }

    /// <inheritdoc/>
    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        _isInBounds = true;
        _graphicsView?.StartHoverInteraction(GetViewPoints(e));
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        var points = GetViewPoints(e);

        _graphicsView?.MoveHoverInteraction(points);

        if (_isTouching)
            _graphicsView?.DragInteraction(points);
    }

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        var points = GetViewPoints(e);
        _isTouching = true;
        _graphicsView?.StartInteraction(points);
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        if (_isTouching)
        {
            _isTouching = false;
            _graphicsView?.CancelInteraction();
        }
    }

    /// <summary>
    /// Connects this view to the specified MAUI graphics view for receiving touch and hover events.
    /// </summary>
    /// <param name="graphicsView">The MAUI graphics view to connect to.</param>
    public void Connect(IGraphicsView graphicsView) => _graphicsView = graphicsView;

    /// <summary>
    /// Disconnects from the current MAUI graphics view, stopping touch and hover event forwarding.
    /// </summary>
    public void Disconnect() => _graphicsView = null;
}
