using Microsoft.Maui.Layouts;

namespace ControlGallery.Controls;

/// <summary>
/// The root stack of a sample page. Its content is as wide as the page up to <see cref="MaximumContentWidth"/>
/// and stays at the left.
/// </summary>
/// <remarks>
/// A view that fills its parent and has a MaximumWidthRequest is centered by MAUI, so the width is limited
/// in the layout manager instead.
/// </remarks>
public class PageLayout : VerticalStackLayout
{
    /// <summary>Bindable property for <see cref="MaximumContentWidth"/>.</summary>
    public static readonly BindableProperty MaximumContentWidthProperty =
        BindableProperty.Create(nameof(MaximumContentWidth), typeof(double), typeof(PageLayout), 720d,
            propertyChanged: (bindable, _, _) => ((PageLayout)bindable).InvalidateMeasure());

    /// <summary>Initializes a new instance of <see cref="PageLayout"/> with the gallery's page padding and spacing.</summary>
    public PageLayout()
    {
        Padding = new Thickness(20);
        Spacing = 24;
    }

    /// <summary>Gets or sets the largest width of the content, not counting the padding.</summary>
    public double MaximumContentWidth
    {
        get => (double)GetValue(MaximumContentWidthProperty);
        set => SetValue(MaximumContentWidthProperty, value);
    }

    /// <inheritdoc/>
    protected override ILayoutManager CreateLayoutManager() => new PageLayoutManager(this);

    private sealed class PageLayoutManager(PageLayout layout) : VerticalStackLayoutManager(layout)
    {
        public override Size Measure(double widthConstraint, double heightConstraint) =>
            base.Measure(Limit(widthConstraint), heightConstraint);

        public override Size ArrangeChildren(Rect bounds)
        {
            var size = base.ArrangeChildren(new Rect(bounds.X, bounds.Y, Limit(bounds.Width), bounds.Height));
            return new Size(bounds.Width, size.Height);
        }

        private double Limit(double width) =>
            Math.Min(width, layout.MaximumContentWidth + layout.Padding.HorizontalThickness);
    }
}
