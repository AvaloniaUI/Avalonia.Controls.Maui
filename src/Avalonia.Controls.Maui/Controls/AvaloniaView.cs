using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Controls
{
    /// <summary>
    /// A view that can host Avalonia content within a .NET MAUI application.
    /// </summary>
    [ContentProperty(nameof(Content))]
    public class AvaloniaView : View
    {
        /// <summary>
        /// Gets or sets the content to be displayed within the AvaloniaView. This can be any Avalonia control or layout.
        /// </summary>
        public static readonly BindableProperty ContentProperty =
            BindableProperty.Create(nameof(Content), typeof(object), typeof(AvaloniaView), true);

        /// <summary>
        /// Gets or sets the content to be displayed within the AvaloniaView. This can be any Avalonia control or layout.
        /// </summary>
        public object Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }
    }
}