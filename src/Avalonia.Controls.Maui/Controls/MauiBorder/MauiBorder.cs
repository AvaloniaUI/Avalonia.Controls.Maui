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
            // Both background and stroke use the same geometry at StrokeThickness/2 offset
            if (_strokeGeometry == null || _lastRenderSize != finalSize)
            {
                _strokeGeometry = CreateGeometry(finalSize, StrokeThickness / 2);
                _backgroundGeometry = _strokeGeometry;
                _lastRenderSize = finalSize;
            }

            if (Child != null)
            {
                var strokeThickness = StrokeThickness;
                var padding = Padding;
                // Child is arranged inside padding only
                // Stroke is drawn at the outer edge and doesn't reduce content area
                var childRect = new Rect(finalSize).Deflate(padding);
                Child.Arrange(childRect);

                if (Shape != null)
                {
                    // Clip child to the border inner edge (relative to Child coordinates)
                    Child.Clip = CreateClipGeometry(childRect, finalSize, strokeThickness);
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

        private Geometry? CreateClipGeometry(Rect childRect, Size borderSize, double strokeThickness)
        {
            if (Shape == null)
                return null;

            // When strokeThickness is 0, padding already keeps content inside the border
            // No clipping is needed for straight edges; rounded corners are handled by the shape
            // UNLESS padding is 0, then we might need to clip to the shape
            if (strokeThickness <= 0 && childRect.X > 0 && childRect.Y > 0)
                return null;

            // Calculate the inner border rectangle (the visible area inside the stroke)
            // relative to the Border's coordinate system
            // Stroke is centered on the path at StrokeThickness/2
            // Calculate the inner border rectangle (the visible area inside the stroke)
            // relative to the Border's coordinate system
            // Stroke is centered on the path at StrokeThickness/2
            // We align the clip with the CENTER of the stroke to ensure robust overlap
            var innerStrokePos = strokeThickness / 2;
            
            var innerBorderRect = new MauiGraphics.Rect(
                innerStrokePos, 
                innerStrokePos, 
                Math.Max(0, borderSize.Width - 2 * innerStrokePos), 
                Math.Max(0, borderSize.Height - 2 * innerStrokePos));

            // Instead of offsetting the rect (which creates negative coordinates that MauiGraphics might mishandle),
            // we generate the geometry in Border coordinates and then apply a TranslateTransform.
            // This is safer and robust against shape generation quirks.
            Geometry? clipGeometry = null;

            if (Shape is MauiShapes.RoundRectangle roundRectangleShape)
            {
                // We need to adjust the corner radius to account for the stroke thickness
                // The shape's CornerRadius is for the outer edge (or center, depending on definition)
                // But effectively we want the radius at the INNER edge of the stroke.
                // We basically want the radius at the clip position (innerStrokePos)
                var radii = roundRectangleShape.CornerRadius;
                
                // Adjust radii based on the actual clip position
                var adjustedRadii = new Microsoft.Maui.CornerRadius(
                    Math.Max(0, radii.TopLeft - innerStrokePos),
                    Math.Max(0, radii.TopRight - innerStrokePos),
                    Math.Max(0, radii.BottomLeft - innerStrokePos),
                    Math.Max(0, radii.BottomRight - innerStrokePos));

                var tempRr = new MauiShapes.RoundRectangle
                {
                    CornerRadius = adjustedRadii
                };

                // Generate path for the adjusted rect and radius
                var pathF = ((MauiGraphics.IShape)tempRr).PathForBounds(innerBorderRect);
                clipGeometry = ShapeExtensions.ToAvaloniaGeometry(pathF);
            }
            else if (Shape is MauiGraphics.IRoundRectangle roundRectangle)
            {
                var pathF = roundRectangle.PathForBounds(innerBorderRect);
                clipGeometry = ShapeExtensions.ToAvaloniaGeometry(pathF);
            }
            else
            {
                // For other shapes, generate path at the clip rect location
                var genericPath = Shape.PathForBounds(innerBorderRect);
                clipGeometry = ShapeExtensions.ToAvaloniaGeometry(genericPath);
            }

            // Apply transformation to shift from Border coordinates to Child coordinates
            if (clipGeometry != null)
            {
                clipGeometry.Transform = new TranslateTransform(-childRect.X, -childRect.Y);
            }

            return clipGeometry;
        }

        public override void Render(DrawingContext context)
        {
            var bounds = new Rect(Bounds.Size);

            if (bounds.Width <= 0 || bounds.Height <= 0)
                return;

            // Ensure geometries are valid
            if (_strokeGeometry == null || _lastRenderSize != bounds.Size)
            {
                // Both background and stroke use the same geometry at StrokeThickness/2 offset
                // This way background fills to the stroke center line
                // Stroke is drawn centered on this path, so it extends from 0 to StrokeThickness at the edge
                _strokeGeometry = CreateGeometry(bounds.Size, StrokeThickness / 2);
                _backgroundGeometry = _strokeGeometry;
                _lastRenderSize = bounds.Size;
            }

            // Draw background first - fills to the stroke center line
            if (Background != null && _backgroundGeometry != null)
            {
                context.DrawGeometry(Background, null, _backgroundGeometry);
            }

            // Draw stroke on top - centered on the same path
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