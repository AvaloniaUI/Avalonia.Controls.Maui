using System;
using System.Threading.Tasks;

namespace Avalonia.Controls.Maui.Tests.TestUtilities;

public static class AssertHelpers
{
    public static async Task AssertEventually(
        Func<bool> assertion,
        int timeout = 1000,
        int interval = 100,
        string? message = null)
    {
        var elapsed = 0;
        while (elapsed < timeout)
        {
            if (assertion())
                return;

            await Task.Delay(interval);
            elapsed += interval;
        }

        throw new TimeoutException(message ?? "Assertion timed out");
    }

    public static async Task<T> AssertEventually<T>(
        Func<T?> func,
        Func<T?, bool> assertion,
        int timeout = 1000,
        int interval = 100,
        string? message = null)
    {
        var elapsed = 0;
        T? result = default;

        while (elapsed < timeout)
        {
            result = func();
            if (assertion(result))
                return result!;

            await Task.Delay(interval);
            elapsed += interval;
        }

        throw new TimeoutException(message ?? $"Assertion timed out. Last value: {result}");
    }
}
