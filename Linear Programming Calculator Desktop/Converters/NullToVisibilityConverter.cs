using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Linear_Programming_Calculator_Desktop.Converters
{
    /// <summary>
    /// Converts a null or empty string value to Visibility.
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a string to Visibility.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="targetType">Target type.</param>
        /// <param name="parameter">Optional parameter.</param>
        /// <param name="culture">Culture info.</param>
        /// <returns><c>Visibility.Collapsed</c> if null or empty; otherwise <c>Visibility.Visible</c>.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrEmpty(value?.ToString()) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
