using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Linear_Programming_Calculator_Desktop.Converters
{
    public class IsNotLastItemConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is int index && values[1] is int count)
            {
                return index < count - 1 ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
