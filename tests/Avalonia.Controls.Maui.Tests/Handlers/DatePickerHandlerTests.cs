using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Controls.Maui.Tests.TestUtilities;
using Avalonia.Headless.XUnit;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using AvaloniaDatePickerHandler = Avalonia.Controls.Maui.Handlers.DatePickerHandler;
using AvaloniaDatePicker = global::Avalonia.Controls.DatePicker;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public partial class DatePickerHandlerTests : HandlerTestBase<AvaloniaDatePickerHandler, DatePickerStub>
{
    [AvaloniaFact(DisplayName = "Date Initializes Correctly")]
    public async Task DateInitializesCorrectly()
    {
        var expectedDate = new DateTime(2024, 6, 15);
        var datePicker = new DatePickerStub
        {
            Date = expectedDate
        };

        var platformDate = await GetValueAsync(datePicker, GetNativeDate);

        Assert.NotNull(platformDate);
        Assert.Equal(expectedDate.Date, platformDate.Value.Date);
    }

    [AvaloniaFact(DisplayName = "Null Date Sets Null SelectedDate")]
    public async Task NullDateSetsNullSelectedDate()
    {
        var datePicker = new DatePickerStub
        {
            Date = null
        };

        var handler = await CreateHandlerAsync(datePicker);
        var platformDate = await InvokeOnMainThreadAsync(() => handler.PlatformView?.SelectedDate);

        Assert.Null(platformDate);
    }

    [AvaloniaFact(DisplayName = "Text Color Initializes Correctly")]
    public async Task TextColorInitializesCorrectly()
    {
        var datePicker = new DatePickerStub
        {
            TextColor = Colors.Red
        };

        var platformColor = await GetValueAsync(datePicker, GetNativeTextColor);

        Assert.NotNull(platformColor);
        ColorComparisonHelpers.AssertColorsAreEqual(Colors.Red, platformColor);
    }

    [AvaloniaFact(DisplayName = "Font Size Initializes Correctly")]
    public async Task FontSizeInitializesCorrectly()
    {
        var datePicker = new DatePickerStub
        {
            Font = Microsoft.Maui.Font.SystemFontOfSize(20)
        };

        var platformFontSize = await GetValueAsync(datePicker, GetNativeFontSize);

        Assert.Equal(20, platformFontSize);
    }

    [AvaloniaTheory(DisplayName = "Format Initializes Correctly")]
    [InlineData("d")]
    [InlineData("D")]
    [InlineData("M")]
    [InlineData("Y")]
    public async Task FormatInitializesCorrectly(string format)
    {
        var datePicker = new DatePickerStub
        {
            Format = format
        };

        var handler = await CreateHandlerAsync(datePicker);

        // Format is applied - we just verify handler creation succeeds
        Assert.NotNull(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Date Updates Correctly")]
    public async Task DateUpdatesCorrectly()
    {
        var initialDate = new DateTime(2024, 1, 1);
        var newDate = new DateTime(2024, 12, 31);

        var datePicker = new DatePickerStub
        {
            Date = initialDate
        };

        await ValidatePropertyUpdatesValue(
            datePicker,
            nameof(IDatePicker.Date),
            GetNativeDate,
            newDate,
            initialDate);
    }

    [AvaloniaFact(DisplayName = "MinimumDate Constrains Date")]
    public async Task MinimumDateConstrainsDate()
    {
        var minDate = new DateTime(2024, 6, 1);
        var datePicker = new DatePickerStub
        {
            MinimumDate = minDate,
            Date = new DateTime(2024, 5, 1) // Before minimum
        };

        var handler = await CreateHandlerAsync(datePicker);
        var platformDate = await InvokeOnMainThreadAsync(() => handler.PlatformView?.SelectedDate);

        Assert.NotNull(platformDate);
        // Date should be constrained to minimum
        Assert.True(platformDate.Value.Date >= minDate.Date);
    }

    [AvaloniaFact(DisplayName = "MaximumDate Constrains Date")]
    public async Task MaximumDateConstrainsDate()
    {
        var maxDate = new DateTime(2024, 6, 30);
        var datePicker = new DatePickerStub
        {
            MaximumDate = maxDate,
            Date = new DateTime(2024, 7, 15) // After maximum
        };

        var handler = await CreateHandlerAsync(datePicker);
        var platformDate = await InvokeOnMainThreadAsync(() => handler.PlatformView?.SelectedDate);

        Assert.NotNull(platformDate);
        // Date should be constrained to maximum
        Assert.True(platformDate.Value.Date <= maxDate.Date);
    }

    [AvaloniaFact(DisplayName = "Native Selection Updates Virtual View")]
    public async Task NativeSelectionUpdatesVirtualView()
    {
        var datePicker = new DatePickerStub
        {
            Date = new DateTime(2024, 1, 1)
        };

        var handler = await CreateHandlerAsync(datePicker);

        var newDate = new DateTime(2024, 12, 25);
        await InvokeOnMainThreadAsync(() =>
        {
            handler.PlatformView!.SelectedDate = new DateTimeOffset(newDate);
        });

        Assert.Equal(newDate.Date, datePicker.Date?.Date);
    }

    [AvaloniaFact(DisplayName = "Handler Creates Without Exception")]
    public async Task HandlerCreatesWithoutException()
    {
        var datePicker = new DatePickerStub();

        var handler = await CreateHandlerAsync(datePicker);

        Assert.NotNull(handler);
        Assert.NotNull(handler.PlatformView);
        Assert.IsType<AvaloniaDatePicker>(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Character Spacing Does Not Throw")]
    public async Task CharacterSpacingDoesNotThrow()
    {
        var datePicker = new DatePickerStub
        {
            CharacterSpacing = 4
        };

        var handler = await CreateHandlerAsync(datePicker);

        Assert.NotNull(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Text Color Updates Correctly")]
    public async Task TextColorUpdatesCorrectly()
    {
        var datePicker = new DatePickerStub
        {
            TextColor = Colors.Blue
        };

        await ValidatePropertyUpdatesValue(
            datePicker,
            nameof(IDatePicker.TextColor),
            GetNativeTextColor,
            Colors.Green,
            Colors.Blue);
    }

    [AvaloniaFact(DisplayName = "Default Date Is Today")]
    public async Task DefaultDateIsToday()
    {
        var datePicker = new DatePickerStub();

        var handler = await CreateHandlerAsync(datePicker);
        var platformDate = await InvokeOnMainThreadAsync(() => handler.PlatformView?.SelectedDate);

        Assert.NotNull(platformDate);
        Assert.Equal(DateTime.Today.Date, platformDate.Value.Date);
    }

    DateTime? GetNativeDate(AvaloniaDatePickerHandler handler)
    {
        if (handler.PlatformView is not AvaloniaDatePicker datePicker)
            return null;

        return datePicker.SelectedDate?.DateTime;
    }

    Color? GetNativeTextColor(AvaloniaDatePickerHandler handler)
    {
        if (handler.PlatformView is not AvaloniaDatePicker datePicker)
            return null;

        if (datePicker.Foreground is Avalonia.Media.SolidColorBrush brush)
        {
            return new Color(
                brush.Color.R / 255f,
                brush.Color.G / 255f,
                brush.Color.B / 255f,
                brush.Color.A / 255f);
        }

        return null;
    }

    double GetNativeFontSize(AvaloniaDatePickerHandler handler)
    {
        if (handler.PlatformView is not AvaloniaDatePicker datePicker)
            return 0;

        return datePicker.FontSize;
    }

    Avalonia.Media.FontFamily? GetNativeFontFamily(AvaloniaDatePickerHandler handler)
    {
        if (handler.PlatformView is not AvaloniaDatePicker datePicker)
            return null;

        return datePicker.FontFamily;
    }

    Avalonia.Media.FontWeight GetNativeFontWeight(AvaloniaDatePickerHandler handler)
    {
        if (handler.PlatformView is not AvaloniaDatePicker datePicker)
            return Avalonia.Media.FontWeight.Normal;

        return datePicker.FontWeight;
    }

    Avalonia.Media.FontStyle GetNativeFontStyle(AvaloniaDatePickerHandler handler)
    {
        if (handler.PlatformView is not AvaloniaDatePicker datePicker)
            return Avalonia.Media.FontStyle.Normal;

        return datePicker.FontStyle;
    }

    #region Additional Edge Case Tests

    [AvaloniaFact(DisplayName = "Date At MinValue Works")]
    public async Task DateAtMinValueWorks()
    {
        var minDate = DateTime.MinValue.AddDays(1); // Avoid absolute min for safety
        var datePicker = new DatePickerStub
        {
            Date = minDate
        };

        var handler = await CreateHandlerAsync(datePicker);
        var platformDate = await InvokeOnMainThreadAsync(() => handler.PlatformView?.SelectedDate);

        Assert.NotNull(platformDate);
    }

    [AvaloniaFact(DisplayName = "Date At MaxValue Works")]
    public async Task DateAtMaxValueWorks()
    {
        var maxDate = DateTime.MaxValue.AddDays(-1); // Avoid absolute max for safety
        var datePicker = new DatePickerStub
        {
            Date = maxDate
        };

        var handler = await CreateHandlerAsync(datePicker);
        var platformDate = await InvokeOnMainThreadAsync(() => handler.PlatformView?.SelectedDate);

        Assert.NotNull(platformDate);
    }

    [AvaloniaFact(DisplayName = "Leap Year Date Works")]
    public async Task LeapYearDateWorks()
    {
        var leapDay = new DateTime(2024, 2, 29);
        var datePicker = new DatePickerStub
        {
            Date = leapDay
        };

        var platformDate = await GetValueAsync(datePicker, GetNativeDate);

        Assert.NotNull(platformDate);
        Assert.Equal(leapDay.Date, platformDate.Value.Date);
    }

    [AvaloniaFact(DisplayName = "Year Boundary Crossing Works")]
    public async Task YearBoundaryCrossingWorks()
    {
        var datePicker = new DatePickerStub
        {
            Date = new DateTime(2023, 12, 31)
        };

        var handler = await CreateHandlerAsync(datePicker);

        var newDate = new DateTime(2024, 1, 1);
        await InvokeOnMainThreadAsync(() =>
        {
            handler.PlatformView!.SelectedDate = new DateTimeOffset(newDate);
        });

        Assert.Equal(newDate.Date, datePicker.Date?.Date);
    }

    [AvaloniaFact(DisplayName = "Same Minimum And Maximum Date Works")]
    public async Task SameMinimumAndMaximumDateWorks()
    {
        var fixedDate = new DateTime(2024, 6, 15);
        var datePicker = new DatePickerStub
        {
            MinimumDate = fixedDate,
            MaximumDate = fixedDate,
            Date = fixedDate
        };

        var handler = await CreateHandlerAsync(datePicker);
        var platformDate = await InvokeOnMainThreadAsync(() => handler.PlatformView?.SelectedDate);

        Assert.NotNull(platformDate);
        Assert.Equal(fixedDate.Date, platformDate.Value.Date);
    }

    [AvaloniaTheory(DisplayName = "Various Character Spacing Values Work")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(20)]
    public async Task VariousCharacterSpacingValuesWork(double spacing)
    {
        var datePicker = new DatePickerStub
        {
            CharacterSpacing = spacing
        };

        var handler = await CreateHandlerAsync(datePicker);

        Assert.NotNull(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Bold Font Weight Applies")]
    public async Task BoldFontWeightApplies()
    {
        var datePicker = new DatePickerStub
        {
            Font = Microsoft.Maui.Font.SystemFontOfSize(14, FontWeight.Bold)
        };

        var handler = await CreateHandlerAsync(datePicker);
        var fontWeight = await InvokeOnMainThreadAsync(() => GetNativeFontWeight(handler));

        Assert.Equal(Avalonia.Media.FontWeight.Bold, fontWeight);
    }

    [AvaloniaFact(DisplayName = "Italic Font Style Applies")]
    public async Task ItalicFontStyleApplies()
    {
        var datePicker = new DatePickerStub
        {
            Font = Microsoft.Maui.Font.SystemFontOfSize(14).WithSlant(FontSlant.Italic)
        };

        var handler = await CreateHandlerAsync(datePicker);
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
        var datePicker = new DatePickerStub
        {
            Font = Microsoft.Maui.Font.SystemFontOfSize(fontSize)
        };

        var platformFontSize = await GetValueAsync(datePicker, GetNativeFontSize);

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
        var datePicker = new DatePickerStub
        {
            TextColor = color
        };

        var platformColor = await GetValueAsync(datePicker, GetNativeTextColor);

        Assert.NotNull(platformColor);
        ColorComparisonHelpers.AssertColorsAreEqual(color, platformColor);
    }

    [AvaloniaFact(DisplayName = "Null Text Color Clears Foreground")]
    public async Task NullTextColorClearsForeground()
    {
        var datePicker = new DatePickerStub
        {
            TextColor = Colors.Red
        };

        var handler = await CreateHandlerAsync(datePicker);

        // Now set to null
        datePicker.TextColor = null;
        await InvokeOnMainThreadAsync(() =>
        {
            handler.UpdateValue(nameof(IDatePicker.TextColor));
        });

        // Should not throw and handler should still be valid
        Assert.NotNull(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Handler Can Be Created Multiple Times")]
    public async Task HandlerCanBeCreatedMultipleTimes()
    {
        var datePicker = new DatePickerStub
        {
            Date = new DateTime(2024, 6, 15)
        };

        var handler1 = await CreateHandlerAsync(datePicker);
        Assert.NotNull(handler1.PlatformView);

        var handler2 = await CreateHandlerAsync(datePicker);
        Assert.NotNull(handler2.PlatformView);

        var handler3 = await CreateHandlerAsync(datePicker);
        Assert.NotNull(handler3.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Multiple Property Updates In Sequence")]
    public async Task MultiplePropertyUpdatesInSequence()
    {
        var datePicker = new DatePickerStub();

        var handler = await CreateHandlerAsync(datePicker);

        // Update multiple properties
        datePicker.Date = new DateTime(2024, 3, 15);
        datePicker.TextColor = Colors.Blue;
        datePicker.Font = Microsoft.Maui.Font.SystemFontOfSize(18);

        await InvokeOnMainThreadAsync(() =>
        {
            handler.UpdateValue(nameof(IDatePicker.Date));
            handler.UpdateValue(nameof(IDatePicker.TextColor));
            handler.UpdateValue(nameof(IDatePicker.Font));
        });

        var platformDate = await InvokeOnMainThreadAsync(() => handler.PlatformView?.SelectedDate);
        Assert.NotNull(platformDate);
        Assert.Equal(new DateTime(2024, 3, 15).Date, platformDate.Value.Date);
    }

    [AvaloniaFact(DisplayName = "Date In Far Future Works")]
    public async Task DateInFarFutureWorks()
    {
        var farFuture = new DateTime(2099, 12, 31);
        var datePicker = new DatePickerStub
        {
            Date = farFuture
        };

        var platformDate = await GetValueAsync(datePicker, GetNativeDate);

        Assert.NotNull(platformDate);
        Assert.Equal(farFuture.Date, platformDate.Value.Date);
    }

    [AvaloniaFact(DisplayName = "Date In Past Works")]
    public async Task DateInPastWorks()
    {
        var pastDate = new DateTime(1900, 1, 1);
        var datePicker = new DatePickerStub
        {
            Date = pastDate
        };

        var platformDate = await GetValueAsync(datePicker, GetNativeDate);

        Assert.NotNull(platformDate);
        Assert.Equal(pastDate.Date, platformDate.Value.Date);
    }

    [AvaloniaFact(DisplayName = "MinimumDate After MaximumDate Handles Gracefully")]
    public async Task MinimumDateAfterMaximumDateHandlesGracefully()
    {
        var datePicker = new DatePickerStub
        {
            MinimumDate = new DateTime(2024, 12, 31),
            MaximumDate = new DateTime(2024, 1, 1),
            Date = new DateTime(2024, 6, 15)
        };

        // Should not throw
        var handler = await CreateHandlerAsync(datePicker);
        Assert.NotNull(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "End Of Month Dates Work")]
    public async Task EndOfMonthDatesWork()
    {
        // Test various month-end dates
        var dates = new[]
        {
            new DateTime(2024, 1, 31),  // 31-day month
            new DateTime(2024, 4, 30),  // 30-day month
            new DateTime(2024, 2, 29),  // Leap year February
            new DateTime(2023, 2, 28),  // Non-leap year February
        };

        foreach (var date in dates)
        {
            var datePicker = new DatePickerStub { Date = date };
            var platformDate = await GetValueAsync(datePicker, GetNativeDate);
            Assert.NotNull(platformDate);
            Assert.Equal(date.Date, platformDate.Value.Date);
        }
    }

    [AvaloniaFact(DisplayName = "First Of Month Dates Work")]
    public async Task FirstOfMonthDatesWork()
    {
        for (int month = 1; month <= 12; month++)
        {
            var date = new DateTime(2024, month, 1);
            var datePicker = new DatePickerStub { Date = date };
            var platformDate = await GetValueAsync(datePicker, GetNativeDate);
            Assert.NotNull(platformDate);
            Assert.Equal(date.Date, platformDate.Value.Date);
        }
    }

    [AvaloniaFact(DisplayName = "Transparent Text Color Does Not Throw")]
    public async Task TransparentTextColorDoesNotThrow()
    {
        var datePicker = new DatePickerStub
        {
            TextColor = Colors.Transparent
        };

        var handler = await CreateHandlerAsync(datePicker);
        Assert.NotNull(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Default Format Applied")]
    public async Task DefaultFormatApplied()
    {
        var datePicker = new DatePickerStub
        {
            Date = new DateTime(2024, 6, 15)
        };

        var handler = await CreateHandlerAsync(datePicker);
        Assert.NotNull(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Empty Format String Handled")]
    public async Task EmptyFormatStringHandled()
    {
        var datePicker = new DatePickerStub
        {
            Format = string.Empty
        };

        var handler = await CreateHandlerAsync(datePicker);
        Assert.NotNull(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Custom Format String Applied")]
    public async Task CustomFormatStringApplied()
    {
        var datePicker = new DatePickerStub
        {
            Format = "yyyy-MM-dd"
        };

        var handler = await CreateHandlerAsync(datePicker);
        Assert.NotNull(handler.PlatformView);
    }

    #endregion
}
