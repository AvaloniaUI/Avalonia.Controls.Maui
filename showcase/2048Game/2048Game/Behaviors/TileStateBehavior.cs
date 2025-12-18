using _2048Game.Models;

namespace _2048Game.Behaviors
{
    public class TileStateBehavior : Behavior<Border>
    {
        private Border border;
        private NumberTile numberTile;

        protected override void OnAttachedTo(Border bindable)
        {
            base.OnAttachedTo(bindable);

            border = bindable;
            border.BindingContextChanged += OnBindingContextChanged;
        }

        protected override void OnDetachingFrom(Border bindable)
        {
            base.OnDetachingFrom(bindable);

            border.BindingContextChanged -= OnBindingContextChanged;

            if (numberTile is not null)
            {
                numberTile.PropertyChanged -= OnTileViewModelPropertyChanged;
            }
        }

        private void OnBindingContextChanged(object sender, EventArgs e)
        {
            if (border.BindingContext is NumberTile tile)
            {
                numberTile = tile;
                numberTile.PropertyChanged += OnTileViewModelPropertyChanged;
            }
        }

        private async void OnTileViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(NumberTile.IsNumberMultiplied))
            {
                if (numberTile.IsNumberMultiplied)
                {
                    double borderScale = border.Scale;
                    await border.ScaleTo(borderScale * 1.25, 150);
                    await border.ScaleTo(borderScale, 150);
                }
                numberTile.IsNumberMultiplied = false;
            }
            else if (e.PropertyName == nameof(NumberTile.IsNewNumberGenerated))
            {
                if (numberTile.IsNewNumberGenerated)
                {
                    var animation = new Animation();

                    animation.WithConcurrent(
                                   f => border.Scale = f,
                                    0.5, 1,
                                   Microsoft.Maui.Easing.Linear, 0, 1);

                    animation.WithConcurrent(
                            (f) => border.Opacity = f,
                            0, 1,
                            null,
                            0, 0.25);
                    border.Animate("BounceIn", animation, 16, Convert.ToUInt32(200));
                }
                numberTile.IsNewNumberGenerated = false;
            }
        }
    }
}