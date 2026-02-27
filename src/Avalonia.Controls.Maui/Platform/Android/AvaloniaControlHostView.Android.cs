using Android.Content;
using Avalonia.Controls.Embedding;
using Avalonia.Controls.Maui.Handlers;
using Avalonia.Media;
using Avalonia.VisualTree;
using Color = Android.Graphics.Color;

namespace Avalonia.Controls.Maui.Platforms.Android.Handlers
{
    /// <summary>
    /// Android platform host view that renders an Avalonia control inside a .NET MAUI handler.
    /// Wraps the control in a <see cref="Avalonia.Controls.Border"/> for background support.
    /// </summary>
    public class AvaloniaControlHostView : Avalonia.Android.AvaloniaView, IAvaloniaControlHost
    {
        readonly Avalonia.Controls.Border _backgroundBorder = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="AvaloniaControlHostView"/> class.
        /// </summary>
        /// <param name="context">The Android context in which the view is running.</param>
        public AvaloniaControlHostView(Context context) : base(context)
        {
            SetBackgroundColor(Color.Transparent);
            // Subscribe before setting Content — Prepare() has already been called
            // by the base AvaloniaView constructor, so setting Content triggers
            // AttachedToVisualTree synchronously.
            _backgroundBorder.AttachedToVisualTree += OnAttachedToVisualTree;
            Content = _backgroundBorder;
        }

        private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            _backgroundBorder.AttachedToVisualTree -= OnAttachedToVisualTree;
            if (e.Root is EmbeddableControlRoot root)
            {
                root.Background = Brushes.Transparent;
                root.TransparencyBackgroundFallback = Brushes.Transparent;

                // TransparencyBackgroundFallback has no property changed handler in TopLevel,
                // so the PART_TransparencyFallback border retains its white background from
                // OnApplyTemplate. Walk the visual tree to clear it directly.
                foreach (var child in root.GetVisualDescendants())
                {
                    if (child is Avalonia.Controls.Border { Name: "PART_TransparencyFallback" } fallback)
                    {
                        fallback.Background = null;
                        break;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void SetControl(Avalonia.Controls.Control? control)
        {
            _backgroundBorder.Child = control;
        }

        /// <inheritdoc/>
        public void UpdateBackground(Avalonia.Media.IBrush? brush)
        {
            _backgroundBorder.Background = brush;
        }
    }
}
