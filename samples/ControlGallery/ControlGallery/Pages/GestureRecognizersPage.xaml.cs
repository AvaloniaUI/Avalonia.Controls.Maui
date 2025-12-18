namespace ControlGallery.Pages;

public partial class GestureRecognizersPage : ContentPage
{
    // Pan gesture tracking
    private double _panX;
    private double _panY;

    // Combined gesture tracking
    private double _combinedPanX;
    private double _combinedPanY;
    private int _combinedTapCount;

    // Pinch gesture tracking
    private double _currentScale = 1;
    private double _startScale = 1;

    public GestureRecognizersPage()
    {
        InitializeComponent();
    }

    #region TapGestureRecognizer

    private void OnSingleTap(object? sender, TappedEventArgs e)
    {
        SingleTapBox.BackgroundColor = Colors.DarkBlue;
        TapResultLabel.Text = "Single tap detected!";

        // Reset color after a short delay
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(200), () =>
        {
            SingleTapBox.BackgroundColor = Colors.LightBlue;
        });
    }

    private void OnDoubleTap(object? sender, TappedEventArgs e)
    {
        DoubleTapBox.BackgroundColor = Colors.DarkGreen;
        TapResultLabel.Text = "Double tap detected!";

        // Reset color after a short delay
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(200), () =>
        {
            DoubleTapBox.BackgroundColor = Colors.LightGreen;
        });
    }

    #endregion

    #region PanGestureRecognizer

    private void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                PanResultLabel.Text = "Pan status: Started";
                break;

            case GestureStatus.Running:
                _panX += e.TotalX;
                _panY += e.TotalY;

                // Clamp to reasonable bounds
                _panX = Math.Clamp(_panX, -100, 100);
                _panY = Math.Clamp(_panY, -50, 50);

                PanBox.TranslationX = _panX;
                PanBox.TranslationY = _panY;
                PanResultLabel.Text = $"Pan status: X={_panX:F0}, Y={_panY:F0}";
                break;

            case GestureStatus.Completed:
                PanResultLabel.Text = $"Pan status: Completed at X={_panX:F0}, Y={_panY:F0}";
                break;

            case GestureStatus.Canceled:
                PanResultLabel.Text = "Pan status: Canceled";
                break;
        }
    }

    #endregion

    #region SwipeGestureRecognizer

    private void OnSwiped(object? sender, SwipedEventArgs e)
    {
        var direction = e.Direction.ToString();
        SwipeResultLabel.Text = $"Swiped: {direction}";
        SwipeDirectionLabel.Text = $"Last: {direction}";

        // Flash the background color based on direction
        var color = e.Direction switch
        {
            SwipeDirection.Left => Colors.IndianRed,
            SwipeDirection.Right => Colors.LightGreen,
            SwipeDirection.Up => Colors.LightBlue,
            SwipeDirection.Down => Colors.Gold,
            _ => Colors.MediumPurple
        };

        SwipeBox.BackgroundColor = color;

        // Reset color after a short delay
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(300), () =>
        {
            SwipeBox.BackgroundColor = Colors.MediumPurple;
        });
    }

    #endregion

    #region PinchGestureRecognizer

    private void OnPinchUpdated(object? sender, PinchGestureUpdatedEventArgs e)
    {
        switch (e.Status)
        {
            case GestureStatus.Started:
                _startScale = _currentScale;
                PinchResultLabel.Text = "Pinch status: Started";
                break;

            case GestureStatus.Running:
                _currentScale = Math.Clamp(_startScale * e.Scale, 0.5, 3.0);
                PinchImage.Scale = _currentScale;
                PinchResultLabel.Text = $"Pinch status: Scale = {_currentScale:F2}";
                break;

            case GestureStatus.Completed:
                PinchResultLabel.Text = $"Pinch status: Completed at Scale = {_currentScale:F2}";
                break;

            case GestureStatus.Canceled:
                PinchResultLabel.Text = "Pinch status: Canceled";
                break;
        }
    }

    private void OnResetPinchScale(object? sender, EventArgs e)
    {
        _currentScale = 1;
        _startScale = 1;
        PinchImage.Scale = 1;
        PinchResultLabel.Text = "Pinch status: Scale = 1.0 (Reset)";
    }

    #endregion

    #region PointerGestureRecognizer

    private void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        PointerBox.BackgroundColor = Colors.DarkCyan;
        PointerStateLabel.Text = "Pointer Inside!";
        PointerResultLabel.Text = "Pointer status: Entered";
    }

    private void OnPointerExited(object? sender, PointerEventArgs e)
    {
        PointerBox.BackgroundColor = Colors.Teal;
        PointerStateLabel.Text = "Hover Over Me";
        PointerPositionLabel.Text = "Position: --";
        PointerResultLabel.Text = "Pointer status: Exited";
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        var position = e.GetPosition(PointerBox);
        if (position.HasValue)
        {
            PointerPositionLabel.Text = $"Position: ({position.Value.X:F0}, {position.Value.Y:F0})";
        }
    }

    private void OnPointerPressed(object? sender, PointerEventArgs e)
    {
        PointerBox.BackgroundColor = Colors.DarkSlateGray;
        PointerStateLabel.Text = "Pressed!";
        PointerResultLabel.Text = "Pointer status: Pressed";
    }

    private void OnPointerReleased(object? sender, PointerEventArgs e)
    {
        PointerBox.BackgroundColor = Colors.DarkCyan;
        PointerStateLabel.Text = "Released";
        PointerResultLabel.Text = "Pointer status: Released";
    }

    #endregion

    #region Combined Gestures

    private void OnCombinedTap(object? sender, TappedEventArgs e)
    {
        _combinedTapCount++;

        // Cycle through colors on tap
        var colors = new[] { Colors.Coral, Colors.Crimson, Colors.DarkOrange, Colors.Tomato };
        CombinedBox.BackgroundColor = colors[_combinedTapCount % colors.Length];

        CombinedResultLabel.Text = $"Tapped {_combinedTapCount} times";
    }

    private void OnCombinedPan(object? sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Running:
                _combinedPanX += e.TotalX;
                _combinedPanY += e.TotalY;

                // Clamp to reasonable bounds
                _combinedPanX = Math.Clamp(_combinedPanX, -100, 100);
                _combinedPanY = Math.Clamp(_combinedPanY, -50, 50);

                CombinedBox.TranslationX = _combinedPanX;
                CombinedBox.TranslationY = _combinedPanY;
                CombinedResultLabel.Text = $"Panning: X={_combinedPanX:F0}, Y={_combinedPanY:F0}";
                break;

            case GestureStatus.Completed:
                CombinedResultLabel.Text = $"Pan completed at X={_combinedPanX:F0}, Y={_combinedPanY:F0}";
                break;
        }
    }

    #endregion
}
