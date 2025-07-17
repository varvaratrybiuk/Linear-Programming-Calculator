using Methods.Enums;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace Linear_Programming_Calculator_Desktop.Converters
{
    public class ConstraintTypeToSignConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not ConstraintType constraintType)
                throw new ArgumentException("Недійсний тип значення для конвертації.");

            return constraintType switch
            {
                ConstraintType.GreaterThanOrEqual => "≥",
                ConstraintType.LessThanOrEqual => "≤",
                ConstraintType.Equal => "=",
                _ => throw new ArgumentOutOfRangeException(nameof(value), "Невідомий тип обмеження.")
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not ComboBoxItem comboBoxItem || comboBoxItem.Content is not string sign)
                throw new ArgumentException("Недійсний тип символу для конвертації.");

            return sign switch
            {
                "≥" => ConstraintType.GreaterThanOrEqual,
                "≤" => ConstraintType.LessThanOrEqual,
                "=" => ConstraintType.Equal,
                _ => throw new ArgumentException("Такого символу не існує!")
            };

        }
    }
}
