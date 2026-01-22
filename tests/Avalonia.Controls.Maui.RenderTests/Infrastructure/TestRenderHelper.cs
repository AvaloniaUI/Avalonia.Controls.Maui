using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

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
            Assert.Fail($"Image dimensions differ. Actual: {actual.Width}x{actual.Height}, Expected: {expected.Width}x{expected.Height}");
        }

        var error = CalculateRmse(actual, expected);
        
        if (error > AllowedError)
        {
            Assert.Fail($"Images differ. RMSE: {error}. Tolerance: {AllowedError}");
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