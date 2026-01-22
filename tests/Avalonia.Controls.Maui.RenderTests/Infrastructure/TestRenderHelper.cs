using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Avalonia.Controls.Maui.RenderTests.Infrastructure;

public static class TestRenderHelper
{
    public const double AllowedError = 0.025;

    public static void AssertCompareImages(string actualPath, string expectedPath)
    {
        using var actual = SixLabors.ImageSharp.Image.Load<Rgba32>(actualPath);
        using var expected = SixLabors.ImageSharp.Image.Load<Rgba32>(expectedPath);

        if (actual.Width != expected.Width || actual.Height != expected.Height)
        {
            GenerateComparisonImage(actual, expected, actualPath, sizeMismatch: true);
            Assert.Fail($"Image dimensions differ. Actual: {actual.Width}x{actual.Height}, Expected: {expected.Width}x{expected.Height}. See comparison image.");
        }

        var error = CalculateRmse(actual, expected);
        
        if (error > AllowedError)
        {
            GenerateComparisonImage(actual, expected, actualPath, sizeMismatch: false);
            Assert.Fail($"Images differ. RMSE: {error}. Tolerance: {AllowedError}. See comparison image.");
        }
    }

    private static void GenerateComparisonImage(Image<Rgba32> actual, Image<Rgba32> expected, string actualPath, bool sizeMismatch)
    {
        var maxWidth = Math.Max(actual.Width, expected.Width);
        var maxHeight = Math.Max(actual.Height, expected.Height);
        
        // Create side-by-side comparison: Expected | Actual | Diff
        var comparisonWidth = maxWidth * 3 + 4; // 2px border between images
        var comparisonHeight = maxHeight + 20; // Space for labels
        
        using var comparison = new Image<Rgba32>(comparisonWidth, comparisonHeight);
        
        // Fill with gray background
        comparison.Mutate(ctx => ctx.BackgroundColor(new Rgba32(128, 128, 128)));
        
        // Draw expected image
        for (int y = 0; y < expected.Height; y++)
        {
            for (int x = 0; x < expected.Width; x++)
            {
                comparison[x, y + 20] = expected[x, y];
            }
        }
        
        // Draw actual image
        var actualOffsetX = maxWidth + 2;
        for (int y = 0; y < actual.Height; y++)
        {
            for (int x = 0; x < actual.Width; x++)
            {
                comparison[actualOffsetX + x, y + 20] = actual[x, y];
            }
        }
        
        // Draw diff image (highlight differences in red)
        var diffOffsetX = maxWidth * 2 + 4;
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
                        comparison[diffOffsetX + x, y + 20] = new Rgba32(intensity, 0, 0, 255);
                    }
                    else
                    {
                        // No difference: show grayscale of original
                        var gray = (byte)((p1.R + p1.G + p1.B) / 3);
                        comparison[diffOffsetX + x, y + 20] = new Rgba32(gray, gray, gray, 255);
                    }
                }
            }
        }
        
        // Draw labels (simple pixel text would be complex, so we use colored bars)
        // Green bar for Expected, Blue bar for Actual, Red bar for Diff
        for (int x = 0; x < maxWidth; x++)
        {
            for (int y = 0; y < 16; y++)
            {
                comparison[x, y + 2] = new Rgba32(0, 180, 0, 255); // Green for Expected
                comparison[actualOffsetX + x, y + 2] = new Rgba32(0, 120, 215, 255); // Blue for Actual
                comparison[diffOffsetX + x, y + 2] = new Rgba32(215, 0, 0, 255); // Red for Diff
            }
        }
        
        var comparisonPath = actualPath.Replace(".out.png", ".diff.png");
        comparison.Save(comparisonPath);
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