using System.Globalization;

namespace _2048Game.Converters
{
    public class StringToTileTextColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int.TryParse(value.ToString(), out int parsedValue);
            switch (parsedValue)
            {
                case 0:
                    return Color.FromArgb("#f9f5f1");
                case 2:
                    return Color.FromArgb("#776e65");
                case 4:
                    return Color.FromArgb("#776e65");
                //rest colors using white text color.  make it default case.
                //case 8:
                //    return Color.FromArgb("#f9f5f1");
                //case 16:
                //    return Color.FromArgb("#f9f5f1");
                //case 32:
                //    return Color.FromArgb("#f9f5f1");
                //case 64:
                //    return Color.FromArgb("#f76148");
                //case 128:
                //    return Color.FromArgb("#f76148");
                //case 256:
                //    return Color.FromArgb("#f76148");
                //case 512:
                //    return Color.FromArgb("#f76148");
                //case 1024:
                //    return Color.FromArgb("#f76148");
                //case 2048:
                //    return Color.FromArgb("#f76148");
                //case 4096:
                //case 8192:
                //case 16384:
                //    return Color.FromArgb("#f76148");
                default:
                    return Color.FromArgb("#f9f5f1");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? 1 : 0;
        }
    }
}
