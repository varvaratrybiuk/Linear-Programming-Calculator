using Methods.Enums;
using System.Globalization;
using System.Windows.Data;

namespace Linear_Programming_Calculator_Desktop.Converters
{
    /// <summary>
    /// Converts between ConstraintType enum values and their corresponding 
    /// string symbols used in the UI.
    /// </summary>
    public class ConstraintTypeToSignConverter : IValueConverter
    {
        /// <summary>
        /// Converts a <see cref="ConstraintType"/> value to its string representation.
        /// </summary>
        /// <param name="value">The <see cref="ConstraintType"/> value to convert.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">Optional parameter.</param>
        /// <param name="culture">The culture information.</param>
        /// <returns>The corresponding symbol as string.</returns>
        /// <exception cref="ArgumentException">Thrown if the input value is not of type <see cref="ConstraintType"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the <see cref="ConstraintType"/> is unknown.</exception>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not ConstraintType constraintType)
                throw new ArgumentException("Invalid value type for conversion.");

            return constraintType switch
            {
                ConstraintType.GreaterThanOrEqual => "≥",
                ConstraintType.LessThanOrEqual => "≤",
                ConstraintType.Equal => "=",
                _ => throw new ArgumentOutOfRangeException(nameof(value), "Unknown constraint type.")
            };
        }

        /// <summary>
        /// Converts a string symbol back to its corresponding <see cref="ConstraintType"/>.
        /// </summary>
        /// <param name="value">The symbol to convert back.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">Optional parameter.</param>
        /// <param name="culture">The culture information.</param>
        /// <returns>The corresponding <see cref="ConstraintType"/>.</returns>
        /// <exception cref="ArgumentException">Thrown if the input is not a valid symbol string or symbol is unknown.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string sign)
                throw new ArgumentException("Invalid symbol type for conversion.");

            return sign switch
            {
                "≥" => ConstraintType.GreaterThanOrEqual,
                "≤" => ConstraintType.LessThanOrEqual,
                "=" => ConstraintType.Equal,
                _ => throw new ArgumentException("This symbol does not exist!")
            };
        }
    }
}
