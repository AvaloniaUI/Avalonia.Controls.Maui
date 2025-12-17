using Avalonia.Controls.Maui.Extensions;
using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Controls.Maui.Tests.TestUtilities;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MauiColor = Microsoft.Maui.Graphics.Color;
using MauiColors = Microsoft.Maui.Graphics.Colors;
using MauiSliderHandler = Avalonia.Controls.Maui.Handlers.SliderHandler;
using AvaloniaSlider = Avalonia.Controls.Slider;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public class SliderHandlerTests : HandlerTestBase<MauiSliderHandler, SliderStub>
{
    [AvaloniaFact(DisplayName = "MinimumTrackColor updates decrease track")]
    public async Task MinimumTrackColorUpdatesDecreaseTrack()
    {
        var slider = new SliderStub
        {
            MinimumTrackColor = MauiColors.Green
        };

        var platformColor = await GetValueAsync(slider, GetMinimumTrackColor);

        Assert.NotNull(platformColor);
        ColorComparisonHelpers.AssertColorsAreEqual(MauiColors.Green, platformColor);
    }

    [AvaloniaFact(DisplayName = "MaximumTrackColor updates increase track")]
    public async Task MaximumTrackColorUpdatesIncreaseTrack()
    {
        var slider = new SliderStub
        {
            MaximumTrackColor = MauiColors.Blue
        };

        var platformColor = await GetValueAsync(slider, GetMaximumTrackColor);

        Assert.NotNull(platformColor);
        ColorComparisonHelpers.AssertColorsAreEqual(MauiColors.Blue, platformColor);
    }

    [AvaloniaFact(DisplayName = "ThumbColor updates thumb background")]
    public async Task ThumbColorUpdatesThumbBackground()
    {
        var slider = new SliderStub
        {
            ThumbColor = MauiColors.Red
        };

        var platformColor = await GetValueAsync(slider, GetThumbColor);

        Assert.NotNull(platformColor);
        ColorComparisonHelpers.AssertColorsAreEqual(MauiColors.Red, platformColor);
    }

    [AvaloniaFact(DisplayName = "Minimum initializes correctly")]
    public async Task MinimumInitializesCorrectly()
    {
        var slider = new SliderStub { Minimum = 5, Maximum = 100 };

        await ValidatePropertyInitValue(
            slider,
            () => slider.Minimum,
            handler => GetPlatformSlider(handler).Minimum,
            5d);
    }

    [AvaloniaFact(DisplayName = "Maximum initializes correctly")]
    public async Task MaximumInitializesCorrectly()
    {
        var slider = new SliderStub { Maximum = 42 };

        await ValidatePropertyInitValue(
            slider,
            () => slider.Maximum,
            handler => GetPlatformSlider(handler).Maximum,
            42d);
    }

    [AvaloniaFact(DisplayName = "Value initializes correctly")]
    public async Task ValueInitializesCorrectly()
    {
        var slider = new SliderStub { Value = 7, Maximum = 100 };

        await ValidatePropertyInitValue(
            slider,
            () => slider.Value,
            handler => GetPlatformSlider(handler).Value,
            7d);
    }

    [AvaloniaFact(DisplayName = "Value updates correctly")]
    public async Task ValueUpdatesCorrectly()
    {
        var slider = new SliderStub { Value = 0, Maximum = 100 };

        await ValidatePropertyUpdatesValue(
            slider,
            nameof(SliderStub.Value),
            handler => GetPlatformSlider(handler).Value,
            expectedSetValue: 10d,
            expectedUnsetValue: 3d);
    }

    [AvaloniaFact(DisplayName = "Platform value change updates virtual view")]
    public async Task PlatformValueChangeUpdatesVirtualView()
    {
        var slider = new SliderStub { Minimum = 0, Maximum = 100, Value = 0 };
        var handler = await CreateHandlerAsync(slider);

        await InvokeOnMainThreadAsync(() =>
        {
            var platform = GetPlatformSlider(handler);
            platform.Value = 42;
        });

        Assert.Equal(42, slider.Value);
    }

    [AvaloniaFact(DisplayName = "Platform clamps value to maximum")]
    public async Task PlatformClampsValueToMaximum()
    {
        var slider = new SliderStub { Minimum = 0, Maximum = 10, Value = 20 };
        var handler = await CreateHandlerAsync(slider);

        var platform = GetPlatformSlider(handler);
        Assert.Equal(10, platform.Value);
        Assert.Equal(10, slider.Value);
    }

    [AvaloniaFact(DisplayName = "ThumbImageSource sets thumb image brush")]
    public async Task ThumbImageSourceSetsThumbImageBrush()
    {
        var slider = new SliderStub();
        var handler = await CreateHandlerAsync(slider);

        Avalonia.Media.IImage platformImage = await CreateBitmapAsync(32, 32);

        await InvokeOnMainThreadAsync(() =>
        {
            SliderExtensions.UpdateThumbImageSource(handler.PlatformView!, platformImage);
            Dispatcher.UIThread.RunJobs();
        });

        var brush = await InvokeOnMainThreadAsync(() =>
            InspectSlider(handler, s => GetImageBrush(GetThumb(s))));

        Assert.NotNull(brush);
    }

    private MauiColor? GetMinimumTrackColor(MauiSliderHandler handler)
    {
        var brush = InspectSlider(handler, slider => GetTrackBrush(slider, "PART_DecreaseButton"));
        return GetColorFromBrush(brush);
    }

    private MauiColor? GetMaximumTrackColor(MauiSliderHandler handler)
    {
        var brush = InspectSlider(handler, slider => GetTrackBrush(slider, "PART_IncreaseButton"));
        return GetColorFromBrush(brush);
    }

    private MauiColor? GetThumbColor(MauiSliderHandler handler)
    {
        return InspectSlider(handler, slider =>
        {
            var thumb = GetThumb(slider);
            var thumbColor = GetColorFromBrush(thumb.Background);

            if (thumbColor != null)
            {
                return thumbColor;
            }

            var thumbBorder = thumb
                .GetVisualDescendants()
                .OfType<Border>()
                .FirstOrDefault();

            var borderColor = GetColorFromBrush(thumbBorder?.Background);
            if (borderColor != null)
            {
                return borderColor;
            }

            var shape = thumb
                .GetVisualDescendants()
                .OfType<global::Avalonia.Controls.Shapes.Shape>()
                .FirstOrDefault();

            var shapeColor = GetColorFromBrush(shape?.Fill);
            if (shapeColor != null)
            {
                return shapeColor;
            }

            var resourceBrush = slider.Resources.TryGetResource("SliderThumbFill", null, out var value)
                ? value as IBrush
                : null;

            return GetColorFromBrush(resourceBrush);
        });
    }

    private static T InspectSlider<T>(MauiSliderHandler handler, Func<AvaloniaSlider, T> inspect)
    {
        var slider = handler.PlatformView;
        Assert.NotNull(slider);

        var window = new Window
        {
            Width = 300,
            Height = 80,
            Content = slider
        };

        try
        {
            window.Show();

            slider.ApplyTemplate();
            slider.UpdateLayout();

            return inspect(slider);
        }
        finally
        {
            window.Close();
        }
    }

    private static IBrush? GetTrackBrush(AvaloniaSlider slider, string partName)
    {
        var repeatButton = slider
            .GetVisualDescendants()
            .OfType<RepeatButton>()
            .FirstOrDefault(x => x.Name == partName);

        Assert.NotNull(repeatButton);

        repeatButton.ApplyTemplate();
        repeatButton.UpdateLayout();

        var directBrush = repeatButton.Background;
        if (directBrush != null)
        {
            return directBrush;
        }

        return repeatButton
            .GetVisualDescendants()
            .OfType<Border>()
            .FirstOrDefault(x => x.Name == "VisualTrack")
            ?.Background;
    }

    private static Thumb GetThumb(AvaloniaSlider slider)
    {
        var thumb = slider
            .GetVisualDescendants()
            .OfType<Thumb>()
            .FirstOrDefault();

        Assert.NotNull(thumb);

        thumb.ApplyTemplate();
        thumb.UpdateLayout();

        return thumb;
    }

    private static IBrush? GetImageBrush(Thumb thumb)
    {
        if (thumb.Background is IImageBrush imgBrush)
            return imgBrush;

        var border = thumb.GetVisualDescendants().OfType<Border>().FirstOrDefault();
        if (border?.Background is IImageBrush borderBrush)
            return borderBrush;

        return null;
    }

    private static async Task<Avalonia.Media.Imaging.RenderTargetBitmap> CreateBitmapAsync(int width, int height)
    {
        var rtb = new Avalonia.Media.Imaging.RenderTargetBitmap(
            new Avalonia.PixelSize(width, height),
            new Avalonia.Vector(96, 96));

        using (var ctx = rtb.CreateDrawingContext())
        {
            ctx.FillRectangle(
                Avalonia.Media.Brushes.Blue,
                new Avalonia.Rect(0, 0, width, height));
        }

        return rtb;
    }

    private static MauiColor? GetColorFromBrush(IBrush? brush)
    {
        if (brush is ISolidColorBrush solid)
        {
            var color = solid.Color;
            return MauiColor.FromRgba(color.R, color.G, color.B, color.A);
        }

        return null;
    }

    private static AvaloniaSlider GetPlatformSlider(MauiSliderHandler handler)
    {
        var slider = handler.PlatformView;
        Assert.NotNull(slider);
        return slider;
    }
}
