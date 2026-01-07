using Microsoft.Maui.Controls;

namespace ControlGallery.Pages;

public partial class GesturesPage : ContentPage
{
    private int _singleTapCount = 0;
    private int _doubleTapCount = 0;
    
    // Pan gesture state
    private double _panX;
    private double _panY;

    public GesturesPage()
    {
        InitializeComponent();
    }

    private void OnSingleTapTapped(object sender, TappedEventArgs e)
    {
        _singleTapCount++;
        SingleTapCountLabel.Text = $"Taps: {_singleTapCount}";
        
        AnimateBox(SingleTapBox);
    }

    private void OnDoubleTapTapped(object sender, TappedEventArgs e)
    {
        _doubleTapCount++;
        DoubleTapCountLabel.Text = $"Double Taps: {_doubleTapCount}";
        
        AnimateBox(DoubleTapBox);
    }

    private async void AnimateBox(View view)
    {
        await view.ScaleTo(0.95, 50, Easing.CubicOut);
        await view.ScaleTo(1.0, 50, Easing.CubicIn);
    }

    private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                PanStatusLabel.Text = "Pan: Started";
                break;
                
            case GestureStatus.Running:
                // Calculate new position
                double newX = _panX + e.TotalX;
                double newY = _panY + e.TotalY;
                
                // Update box position using TranslationX/Y
                PanBox.TranslationX = newX;
                PanBox.TranslationY = newY;
                
                PanStatusLabel.Text = $"Pan: X={e.TotalX:F1}, Y={e.TotalY:F1}";
                break;
                
            case GestureStatus.Completed:
                // Store the final position
                _panX = PanBox.TranslationX;
                _panY = PanBox.TranslationY;
                PanStatusLabel.Text = "Pan: Completed";
                break;
                
            case GestureStatus.Canceled:
                PanStatusLabel.Text = "Pan: Canceled";
                break;
        }
    }

    private void OnPointerEntered(object sender, PointerEventArgs e)
    {
        PointerStatusLabel.Text = "Status: Entered";
        UpdatePointerPosition(e);
        PointerBox.BackgroundColor = Colors.LightGreen;
    }

    private void OnPointerExited(object sender, PointerEventArgs e)
    {
        PointerStatusLabel.Text = "Status: Exited";
        PointerPositionLabel.Text = "Position: -";
        PointerBox.BackgroundColor = Colors.LightGoldenrodYellow;
    }

    private void OnPointerMoved(object sender, PointerEventArgs e)
    {
        PointerStatusLabel.Text = "Status: Moving";
        UpdatePointerPosition(e);
    }

    private void OnPointerPressed(object sender, PointerEventArgs e)
    {
        PointerStatusLabel.Text = "Status: Pressed";
        UpdatePointerPosition(e);
        PointerBox.BackgroundColor = Colors.Orange;
    }

    private void OnPointerReleased(object sender, PointerEventArgs e)
    {
        PointerStatusLabel.Text = "Status: Released";
        UpdatePointerPosition(e);
        PointerBox.BackgroundColor = Colors.LightGreen; // Back to hover state
    }

    private void UpdatePointerPosition(PointerEventArgs e)
    {
        var relativeToBox = e.GetPosition(PointerBox);
        var relativeToPage = e.GetPosition(this);
        
        // Handle null points safely
        string boxPos = relativeToBox.HasValue ? $"({relativeToBox.Value.X:F0}, {relativeToBox.Value.Y:F0})" : "null";
        string pagePos = relativeToPage.HasValue ? $"({relativeToPage.Value.X:F0}, {relativeToPage.Value.Y:F0})" : "null";

        PointerPositionLabel.Text = $"Box: {boxPos} | Page: {pagePos}";
    }

    private void OnSwiped(object sender, SwipedEventArgs e)
    {
        SwipeStatusLabel.Text = $"Last Swipe: {e.Direction}";
        SwipeBox.BackgroundColor = GetColorForDirection(e.Direction);
    }
    
    private Color GetColorForDirection(SwipeDirection direction)
    {
        return direction switch
        {
            SwipeDirection.Left => Colors.LightPink,
            SwipeDirection.Right => Colors.LightGreen,
            SwipeDirection.Up => Colors.LightBlue,
            SwipeDirection.Down => Colors.LightYellow,
            _ => Colors.LightCyan
        };
    }
}
