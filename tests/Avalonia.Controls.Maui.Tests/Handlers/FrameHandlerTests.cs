using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Controls.Maui.Tests.TestUtilities;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using MauiFrameHandler = Avalonia.Controls.Maui.Handlers.FrameHandler;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public partial class FrameHandlerTests : HandlerTestBase<MauiFrameHandler, FrameStub>
{
    [AvaloniaFact(DisplayName = "Handler Creates Border")]
    public async Task HandlerCreatesBorder()
    {
        var frame = new FrameStub
        {
            BorderColor = Colors.Gray,
            CornerRadius = 5
        };

        var handler = await CreateHandlerAsync(frame);

        Assert.NotNull(handler.PlatformView);
        Assert.IsType<Border>(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "BorderColor Initializes Correctly")]
    public async Task BorderColorInitializesCorrectly()
    {
        var frame = new FrameStub
        {
            BorderColor = Colors.Red,
            CornerRadius = 5
        };

        var handler = await CreateHandlerAsync(frame);
        var platformView = handler.PlatformView as Border;

        Assert.NotNull(platformView);
        Assert.NotNull(platformView.BorderBrush);

        var color = GetNativeBorderColor(handler);
        Assert.NotNull(color);
        ColorComparisonHelpers.AssertColorsAreEqual(Colors.Red, color);
    }

    [AvaloniaTheory(DisplayName = "BorderColor Updates Correctly")]
    [InlineData(255, 0, 0)]      // Red
    [InlineData(0, 255, 0)]      // Green
    [InlineData(0, 0, 255)]      // Blue
    [InlineData(128, 128, 128)]  // Gray
    public async Task BorderColorUpdatesCorrectly(byte r, byte g, byte b)
    {
        var frame = new FrameStub
        {
            BorderColor = Colors.White,
            CornerRadius = 5
        };

        var newColor = Color.FromRgb(r, g, b);

        var handler = await CreateHandlerAsync(frame);

        frame.BorderColor = newColor;
        handler.UpdateValue(nameof(FrameStub.BorderColor));

        var color = GetNativeBorderColor(handler);
        Assert.NotNull(color);
        ColorComparisonHelpers.AssertColorsAreEqual(newColor, color);
    }

    [AvaloniaFact(DisplayName = "Null BorderColor Clears Border")]
    public async Task NullBorderColorClearsBorder()
    {
        var frame = new FrameStub
        {
            BorderColor = Colors.Red,
            CornerRadius = 5
        };

        var handler = await CreateHandlerAsync(frame);
        var platformView = handler.PlatformView as Border;

        Assert.NotNull(platformView);
        Assert.NotNull(platformView.BorderBrush);

        // Clear the property so it reverts to default
        frame.ClearValue(Frame.BorderColorProperty);
        handler.UpdateValue(nameof(FrameStub.BorderColor));

        // Border should be cleared
        Assert.Null(platformView.BorderBrush);
        Assert.Equal(new global::Avalonia.Thickness(0), platformView.BorderThickness);
    }

    [AvaloniaFact(DisplayName = "CornerRadius Initializes Correctly")]
    public async Task CornerRadiusInitializesCorrectly()
    {
        var frame = new FrameStub
        {
            BorderColor = Colors.Gray,
            CornerRadius = 10
        };

        var handler = await CreateHandlerAsync(frame);
        var platformView = handler.PlatformView as Border;

        Assert.NotNull(platformView);
        Assert.Equal(10, platformView.CornerRadius.TopLeft);
        Assert.Equal(10, platformView.CornerRadius.TopRight);
        Assert.Equal(10, platformView.CornerRadius.BottomLeft);
        Assert.Equal(10, platformView.CornerRadius.BottomRight);
    }

    [AvaloniaTheory(DisplayName = "CornerRadius Updates Correctly")]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    public async Task CornerRadiusUpdatesCorrectly(float radius)
    {
        var frame = new FrameStub
        {
            BorderColor = Colors.Gray,
            CornerRadius = 0
        };

        var handler = await CreateHandlerAsync(frame);

        frame.CornerRadius = radius;
        handler.UpdateValue(nameof(FrameStub.CornerRadius));

        var platformView = handler.PlatformView as Border;
        Assert.NotNull(platformView);
        Assert.Equal(radius, platformView.CornerRadius.TopLeft);
        Assert.Equal(radius, platformView.CornerRadius.TopRight);
        Assert.Equal(radius, platformView.CornerRadius.BottomLeft);
        Assert.Equal(radius, platformView.CornerRadius.BottomRight);
    }

    [AvaloniaFact(DisplayName = "HasShadow True Adds BoxShadow")]
    public async Task HasShadowTrueAddsBoxShadow()
    {
        var frame = new FrameStub
        {
            BorderColor = Colors.Gray,
            CornerRadius = 5,
            HasShadow = true
        };

        var handler = await CreateHandlerAsync(frame);
        var platformView = handler.PlatformView as Border;

        Assert.NotNull(platformView);
        Assert.NotEqual(default(global::Avalonia.Media.BoxShadows), platformView.BoxShadow);
        Assert.True(platformView.BoxShadow.Count > 0);
    }

    [AvaloniaFact(DisplayName = "HasShadow False Has No BoxShadow")]
    public async Task HasShadowFalseHasNoBoxShadow()
    {
        var frame = new FrameStub
        {
            BorderColor = Colors.Gray,
            CornerRadius = 5,
            HasShadow = false
        };

        var handler = await CreateHandlerAsync(frame);
        var platformView = handler.PlatformView as Border;

        Assert.NotNull(platformView);
        Assert.Equal(0, platformView.BoxShadow.Count);
    }

    [AvaloniaFact(DisplayName = "HasShadow Updates From True To False")]
    public async Task HasShadowUpdatesFromTrueToFalse()
    {
        var frame = new FrameStub
        {
            BorderColor = Colors.Gray,
            CornerRadius = 5,
            HasShadow = true
        };

        var handler = await CreateHandlerAsync(frame);
        var platformView = handler.PlatformView as Border;

        Assert.NotNull(platformView);
        Assert.True(platformView.BoxShadow.Count > 0);

        // Disable shadow
        frame.HasShadow = false;
        handler.UpdateValue(nameof(FrameStub.HasShadow));

        Assert.Equal(0, platformView.BoxShadow.Count);
    }

    [AvaloniaFact(DisplayName = "HasShadow Updates From False To True")]
    public async Task HasShadowUpdatesFromFalseToTrue()
    {
        var frame = new FrameStub
        {
            BorderColor = Colors.Gray,
            CornerRadius = 5,
            HasShadow = false
        };

        var handler = await CreateHandlerAsync(frame);
        var platformView = handler.PlatformView as Border;

        Assert.NotNull(platformView);
        Assert.Equal(0, platformView.BoxShadow.Count);

        // Enable shadow
        frame.HasShadow = true;
        handler.UpdateValue(nameof(FrameStub.HasShadow));

        Assert.True(platformView.BoxShadow.Count > 0);
    }

    [AvaloniaFact(DisplayName = "BackgroundColor Updates Background")]
    public async Task BackgroundColorUpdatesBackground()
    {
        var frame = new FrameStub
        {
            BorderColor = Colors.Gray,
            CornerRadius = 5,
            HasShadow = false,
            BackgroundColor = Colors.Red
        };

        var handler = await CreateHandlerAsync(frame);
        var platformView = handler.PlatformView as Border;

        Assert.NotNull(platformView);

        var initialBackground = GetNativeBackgroundColor(handler);
        ColorComparisonHelpers.AssertColorsAreEqual(Colors.Red, initialBackground);

        frame.BackgroundColor = Colors.Blue;
        handler.UpdateValue(nameof(Frame.BackgroundColor));

        var updatedBackground = GetNativeBackgroundColor(handler);
        ColorComparisonHelpers.AssertColorsAreEqual(Colors.Blue, updatedBackground);
    }

    [AvaloniaFact(DisplayName = "Content Property Works Without Content")]
    public async Task ContentPropertyWorksWithoutContent()
    {
        // Test that Frame handler works without content
        // (Testing with actual content requires registering LabelHandler which is beyond Frame tests scope)
        var frame = new FrameStub
        {
            BorderColor = Colors.Gray,
            CornerRadius = 5,
            Content = null
        };

        var handler = await CreateHandlerAsync(frame);
        var platformView = handler.PlatformView as Border;

        Assert.NotNull(platformView);
        // When content is null, Child should also be null
        Assert.Null(platformView.Child);
    }

    [AvaloniaTheory(DisplayName = "BorderColor Does Not Affect CornerRadius")]
    [InlineData(5f)]
    [InlineData(10f)]
    [InlineData(20f)]
    public async Task BorderColorDoesNotAffectCornerRadius(float cornerRadius)
    {
        var frame = new FrameStub
        {
            BorderColor = Colors.Blue,
            CornerRadius = cornerRadius,
            HasShadow = false
        };

        await ValidateUnrelatedPropertyUnaffected(
            frame,
            handler =>
            {
                var platformView = handler.PlatformView as Border;
                return platformView?.CornerRadius.TopLeft ?? 0;
            },
            nameof(FrameStub.BorderColor),
            () => frame.BorderColor = Colors.Red);
    }

    [AvaloniaTheory(DisplayName = "CornerRadius Does Not Affect BorderColor")]
    [InlineData(255, 0, 0)]    // Red
    [InlineData(0, 255, 0)]    // Green
    [InlineData(0, 0, 255)]    // Blue
    public async Task CornerRadiusDoesNotAffectBorderColor(byte r, byte g, byte b)
    {
        var color = Color.FromRgb(r, g, b);
        var frame = new FrameStub
        {
            BorderColor = color,
            CornerRadius = 10,
            HasShadow = false
        };

        await ValidateUnrelatedPropertyUnaffected(
            frame,
            GetNativeBorderColor,
            nameof(FrameStub.CornerRadius),
            () => frame.CornerRadius = 20);
    }

    [AvaloniaTheory(DisplayName = "HasShadow Does Not Affect BorderColor")]
    [InlineData(true)]
    [InlineData(false)]
    public async Task HasShadowDoesNotAffectBorderColor(bool hasShadow)
    {
        var frame = new FrameStub
        {
            BorderColor = Colors.Purple,
            CornerRadius = 10,
            HasShadow = !hasShadow
        };

        await ValidateUnrelatedPropertyUnaffected(
            frame,
            GetNativeBorderColor,
            nameof(FrameStub.HasShadow),
            () => frame.HasShadow = hasShadow);
    }

    [AvaloniaFact(DisplayName = "Shadow Properties Are Correct")]
    public async Task ShadowPropertiesAreCorrect()
    {
        var frame = new FrameStub
        {
            BorderColor = Colors.Gray,
            CornerRadius = 5,
            HasShadow = true
        };

        var handler = await CreateHandlerAsync(frame);
        var platformView = handler.PlatformView as Border;

        Assert.NotNull(platformView);
        Assert.True(platformView.BoxShadow.Count > 0);

        var boxShadow = platformView.BoxShadow[0];
        Assert.Equal(global::Avalonia.Media.Color.FromArgb(76, 0, 0, 0), boxShadow.Color); // 30% opacity black
        Assert.Equal(0, boxShadow.OffsetX);
        Assert.Equal(2, boxShadow.OffsetY);
        Assert.Equal(6, boxShadow.Blur);
        Assert.Equal(0, boxShadow.Spread);
    }

    // Platform-specific property getters

    /// <summary>
    /// Gets the native border color from the Border control.
    /// </summary>
    Color? GetNativeBorderColor(MauiFrameHandler handler)
    {
        var platformView = handler.PlatformView as Border;
        Assert.NotNull(platformView);

        if (platformView.BorderBrush is global::Avalonia.Media.SolidColorBrush brush)
        {
            var color = brush.Color;
            return Color.FromRgba(color.R, color.G, color.B, color.A);
        }

        return null;
    }

    Color? GetNativeBackgroundColor(MauiFrameHandler handler)
    {
        var platformView = handler.PlatformView as Border;
        Assert.NotNull(platformView);

        if (platformView.Background is global::Avalonia.Media.ISolidColorBrush brush)
        {
            var color = brush.Color;
            return Color.FromRgba(color.R, color.G, color.B, color.A);
        }

        return null;
    }
}
