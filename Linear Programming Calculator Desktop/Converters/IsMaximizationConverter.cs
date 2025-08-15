using System.Globalization;
using System.Windows.Data;

namespace Linear_Programming_Calculator_Desktop.Converters
{
    /// <summary>
    /// Converts the numeric selected value to a boolean.
    /// </summary>
    public class IsMaximizationConverter : IValueConverter
    {
        /// <summary>
        /// Always returns <c>false</c>. This method does not perform any conversion.
        /// </summary>
        /// <param name="value">The boolean value to convert.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">Optional parameter.</param>
        /// <param name="culture">Culture info.</param>
        /// <returns>Always <c>false</c>.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }

        /// <summary>
        /// Converts back an integer (expected 0 or 1) to a boolean.
        /// </summary>
        /// <param name="value">The integer value to convert back.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">Optional parameter.</param>
        /// <param name="culture">Culture info.</param>
        /// <returns>Boolean value: <c>true</c> if value is 0, otherwise <c>false</c>.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value == 0;
        }
    }
}
