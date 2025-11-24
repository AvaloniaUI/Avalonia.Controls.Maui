using System.Globalization;

namespace WeatherTwentyOne.Converters;

public class MinTempOffsetConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        const double min = 40;

        var minTemp = System.Convert.ToDouble(value);
        var bottomMargin = minTemp - min;

        return new Thickness(0, 0, 0, bottomMargin);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}