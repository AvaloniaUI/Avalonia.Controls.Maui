using Microsoft.Maui.Graphics;
using System;

namespace Avalonia.Controls.Maui.Tests.TestUtilities;

public static class ColorComparisonHelpers
{
    public static bool ColorsAreEqual(Color? expected, Color? actual, double tolerance = 0.01)
    {
        if (expected == null && actual == null)
            return true;

        if (expected == null || actual == null)
            return false;

        return Math.Abs(expected.Red - actual.Red) < tolerance &&
               Math.Abs(expected.Green - actual.Green) < tolerance &&
               Math.Abs(expected.Blue - actual.Blue) < tolerance &&
               Math.Abs(expected.Alpha - actual.Alpha) < tolerance;
    }

    public static void AssertColorsAreEqual(Color? expected, Color? actual, double tolerance = 0.01, string? message = null)
    {
        if (!ColorsAreEqual(expected, actual, tolerance))
        {
            throw new Xunit.Sdk.XunitException(
                message ?? $"Expected color {expected}, but got {actual}");
        }
    }
}
