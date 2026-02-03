using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Controls.Maui.Tests.TestUtilities;
using Avalonia.Media;
using MauiColors = Microsoft.Maui.Graphics.Colors;
using MauiFrameHandler = Avalonia.Controls.Maui.Compatibility.Handlers.FrameHandler;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public partial class FrameHandlerTests : HandlerTestBase<MauiFrameHandler, FrameStub>
{
    [AvaloniaFact(DisplayName = "Default Padding Initializes Correctly")]
    public async Task DefaultPaddingInitializesCorrectly()
    {
        var frame = new FrameStub();
        
        var handler = await CreateHandlerAsync(frame);
        var padding = GetNativePadding(handler);
        
        Assert.Equal(new Microsoft.Maui.Thickness(20), padding);
    }

    [AvaloniaFact(DisplayName = "BorderColor Initializes Correctly")]
    public async Task BorderColorInitializesCorrectly()
    {
        var frame = new FrameStub
        {
            BorderColor = MauiColors.Red
        };

        var platformColor = await GetValueAsync(frame, GetNativeBorderColor);

        Assert.NotNull(platformColor);
        ColorComparisonHelpers.AssertColorsAreEqual(MauiColors.Red, platformColor);
    }

    [AvaloniaFact(DisplayName = "CornerRadius Initializes Correctly")]
    public async Task CornerRadiusInitializesCorrectly()
    {
        var frame = new FrameStub
        {
            CornerRadius = 15
        };

        var handler = await CreateHandlerAsync(frame);
        var cornerRadius = GetNativeCornerRadius(handler);
        
        Assert.Equal(new CornerRadius(15), cornerRadius);
    }

    [AvaloniaFact(DisplayName = "HasShadow True Initializes Correctly")]
    public async Task HasShadowTrueInitializesCorrectly()
    {
        var frame = new FrameStub
        {
            HasShadow = true
        };

        var handler = await CreateHandlerAsync(frame);
        var hasShadow = GetNativeHasShadow(handler);
        
        Assert.True(hasShadow);
    }

    [AvaloniaFact(DisplayName = "HasShadow False Initializes Correctly")]
    public async Task HasShadowFalseInitializesCorrectly()
    {
        var frame = new FrameStub
        {
            HasShadow = false
        };

        var handler = await CreateHandlerAsync(frame);
        var hasShadow = GetNativeHasShadow(handler);
        
        Assert.False(hasShadow);
    }

    [AvaloniaFact(DisplayName = "BackgroundColor Initializes Correctly")]
    public async Task BackgroundColorInitializesCorrectly()
    {
        var frame = new FrameStub
        {
            BackgroundColor = MauiColors.LightBlue
        };

        var platformColor = await GetValueAsync(frame, GetNativeBackgroundColor);

        Assert.NotNull(platformColor);
        ColorComparisonHelpers.AssertColorsAreEqual(MauiColors.LightBlue, platformColor);
    }

    [AvaloniaFact(DisplayName = "Content Initializes Correctly")]
    public async Task ContentInitializesCorrectly()
    {
        var content = new Microsoft.Maui.Controls.Label { Text = "Test Content" };
        var frame = new FrameStub
        {
            Content = content
        };

        var handler = await CreateHandlerAsync(frame);

        Assert.NotNull(handler.PlatformView);
        Assert.NotNull(handler.PlatformView.Child);
    }

    [AvaloniaFact(DisplayName = "Null BorderColor Sets No Border")]
    public async Task NullBorderColorSetsNoBorder()
    {
        var frame = new FrameStub
        {
            BorderColor = null
        };

        var handler = await CreateHandlerAsync(frame);
        var thickness = GetNativeBorderThickness(handler);

        Assert.Equal(new Thickness(0), thickness);
    }

    [AvaloniaFact(DisplayName = "Transparent BorderColor Sets No Border")]
    public async Task TransparentBorderColorSetsNoBorder()
    {
        var frame = new FrameStub
        {
            BorderColor = MauiColors.Transparent
        };

        var handler = await CreateHandlerAsync(frame);
        var thickness = GetNativeBorderThickness(handler);

        Assert.Equal(new Thickness(0), thickness);
    }

    [AvaloniaTheory(DisplayName = "CornerRadius Updates Correctly")]
    [InlineData(0, 20)]
    [InlineData(20, 0)]
    [InlineData(10, 30)]
    public async Task CornerRadiusUpdatesCorrectly(float initial, float updated)
    {
        var frame = new FrameStub
        {
            CornerRadius = initial
        };

        var handler = await CreateHandlerAsync(frame);
        var initialRadius = GetNativeCornerRadius(handler);
        Assert.Equal(new CornerRadius(Math.Max(0, initial)), initialRadius);

        frame.CornerRadius = updated;
        handler.UpdateValue(nameof(Microsoft.Maui.Controls.Frame.CornerRadius));

        var updatedRadius = GetNativeCornerRadius(handler);
        Assert.Equal(new CornerRadius(Math.Max(0, updated)), updatedRadius);
    }

    [AvaloniaFact(DisplayName = "BorderColor Updates Correctly")]
    public async Task BorderColorUpdatesCorrectly()
    {
        var frame = new FrameStub
        {
            BorderColor = MauiColors.Red
        };

        var handler = await CreateHandlerAsync(frame);
        var initialColor = GetNativeBorderColor(handler);
        Assert.NotNull(initialColor);
        ColorComparisonHelpers.AssertColorsAreEqual(MauiColors.Red, initialColor);

        frame.BorderColor = MauiColors.Blue;
        handler.UpdateValue(nameof(Microsoft.Maui.Controls.Frame.BorderColor));

        var updatedColor = GetNativeBorderColor(handler);
        Assert.NotNull(updatedColor);
        ColorComparisonHelpers.AssertColorsAreEqual(MauiColors.Blue, updatedColor);
    }

    [AvaloniaFact(DisplayName = "HasShadow Updates Correctly")]
    public async Task HasShadowUpdatesCorrectly()
    {
        var frame = new FrameStub
        {
            HasShadow = false
        };

        var handler = await CreateHandlerAsync(frame);
        Assert.False(GetNativeHasShadow(handler));

        frame.HasShadow = true;
        handler.UpdateValue(nameof(Microsoft.Maui.Controls.Frame.HasShadow));

        Assert.True(GetNativeHasShadow(handler));
    }

    [AvaloniaFact(DisplayName = "Padding Updates Correctly")]
    public async Task PaddingUpdatesCorrectly()
    {
        var frame = new FrameStub
        {
            Padding = new Microsoft.Maui.Thickness(10)
        };

        var handler = await CreateHandlerAsync(frame);
        var initialPadding = GetNativePadding(handler);
        Assert.Equal(new Microsoft.Maui.Thickness(10), initialPadding);

        frame.Padding = new Microsoft.Maui.Thickness(5, 10, 15, 20);
        handler.UpdateValue(nameof(Microsoft.Maui.Controls.Frame.Padding));

        var updatedPadding = GetNativePadding(handler);
        Assert.Equal(new Microsoft.Maui.Thickness(5, 10, 15, 20), updatedPadding);
    }

    [AvaloniaFact(DisplayName = "IsClippedToBounds True Initializes Correctly")]
    public async Task IsClippedToBoundsTrueInitializesCorrectly()
    {
        var frame = new FrameStub
        {
            IsClippedToBounds = true
        };

        var handler = await CreateHandlerAsync(frame);
        
        Assert.True(handler.PlatformView.ClipToBounds);
    }

    [AvaloniaFact(DisplayName = "IsClippedToBounds False Initializes Correctly")]
    public async Task IsClippedToBoundsFalseInitializesCorrectly()
    {
        var frame = new FrameStub
        {
            IsClippedToBounds = false
        };

        var handler = await CreateHandlerAsync(frame);
        
        Assert.False(handler.PlatformView.ClipToBounds);
    }

    [AvaloniaFact(DisplayName = "Negative CornerRadius Treated As Zero")]
    public async Task NegativeCornerRadiusTreatedAsZero()
    {
        var frame = new FrameStub
        {
            CornerRadius = -5
        };

        var handler = await CreateHandlerAsync(frame);
        var cornerRadius = GetNativeCornerRadius(handler);
        
        Assert.Equal(new CornerRadius(0), cornerRadius);
    }

    [AvaloniaFact(DisplayName = "Large CornerRadius Works Correctly")]
    public async Task LargeCornerRadiusWorksCorrectly()
    {
        var frame = new FrameStub
        {
            CornerRadius = 100
        };

        var handler = await CreateHandlerAsync(frame);
        var cornerRadius = GetNativeCornerRadius(handler);
        
        Assert.Equal(new CornerRadius(100), cornerRadius);
    }

    [AvaloniaFact(DisplayName = "Zero Padding Initializes Correctly")]
    public async Task ZeroPaddingInitializesCorrectly()
    {
        var frame = new FrameStub
        {
            Padding = new Microsoft.Maui.Thickness(0)
        };

        var handler = await CreateHandlerAsync(frame);
        var padding = GetNativePadding(handler);
        
        Assert.Equal(new Microsoft.Maui.Thickness(0), padding);
    }

    [AvaloniaFact(DisplayName = "Background Updates Correctly")]
    public async Task BackgroundUpdatesCorrectly()
    {
        var frame = new FrameStub
        {
            BackgroundColor = MauiColors.Red
        };

        var handler = await CreateHandlerAsync(frame);
        var initialColor = GetNativeBackgroundColor(handler);
        Assert.NotNull(initialColor);

        frame.BackgroundColor = MauiColors.Green;
        handler.UpdateValue(nameof(Microsoft.Maui.Controls.Frame.BackgroundColor));

        var updatedColor = GetNativeBackgroundColor(handler);
        Assert.NotNull(updatedColor);
        ColorComparisonHelpers.AssertColorsAreEqual(MauiColors.Green, updatedColor);
    }

    [AvaloniaFact(DisplayName = "Content Can Be Null")]
    public async Task ContentCanBeNull()
    {
        var frame = new FrameStub
        {
            Content = null
        };

        var handler = await CreateHandlerAsync(frame);

        Assert.NotNull(handler.PlatformView);
        Assert.Null(handler.PlatformView.Child);
    }

    [AvaloniaFact(DisplayName = "Handler Can Be Created Multiple Times")]
    public async Task HandlerCanBeCreatedMultipleTimes()
    {
        var frame1 = new FrameStub { CornerRadius = 10 };
        var frame2 = new FrameStub { CornerRadius = 20 };
        var frame3 = new FrameStub { CornerRadius = 30 };

        var handler1 = await CreateHandlerAsync(frame1);
        var handler2 = await CreateHandlerAsync(frame2);
        var handler3 = await CreateHandlerAsync(frame3);

        Assert.Equal(new CornerRadius(10), GetNativeCornerRadius(handler1));
        Assert.Equal(new CornerRadius(20), GetNativeCornerRadius(handler2));
        Assert.Equal(new CornerRadius(30), GetNativeCornerRadius(handler3));
    }

    Microsoft.Maui.Graphics.Color? GetNativeBorderColor(MauiFrameHandler handler)
    {
        if (handler.PlatformView is not Border border)
            return null;

        if (border.BorderBrush is ISolidColorBrush brush)
        {
            return new Microsoft.Maui.Graphics.Color(
                brush.Color.R / 255f,
                brush.Color.G / 255f,
                brush.Color.B / 255f,
                brush.Color.A / 255f);
        }

        return null;
    }

    Thickness GetNativeBorderThickness(MauiFrameHandler handler)
    {
        return handler.PlatformView is Border border ? border.BorderThickness : default;
    }

    CornerRadius GetNativeCornerRadius(MauiFrameHandler handler)
    {
        return handler.PlatformView is Border border ? border.CornerRadius : default;
    }

    bool GetNativeHasShadow(MauiFrameHandler handler)
    {
        if (handler.PlatformView is not Border border)
            return false;

        return border.BoxShadow.Count > 0;
    }

    Microsoft.Maui.Graphics.Color? GetNativeBackgroundColor(MauiFrameHandler handler)
    {
        if (handler.PlatformView is not Border border)
            return null;

        if (border.Background is ISolidColorBrush brush)
        {
            return new Microsoft.Maui.Graphics.Color(
                brush.Color.R / 255f,
                brush.Color.G / 255f,
                brush.Color.B / 255f,
                brush.Color.A / 255f);
        }

        return null;
    }

    Microsoft.Maui.Thickness GetNativePadding(MauiFrameHandler handler)
    {
        if (handler.PlatformView is not Border border)
            return default;

        var p = border.Padding;
        return new Microsoft.Maui.Thickness(p.Left, p.Top, p.Right, p.Bottom);
    }
}
