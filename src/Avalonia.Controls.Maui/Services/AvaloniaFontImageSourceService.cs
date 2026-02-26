using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Platform;
using AvaloniaFontManager = Avalonia.Controls.Maui.FontManager;

namespace Avalonia.Controls.Maui.Services;

/// <summary>
/// Avalonia implementation of <see cref="IImageSourceService"/> that renders font glyphs as bitmap images.
/// </summary>
/// <remarks>
/// The service uses Avalonia's text rendering pipeline to draw a single glyph onto a
/// <see cref="RenderTargetBitmap"/>, applying the specified color, font family, weight, and style.
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
    /// Attempts to render a bitmap from the specified image source by casting it to <see cref="IFontImageSource"/>.
    /// </summary>
    /// <param name="imageSource">The image source to render. Must implement <see cref="IFontImageSource"/> to produce a result.</param>
    /// <param name="scale">The display scale factor applied during glyph rendering.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="IImageSourceServiceResult{Bitmap}"/> containing the rendered glyph bitmap, or <see langword="null"/>
    /// if the source is not a font image source.
    /// </returns>
    public Task<IImageSourceServiceResult<Bitmap>?> GetImageAsync(
        IImageSource imageSource,
        float scale = 1,
        CancellationToken cancellationToken = default)
    {
        if (imageSource is IFontImageSource fontImageSource)
        {
            return GetImageAsync(fontImageSource, scale, cancellationToken);
        }

        return Task.FromResult<IImageSourceServiceResult<Bitmap>?>(null);
    }

    /// <summary>
    /// Renders the font glyph specified by the <see cref="IFontImageSource"/> into an Avalonia bitmap.
    /// </summary>
    /// <param name="imageSource">The font image source containing the glyph, font, and color information.</param>
    /// <param name="scale">The display scale factor applied during glyph rendering.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="IImageSourceServiceResult{Bitmap}"/> containing the rendered glyph bitmap, or <see langword="null"/>
    /// if the source or its glyph is <see langword="null"/> or empty.
    /// </returns>
    public Task<IImageSourceServiceResult<Bitmap>?> GetImageAsync(
        IFontImageSource imageSource,
        float scale = 1,
        CancellationToken cancellationToken = default)
    {
        if (imageSource == null || string.IsNullOrEmpty(imageSource.Glyph))
            return Task.FromResult<IImageSourceServiceResult<Bitmap>?>(null);

        try
        {
            _logger?.LogDebug("Rendering font glyph: {Glyph}", imageSource.Glyph);

            var font = imageSource.Font;
            var fontSize = font.Size > 0 ? font.Size : DefaultSize;
            var size = (int)(fontSize * scale * 1.5); // Add some padding

            // Create a render target bitmap
            var pixelSize = new PixelSize(size, size);
            var dpi = new Vector(96 * scale, 96 * scale);
            var renderTarget = new RenderTargetBitmap(pixelSize, dpi);

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

            // Create formatted text
            var formattedText = new FormattedText(
                imageSource.Glyph,
                System.Globalization.CultureInfo.CurrentCulture,
                Avalonia.Media.FlowDirection.LeftToRight,
                typeface,
                fontSize * scale,
                new SolidColorBrush(avaloniaColor));

            // Render to bitmap
            using (var context = renderTarget.CreateDrawingContext())
            {
                // Calculate centered position
                var textWidth = formattedText.Width;
                var textHeight = formattedText.Height;
                var x = (size - textWidth) / 2;
                var y = (size - textHeight) / 2;

                context.DrawText(formattedText, new Point(x, y));
            }

            _logger?.LogDebug("Successfully rendered font glyph");

            return Task.FromResult<IImageSourceServiceResult<Bitmap>?>(new ImageSourceServiceResult(renderTarget));
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error rendering font glyph: {Glyph}", imageSource.Glyph);
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