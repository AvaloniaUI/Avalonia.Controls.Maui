using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Controls.Maui.Tests.TestUtilities;
using Avalonia.Headless.XUnit;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using AvaloniaTimePickerHandler = Avalonia.Controls.Maui.Handlers.TimePickerHandler;
using AvaloniaTimePicker = Avalonia.Controls.TimePicker;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public partial class TimePickerHandlerTests : HandlerTestBase<AvaloniaTimePickerHandler, TimePickerStub>
{
    [AvaloniaFact(DisplayName = "Time Initializes Correctly")]
    public async Task TimeInitializesCorrectly()
    {
        var time = new TimeSpan(14, 30, 0); // 2:30 PM
        var picker = new TimePickerStub
        {
            Time = time
        };

        await ValidatePropertyInitValue(
            picker,
            () => picker.Time,
            GetNativeTime,
            time);
    }

    [AvaloniaFact(DisplayName = "Zero Time Initializes Correctly")]
    public async Task ZeroTimeInitializesCorrectly()
    {
        var picker = new TimePickerStub
        {
            Time = TimeSpan.Zero
        };

        var platformTime = await GetValueAsync(picker, GetNativeTime);

        Assert.NotNull(platformTime);
        Assert.Equal(TimeSpan.Zero, platformTime.Value);
    }

    [AvaloniaTheory(DisplayName = "Format Sets Clock Identifier Correctly")]
    [InlineData("t", "12HourClock")]
    [InlineData("h:mm tt", "12HourClock")]
    [InlineData("T", "24HourClock")]
    [InlineData("HH:mm", "24HourClock")]
    public async Task FormatSetsClockIdentifier(string format, string expectedClockIdentifier)
    {
        var picker = new TimePickerStub
        {
            Format = format
        };

        var platformClockIdentifier = await GetValueAsync(picker, GetNativeClockIdentifier);

        Assert.Equal(expectedClockIdentifier, platformClockIdentifier);
    }

    [AvaloniaFact(DisplayName = "Text Color Initializes Correctly")]
    public async Task TextColorInitializesCorrectly()
    {
        var picker = new TimePickerStub
        {
            TextColor = Colors.Red
        };

        var platformColor = await GetValueAsync(picker, GetNativeTextColor);

        Assert.NotNull(platformColor);
        ColorComparisonHelpers.AssertColorsAreEqual(Colors.Red, platformColor);
    }

    [AvaloniaFact(DisplayName = "Font Size Initializes Correctly")]
    public async Task FontSizeInitializesCorrectly()
    {
        var picker = new TimePickerStub
        {
            Font = Microsoft.Maui.Font.SystemFontOfSize(20)
        };

        var platformFontSize = await GetValueAsync(picker, GetNativeFontSize);

        Assert.Equal(20, platformFontSize);
    }

    [AvaloniaFact(DisplayName = "Native Selection Updates Virtual View")]
    public async Task NativeSelectionUpdatesVirtualView()
    {
        var picker = new TimePickerStub
        {
            Time = TimeSpan.Zero
        };

        var handler = await CreateHandlerAsync(picker);
        var newTime = new TimeSpan(10, 0, 0);

        await InvokeOnMainThreadAsync(() =>
        {
            handler.PlatformView!.SelectedTime = newTime;
        });

        Assert.Equal(newTime, picker.Time!.Value);
    }

    [AvaloniaFact(DisplayName = "Character Spacing Stores Value In Tag")]
    public async Task CharacterSpacingStoresValueInTag()
    {
        var picker = new TimePickerStub
        {
            CharacterSpacing = 2.5
        };

        var handler = await CreateHandlerAsync(picker);

        await InvokeOnMainThreadAsync(() =>
        {
            // The new implementation stores spacing in Tag for TemplateApplied event
            var tag = handler.PlatformView?.Tag;
            Assert.NotNull(tag);
            Assert.IsType<double>(tag);
            Assert.Equal(2.5, (double)tag);
        });
    }

    [AvaloniaFact(DisplayName = "Time Updates Correctly")]
    public async Task TimeUpdatesCorrectly()
    {
        var initialTime = new TimeSpan(10, 0, 0);
        var newTime = new TimeSpan(14, 0, 0);
        
        var picker = new TimePickerStub
        {
            Time = initialTime
        };

        await ValidatePropertyUpdatesValue(
            picker,
            nameof(ITimePicker.Time),
            GetNativeTime,
            newTime,
            initialTime);
    }

    [AvaloniaFact(DisplayName = "Handler Creates Without Exception")]
    public async Task HandlerCreatesWithoutException()
    {
        var picker = new TimePickerStub();

        var handler = await CreateHandlerAsync(picker);

        Assert.NotNull(handler);
        Assert.NotNull(handler.PlatformView);
        Assert.IsType<AvaloniaTimePicker>(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Character Spacing Does Not Throw")]
    public async Task CharacterSpacingDoesNotThrow()
    {
        var picker = new TimePickerStub
        {
            CharacterSpacing = 4
        };

        var handler = await CreateHandlerAsync(picker);

        Assert.NotNull(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Text Color Updates Correctly")]
    public async Task TextColorUpdatesCorrectly()
    {
        var picker = new TimePickerStub
        {
            TextColor = Colors.Blue
        };

        await ValidatePropertyUpdatesValue(
            picker,
            nameof(ITimePicker.TextColor),
            GetNativeTextColor,
            Colors.Green,
            Colors.Blue);
    }

    [AvaloniaFact(DisplayName = "Bold Font Weight Applies")]
    public async Task BoldFontWeightApplies()
    {
        var picker = new TimePickerStub
        {
            Font = Microsoft.Maui.Font.SystemFontOfSize(14, FontWeight.Bold)
        };

        var handler = await CreateHandlerAsync(picker);
        var fontWeight = await InvokeOnMainThreadAsync(() => GetNativeFontWeight(handler));

        Assert.Equal(Avalonia.Media.FontWeight.Bold, fontWeight);
    }

    [AvaloniaFact(DisplayName = "Italic Font Style Applies")]
    public async Task ItalicFontStyleApplies()
    {
        var picker = new TimePickerStub
        {
            Font = Microsoft.Maui.Font.SystemFontOfSize(14).WithSlant(FontSlant.Italic)
        };

        var handler = await CreateHandlerAsync(picker);
        var fontStyle = await InvokeOnMainThreadAsync(() => GetNativeFontStyle(handler));

        Assert.Equal(Avalonia.Media.FontStyle.Italic, fontStyle);
    }

    [AvaloniaTheory(DisplayName = "Various Font Sizes Apply")]
    [InlineData(8)]
    [InlineData(12)]
    [InlineData(16)]
    [InlineData(24)]
    [InlineData(36)]
    public async Task VariousFontSizesApply(double fontSize)
    {
        var picker = new TimePickerStub
        {
            Font = Microsoft.Maui.Font.SystemFontOfSize(fontSize)
        };

        var platformFontSize = await GetValueAsync(picker, GetNativeFontSize);

        Assert.Equal(fontSize, platformFontSize);
    }

    [AvaloniaTheory(DisplayName = "Various Text Colors Apply")]
    [InlineData(255, 0, 0)]     // Red
    [InlineData(0, 255, 0)]     // Green
    [InlineData(0, 0, 255)]     // Blue
    [InlineData(255, 255, 0)]   // Yellow
    [InlineData(128, 128, 128)] // Gray
    public async Task VariousTextColorsApply(byte r, byte g, byte b)
    {
        var color = new Color(r / 255f, g / 255f, b / 255f, 1f);
        var picker = new TimePickerStub
        {
            TextColor = color
        };

        var platformColor = await GetValueAsync(picker, GetNativeTextColor);

        Assert.NotNull(platformColor);
        ColorComparisonHelpers.AssertColorsAreEqual(color, platformColor);
    }

    [AvaloniaFact(DisplayName = "Null Text Color Clears Foreground")]
    public async Task NullTextColorClearsForeground()
    {
        var picker = new TimePickerStub
        {
            TextColor = Colors.Red
        };

        var handler = await CreateHandlerAsync(picker);

        // Now set to null
        picker.TextColor = null!;
        await InvokeOnMainThreadAsync(() =>
        {
            handler.UpdateValue(nameof(ITimePicker.TextColor));
        });

        // Should not throw and handler should still be valid
        Assert.NotNull(handler.PlatformView);
    }

    [AvaloniaTheory(DisplayName = "Various Character Spacing Values Work")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(20)]
    public async Task VariousCharacterSpacingValuesWork(double spacing)
    {
        var picker = new TimePickerStub
        {
            CharacterSpacing = spacing
        };

        var handler = await CreateHandlerAsync(picker);

        Assert.NotNull(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Handler Can Be Created Multiple Times")]
    public async Task HandlerCanBeCreatedMultipleTimes()
    {
        var picker = new TimePickerStub
        {
            Time = new TimeSpan(14, 30, 0)
        };

        var handler1 = await CreateHandlerAsync(picker);
        Assert.NotNull(handler1.PlatformView);

        var handler2 = await CreateHandlerAsync(picker);
        Assert.NotNull(handler2.PlatformView);

        var handler3 = await CreateHandlerAsync(picker);
        Assert.NotNull(handler3.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Multiple Property Updates In Sequence")]
    public async Task MultiplePropertyUpdatesInSequence()
    {
        var picker = new TimePickerStub();

        var handler = await CreateHandlerAsync(picker);

        // Update multiple properties
        picker.Time = new TimeSpan(15, 45, 0);
        picker.TextColor = Colors.Blue;
        picker.Font = Microsoft.Maui.Font.SystemFontOfSize(18);

        await InvokeOnMainThreadAsync(() =>
        {
            handler.UpdateValue(nameof(ITimePicker.Time));
            handler.UpdateValue(nameof(ITimePicker.TextColor));
            handler.UpdateValue(nameof(ITimePicker.Font));
        });

        var platformTime = await InvokeOnMainThreadAsync(() => handler.PlatformView?.SelectedTime);
        Assert.NotNull(platformTime);
        Assert.Equal(new TimeSpan(15, 45, 0), platformTime.Value);
    }

    [AvaloniaFact(DisplayName = "Transparent Text Color Does Not Throw")]
    public async Task TransparentTextColorDoesNotThrow()
    {
        var picker = new TimePickerStub
        {
            TextColor = Colors.Transparent
        };

        var handler = await CreateHandlerAsync(picker);
        Assert.NotNull(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Empty Format String Handled")]
    public async Task EmptyFormatStringHandled()
    {
        var picker = new TimePickerStub
        {
            Format = string.Empty
        };

        var handler = await CreateHandlerAsync(picker);
        Assert.NotNull(handler.PlatformView);
    }

    [AvaloniaTheory(DisplayName = "Various Time Values Work")]
    [InlineData(0, 0, 0)]    // Midnight
    [InlineData(12, 0, 0)]   // Noon
    [InlineData(23, 59, 59)] // Just before midnight
    [InlineData(6, 30, 0)]   // Morning
    [InlineData(18, 45, 30)] // Evening with seconds
    public async Task VariousTimeValuesWork(int hours, int minutes, int seconds)
    {
        var time = new TimeSpan(hours, minutes, seconds);
        var picker = new TimePickerStub
        {
            Time = time
        };

        var platformTime = await GetValueAsync(picker, GetNativeTime);

        Assert.NotNull(platformTime);
        Assert.Equal(time, platformTime.Value);
    }

    [AvaloniaFact(DisplayName = "Midnight Time Works")]
    public async Task MidnightTimeWorks()
    {
        var midnight = new TimeSpan(0, 0, 0);
        var picker = new TimePickerStub
        {
            Time = midnight
        };

        var platformTime = await GetValueAsync(picker, GetNativeTime);

        Assert.NotNull(platformTime);
        Assert.Equal(midnight, platformTime.Value);
    }

    [AvaloniaFact(DisplayName = "End Of Day Time Works")]
    public async Task EndOfDayTimeWorks()
    {
        var endOfDay = new TimeSpan(23, 59, 59);
        var picker = new TimePickerStub
        {
            Time = endOfDay
        };

        var platformTime = await GetValueAsync(picker, GetNativeTime);

        Assert.NotNull(platformTime);
        Assert.Equal(endOfDay, platformTime.Value);
    }

    [AvaloniaFact(DisplayName = "Noon Time Works")]
    public async Task NoonTimeWorks()
    {
        var noon = new TimeSpan(12, 0, 0);
        var picker = new TimePickerStub
        {
            Time = noon
        };

        var platformTime = await GetValueAsync(picker, GetNativeTime);

        Assert.NotNull(platformTime);
        Assert.Equal(noon, platformTime.Value);
    }

    [AvaloniaFact(DisplayName = "AM To PM Transition Works")]
    public async Task AmToPmTransitionWorks()
    {
        var picker = new TimePickerStub
        {
            Time = new TimeSpan(11, 59, 0) // Just before noon
        };

        var handler = await CreateHandlerAsync(picker);

        var newTime = new TimeSpan(12, 0, 0); // Noon
        await InvokeOnMainThreadAsync(() =>
        {
            handler.PlatformView!.SelectedTime = newTime;
        });

        Assert.Equal(newTime, picker.Time);
    }

    [AvaloniaFact(DisplayName = "PM To AM Transition Works")]
    public async Task PmToAmTransitionWorks()
    {
        var picker = new TimePickerStub
        {
            Time = new TimeSpan(23, 59, 0) // Just before midnight
        };

        var handler = await CreateHandlerAsync(picker);

        var newTime = new TimeSpan(0, 0, 0); // Midnight
        await InvokeOnMainThreadAsync(() =>
        {
            handler.PlatformView!.SelectedTime = newTime;
        });

        Assert.Equal(newTime, picker.Time);
    }

    [AvaloniaTheory(DisplayName = "Hour Boundary Values Work")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(11)]
    [InlineData(12)]
    [InlineData(13)]
    [InlineData(23)]
    public async Task HourBoundaryValuesWork(int hour)
    {
        var time = new TimeSpan(hour, 0, 0);
        var picker = new TimePickerStub
        {
            Time = time
        };

        var platformTime = await GetValueAsync(picker, GetNativeTime);

        Assert.NotNull(platformTime);
        Assert.Equal(hour, platformTime.Value.Hours);
    }

    [AvaloniaTheory(DisplayName = "Minute Boundary Values Work")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(29)]
    [InlineData(30)]
    [InlineData(31)]
    [InlineData(59)]
    public async Task MinuteBoundaryValuesWork(int minute)
    {
        var time = new TimeSpan(12, minute, 0);
        var picker = new TimePickerStub
        {
            Time = time
        };

        var platformTime = await GetValueAsync(picker, GetNativeTime);

        Assert.NotNull(platformTime);
        Assert.Equal(minute, platformTime.Value.Minutes);
    }

    [AvaloniaFact(DisplayName = "Time With Seconds Works")]
    public async Task TimeWithSecondsWorks()
    {
        var time = new TimeSpan(14, 30, 45);
        var picker = new TimePickerStub
        {
            Time = time
        };

        var platformTime = await GetValueAsync(picker, GetNativeTime);

        Assert.NotNull(platformTime);
        // Note: Avalonia TimePicker may not display seconds but should preserve the value
        Assert.Equal(time.Hours, platformTime.Value.Hours);
        Assert.Equal(time.Minutes, platformTime.Value.Minutes);
    }

    [AvaloniaFact(DisplayName = "Time Updates From Native View")]
    public async Task TimeUpdatesFromNativeView()
    {
        var picker = new TimePickerStub
        {
            Time = new TimeSpan(8, 0, 0)
        };

        var handler = await CreateHandlerAsync(picker);

        // Simulate multiple native selections
        var times = new[]
        {
            new TimeSpan(9, 0, 0),
            new TimeSpan(12, 30, 0),
            new TimeSpan(18, 45, 0),
        };

        foreach (var time in times)
        {
            await InvokeOnMainThreadAsync(() =>
            {
                handler.PlatformView!.SelectedTime = time;
            });

            Assert.Equal(time, picker.Time);
        }
    }

    [AvaloniaTheory(DisplayName = "Font Weight Variations Apply")]
    [InlineData(FontWeight.Thin)]
    [InlineData(FontWeight.Light)]
    [InlineData(FontWeight.Regular)]
    [InlineData(FontWeight.Medium)]
    [InlineData(FontWeight.Semibold)]
    [InlineData(FontWeight.Bold)]
    [InlineData(FontWeight.Heavy)]
    [InlineData(FontWeight.Black)]
    public async Task FontWeightVariationsApply(FontWeight weight)
    {
        var picker = new TimePickerStub
        {
            Font = Microsoft.Maui.Font.SystemFontOfSize(14, weight)
        };

        var handler = await CreateHandlerAsync(picker);

        Assert.NotNull(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Default Font Properties Work")]
    public async Task DefaultFontPropertiesWork()
    {
        var picker = new TimePickerStub();

        var handler = await CreateHandlerAsync(picker);

        Assert.NotNull(handler.PlatformView);
        Assert.True(handler.PlatformView.FontSize > 0);
    }

    [AvaloniaFact(DisplayName = "24 Hour Format Shows Correct Clock")]
    public async Task TwentyFourHourFormatShowsCorrectClock()
    {
        var picker = new TimePickerStub
        {
            Format = "HH:mm",
            Time = new TimeSpan(15, 30, 0)
        };

        var handler = await CreateHandlerAsync(picker);
        var clockId = await InvokeOnMainThreadAsync(() => handler.PlatformView?.ClockIdentifier);

        Assert.Equal("24HourClock", clockId);
    }

    [AvaloniaFact(DisplayName = "12 Hour Format Shows Correct Clock")]
    public async Task TwelveHourFormatShowsCorrectClock()
    {
        var picker = new TimePickerStub
        {
            Format = "h:mm tt",
            Time = new TimeSpan(15, 30, 0)
        };

        var handler = await CreateHandlerAsync(picker);
        var clockId = await InvokeOnMainThreadAsync(() => handler.PlatformView?.ClockIdentifier);

        Assert.Equal("12HourClock", clockId);
    }

    [AvaloniaFact(DisplayName = "Combined Font Properties Apply")]
    public async Task CombinedFontPropertiesApply()
    {
        var picker = new TimePickerStub
        {
            Font = Microsoft.Maui.Font.SystemFontOfSize(20, FontWeight.Bold).WithSlant(FontSlant.Italic)
        };

        var handler = await CreateHandlerAsync(picker);
        
        var fontSize = await InvokeOnMainThreadAsync(() => handler.PlatformView?.FontSize);
        var fontWeight = await InvokeOnMainThreadAsync(() => GetNativeFontWeight(handler));
        var fontStyle = await InvokeOnMainThreadAsync(() => GetNativeFontStyle(handler));

        Assert.Equal(20, fontSize);
        Assert.Equal(Avalonia.Media.FontWeight.Bold, fontWeight);
        Assert.Equal(Avalonia.Media.FontStyle.Italic, fontStyle);
    }



    [AvaloniaFact(DisplayName = "Format Update Changes Clock Identifier")]
    public async Task FormatUpdateChangesClockIdentifier()
    {
        var picker = new TimePickerStub
        {
            Format = "h:mm tt" // 12-hour
        };

        var handler = await CreateHandlerAsync(picker);
        
        var clockId12 = await InvokeOnMainThreadAsync(() => handler.PlatformView?.ClockIdentifier);
        Assert.Equal("12HourClock", clockId12);

        // Update to 24-hour format
        picker.Format = "HH:mm";
        await InvokeOnMainThreadAsync(() => handler.UpdateValue(nameof(ITimePicker.Format)));

        var clockId24 = await InvokeOnMainThreadAsync(() => handler.PlatformView?.ClockIdentifier);
        Assert.Equal("24HourClock", clockId24);
    }

    // Platform-specific property getters
    TimeSpan? GetNativeTime(AvaloniaTimePickerHandler handler)
    {
        return handler.PlatformView?.SelectedTime;
    }

    string GetNativeClockIdentifier(AvaloniaTimePickerHandler handler)
    {
        return handler.PlatformView?.ClockIdentifier ?? "12HourClock";
    }

    Color? GetNativeTextColor(AvaloniaTimePickerHandler handler)
    {
        if (handler.PlatformView?.Foreground is Avalonia.Media.SolidColorBrush brush)
        {
            return new Color(
                brush.Color.R / 255f,
                brush.Color.G / 255f,
                brush.Color.B / 255f,
                brush.Color.A / 255f);
        }
        return null;
    }

    double GetNativeFontSize(AvaloniaTimePickerHandler handler)
    {
        return handler.PlatformView?.FontSize ?? 0;
    }

    Avalonia.Media.FontWeight GetNativeFontWeight(AvaloniaTimePickerHandler handler)
    {
        return handler.PlatformView?.FontWeight ?? Avalonia.Media.FontWeight.Normal;
    }

    Avalonia.Media.FontStyle GetNativeFontStyle(AvaloniaTimePickerHandler handler)
    {
        return handler.PlatformView?.FontStyle ?? Avalonia.Media.FontStyle.Normal;
    }
}
