using System.Diagnostics.CodeAnalysis;
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

        [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode",
            Justification = "Binding.DoNothing is a static sentinel value, safe for AOT")]
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue)
                return TrueValue!;

            return Binding.DoNothing;
        }
    }
}
