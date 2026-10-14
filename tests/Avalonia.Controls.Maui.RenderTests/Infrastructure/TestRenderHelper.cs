using SkiaSharp;

namespace Avalonia.Controls.Maui.RenderTests.Infrastructure;

public static class TestRenderHelper
{
    public const double DefaultAllowedError = 0.02;

    public static void AssertCompareImages(string actualPath, string expectedPath, double? tolerance = null)
    {
        var allowedError = tolerance ?? DefaultAllowedError;
        
        using var actual = LoadImage(actualPath);
        using var expected = LoadImage(expectedPath);

        if (actual.Width != expected.Width || actual.Height != expected.Height)
        {
            GenerateComparisonImage(actual, expected, actualPath, sizeMismatch: true);
            Assert.Fail($"Image dimensions differ. Actual: {actual.Width}x{actual.Height}, Expected: {expected.Width}x{expected.Height}. See comparison image.");
        }

        var error = CalculateRmse(actual, expected);
        
        if (error > allowedError)
        {
            GenerateComparisonImage(actual, expected, actualPath, sizeMismatch: false);
            Assert.Fail($"Images differ. RMSE: {error}. Tolerance: {allowedError}. See comparison image.");
        }
    }

    private static void GenerateComparisonImage(SKBitmap actual, SKBitmap expected, string actualPath, bool sizeMismatch)
    {
        try
        {
            var maxWidth = Math.Max(actual.Width, expected.Width);
            var maxHeight = Math.Max(actual.Height, expected.Height);
            
            // Create side-by-side comparison: Expected | Actual | Diff
            var labelHeight = 20;
            var comparisonWidth = maxWidth * 3 + 4; // 2px border between images
            var comparisonHeight = maxHeight + labelHeight;
            
            using var comparison = CreateImage(comparisonWidth, comparisonHeight);
            
            // Fill with gray background
            comparison.Erase(new SKColor(128, 128, 128));
            
            // Draw colored label bars: Green=Expected, Blue=Actual, Red=Diff
            var actualOffsetX = maxWidth + 2;
            var diffOffsetX = maxWidth * 2 + 4;
            
            for (int x = 0; x < maxWidth; x++)
            {
                for (int y = 2; y < labelHeight - 2; y++)
                {
                    comparison.SetPixel(x, y, new SKColor(0, 180, 0, 255)); // Green for Expected
                    comparison.SetPixel(actualOffsetX + x, y, new SKColor(0, 120, 215, 255)); // Blue for Actual
                    comparison.SetPixel(diffOffsetX + x, y, new SKColor(215, 0, 0, 255)); // Red for Diff
                }
            }
            
            // Draw expected image
            for (int y = 0; y < expected.Height; y++)
            {
                for (int x = 0; x < expected.Width; x++)
                {
                    comparison.SetPixel(x, y + labelHeight, expected.GetPixel(x, y));
                }
            }
            
            // Draw actual image
            for (int y = 0; y < actual.Height; y++)
            {
                for (int x = 0; x < actual.Width; x++)
                {
                    comparison.SetPixel(actualOffsetX + x, y + labelHeight, actual.GetPixel(x, y));
                }
            }
            
            // Draw diff image (highlight differences in red)
            if (!sizeMismatch)
            {
                for (int y = 0; y < actual.Height; y++)
                {
                    for (int x = 0; x < actual.Width; x++)
                    {
                        var p1 = actual.GetPixel(x, y);
                        var p2 = expected.GetPixel(x, y);
                        
                        var rDiff = Math.Abs(p1.Red - p2.Red);
                        var gDiff = Math.Abs(p1.Green - p2.Green);
                        var bDiff = Math.Abs(p1.Blue - p2.Blue);
                        var maxDiff = Math.Max(rDiff, Math.Max(gDiff, bDiff));
                        
                        if (maxDiff > 0)
                        {
                            // Highlight differences: amplify and show in red channel
                            var intensity = (byte)Math.Min(255, maxDiff * 4);
                            comparison.SetPixel(diffOffsetX + x, y + labelHeight, new SKColor(intensity, 0, 0, 255));
                        }
                        else
                        {
                            // No difference: show grayscale of original
                            var gray = (byte)((p1.Red + p1.Green + p1.Blue) / 3);
                            comparison.SetPixel(diffOffsetX + x, y + labelHeight, new SKColor(gray, gray, gray, 255));
                        }
                    }
                }
            }
            
            var comparisonPath = actualPath.Replace(".out.png", ".diff.png");
            using var data = comparison.Encode(SKEncodedImageFormat.Png, 100);
            using var stream = File.Create(comparisonPath);
            data.SaveTo(stream);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to generate comparison image: {ex.Message}");
        }
    }

    private static SKBitmap LoadImage(string path)
    {
        using var codec = SKCodec.Create(path) ?? throw new InvalidOperationException($"Could not decode image: {path}");
        var info = new SKImageInfo(codec.Info.Width, codec.Info.Height, SKColorType.Rgba8888, SKAlphaType.Unpremul);
        return SKBitmap.Decode(codec, info);
    }

    private static SKBitmap CreateImage(int width, int height) =>
        new(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul));

    private static double CalculateRmse(SKBitmap actual, SKBitmap expected)
    {
        double totalError = 0;
        int pixels = actual.Width * actual.Height;

        for (int y = 0; y < actual.Height; y++)
        {
            for (int x = 0; x < actual.Width; x++)
            {
                var p1 = actual.GetPixel(x, y);
                var p2 = expected.GetPixel(x, y);

                var rDiff = p1.Red - p2.Red;
                var gDiff = p1.Green - p2.Green;
                var bDiff = p1.Blue - p2.Blue;

                totalError += rDiff * (double)rDiff + gDiff * (double)gDiff + bDiff * (double)bDiff;
            }
        }

        return Math.Sqrt(totalError / pixels) / 255.0;
    }
}