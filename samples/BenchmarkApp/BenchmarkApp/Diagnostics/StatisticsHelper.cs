
namespace BenchmarkApp.Diagnostics;

/// <summary>
/// Shared statistical utility methods for analyzing memory growth trends.
/// </summary>
public static class StatisticsHelper
{
    /// <summary>
    /// Computes a simple linear regression on the given (X, Y) samples.
    /// </summary>
    /// <param name="samples">List of (X, Y) data points where X is typically elapsed time and Y is memory bytes.</param>
    /// <returns>
    /// A tuple of (Slope, RSquared) where Slope is the Y growth per unit X,
    /// and RSquared indicates the goodness of fit (0..1).
    /// Returns (0, 0) if fewer than 3 samples are provided.
    /// </returns>
    public static (double Slope, double RSquared) ComputeLinearRegression(List<(double X, long Y)> samples)
    {
        int n = samples.Count;
        if (n < 3)
        {
            return (0, 0);
        }

        double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0, sumY2 = 0;
        for (int i = 0; i < n; i++)
        {
            double x = samples[i].X;
            double y = samples[i].Y;
            sumX += x;
            sumY += y;
            sumXY += x * y;
            sumX2 += x * x;
            sumY2 += y * y;
        }

        var denominator = (n * sumX2) - (sumX * sumX);
        if (denominator == 0)
        {
            return (0, 0);
        }

        var slope = ((n * sumXY) - (sumX * sumY)) / denominator;
        var intercept = (sumY - (slope * sumX)) / n;

        var yMean = sumY / n;
        var ssTot = sumY2 - (n * yMean * yMean);

        double ssRes = 0;
        for (int i = 0; i < n; i++)
        {
            double predicted = intercept + (slope * samples[i].X);
            double residual = samples[i].Y - predicted;
            ssRes += residual * residual;
        }

        var rSquared = ssTot > 0 ? 1.0 - (ssRes / ssTot) : 0;
        return (slope, rSquared);
    }
}
