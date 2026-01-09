using Avalonia.Controls;
using Avalonia.Media;

namespace Avalonia.Controls.Maui.Controls
{
    /// <summary>
    /// Custom Editor control for MAUI integration.
    /// </summary>
    public class MauiEditor : TextBox
    {
        /// <summary>
        /// Defines the <see cref="PlaceholderForeground"/> property.
        /// </summary>
        public static readonly StyledProperty<IBrush?> PlaceholderForegroundProperty =
            AvaloniaProperty.Register<MauiEditor, IBrush?>(nameof(PlaceholderForeground));

        /// <summary>
        /// Gets or sets the brush used to draw the placeholder text.
        /// </summary>
        public IBrush? PlaceholderForeground
        {
            get => GetValue(PlaceholderForegroundProperty);
            set => SetValue(PlaceholderForegroundProperty, value);
        }

        public event EventHandler<Interactivity.RoutedEventArgs>? SelectionChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="MauiEditor"/> class.
        /// </summary>
        public MauiEditor()
        {
            AcceptsReturn = true;
            TextWrapping = TextWrapping.Wrap;
            
            // Ensure selection is visible
            SelectionBrush = Brushes.Blue;
            SelectionForegroundBrush = Brushes.White;
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == CaretIndexProperty ||
                change.Property == SelectionStartProperty ||
                change.Property == SelectionEndProperty)
            {
                RaiseSelectionChanged();
            }
        }

        public void RaiseSelectionChanged()
        {
            SelectionChanged?.Invoke(this, new Interactivity.RoutedEventArgs());
        }
    }
}
