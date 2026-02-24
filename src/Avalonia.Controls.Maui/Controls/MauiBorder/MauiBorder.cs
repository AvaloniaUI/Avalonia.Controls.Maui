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

        protected override Size ArrangeOverride(Size finalSize)
        {
            var arrangedSize = base.ArrangeOverride(finalSize);

            if (finalSize.Width > 0 && finalSize.Height > 0)
            {
                _strokeGeometry = CreateGeometry(finalSize);
                _backgroundGeometry = _strokeGeometry;
                _lastRenderSize = finalSize;

                if (Child != null)
                {
                    if (_strokeGeometry != null)
                    {
                        var childBounds = Child.Bounds;
                        if (childBounds.X == 0 && childBounds.Y == 0)
                        {
                            // Child starts at origin, clip geometry can be used directly
                            Child.Clip = _strokeGeometry;
                        }
                        else
                        {
                            // Child is offset within the border (e.g. centered and smaller),
                            // translate the clip geometry to the child's coordinate space
                            var clipGeometry = CreateGeometry(finalSize);
                            if (clipGeometry != null)
                            {
                                clipGeometry.Transform = new TranslateTransform(-childBounds.X, -childBounds.Y);
                            }

                            Child.Clip = clipGeometry;
                        }
                    }
                    else
                    {
                        Child.Clip = null;
                    }
                }
            }

            return arrangedSize;
        }

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

        private Geometry? CreateGeometry(Size size)
        {
            if (Shape is MauiShapes.RoundRectangle rr)
            {
                var rect = new Rect(size).Deflate(0);
                
                var mauiRect = new MauiGraphics.Rect(rect.X, rect.Y, rect.Width, rect.Height);
            
                var radii = rr.CornerRadius;
            
                var newRadii = new Microsoft.Maui.CornerRadius(
                    Math.Max(0, radii.TopLeft),
                    Math.Max(0, radii.TopRight),
                    Math.Max(0, radii.BottomLeft),
                    Math.Max(0, radii.BottomRight)); 
            
                var tempRr = new MauiShapes.RoundRectangle
                {
                    CornerRadius = newRadii
                };
                
                var pathF = ((MauiGraphics.IShape)tempRr).PathForBounds(mauiRect);
                return ShapeExtensions.ToAvaloniaGeometry(pathF);
            }

            if (Shape != null)
            {
                var bounds = new MauiGraphics.Rect(0, 0, size.Width, size.Height);
                var pathF = Shape.PathForBounds(bounds);
                return ShapeExtensions.ToAvaloniaGeometry(pathF);
            }

            return new RectangleGeometry(new Rect(size).Deflate(0));
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