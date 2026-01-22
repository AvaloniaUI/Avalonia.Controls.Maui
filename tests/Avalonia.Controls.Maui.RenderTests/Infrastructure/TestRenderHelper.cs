using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Avalonia.Controls.Maui.RenderTests.Infrastructure;

public static class TestRenderHelper
{
    public const double DefaultAllowedError = 0.02;

    public static void AssertCompareImages(string actualPath, string expectedPath, double? tolerance = null)
    {
        var allowedError = tolerance ?? DefaultAllowedError;
        
        using var actual = SixLabors.ImageSharp.Image.Load<Rgba32>(actualPath);
        using var expected = SixLabors.ImageSharp.Image.Load<Rgba32>(expectedPath);

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

    private static void GenerateComparisonImage(Image<Rgba32> actual, Image<Rgba32> expected, string actualPath, bool sizeMismatch)
    {
        try
        {
            var maxWidth = Math.Max(actual.Width, expected.Width);
            var maxHeight = Math.Max(actual.Height, expected.Height);
            
            // Create side-by-side comparison: Expected | Actual | Diff
            var labelHeight = 20;
            var comparisonWidth = maxWidth * 3 + 4; // 2px border between images
            var comparisonHeight = maxHeight + labelHeight;
            
            using var comparison = new Image<Rgba32>(comparisonWidth, comparisonHeight);
            
            // Fill with gray background
            comparison.Mutate(ctx => ctx.BackgroundColor(new Rgba32(128, 128, 128)));
            
            // Draw colored label bars: Green=Expected, Blue=Actual, Red=Diff
            var actualOffsetX = maxWidth + 2;
            var diffOffsetX = maxWidth * 2 + 4;
            
            for (int x = 0; x < maxWidth; x++)
            {
                for (int y = 2; y < labelHeight - 2; y++)
                {
                    comparison[x, y] = new Rgba32(0, 180, 0, 255); // Green for Expected
                    comparison[actualOffsetX + x, y] = new Rgba32(0, 120, 215, 255); // Blue for Actual
                    comparison[diffOffsetX + x, y] = new Rgba32(215, 0, 0, 255); // Red for Diff
                }
            }
            
            // Draw expected image
            for (int y = 0; y < expected.Height; y++)
            {
                for (int x = 0; x < expected.Width; x++)
                {
                    comparison[x, y + labelHeight] = expected[x, y];
                }
            }
            
            // Draw actual image
            for (int y = 0; y < actual.Height; y++)
            {
                for (int x = 0; x < actual.Width; x++)
                {
                    comparison[actualOffsetX + x, y + labelHeight] = actual[x, y];
                }
            }
            
            // Draw diff image (highlight differences in red)
            if (!sizeMismatch)
            {
                for (int y = 0; y < actual.Height; y++)
                {
                    for (int x = 0; x < actual.Width; x++)
                    {
                        var p1 = actual[x, y];
                        var p2 = expected[x, y];
                        
                        var rDiff = Math.Abs(p1.R - p2.R);
                        var gDiff = Math.Abs(p1.G - p2.G);
                        var bDiff = Math.Abs(p1.B - p2.B);
                        var maxDiff = Math.Max(rDiff, Math.Max(gDiff, bDiff));
                        
                        if (maxDiff > 0)
                        {
                            // Highlight differences: amplify and show in red channel
                            var intensity = (byte)Math.Min(255, maxDiff * 4);
                            comparison[diffOffsetX + x, y + labelHeight] = new Rgba32(intensity, 0, 0, 255);
                        }
                        else
                        {
                            // No difference: show grayscale of original
                            var gray = (byte)((p1.R + p1.G + p1.B) / 3);
                            comparison[diffOffsetX + x, y + labelHeight] = new Rgba32(gray, gray, gray, 255);
                        }
                    }
                }
            }
            
            var comparisonPath = actualPath.Replace(".out.png", ".diff.png");
            comparison.Save(comparisonPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to generate comparison image: {ex.Message}");
        }
    }

    private static double CalculateRmse(Image<Rgba32> actual, Image<Rgba32> expected)
    {
        double totalError = 0;
        int pixels = actual.Width * actual.Height;

        for (int y = 0; y < actual.Height; y++)
        {
            for (int x = 0; x < actual.Width; x++)
            {
                var p1 = actual[x, y];
                var p2 = expected[x, y];

                var rDiff = p1.R - p2.R;
                var gDiff = p1.G - p2.G;
                var bDiff = p1.B - p2.B;

                totalError += rDiff * (double)rDiff + gDiff * (double)gDiff + bDiff * (double)bDiff;
            }
        }

        return Math.Sqrt(totalError / pixels) / 255.0;
    }
}