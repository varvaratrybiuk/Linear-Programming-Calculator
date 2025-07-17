using System.Globalization;
using System.Windows.Data;

namespace Linear_Programming_Calculator_Desktop.Converters
{
    public class IsMaximizationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is string str && str == "max";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value == 0 ? "max" : "min";
        }
    }
}
