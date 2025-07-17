using System.ComponentModel.DataAnnotations;

namespace Linear_Programming_Calculator_Desktop.Attributes
{
    public class ValidVariableCountAttribute : ValidationAttribute
    {
        public int MinValue { get; }

        public ValidVariableCountAttribute(int minValue)
        {
            MinValue = minValue;
        }

        public override bool IsValid(object? value)
        {
            string? input = (value == null) ? string.Empty : value.ToString();

            if (string.IsNullOrWhiteSpace(input.ToString()))
            {
                ErrorMessage = "Поле обов'язкове";
                return false;
            }

            if (!int.TryParse(input.ToString(), out int count))
            {
                ErrorMessage = "Значення повинно бути цілим числом!";
                return false;
            }

            if (count < MinValue)
            {
                ErrorMessage = $"Кількість повинна бути більше {MinValue - 1}";
                return false;
            }

            return true;
        }
    }
}
