using System.ComponentModel;
using Avalonia.Media;
using MauiGraphics = Microsoft.Maui.Graphics;
using MauiShapes = Microsoft.Maui.Controls.Shapes;

namespace Avalonia.Controls.Maui
{
    /// <summary>
    /// A container control that draws a border, background, or both, around another control.
    /// This is the Avalonia implementation of the .NET MAUI Border control.
    /// </summary>
    public class MauiBorder : Decorator
    {
        /// <summary>
        /// Defines the <see cref="Background"/> property.
        /// </summary>
        public static readonly StyledProperty<IBrush?> BackgroundProperty =
            AvaloniaProperty.Register<MauiBorder, IBrush?>(nameof(Background));

        /// <summary>
        /// Defines the <see cref="Stroke"/> property.
        /// </summary>
        public static readonly StyledProperty<IBrush?> StrokeProperty =
            AvaloniaProperty.Register<MauiBorder, IBrush?>(nameof(Stroke));

        /// <summary>
        /// Defines the <see cref="StrokeThickness"/> property.
        /// </summary>
        public static readonly StyledProperty<double> StrokeThicknessProperty =
            AvaloniaProperty.Register<MauiBorder, double>(nameof(StrokeThickness), 0.0);

        /// <summary>
        /// Defines the <see cref="Shape"/> property.
        /// </summary>
        public static readonly StyledProperty<MauiGraphics.IShape?> ShapeProperty =
            AvaloniaProperty.Register<MauiBorder, MauiGraphics.IShape?>(nameof(Shape));

        /// <summary>
        /// Defines the <see cref="StrokeDashPattern"/> property.
        /// </summary>
        public static readonly StyledProperty<float[]?> StrokeDashPatternProperty =
            AvaloniaProperty.Register<MauiBorder, float[]?>(nameof(StrokeDashPattern));

        /// <summary>
        /// Defines the <see cref="StrokeDashOffset"/> property.
        /// </summary>
        public static readonly StyledProperty<float> StrokeDashOffsetProperty =
            AvaloniaProperty.Register<MauiBorder, float>(nameof(StrokeDashOffset), 0f);

        /// <summary>
        /// Defines the <see cref="StrokeLineCap"/> property.
        /// </summary>
        public static readonly StyledProperty<MauiGraphics.LineCap> StrokeLineCapProperty =
            AvaloniaProperty.Register<MauiBorder, MauiGraphics.LineCap>(nameof(StrokeLineCap), MauiGraphics.LineCap.Butt);

        /// <summary>
        /// Defines the <see cref="StrokeLineJoin"/> property.
        /// </summary>
        public static readonly StyledProperty<MauiGraphics.LineJoin> StrokeLineJoinProperty =
            AvaloniaProperty.Register<MauiBorder, MauiGraphics.LineJoin>(nameof(StrokeLineJoin), MauiGraphics.LineJoin.Miter);

        /// <summary>
        /// Defines the <see cref="StrokeMiterLimit"/> property.
        /// </summary>
        public static readonly StyledProperty<float> StrokeMiterLimitProperty =
            AvaloniaProperty.Register<MauiBorder, float>(nameof(StrokeMiterLimit), 10f);

        private Geometry? _backgroundGeometry;
        private Geometry? _strokeGeometry;
        private Size _lastRenderSize;

        static MauiBorder()
        {
            AffectsRender<MauiBorder>(
                BackgroundProperty,
                StrokeProperty,
                StrokeThicknessProperty,
                ShapeProperty,
                StrokeDashPatternProperty,
                StrokeDashOffsetProperty,
                StrokeLineCapProperty,
                StrokeLineJoinProperty,
                StrokeMiterLimitProperty,
                PaddingProperty);

            AffectsMeasure<MauiBorder>(
                StrokeThicknessProperty,
                ShapeProperty,
                PaddingProperty);
        }

        /// <summary>
        /// Gets or sets the background brush.
        /// </summary>
        public IBrush? Background
        {
            get => GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the stroke brush.
        /// </summary>
        public IBrush? Stroke
        {
            get => GetValue(StrokeProperty);
            set => SetValue(StrokeProperty, value);
        }

        /// <summary>
        /// Gets or sets the stroke thickness.
        /// </summary>
        public double StrokeThickness
        {
            get => GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        /// <summary>
        /// Gets or sets the MAUI shape.
        /// </summary>
        public MauiGraphics.IShape? Shape
        {
            get => GetValue(ShapeProperty);
            set => SetValue(ShapeProperty, value);
        }

        /// <summary>
        /// Gets or sets the stroke dash pattern.
        /// </summary>
        public float[]? StrokeDashPattern
        {
            get => GetValue(StrokeDashPatternProperty);
            set => SetValue(StrokeDashPatternProperty, value);
        }

        /// <summary>
        /// Gets or sets the stroke dash offset.
        /// </summary>
        public float StrokeDashOffset
        {
            get => GetValue(StrokeDashOffsetProperty);
            set => SetValue(StrokeDashOffsetProperty, value);
        }

        /// <summary>
        /// Gets or sets the stroke line cap.
        /// </summary>
        public MauiGraphics.LineCap StrokeLineCap
        {
            get => GetValue(StrokeLineCapProperty);
            set => SetValue(StrokeLineCapProperty, value);
        }

        /// <summary>
        /// Gets or sets the stroke line join.
        /// </summary>
        public MauiGraphics.LineJoin StrokeLineJoin
        {
            get => GetValue(StrokeLineJoinProperty);
            set => SetValue(StrokeLineJoinProperty, value);
        }

        /// <summary>
        /// Gets or sets the stroke miter limit.
        /// </summary>
        public float StrokeMiterLimit
        {
            get => GetValue(StrokeMiterLimitProperty);
            set => SetValue(StrokeMiterLimitProperty, value);
        }

        /// <inheritdoc/>
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == ShapeProperty)
            {
                if (change.OldValue is INotifyPropertyChanged oldNotify)
                {
                    oldNotify.PropertyChanged -= OnShapePropertyChanged;
                }

                if (change.NewValue is INotifyPropertyChanged newNotify)
                {
                    newNotify.PropertyChanged += OnShapePropertyChanged;
                }

                _backgroundGeometry = null;
                _strokeGeometry = null;
                Child?.Clip = null;
                InvalidateMeasure();
                InvalidateVisual();
            }
            else if (change.Property == StrokeThicknessProperty ||
                     change.Property == PaddingProperty)
            {
                _backgroundGeometry = null;
                _strokeGeometry = null;
                Child?.Clip = null;
                InvalidateMeasure();
                InvalidateVisual();
            }
            else if (change.Property == StrokeProperty ||
                     change.Property == StrokeDashPatternProperty ||
                     change.Property == StrokeDashOffsetProperty ||
                     change.Property == StrokeLineCapProperty ||
                     change.Property == StrokeLineJoinProperty ||
                     change.Property == StrokeMiterLimitProperty)
            {
                InvalidateVisual();
            }
        }

        private void OnShapePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            InvalidateShape();
        }

        /// <inheritdoc/>
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            if (Shape is INotifyPropertyChanged notify)
            {
                notify.PropertyChanged -= OnShapePropertyChanged;
            }
        }

        /// <inheritdoc/>
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            if (Shape is INotifyPropertyChanged notify)
            {
                // Remove-then-add guards against double subscription since
                // OnPropertyChanged also subscribes when Shape is first set.
                notify.PropertyChanged -= OnShapePropertyChanged;
                notify.PropertyChanged += OnShapePropertyChanged;
            }
        }

        // The content goes inside the stroke and the padding, as on the native platforms
        private Thickness StrokeInset => new(StrokeThickness);

        /// <inheritdoc/>
        protected override Size MeasureOverride(Size availableSize) =>
            Layout.LayoutHelper.MeasureChild(Child, availableSize, Padding, StrokeInset);

        /// <inheritdoc/>
        protected override Size ArrangeOverride(Size finalSize)
        {
            var arrangedSize = Layout.LayoutHelper.ArrangeChild(Child, finalSize, Padding, StrokeInset);

            if (finalSize.Width > 0 && finalSize.Height > 0)
            {
                _strokeGeometry = CreateGeometry(finalSize);
                _backgroundGeometry = _strokeGeometry;
                _lastRenderSize = finalSize;

                if (Child != null)
                {
                    // Clip the child to the inner edge of the stroke, as the native platforms do.
                    // The clip is in the child's coordinate space, which is offset by padding or alignment.
                    var clipGeometry = CreateClipGeometry(finalSize);
                    if (clipGeometry != null)
                    {
                        var childBounds = Child.Bounds;
                        clipGeometry.Transform = new TranslateTransform(
                            StrokeThickness - childBounds.X,
                            StrokeThickness - childBounds.Y);
                    }

                    Child.Clip = clipGeometry;
                }
            }

            return arrangedSize;
        }

        /// <inheritdoc/>
        public override void Render(DrawingContext context)
        {
            var bounds = new Rect(Bounds.Size);

            if (bounds.Width <= 0 || bounds.Height <= 0)
                return;

            if (_strokeGeometry == null || _lastRenderSize != bounds.Size)
            {
                _strokeGeometry = CreateGeometry(bounds.Size);
                _backgroundGeometry = _strokeGeometry;
                _lastRenderSize = bounds.Size;
            }

            if (Background != null && _backgroundGeometry != null)
            {
                context.DrawGeometry(Background, null, _backgroundGeometry);
            }

            if (Stroke != null && StrokeThickness > 0 && _strokeGeometry != null)
            {
                var pen = CreatePen();
                context.DrawGeometry(null, pen, _strokeGeometry);
            }
        }

        // The stroke is drawn inside the bounds, centered on a path inset by half its thickness
        private Geometry? CreateGeometry(Size size)
        {
            var inset = StrokeThickness / 2;
            var geometry = CreateShapeGeometry(size, inset, 0);
            if (geometry != null)
                geometry.Transform = new TranslateTransform(inset, inset);

            return geometry;
        }

        // The content is clipped to the inner edge of the stroke. The geometry is at the origin;
        // ArrangeOverride moves it into the child's coordinate space.
        private Geometry? CreateClipGeometry(Size size) =>
            CreateShapeGeometry(size, StrokeThickness, StrokeThickness);

        // Creates the shape's geometry at the origin for the bounds inset by inset on each side,
        // with the corners of a round rectangle reduced by cornerRadiusReduction
        private Geometry? CreateShapeGeometry(Size size, double inset, double cornerRadiusReduction)
        {
            if (Shape == null)
                return null;

            var bounds = new MauiGraphics.Rect(
                0,
                0,
                Math.Max(0, size.Width - 2 * inset),
                Math.Max(0, size.Height - 2 * inset));

            MauiGraphics.PathF pathF;
            if (Shape is MauiShapes.RoundRectangle rr)
            {
                var radii = rr.CornerRadius;
                var roundRectangle = new MauiShapes.RoundRectangle
                {
                    CornerRadius = new Microsoft.Maui.CornerRadius(
                        Math.Max(0, radii.TopLeft - cornerRadiusReduction),
                        Math.Max(0, radii.TopRight - cornerRadiusReduction),
                        Math.Max(0, radii.BottomLeft - cornerRadiusReduction),
                        Math.Max(0, radii.BottomRight - cornerRadiusReduction))
                };
                pathF = ((MauiGraphics.IShape)roundRectangle).PathForBounds(bounds);
            }
            else
            {
                pathF = Shape.PathForBounds(bounds);
            }

            return ShapeExtensions.ToAvaloniaGeometry(pathF);
        }

        private Pen CreatePen()
        {
            var pen = new Pen(Stroke, StrokeThickness)
            {
                LineCap = ShapeExtensions.ToAvaloniaPenLineCap(StrokeLineCap),
                LineJoin = ShapeExtensions.ToAvaloniaPenLineJoin(StrokeLineJoin),
                MiterLimit = StrokeMiterLimit
            };

            if (StrokeDashPattern != null && StrokeDashPattern.Length > 0)
            {
                var dashes = new double[StrokeDashPattern.Length];
                for (int i = 0; i < StrokeDashPattern.Length; i++)
                {
                    dashes[i] = StrokeDashPattern[i];
                }
                pen.DashStyle = new DashStyle(dashes, StrokeDashOffset);
            }

            return pen;
        }

        /// <summary>
        /// Invalidates the cached shape geometry, forcing the border to recalculate and re-render its shape.
        /// </summary>
        public void InvalidateShape()
        {
            _backgroundGeometry = null;
            _strokeGeometry = null;
            Child?.Clip = null;
            InvalidateMeasure();
            InvalidateArrange();
            InvalidateVisual();
        }
    }
}