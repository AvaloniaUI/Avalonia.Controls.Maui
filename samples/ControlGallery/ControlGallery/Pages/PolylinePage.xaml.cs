using Microsoft.Maui.Controls;

namespace ControlGallery.Pages;

public partial class PolylinePage : ContentPage
{
    private Animation? _dashAnimation;

    public PolylinePage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Starts the dash offset animation when the page appears.
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (AnimatedPolyline != null && _dashAnimation == null)
        {
            _dashAnimation = new Animation(v =>
            {
                // Ensure dash updates happen on the UI thread to avoid cross-thread exceptions.
                if (Dispatcher.IsDispatchRequired)
                {
                    Dispatcher.Dispatch(() => AnimatedPolyline.StrokeDashOffset = v);
                }
                else
                {
                    AnimatedPolyline.StrokeDashOffset = v;
                }
            }, 0, 16, Easing.Linear);
            _dashAnimation.Commit(this, "PolylineDashOffset", length: 1600, repeat: () => true);
        }
    }

    /// <summary>
    /// Stops the dash offset animation when the page is disappearing.
    /// </summary>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        this.AbortAnimation("PolylineDashOffset");
        _dashAnimation = null;
    }
}
