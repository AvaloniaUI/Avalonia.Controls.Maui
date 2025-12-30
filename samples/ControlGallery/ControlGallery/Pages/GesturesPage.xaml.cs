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
}
