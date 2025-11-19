using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Controls.Maui.Tests.TestUtilities;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using MauiProgressBarHandler = Avalonia.Controls.Maui.Handlers.ProgressBarHandler;

namespace Avalonia.Controls.Maui.Tests.Handlers;

/// <summary>
/// Tests for ProgressBarHandler that maps IProgress to Avalonia ProgressBar.
/// </summary>
public partial class ProgressBarHandlerTests : HandlerTestBase<MauiProgressBarHandler, ProgressBarStub>
{
    [AvaloniaFact(DisplayName = "Progress Initializes Correctly")]
    public async Task ProgressInitializesCorrectly()
    {
        var progressBar = new ProgressBarStub
        {
            Progress = 0.5
        };

        var values = await GetValueAsync(progressBar, (handler) =>
        {
            return new
            {
                ViewValue = progressBar.Progress,
                PlatformViewValue = GetPlatformProgress(handler)
            };
        });

        Assert.Equal(0.5, values.ViewValue, 0.01);
        Assert.Equal(0.5, values.PlatformViewValue, 0.01);
    }

    [AvaloniaTheory(DisplayName = "Progress Updates Correctly")]
    [InlineData(0.0)]    // 0%
    [InlineData(0.25)]   // 25%
    [InlineData(0.5)]    // 50%
    [InlineData(0.75)]   // 75%
    [InlineData(1.0)]    // 100%
    public async Task ProgressUpdatesCorrectly(double progress)
    {
        var progressBar = new ProgressBarStub
        {
            Progress = 0.0
        };

        var values = await GetValueAsync(progressBar, (handler) =>
        {
            progressBar.Progress = progress;
            handler.UpdateValue(nameof(IProgress.Progress));

            return new
            {
                ViewValue = progressBar.Progress,
                PlatformViewValue = GetPlatformProgress(handler)
            };
        });

        Assert.Equal(progress, values.ViewValue, 0.01);
        Assert.Equal(progress, values.PlatformViewValue, 0.01);
    }

    [AvaloniaTheory(DisplayName = "Progress Clamping Works")]
    [InlineData(-0.5, 0.0)]  // Negative clamped to 0
    [InlineData(-1.0, 0.0)]  // Negative clamped to 0
    [InlineData(1.5, 1.0)]   // Over 1.0 clamped to 1.0
    [InlineData(2.0, 1.0)]   // Over 1.0 clamped to 1.0
    public async Task ProgressClampingWorks(double inputProgress, double expectedProgress)
    {
        var progressBar = new ProgressBarStub
        {
            Progress = inputProgress
        };

        var handler = await CreateHandlerAsync(progressBar);
        var platformProgress = GetPlatformProgress(handler);

        Assert.Equal(expectedProgress, platformProgress, 0.01);
    }

    [AvaloniaFact(DisplayName = "ProgressColor Initializes Correctly")]
    public async Task ProgressColorInitializesCorrectly()
    {
        var progressBar = new ProgressBarStub
        {
            Progress = 0.5,
            ProgressColor = Colors.Red
        };

        var values = await GetValueAsync(progressBar, (handler) =>
        {
            return new
            {
                ViewValue = progressBar.ProgressColor,
                PlatformViewValue = GetPlatformProgressColor(handler)
            };
        });

        Assert.NotNull(values.PlatformViewValue);
        ColorComparisonHelpers.AssertColorsAreEqual(Colors.Red, values.PlatformViewValue);
    }

    [AvaloniaTheory(DisplayName = "ProgressColor Updates Correctly")]
    [InlineData(255, 0, 0)]      // Red
    [InlineData(0, 255, 0)]      // Green
    [InlineData(0, 0, 255)]      // Blue
    [InlineData(255, 255, 0)]    // Yellow
    [InlineData(128, 0, 128)]    // Purple
    public async Task ProgressColorUpdatesCorrectly(byte r, byte g, byte b)
    {
        var progressBar = new ProgressBarStub
        {
            Progress = 0.5,
            ProgressColor = Colors.White
        };

        var newColor = Color.FromRgb(r, g, b);

        var values = await GetValueAsync(progressBar, (handler) =>
        {
            progressBar.ProgressColor = newColor;
            handler.UpdateValue(nameof(IProgress.ProgressColor));

            return new
            {
                ViewValue = progressBar.ProgressColor,
                PlatformViewValue = GetPlatformProgressColor(handler)
            };
        });

        Assert.NotNull(values.PlatformViewValue);
        ColorComparisonHelpers.AssertColorsAreEqual(newColor, values.PlatformViewValue);
    }

    [AvaloniaFact(DisplayName = "Null ProgressColor Doesn't Crash")]
    public async Task NullProgressColorDoesntCrash()
    {
        var progressBar = new ProgressBarStub
        {
            Progress = 0.5,
            ProgressColor = null!
        };

        await CreateHandlerAsync(progressBar);
    }

    [AvaloniaFact(DisplayName = "Handler Creates ProgressBar")]
    public async Task HandlerCreatesProgressBar()
    {
        var progressBar = new ProgressBarStub
        {
            Progress = 0.5
        };

        var handler = await CreateHandlerAsync(progressBar);

        Assert.NotNull(handler.PlatformView);
        Assert.IsType<global::Avalonia.Controls.ProgressBar>(handler.PlatformView);

        var platformProgressBar = handler.PlatformView as global::Avalonia.Controls.ProgressBar;
        Assert.NotNull(platformProgressBar);
        Assert.Equal(0, platformProgressBar.Minimum);
        Assert.Equal(1, platformProgressBar.Maximum);
    }

    [AvaloniaTheory(DisplayName = "Updating Progress Does Not Affect ProgressColor")]
    [InlineData(255, 0, 0)]      // Red
    [InlineData(0, 255, 0)]      // Green
    [InlineData(0, 0, 255)]      // Blue
    public async Task ProgressDoesNotAffectProgressColor(byte r, byte g, byte b)
    {
        var color = Color.FromRgb(r, g, b);
        var progressBar = new ProgressBarStub
        {
            Progress = 0.5,
            ProgressColor = color
        };

        await ValidateUnrelatedPropertyUnaffected(
            progressBar,
            GetPlatformProgressColor,
            nameof(IProgress.Progress),
            () => progressBar.Progress = 0.75);
    }

    [AvaloniaTheory(DisplayName = "Updating ProgressColor Does Not Affect Progress")]
    [InlineData(0.25)]
    [InlineData(0.5)]
    [InlineData(0.75)]
    public async Task ProgressColorDoesNotAffectProgress(double progress)
    {
        var progressBar = new ProgressBarStub
        {
            Progress = progress,
            ProgressColor = Colors.Blue
        };

        await ValidateUnrelatedPropertyUnaffected(
            progressBar,
            GetPlatformProgress,
            nameof(IProgress.ProgressColor),
            () => progressBar.ProgressColor = Colors.Red);
    }

    [AvaloniaFact(DisplayName = "Zero Progress Works")]
    public async Task ZeroProgressWorks()
    {
        var progressBar = new ProgressBarStub
        {
            Progress = 0.0
        };

        var handler = await CreateHandlerAsync(progressBar);
        var platformProgress = GetPlatformProgress(handler);

        Assert.Equal(0.0, platformProgress, 0.01);
    }

    [AvaloniaFact(DisplayName = "Full Progress Works")]
    public async Task FullProgressWorks()
    {
        var progressBar = new ProgressBarStub
        {
            Progress = 1.0
        };

        var handler = await CreateHandlerAsync(progressBar);
        var platformProgress = GetPlatformProgress(handler);

        Assert.Equal(1.0, platformProgress, 0.01);
    }

    double GetPlatformProgress(MauiProgressBarHandler handler)
    {
        var platformProgressBar = handler.PlatformView as global::Avalonia.Controls.ProgressBar;
        Assert.NotNull(platformProgressBar);
        return platformProgressBar.Value;
    }

    Color? GetPlatformProgressColor(MauiProgressBarHandler handler)
    {
        var platformProgressBar = handler.PlatformView as global::Avalonia.Controls.ProgressBar;
        Assert.NotNull(platformProgressBar);

        if (platformProgressBar.Foreground is Media.SolidColorBrush brush)
        {
            var color = brush.Color;
            return Color.FromRgba(color.R, color.G, color.B, color.A);
        }

        return null;
    }
}
