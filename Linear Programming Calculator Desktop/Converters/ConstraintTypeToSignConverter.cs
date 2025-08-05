using Methods.Enums;
using System.Globalization;
using System.Windows.Data;

namespace Linear_Programming_Calculator_Desktop.Converters
{
    public class ConstraintTypeToSignConverter : IValueConverter
    {
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
