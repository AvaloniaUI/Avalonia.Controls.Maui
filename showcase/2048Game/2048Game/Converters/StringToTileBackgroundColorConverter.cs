using System.Globalization;

namespace _2048Game.Converters
{
    public class StringToTileBackgroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int.TryParse(value.ToString(), out int parsedValue);
            switch (parsedValue)
            {
                case 0:
                    return Color.FromArgb("#bdb0a0");
                case 2:
                    return Color.FromArgb("#eee4da");
                case 4:
                    return Color.FromArgb("#eee1ce");
                case 8:
                    return Color.FromArgb("#f4b27e");
                case 16:
                    return Color.FromArgb("#f5976b");
                case 32:
                    return Color.FromArgb("#f77e69");
                case 64:
                    return Color.FromArgb("#f76148");
                case 128:
                    return Color.FromArgb("#EBD69A");
                case 256:
                    return Color.FromArgb("#EBD69A");
                case 512:
                    return Color.FromArgb("#EBD69A");
                case 1024:
                    return Color.FromArgb("#EBD69A");
                case 2048:
                    return Color.FromArgb("#EBD69A");
                case 4096:
                case 8192:
                case 16384:
                    return Color.FromArgb("#EBD69A");
                default:
                    return Color.FromArgb("#beafa0");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? 1 : 0;
        }
    }
}
