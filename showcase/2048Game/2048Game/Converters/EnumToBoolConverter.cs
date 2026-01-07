using System.Globalization;

namespace _2048Game.Converters
{
    public class EnumToBoolConverter : IValueConverter
    {
        public object? TrueValue { get; set; }

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null || TrueValue == null)
                return false;

            return value.Equals(TrueValue);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue)
                return TrueValue!;

            return Binding.DoNothing;
        }
    }
}
