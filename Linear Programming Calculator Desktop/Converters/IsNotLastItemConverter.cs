using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Linear_Programming_Calculator_Desktop.Converters
{

    /// <summary>
    /// Converts a pair of values to Visibility,
    /// </summary>
    public class IsNotLastItemConverter : IMultiValueConverter
    {
        /// <summary>
        /// Converts index and count to Visibility based on whether the index is less than count - 1.
        /// </summary>
        /// <param name="values">Array containing index and count.</param>
        /// <param name="targetType">Target type.</param>
        /// <param name="parameter">Optional parameter.</param>
        /// <param name="culture">Culture info.</param>
        /// <returns><c>Visibility.Collapsed</c> if null or empty; otherwise <c>Visibility.Visible</c>.</returns>
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
