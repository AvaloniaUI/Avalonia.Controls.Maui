using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Controls.Maui.Tests.TestUtilities;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using MauiBoxViewHandler = Avalonia.Controls.Maui.Handlers.BoxViewHandler;
using MauiCornerRadius = Microsoft.Maui.CornerRadius;
using AvaloniaCornerRadius = Avalonia.CornerRadius;

namespace Avalonia.Controls.Maui.Tests.Handlers;

/// <summary>
/// Tests for BoxViewHandler that maps BoxView to Border.
/// </summary>
public partial class BoxViewHandlerTests : HandlerTestBase<MauiBoxViewHandler, BoxViewStub>
{
    [AvaloniaFact(DisplayName = "Color Initializes Correctly")]
    public async Task ColorInitializesCorrectly()
    {
        var boxView = new BoxViewStub
        {
            Color = Colors.Red
        };

        var values = await GetValueAsync(boxView, (handler) =>
        {
            return new
            {
                ViewValue = boxView.Color,
                PlatformViewValue = GetPlatformColor(handler)
            };
        });

        Assert.NotNull(values.PlatformViewValue);
        ColorComparisonHelpers.AssertColorsAreEqual(Colors.Red, values.PlatformViewValue);
    }

    [AvaloniaTheory(DisplayName = "Color Updates Correctly")]
    [InlineData(255, 0, 0)]      // Red
    [InlineData(0, 255, 0)]      // Green
    [InlineData(0, 0, 255)]      // Blue
    [InlineData(255, 255, 0)]    // Yellow
    [InlineData(128, 128, 128)]  // Gray
    public async Task ColorUpdatesCorrectly(byte r, byte g, byte b)
    {
        var boxView = new BoxViewStub
        {
            Color = Colors.White
        };

        var newColor = Color.FromRgb(r, g, b);

        var values = await GetValueAsync(boxView, (handler) =>
        {
            boxView.Color = newColor;
            handler.UpdateValue(nameof(BoxView.Color));

            return new
            {
                ViewValue = boxView.Color,
                PlatformViewValue = GetPlatformColor(handler)
            };
        });

        Assert.NotNull(values.PlatformViewValue);
        ColorComparisonHelpers.AssertColorsAreEqual(newColor, values.PlatformViewValue);
    }

    [AvaloniaFact(DisplayName = "Null Color Doesn't Crash")]
    public async Task NullColorDoesntCrash()
    {
        var boxView = new BoxViewStub
        {
            Color = null!
        };

        await CreateHandlerAsync(boxView);
    }

    [AvaloniaFact(DisplayName = "Null Color Clears Background")]
    public async Task NullColorClearsBackground()
    {
        var boxView = new BoxViewStub
        {
            Color = Colors.Red
        };

        var handler = await CreateHandlerAsync(boxView);
        
        var colorBeforeNull = GetPlatformColor(handler);
        Assert.NotNull(colorBeforeNull);

        boxView.Color = null!;
        handler.UpdateValue(nameof(BoxView.Color));

        var border = handler.PlatformView as Border;
        Assert.NotNull(border);
        Assert.Null(border.Background);
    }

    [AvaloniaFact(DisplayName = "Transparent Color Works")]
    public async Task TransparentColorWorks()
    {
        var boxView = new BoxViewStub
        {
            Color = Colors.Transparent
        };

        var handler = await CreateHandlerAsync(boxView);
        var color = GetPlatformColor(handler);

        Assert.NotNull(color);
        Assert.Equal(0, color.Alpha, 0.01f);
    }

    [AvaloniaTheory(DisplayName = "Semi-Transparent Colors Work")]
    [InlineData(128)] // 50% alpha
    [InlineData(64)]  // 25% alpha
    [InlineData(192)] // 75% alpha
    public async Task SemiTransparentColorsWork(byte alpha)
    {
        var color = Color.FromRgba((byte)255, (byte)0, (byte)0, alpha);
        var boxView = new BoxViewStub
        {
            Color = color
        };

        var handler = await CreateHandlerAsync(boxView);
        var nativeColor = GetPlatformColor(handler);

        Assert.NotNull(nativeColor);
        Assert.Equal(alpha / 255f, nativeColor.Alpha, 0.02f);
    }

    [AvaloniaFact(DisplayName = "Color Maps To Border Background")]
    public async Task ColorMapsToBorderBackground()
    {
        var color = Colors.Purple;
        var boxView = new BoxViewStub
        {
            Color = color
        };

        var handler = await CreateHandlerAsync(boxView);
        var border = handler.PlatformView as Border;

        Assert.NotNull(border);
        Assert.NotNull(border.Background);
        Assert.IsType<Media.SolidColorBrush>(border.Background);
        
        var brush = (Media.SolidColorBrush)border.Background;
        var nativeColor = Color.FromRgba(brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A);
        
        ColorComparisonHelpers.AssertColorsAreEqual(color, nativeColor);
    }

    [AvaloniaFact(DisplayName = "CornerRadius Initializes Correctly")]
    public async Task CornerRadiusInitializesCorrectly()
    {
        var cornerRadius = new MauiCornerRadius(10);
        var boxView = new BoxViewStub
        {
            Color = Colors.Blue,
            CornerRadius = cornerRadius
        };

        var values = await GetValueAsync(boxView, (handler) =>
        {
            return new
            {
                ViewValue = boxView.CornerRadius,
                PlatformViewValue = GetPlatformCornerRadius(handler)
            };
        });

        Assert.Equal(cornerRadius.TopLeft, values.PlatformViewValue.TopLeft);
        Assert.Equal(cornerRadius.TopRight, values.PlatformViewValue.TopRight);
        Assert.Equal(cornerRadius.BottomLeft, values.PlatformViewValue.BottomLeft);
        Assert.Equal(cornerRadius.BottomRight, values.PlatformViewValue.BottomRight);
    }

    [AvaloniaTheory(DisplayName = "CornerRadius Updates Correctly")]
    [InlineData(0, 0, 0, 0)]
    [InlineData(5, 5, 5, 5)]
    [InlineData(10, 20, 30, 40)]
    [InlineData(15, 0, 15, 0)]
    public async Task CornerRadiusUpdatesCorrectly(double topLeft, double topRight, double bottomRight, double bottomLeft)
    {
        var boxView = new BoxViewStub
        {
            Color = Colors.Blue,
            CornerRadius = new MauiCornerRadius(0)
        };

        var newCornerRadius = new MauiCornerRadius(topLeft, topRight, bottomRight, bottomLeft);

        var values = await GetValueAsync(boxView, (handler) =>
        {
            boxView.CornerRadius = newCornerRadius;
            handler.UpdateValue(nameof(BoxView.CornerRadius));

            return new
            {
                ViewValue = boxView.CornerRadius,
                PlatformViewValue = GetPlatformCornerRadius(handler)
            };
        });

        Assert.Equal(topLeft, values.PlatformViewValue.TopLeft);
        Assert.Equal(topRight, values.PlatformViewValue.TopRight);
        Assert.Equal(bottomRight, values.PlatformViewValue.BottomRight);
        Assert.Equal(bottomLeft, values.PlatformViewValue.BottomLeft);
    }

    [AvaloniaFact(DisplayName = "Uniform CornerRadius Works")]
    public async Task UniformCornerRadiusWorks()
    {
        var boxView = new BoxViewStub
        {
            Color = Colors.Green,
            CornerRadius = new MauiCornerRadius(15)
        };

        var handler = await CreateHandlerAsync(boxView);
        var cornerRadius = GetPlatformCornerRadius(handler);

        Assert.Equal(15, cornerRadius.TopLeft);
        Assert.Equal(15, cornerRadius.TopRight);
        Assert.Equal(15, cornerRadius.BottomLeft);
        Assert.Equal(15, cornerRadius.BottomRight);
    }

    [AvaloniaFact(DisplayName = "Zero CornerRadius Creates Sharp Corners")]
    public async Task ZeroCornerRadiusCreatesSharpCorners()
    {
        var boxView = new BoxViewStub
        {
            Color = Colors.Purple,
            CornerRadius = new MauiCornerRadius(0)
        };

        var handler = await CreateHandlerAsync(boxView);
        var cornerRadius = GetPlatformCornerRadius(handler);

        Assert.Equal(0, cornerRadius.TopLeft);
        Assert.Equal(0, cornerRadius.TopRight);
        Assert.Equal(0, cornerRadius.BottomLeft);
        Assert.Equal(0, cornerRadius.BottomRight);
    }

    [AvaloniaFact(DisplayName = "Large CornerRadius Works")]
    public async Task LargeCornerRadiusWorks()
    {
        var boxView = new BoxViewStub
        {
            Color = Colors.Orange,
            CornerRadius = new MauiCornerRadius(50)
        };

        var handler = await CreateHandlerAsync(boxView);
        var cornerRadius = GetPlatformCornerRadius(handler);

        Assert.Equal(50, cornerRadius.TopLeft);
        Assert.Equal(50, cornerRadius.TopRight);
        Assert.Equal(50, cornerRadius.BottomLeft);
        Assert.Equal(50, cornerRadius.BottomRight);
    }

    [AvaloniaFact(DisplayName = "Small CornerRadius Values Work")]
    public async Task SmallCornerRadiusValuesWork()
    {
        var boxView = new BoxViewStub
        {
            Color = Colors.Blue,
            CornerRadius = new MauiCornerRadius(0.5)
        };

        var handler = await CreateHandlerAsync(boxView);
        var cornerRadius = GetPlatformCornerRadius(handler);

        Assert.Equal(0.5, cornerRadius.TopLeft, 0.01);
        Assert.Equal(0.5, cornerRadius.TopRight, 0.01);
        Assert.Equal(0.5, cornerRadius.BottomLeft, 0.01);
        Assert.Equal(0.5, cornerRadius.BottomRight, 0.01);
    }

    [AvaloniaFact(DisplayName = "Mixed Corner Radii Work")]
    public async Task MixedCornerRadiiWork()
    {
        var boxView = new BoxViewStub
        {
            Color = Colors.Blue,
            CornerRadius = new MauiCornerRadius(15, 15, 0, 0)
        };

        var handler = await CreateHandlerAsync(boxView);
        var cornerRadius = GetPlatformCornerRadius(handler);

        Assert.Equal(15, cornerRadius.TopLeft);
        Assert.Equal(15, cornerRadius.TopRight);
        Assert.Equal(0, cornerRadius.BottomLeft);
        Assert.Equal(0, cornerRadius.BottomRight);
    }

    [AvaloniaTheory(DisplayName = "Updating Color Does Not Affect CornerRadius")]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(20)]
    public async Task ColorDoesNotAffectCornerRadius(double radius)
    {
        var cornerRadius = new MauiCornerRadius(radius);
        var boxView = new BoxViewStub
        {
            Color = Colors.Blue,
            CornerRadius = cornerRadius
        };

        await ValidateUnrelatedPropertyUnaffected(
            boxView,
            GetPlatformCornerRadius,
            nameof(BoxView.Color),
            () => boxView.Color = Colors.Red);
    }

    [AvaloniaTheory(DisplayName = "Updating CornerRadius Does Not Affect Color")]
    [InlineData(255, 0, 0)]
    [InlineData(0, 255, 0)]
    public async Task CornerRadiusDoesNotAffectColor(byte r, byte g, byte b)
    {
        var color = Color.FromRgb(r, g, b);
        var boxView = new BoxViewStub
        {
            Color = color,
            CornerRadius = new MauiCornerRadius(5)
        };

        await ValidateUnrelatedPropertyUnaffected(
            boxView,
            GetPlatformColor,
            nameof(BoxView.CornerRadius),
            () => boxView.CornerRadius = new MauiCornerRadius(15));
    }

    [AvaloniaFact(DisplayName = "Default State Has No Background")]
    public async Task DefaultStateHasNoBackground()
    {
        var boxView = new BoxViewStub();

        var handler = await CreateHandlerAsync(boxView);
        var border = handler.PlatformView as Border;

        Assert.NotNull(border);
        Assert.Null(border.Background);
    }

    [AvaloniaFact(DisplayName = "Default CornerRadius Is Zero")]
    public async Task DefaultCornerRadiusIsZero()
    {
        var boxView = new BoxViewStub
        {
            Color = Colors.Blue
        };

        var handler = await CreateHandlerAsync(boxView);
        var cornerRadius = GetPlatformCornerRadius(handler);

        Assert.Equal(0, cornerRadius.TopLeft);
        Assert.Equal(0, cornerRadius.TopRight);
        Assert.Equal(0, cornerRadius.BottomLeft);
        Assert.Equal(0, cornerRadius.BottomRight);
    }

    [AvaloniaFact(DisplayName = "Handler Creates Border")]
    public async Task HandlerCreatesBorder()
    {
        var boxView = new BoxViewStub
        {
            Color = Colors.Red
        };

        var handler = await CreateHandlerAsync(boxView);

        Assert.NotNull(handler.PlatformView);
        Assert.IsType<Border>(handler.PlatformView);
    }
    
    Color? GetPlatformColor(MauiBoxViewHandler handler)
    {
        var border = handler.PlatformView as Border;
        Assert.NotNull(border);

        if (border.Background is Media.SolidColorBrush brush)
        {
            var color = brush.Color;
            return Color.FromRgba(color.R, color.G, color.B, color.A);
        }

        return null;
    }
    
    MauiCornerRadius GetPlatformCornerRadius(MauiBoxViewHandler handler)
    {
        var border = handler.PlatformView;
        Assert.NotNull(border);

        var cr = border.CornerRadius;
        return new MauiCornerRadius(cr.TopLeft, cr.TopRight, cr.BottomRight, cr.BottomLeft);
    }
}