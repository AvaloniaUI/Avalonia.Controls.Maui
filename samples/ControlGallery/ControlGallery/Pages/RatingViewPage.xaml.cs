using CommunityToolkit.Maui.Core;

namespace ControlGallery.Pages;

public partial class RatingViewPage : ContentPage
{
    public RatingViewPage()
    {
        InitializeComponent();
    }

    private void OnDefaultRatingChanged(object? sender, RatingChangedEventArgs e)
    {
        DefaultRatingLabel.Text = $"Rating: {e.Rating}";
    }

    private void OnFractionalSliderChanged(object? sender, ValueChangedEventArgs e)
    {
        var rating = Math.Round(e.NewValue, 1);
        FractionalRating.Rating = rating;
        FractionalRatingLabel.Text = $"Rating: {rating}";
    }
}
