using System.Globalization;

namespace MyConference.Converters;

public class DateTimeFormatConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DateTime dateTime)
            return string.Empty;

        var format = parameter as string;

        return format?.ToLowerInvariant() switch
        {
            "time" => dateTime.ToString("HH:mm", CultureInfo.InvariantCulture),
            "date" => dateTime.ToString("MMM d", CultureInfo.InvariantCulture),
            "full" => dateTime.ToString("MMM d, HH:mm", CultureInfo.InvariantCulture),
            _ => dateTime.ToString("HH:mm", CultureInfo.InvariantCulture)
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
