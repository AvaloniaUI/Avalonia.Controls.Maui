using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Platform;
using AvaloniaFontManager = Avalonia.Controls.Maui.FontManager;
using IImage = Avalonia.Media.IImage;

namespace Avalonia.Controls.Maui.Services;

/// <summary>
/// Avalonia implementation of <see cref="IImageSourceService"/> that creates vector images of font glyphs.
/// </summary>
/// <remarks>
/// The glyph is recorded as glyph runs in a <see cref="DrawingImage"/>, with the specified color, font family,
/// weight, and style. The image is sharp at every display scale.
/// </remarks>
public partial class AvaloniaFontImageSourceService : IAvaloniaImageSourceService, IImageSourceService<IFontImageSource>
{
    private readonly ILogger<AvaloniaFontImageSourceService>? _logger;
    private readonly IFontManager? _fontManager;
    private const int DefaultSize = 100;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaloniaFontImageSourceService"/> class.
    /// </summary>
    /// <param name="fontManager">An optional MAUI font manager used to resolve registered font families.</param>
    /// <param name="logger">An optional logger for diagnostic messages during glyph rendering.</param>
    public AvaloniaFontImageSourceService(IFontManager? fontManager = null, ILogger<AvaloniaFontImageSourceService>? logger = null)
    {
        _fontManager = fontManager;
        _logger = logger;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaloniaFontImageSourceService"/> class with no dependencies.
    /// </summary>
    public AvaloniaFontImageSourceService()
    {
    }

    /// <summary>
    /// Gets the IFontManager, either from constructor injection or from the platform application services.
    /// </summary>
    private IFontManager? GetFontManager()
    {
        if (_fontManager != null)
            return _fontManager;

        // Fall back to getting IFontManager from the platform application services at runtime
        // This is needed because MauiFactory uses Activator.CreateInstance which doesn't support DI
        return IPlatformApplication.Current?.Services?.GetService<IFontManager>();
    }

    /// <summary>
    /// Attempts to create a vector image from the specified image source by casting it to <see cref="IFontImageSource"/>.
    /// </summary>
    /// <param name="imageSource">The image source to render. Must implement <see cref="IFontImageSource"/> to produce a result.</param>
    /// <param name="scale">Not used: the glyph is a vector image that is sharp at every display scale.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="IImageSourceServiceResult{IImage}"/> containing the glyph as a <see cref="DrawingImage"/>,
    /// or <see langword="null"/> if the source is not a font image source.
    /// </returns>
    public Task<IImageSourceServiceResult<IImage>?> GetImageAsync(
        IImageSource imageSource,
        float scale = 1,
        CancellationToken cancellationToken = default)
    {
        if (imageSource is IFontImageSource fontImageSource)
        {
            return GetImageAsync(fontImageSource, scale, cancellationToken);
        }

        return Task.FromResult<IImageSourceServiceResult<IImage>?>(null);
    }

    /// <summary>
    /// Creates a vector image of the font glyph specified by the <see cref="IFontImageSource"/>.
    /// </summary>
    /// <param name="imageSource">The font image source containing the glyph, font, and color information.</param>
    /// <param name="scale">Not used: the glyph is a vector image that is sharp at every display scale.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="IImageSourceServiceResult{IImage}"/> containing the glyph as a <see cref="DrawingImage"/>,
    /// or <see langword="null"/> if the source or its glyph is <see langword="null"/> or empty.
    /// </returns>
    /// <remarks>
    /// The image is sized to the glyph's ink bounds plus a one-unit border on each side, as MAUI does on Windows.
    /// </remarks>
    public Task<IImageSourceServiceResult<IImage>?> GetImageAsync(
        IFontImageSource imageSource,
        float scale = 1,
        CancellationToken cancellationToken = default)
    {
        if (imageSource == null || string.IsNullOrEmpty(imageSource.Glyph))
            return Task.FromResult<IImageSourceServiceResult<IImage>?>(null);

        // DrawingImage and DrawingGroup are AvaloniaObjects, so they are built on the UI thread
        if (!Dispatcher.UIThread.CheckAccess())
            return Dispatcher.UIThread.InvokeAsync(() => CreateImage(imageSource), DispatcherPriority.Normal, cancellationToken).GetTask();

        return Task.FromResult(CreateImage(imageSource));
    }

    private IImageSourceServiceResult<IImage>? CreateImage(IFontImageSource imageSource)
    {
        try
        {
            _logger?.LogDebug("Creating font glyph image: {Glyph}", imageSource.Glyph);

            var font = imageSource.Font;
            var fontSize = font.Size > 0 ? font.Size : DefaultSize;

            // Create Avalonia font - use FontManager if available to resolve registered fonts
            FontFamily fontFamily;
            var fontManager = GetFontManager();
            if (fontManager is AvaloniaFontManager avaloniaFontManager)
            {
                fontFamily = avaloniaFontManager.GetFontFamily(font);
            }
            else if (string.IsNullOrEmpty(font.Family))
            {
                fontFamily = FontFamily.Default;
            }
            else
            {
                fontFamily = new FontFamily(font.Family);
            }
            var fontWeight = GetAvaloniaFontWeight(font.Weight);
            var fontStyle = font.Slant == Microsoft.Maui.FontSlant.Italic ? FontStyle.Italic : FontStyle.Normal;

            var typeface = new Typeface(fontFamily, fontStyle, fontWeight);
            var avaloniaColor = GetAvaloniaColor(imageSource.Color);

            var formattedText = new FormattedText(
                imageSource.Glyph,
                System.Globalization.CultureInfo.CurrentCulture,
                Avalonia.Media.FlowDirection.LeftToRight,
                typeface,
                fontSize,
                new SolidColorBrush(avaloniaColor));

            // A glyph with no outline (such as a space) keeps its layout box
            var glyphBounds = formattedText.BuildGeometry(default)?.Bounds ?? default;
            if (glyphBounds.Width <= 0 || glyphBounds.Height <= 0)
                glyphBounds = new Rect(0, 0, formattedText.WidthIncludingTrailingWhitespace, formattedText.Height);

            // Whole units keep antialiased edges inside the border, and match the size of the bitmap this replaced
            var left = Math.Floor(glyphBounds.X) - 1;
            var top = Math.Floor(glyphBounds.Y) - 1;
            var right = Math.Ceiling(glyphBounds.Right) + 1;
            var bottom = Math.Ceiling(glyphBounds.Bottom) + 1;
            var imageRect = new Rect(left, top, Math.Max(1, right - left), Math.Max(1, bottom - top));

            // Greyscale, as on every MAUI platform
            var glyphs = new DrawingGroup();
            using (var context = glyphs.Open())
            using (context.PushTextOptions(new TextOptions { TextRenderingMode = TextRenderingMode.Antialias }))
            {
                context.DrawText(formattedText, default);
            }

            // A host's own text mode overrides a pushed one, but brush content renders in a context of its own
            var group = new DrawingGroup
            {
                Children =
                {
                    new GeometryDrawing
                    {
                        Geometry = new RectangleGeometry(imageRect),
                        Brush = new DrawingBrush(glyphs)
                        {
                            SourceRect = new RelativeRect(imageRect, RelativeUnit.Absolute),
                            Stretch = Stretch.Fill
                        }
                    }
                }
            };

            var image = new DrawingImage(group) { Viewbox = imageRect };
            return new ImageSourceServiceResult(image, resolutionDependent: false);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error creating font glyph image: {Glyph}", imageSource.Glyph);
            throw;
        }
    }

    private static Color GetAvaloniaColor(Microsoft.Maui.Graphics.Color? mauiColor)
    {
        if (mauiColor == null)
            return Colors.Black;

        return Color.FromArgb(
            (byte)(mauiColor.Alpha * 255),
            (byte)(mauiColor.Red * 255),
            (byte)(mauiColor.Green * 255),
            (byte)(mauiColor.Blue * 255)
        );
    }

    private static Avalonia.Media.FontWeight GetAvaloniaFontWeight(Microsoft.Maui.FontWeight mauiWeight)
    {
        // MAUI FontWeight is an enum, need to convert to Avalonia's int-based FontWeight
        return mauiWeight switch
        {
            Microsoft.Maui.FontWeight.Thin => Avalonia.Media.FontWeight.Thin,
            Microsoft.Maui.FontWeight.Light => Avalonia.Media.FontWeight.Light,
            Microsoft.Maui.FontWeight.Regular => Avalonia.Media.FontWeight.Regular,
            Microsoft.Maui.FontWeight.Medium => Avalonia.Media.FontWeight.Medium,
            Microsoft.Maui.FontWeight.Bold => Avalonia.Media.FontWeight.Bold,
            Microsoft.Maui.FontWeight.Black => Avalonia.Media.FontWeight.Black,
            _ => Avalonia.Media.FontWeight.Regular
        };
    }
}