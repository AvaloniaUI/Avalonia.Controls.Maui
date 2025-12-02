#nullable disable
using Microsoft.Maui.Controls.Shapes;
using System.Text;

namespace Avalonia.Controls.Maui.Extensions;

/// <summary>
/// Extension methods for converting .NET MAUI geometry objects to Avalonia geometry objects.
/// </summary>
/// <remarks>
/// <para>
/// This class provides the core geometry conversion infrastructure for the Avalonia.Controls.Maui bridge.
/// It handles conversion between MAUI's <see cref="Microsoft.Maui.Controls.Shapes.Geometry"/> types
/// and Avalonia's <see cref="global::Avalonia.Media.Geometry"/> types.
/// </para>
/// <para><b>Conversion Strategy:</b></para>
/// <list type="bullet">
///   <item>Preserves geometry properties (coordinates, radii, corner radii, etc.)</item>
///   <item>Converts complex path definitions to SVG-compatible path data strings</item>
///   <item>Handles nested geometry groups recursively</item>
///   <item>Returns null for unsupported geometry types or invalid configurations</item>
/// </list>
/// </remarks>
public static class GeometryExtensions
{
    /// <summary>
    /// Converts a .NET MAUI geometry to an Avalonia platform geometry.
    /// </summary>
    /// <param name="geometry">The MAUI geometry to convert.</param>
    /// <returns>
    /// An Avalonia geometry equivalent to the input MAUI geometry, or null if:
    /// <list type="bullet">
    ///   <item>The input geometry is null</item>
    ///   <item>The geometry type is not supported</item>
    ///   <item>The geometry has invalid or missing required properties (e.g., RoundRectangleGeometry without Rect)</item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para><b>Supported Geometry Types:</b></para>
    /// <list type="table">
    ///   <listheader>
    ///     <term>MAUI Type</term>
    ///     <description>Avalonia Type</description>
    ///     <description>Properties Preserved</description>
    ///   </listheader>
    ///   <item>
    ///     <term><see cref="LineGeometry"/></term>
    ///     <description><see cref="global::Avalonia.Media.LineGeometry"/></description>
    ///     <description>StartPoint, EndPoint</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="RectangleGeometry"/></term>
    ///     <description><see cref="global::Avalonia.Media.RectangleGeometry"/></description>
    ///     <description>Rect (X, Y, Width, Height)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="EllipseGeometry"/></term>
    ///     <description><see cref="global::Avalonia.Media.EllipseGeometry"/></description>
    ///     <description>Center, RadiusX, RadiusY (converted to bounding Rect)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="RoundRectangleGeometry"/></term>
    ///     <description><see cref="global::Avalonia.Media.RectangleGeometry"/> with corner radius</description>
    ///     <description>Rect, CornerRadius (max of all corners used for uniform radius)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="PathGeometry"/></term>
    ///     <description><see cref="global::Avalonia.Media.Geometry"/> (parsed from SVG path data)</description>
    ///     <description>All figures and segments (line, bezier, arc, etc.)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="GeometryGroup"/></term>
    ///     <description><see cref="global::Avalonia.Media.GeometryGroup"/></description>
    ///     <description>FillRule, Children (converted recursively)</description>
    ///   </item>
    /// </list>
    ///
    /// <para><b>Special Cases and Limitations:</b></para>
    /// <list type="number">
    ///   <item>
    ///     <term>EllipseGeometry</term>
    ///     <description>
    ///     Converted using Center and Radii properties to create a bounding rectangle.
    ///     Formula: Rect(Center.X - RadiusX, Center.Y - RadiusY, RadiusX*2, RadiusY*2)
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term>RoundRectangleGeometry</term>
    ///     <description>
    ///     <b>Limitation:</b> Avalonia only supports uniform corner radius, not per-corner radii.
    ///     Uses the maximum of (TopLeft, TopRight, BottomLeft, BottomRight) to ensure
    ///     all corners are at least as rounded as specified.
    ///     <br/>
    ///     <b>Returns null if no Rect specified</b> - This signals to the caller (e.g., UpdateClip)
    ///     that bounds-based sizing should be used instead.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term>PathGeometry</term>
    ///     <description>
    ///     Converted to SVG path data string format (M, L, C, Q, A, Z commands).
    ///     Supports: Line, Bezier, Quadratic Bezier, Arc, PolyLine, PolyBezier, PolyQuadratic segments.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term>GeometryGroup</term>
    ///     <description>
    ///     Recursively converts all children. Null children are skipped.
    ///     FillRule mapped: EvenOdd → EvenOdd, NonZero → NonZero.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// <para><b>Usage Examples:</b></para>
    /// <code>
    /// // Convert ellipse geometry
    /// var mauiEllipse = new EllipseGeometry
    /// {
    ///     Center = new Point(100, 100),
    ///     RadiusX = 50,
    ///     RadiusY = 30
    /// };
    /// var avaloniaGeometry = mauiEllipse.ToPlatform();
    /// // Returns: EllipseGeometry with Rect(50, 70, 100, 60)
    ///
    /// // Convert round rectangle with explicit rect
    /// var mauiRoundRect = new RoundRectangleGeometry
    /// {
    ///     Rect = new Rect(0, 0, 200, 100),
    ///     CornerRadius = new CornerRadius(10, 20, 30, 40)
    /// };
    /// var avaloniaRoundRect = mauiRoundRect.ToPlatform();
    /// // Returns: RectangleGeometry with radius=40 (max of all corners)
    ///
    /// // Convert round rectangle without rect (bounds-based)
    /// var mauiRoundRect2 = new RoundRectangleGeometry
    /// {
    ///     CornerRadius = new CornerRadius(20)
    /// };
    /// var result = mauiRoundRect2.ToPlatform();
    /// // Returns: null (caller should use container bounds)
    /// </code>
    ///
    /// <para><b>Performance Considerations:</b></para>
    /// <list type="bullet">
    ///   <item>Simple geometries (Line, Rectangle, Ellipse) - Fast, direct object creation</item>
    ///   <item>PathGeometry - Moderate, requires string building and parsing</item>
    ///   <item>GeometryGroup - Performance depends on number and complexity of children</item>
    /// </list>
    /// </remarks>
    /// <seealso cref="Microsoft.Maui.Controls.Shapes.Geometry"/>
    /// <seealso cref="global::Avalonia.Media.Geometry"/>
    public static global::Avalonia.Media.Geometry ToPlatform(this Geometry geometry)
    {
        // Null check - return early for null input
        if (geometry is null)
            return null;

        // === LineGeometry Conversion ===
        // Simple 1:1 mapping of start and end points
        if (geometry is LineGeometry lineGeometry)
        {
            return new global::Avalonia.Media.LineGeometry(
                new global::Avalonia.Point(lineGeometry.StartPoint.X, lineGeometry.StartPoint.Y),
                new global::Avalonia.Point(lineGeometry.EndPoint.X, lineGeometry.EndPoint.Y));
        }

        // === RectangleGeometry Conversion ===
        // Direct conversion of Rect property (X, Y, Width, Height)
        if (geometry is RectangleGeometry rectGeometry)
        {
            return new global::Avalonia.Media.RectangleGeometry(
                new global::Avalonia.Rect(
                    rectGeometry.Rect.X,
                    rectGeometry.Rect.Y,
                    rectGeometry.Rect.Width,
                    rectGeometry.Rect.Height));
        }

        // === EllipseGeometry Conversion ===
        // Convert Center+Radii representation to bounding Rect representation
        // MAUI uses: Center(x,y) + RadiusX/Y
        // Avalonia uses: Rect(x, y, width, height)
        // Conversion: Top-left = (Center.X - RadiusX, Center.Y - RadiusY)
        //             Size = (RadiusX*2, RadiusY*2)
        if (geometry is EllipseGeometry ellipseGeometry)
        {
            return new global::Avalonia.Media.EllipseGeometry(
                new global::Avalonia.Rect(
                    ellipseGeometry.Center.X - ellipseGeometry.RadiusX,
                    ellipseGeometry.Center.Y - ellipseGeometry.RadiusY,
                    ellipseGeometry.RadiusX * 2,
                    ellipseGeometry.RadiusY * 2));
        }

        // === RoundRectangleGeometry Conversion ===
        // Complex case: MAUI supports per-corner radii, Avalonia only supports uniform radius
        if (geometry is RoundRectangleGeometry roundRectGeometry)
        {
            // LIMITATION: Avalonia's RectangleGeometry only supports uniform corner radius
            // MAUI's RoundRectangle has TopLeft, TopRight, BottomLeft, BottomRight radii
            // STRATEGY: Use the largest corner radius to ensure all corners are at least
            //           as rounded as the most rounded corner in the MAUI definition
            var radius = System.Math.Max(
                roundRectGeometry.CornerRadius.TopLeft,
                System.Math.Max(
                    roundRectGeometry.CornerRadius.TopRight,
                    System.Math.Max(roundRectGeometry.CornerRadius.BottomLeft,
                                    roundRectGeometry.CornerRadius.BottomRight)));

            // Check if an explicit Rect was provided
            // If yes: Create geometry with that rect
            // If no: Return null to signal caller should use container bounds
            if (roundRectGeometry.Rect is { Width: > 0, Height: > 0 } rect)
            {
                return new global::Avalonia.Media.RectangleGeometry(
                    new global::Avalonia.Rect(rect.X, rect.Y, rect.Width, rect.Height),
                    radius,  // radiusX (horizontal)
                    radius); // radiusY (vertical)
            }

            // No explicit Rect property means this should adapt to container bounds
            // Return null to signal that UpdateClip or similar method needs to provide bounds
            // Example XAML: <RoundRectangleGeometry CornerRadius="20" /> without Rect property
            return null;
        }

        // === PathGeometry Conversion ===
        // Convert complex path with figures and segments to SVG path data string
        // Then parse that string into Avalonia geometry
        if (geometry is PathGeometry pathGeometry)
        {
            // ToDefinitionString() converts the path to SVG format (M, L, C, Q, A, Z commands)
            // Avalonia's Geometry.Parse() then parses this string into a native geometry
            return Media.Geometry.Parse(pathGeometry.ToDefinitionString());
        }

        // === GeometryGroup Conversion ===
        // Container for multiple geometries with a fill rule
        if (geometry is GeometryGroup geometryGroup)
        {
            var avaloniaGroup = new global::Avalonia.Media.GeometryGroup
            {
                // Map FillRule: Controls how overlapping areas are filled
                // EvenOdd: Alternating regions are filled
                // NonZero: All regions based on winding direction
                FillRule = geometryGroup.FillRule == FillRule.EvenOdd
                    ? global::Avalonia.Media.FillRule.EvenOdd
                    : global::Avalonia.Media.FillRule.NonZero
            };

            // Recursively convert all child geometries
            foreach (var child in geometryGroup.Children)
            {
                var avaloniaChild = child.ToPlatform();
                if (avaloniaChild != null)
                    avaloniaGroup.Children.Add(avaloniaChild);
                // Null children are silently skipped (could be from unsupported types)
            }

            return avaloniaGroup;
        }

        // Unsupported geometry type - return null
        // This could be a custom geometry type or future MAUI geometry type not yet implemented
        return null;
    }

    /// <summary>
    /// Converts a PathGeometry to SVG path data string format.
    /// </summary>
    /// <param name="pathGeometry">The MAUI PathGeometry to convert.</param>
    /// <returns>
    /// An SVG-compatible path data string using standard commands:
    /// M (MoveTo), L (LineTo), C (CubicBezier), Q (QuadraticBezier), A (Arc), Z (ClosePath)
    /// </returns>
    /// <remarks>
    /// This method builds a path data string by iterating through all figures and segments,
    /// converting each to its SVG equivalent. The resulting string can be parsed by
    /// Avalonia's Geometry.Parse() method.
    /// </remarks>
    private static string ToDefinitionString(this PathGeometry pathGeometry)
    {
        var sb = new StringBuilder();

        // Process each figure (a figure is a connected series of segments)
        foreach (var figure in pathGeometry.Figures)
        {
            // M = MoveTo: Moves the current point to the start of this figure
            sb.Append($"M {figure.StartPoint.X},{figure.StartPoint.Y} ");

            // Process each segment within the figure
            foreach (var segment in figure.Segments)
            {
                // === Basic Segments ===

                if (segment is LineSegment lineSegment)
                {
                    // L = LineTo: Draw straight line to point
                    sb.Append($"L {lineSegment.Point.X},{lineSegment.Point.Y} ");
                }
                else if (segment is BezierSegment bezierSegment)
                {
                    // C = CubicBezierTo: Smooth curve with 2 control points
                    // Format: C controlPoint1 controlPoint2 endPoint
                    sb.Append($"C {bezierSegment.Point1.X},{bezierSegment.Point1.Y} ");
                    sb.Append($"{bezierSegment.Point2.X},{bezierSegment.Point2.Y} ");
                    sb.Append($"{bezierSegment.Point3.X},{bezierSegment.Point3.Y} ");
                }
                else if (segment is QuadraticBezierSegment quadSegment)
                {
                    // Q = QuadraticBezierTo: Smooth curve with 1 control point
                    // Format: Q controlPoint endPoint
                    sb.Append($"Q {quadSegment.Point1.X},{quadSegment.Point1.Y} ");
                    sb.Append($"{quadSegment.Point2.X},{quadSegment.Point2.Y} ");
                }
                else if (segment is ArcSegment arcSegment)
                {
                    // A = ArcTo: Elliptical arc curve
                    // Format: A radiusX,radiusY rotation largeArcFlag sweepFlag endPoint
                    sb.Append($"A {arcSegment.Size.Width},{arcSegment.Size.Height} ");
                    sb.Append($"{arcSegment.RotationAngle} ");
                    sb.Append($"{(arcSegment.IsLargeArc ? 1 : 0)} ");
                    sb.Append($"{(arcSegment.SweepDirection == Microsoft.Maui.Controls.SweepDirection.Clockwise ? 1 : 0)} ");
                    sb.Append($"{arcSegment.Point.X},{arcSegment.Point.Y} ");
                }

                // === Poly Segments (multiple points in one segment) ===

                else if (segment is PolyLineSegment polyLineSegment)
                {
                    // Multiple line segments in sequence
                    foreach (var point in polyLineSegment.Points)
                    {
                        sb.Append($"L {point.X},{point.Y} ");
                    }
                }
                else if (segment is PolyBezierSegment polyBezierSegment)
                {
                    // Multiple cubic bezier curves in sequence
                    // Each bezier needs 3 points: control1, control2, endpoint
                    for (int i = 0; i < polyBezierSegment.Points.Count; i += 3)
                    {
                        if (i + 2 < polyBezierSegment.Points.Count)
                        {
                            sb.Append($"C {polyBezierSegment.Points[i].X},{polyBezierSegment.Points[i].Y} ");
                            sb.Append($"{polyBezierSegment.Points[i + 1].X},{polyBezierSegment.Points[i + 1].Y} ");
                            sb.Append($"{polyBezierSegment.Points[i + 2].X},{polyBezierSegment.Points[i + 2].Y} ");
                        }
                    }
                }
                else if (segment is PolyQuadraticBezierSegment polyQuadSegment)
                {
                    // Multiple quadratic bezier curves in sequence
                    // Each quadratic bezier needs 2 points: control, endpoint
                    for (int i = 0; i < polyQuadSegment.Points.Count; i += 2)
                    {
                        if (i + 1 < polyQuadSegment.Points.Count)
                        {
                            sb.Append($"Q {polyQuadSegment.Points[i].X},{polyQuadSegment.Points[i].Y} ");
                            sb.Append($"{polyQuadSegment.Points[i + 1].X},{polyQuadSegment.Points[i + 1].Y} ");
                        }
                    }
                }
                // Note: Unknown segment types are silently skipped
            }

            // Z = ClosePath: Draw line back to start point if figure is closed
            if (figure.IsClosed)
            {
                sb.Append("Z ");
            }
        }

        // Trim trailing whitespace and return the complete path data string
        return sb.ToString().Trim();
    }
}