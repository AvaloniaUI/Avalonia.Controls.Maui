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
        /// Occurs when the text selection changes.
        /// </summary>
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

        /// <inheritdoc/>
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

        /// <summary>
        /// Raises the <see cref="SelectionChanged"/> event.
        /// </summary>
        public void RaiseSelectionChanged()
        {
            SelectionChanged?.Invoke(this, new Interactivity.RoutedEventArgs());
        }
    }
}
