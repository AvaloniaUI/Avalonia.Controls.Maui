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
                InvalidateMeasure();
                InvalidateVisual();
            }
            else if (change.Property == BoundsProperty)
            {
                _backgroundGeometry = null;
                _strokeGeometry = null;
            }
            else if (change.Property == StrokeThicknessProperty ||
                     change.Property == StrokeProperty ||
                     change.Property == StrokeDashPatternProperty ||
                     change.Property == StrokeDashOffsetProperty ||
                     change.Property == StrokeLineCapProperty ||
                     change.Property == StrokeLineJoinProperty ||
                     change.Property == StrokeMiterLimitProperty ||
                     change.Property == PaddingProperty)
            {
                _backgroundGeometry = null;
                _strokeGeometry = null;
            }
        }

        private void OnShapePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            InvalidateShape();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var strokeThickness = StrokeThickness;
            var padding = Padding;
            var constraint = availableSize.Deflate(new Thickness(strokeThickness) + padding);

            if (Child != null)
            {
                Child.Measure(constraint);
                var childSize = Child.DesiredSize;
                return new Size(
                    childSize.Width + strokeThickness * 2 + padding.Left + padding.Right,
                    childSize.Height + strokeThickness * 2 + padding.Top + padding.Bottom);
            }

            return new Size(strokeThickness * 2 + padding.Left + padding.Right,
                strokeThickness * 2 + padding.Top + padding.Bottom);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            // Update geometries used for rendering
            // Both background and stroke use the same geometry path (stroke is centered on this path)
            if (_strokeGeometry == null || _lastRenderSize != finalSize)
            {
                // Use StrokeThickness/2 offset so the path is at the center of where the stroke will be drawn
                // Background fills this path, stroke is drawn on this path
                _strokeGeometry = CreateGeometry(finalSize, StrokeThickness / 2);
                _backgroundGeometry = _strokeGeometry;
                _lastRenderSize = finalSize;
            }

            if (Child != null)
            {
                var strokeThickness = StrokeThickness;
                var padding = Padding;
                // Child is arranged with only padding - content can extend under the stroke
                var childRect = new Rect(finalSize).Deflate(padding);
                Child.Arrange(childRect);

                if (Shape != null)
                {
                    // Clip child to the stroke path (same geometry used for stroke/background)
                    Child.Clip = CreateClipGeometry(childRect.Size, strokeThickness);
                }
                else
                {
                    Child.Clip = null;
                }
            }
            else
            {
                Clip = null;
            }

            return finalSize;
        }

        private Geometry? CreateClipGeometry(Size childSize, double strokeThickness)
        {
            if (Shape == null)
                return null;

            // The clip should match the stroke path (at StrokeThickness/2 offset from outer edge)
            // Since child is arranged at padding only, we need to offset the clip by strokeThickness/2
            var halfStroke = strokeThickness / 2;
        
            if (Shape is MauiShapes.RoundRectangle roundRectangleShape)
            {
                // Clip bounds offset by half stroke to align with stroke path
                var clipBounds = new MauiGraphics.Rect(
                    halfStroke,
                    halfStroke,
                    Math.Max(0, childSize.Width - strokeThickness),
                    Math.Max(0, childSize.Height - strokeThickness));

                var radii = roundRectangleShape.CornerRadius;
                // Adjust corner radii by half stroke (to match stroke path geometry)
                var adjustedRadii = new Microsoft.Maui.CornerRadius(
                    Math.Max(0, radii.TopLeft - halfStroke),
                    Math.Max(0, radii.TopRight - halfStroke),
                    Math.Max(0, radii.BottomLeft - halfStroke),
                    Math.Max(0, radii.BottomRight - halfStroke));

                var tempRr = new MauiShapes.RoundRectangle
                {
                    CornerRadius = adjustedRadii
                };

                var pathF = ((MauiGraphics.IShape)tempRr).PathForBounds(clipBounds);
                return ShapeExtensions.ToAvaloniaGeometry(pathF);
            }

            if (Shape is MauiGraphics.IRoundRectangle roundRectangle)
            {
                var clipBounds = new MauiGraphics.Rect(
                    halfStroke,
                    halfStroke,
                    Math.Max(0, childSize.Width - strokeThickness),
                    Math.Max(0, childSize.Height - strokeThickness));

                var innerPath = roundRectangle.InnerPathForBounds(clipBounds, (float)halfStroke);
                if (innerPath is not null)
                {
                    return ShapeExtensions.ToAvaloniaGeometry(innerPath);
                }
            }

            // For other shapes, clip using the shape path offset by half stroke
            var genericClipBounds = new MauiGraphics.Rect(
                halfStroke,
                halfStroke,
                Math.Max(0, childSize.Width - strokeThickness),
                Math.Max(0, childSize.Height - strokeThickness));
            var genericPath = Shape.PathForBounds(genericClipBounds);
            return ShapeExtensions.ToAvaloniaGeometry(genericPath);
        }

        public override void Render(DrawingContext context)
        {
            var bounds = new Rect(Bounds.Size);

            if (bounds.Width <= 0 || bounds.Height <= 0)
                return;

            // Ensure geometries are valid (ArrangeOverride usually handles this, but safe to check)
            if (_strokeGeometry == null || _lastRenderSize != bounds.Size)
            {
                // Use StrokeThickness/2 offset so the path is at the center of where the stroke will be drawn
                _strokeGeometry = CreateGeometry(bounds.Size, StrokeThickness / 2);
                _backgroundGeometry = _strokeGeometry;
                _lastRenderSize = bounds.Size;
            }

            // Draw background (fills the entire shape area)
            if (Background != null && _backgroundGeometry != null)
            {
                context.DrawGeometry(Background, null, _backgroundGeometry);
            }

            // Draw stroke (centered on the edge, handled by CreateGeometry offset)
            if (Stroke != null && StrokeThickness > 0 && _strokeGeometry != null)
            {
                var pen = CreatePen();
                context.DrawGeometry(null, pen, _strokeGeometry);
            }
        }

        private Geometry? CreateGeometry(Size size, double offset)
        {
            if (Shape is MauiShapes.RoundRectangle rr)
            {
                // Special handling for RoundRectangle to ensure concentric borders
                // We adjust the corner radius by the offset
                var rect = new Rect(size).Deflate(offset);
                
                var mauiRect = new MauiGraphics.Rect(rect.X, rect.Y, rect.Width, rect.Height);
            
                var radii = rr.CornerRadius; // MAUI CornerRadius
            
                // Adjust radius: Subtract offset, clamp to 0
                // MAUI CornerRadius constructor order: TopLeft, TopRight, BottomLeft, BottomRight
                var newRadii = new Microsoft.Maui.CornerRadius(
                    Math.Max(0, radii.TopLeft - offset),
                    Math.Max(0, radii.TopRight - offset),
                    Math.Max(0, radii.BottomLeft - offset),
                    Math.Max(0, radii.BottomRight - offset)); 
            
                // Create a temporary shape to generate the path
                // We use the temp shape to generate the PathF which we then convert to Avalonia Geometry
                var tempRr = new MauiShapes.RoundRectangle
                {
                    CornerRadius = newRadii
                };
                
                var pathF = ((MauiGraphics.IShape)tempRr).PathForBounds(mauiRect);
                return ShapeExtensions.ToAvaloniaGeometry(pathF);
            }

            if (Shape != null)
            {
                var bounds = new MauiGraphics.Rect(offset, offset, size.Width - 2 * offset, size.Height - 2 * offset);
                var pathF = Shape.PathForBounds(bounds);
                return ShapeExtensions.ToAvaloniaGeometry(pathF);
            }

            // Default to rectangle
            return new RectangleGeometry(new Rect(size).Deflate(offset));
        }

        private Pen CreatePen()
        {
            var pen = new Pen(Stroke, StrokeThickness)
            {
                LineCap = ShapeExtensions.ToAvaloniaPenLineCap(StrokeLineCap),
                LineJoin = ShapeExtensions.ToAvaloniaPenLineJoin(StrokeLineJoin),
                MiterLimit = StrokeMiterLimit
            };

            // Set dash style if pattern is provided
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

        public void InvalidateShape()
        {
            _backgroundGeometry = null;
            _strokeGeometry = null;
            InvalidateMeasure();
            InvalidateArrange();
            InvalidateVisual();
        }
    }
}