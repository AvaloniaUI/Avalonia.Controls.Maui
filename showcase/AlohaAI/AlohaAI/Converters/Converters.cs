using System.Globalization;

namespace AlohaAI.Converters;

public class InvertedBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : value;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : value;
}

public class ProgressToWidthConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double progress && parameter is string maxWidthStr && double.TryParse(maxWidthStr, out var maxWidth))
            return progress * maxWidth;
        return 0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class BoolToHorizontalOptionsConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isUser)
            return isUser ? LayoutOptions.End : LayoutOptions.Start;
        return LayoutOptions.Start;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class UserMessageColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
        if (value is bool isUser)
        {
            // User message: blue (same both modes)
            // AI message: dark purple (dark mode) or light purple (light mode)
            return isUser
                ? Color.FromArgb("#5B8FD4")
                : Color.FromArgb(isDark ? "#E81A1035" : "#E8E0F2");
        }
        return Color.FromArgb(isDark ? "#E81A1035" : "#E8E0F2");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
