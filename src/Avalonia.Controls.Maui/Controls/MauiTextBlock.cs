using Avalonia.Layout;
using Avalonia.Media;

namespace Avalonia.Controls.Maui.Controls
{
    /// <summary>
    /// Text block for MAUI labels that can align its text vertically within its height.
    /// </summary>
    public class MauiTextBlock : TextBlock
    {
        /// <summary>Defines the <see cref="VerticalTextAlignment"/> property.</summary>
        public static readonly StyledProperty<VerticalAlignment> VerticalTextAlignmentProperty =
            AvaloniaProperty.Register<MauiTextBlock, VerticalAlignment>(nameof(VerticalTextAlignment), VerticalAlignment.Top);

        static MauiTextBlock()
        {
            AffectsRender<MauiTextBlock>(VerticalTextAlignmentProperty);
        }

        /// <summary>
        /// Gets the style key override, resolving to the base <see cref="TextBlock"/> type.
        /// </summary>
        protected override System.Type StyleKeyOverride => typeof(TextBlock);

        /// <summary>
        /// Gets or sets the vertical position of the text within the space inside the padding.
        /// </summary>
        public VerticalAlignment VerticalTextAlignment
        {
            get => GetValue(VerticalTextAlignmentProperty);
            set => SetValue(VerticalTextAlignmentProperty, value);
        }

        /// <inheritdoc/>
        protected override Size ArrangeOverride(Size finalSize)
        {
            var size = base.ArrangeOverride(finalSize);
            UpdateClipToBounds(size);
            return size;
        }

        // Text is measured by advance width, which glyphs such as italic letters, a hooked f or
        // a script flourish draw past. Text that fits is not clipped, so those glyphs are drawn
        // whole; text that does not fit is clipped to the label's bounds.
        private void UpdateClipToBounds(Size size)
        {
            // Allows for layout rounding
            const double tolerance = 0.5;

            var padding = Padding;
            var fits = TextLayout.WidthIncludingTrailingWhitespace + padding.Left + padding.Right <= size.Width + tolerance
                && TextLayout.Height + padding.Top + padding.Bottom <= size.Height + tolerance;

            if (fits)
                ClipToBounds = false;
            else
                ClearValue(ClipToBoundsProperty);
        }

        /// <inheritdoc/>
        protected override void RenderTextLayout(DrawingContext context, Point origin)
        {
            var space = Bounds.Height - Padding.Top - Padding.Bottom - TextLayout.Height;
            if (space > 0)
            {
                origin = VerticalTextAlignment switch
                {
                    VerticalAlignment.Center => origin.WithY(origin.Y + space / 2),
                    VerticalAlignment.Bottom => origin.WithY(origin.Y + space),
                    _ => origin,
                };
            }

            base.RenderTextLayout(context, origin);
        }
    }
}
