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
