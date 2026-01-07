using Microsoft.Maui.Controls;

namespace ControlGallery.Behaviors
{
    public class NumericValidationBehavior : Behavior<Entry>
    {
        protected override void OnAttachedTo(Entry entry)
        {
            entry.TextChanged += OnEntryTextChanged;
            base.OnAttachedTo(entry);
        }

        protected override void OnDetachingFrom(Entry entry)
        {
            entry.TextChanged -= OnEntryTextChanged;
            base.OnDetachingFrom(entry);
        }

        void OnEntryTextChanged(object? sender, TextChangedEventArgs args)
        {
            if (string.IsNullOrWhiteSpace(args.NewTextValue))
                return;

            bool isValid = double.TryParse(args.NewTextValue, out _);
            ((Entry)sender!).TextColor = isValid ? Microsoft.Maui.Graphics.Colors.Green : Microsoft.Maui.Graphics.Colors.Red;
        }
    }
}
