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
                ErrorMessage = "This field is required";
                return false;
            }

            if (!int.TryParse(input.ToString(), out int count))
            {
                ErrorMessage = "The value must be an integer!";
                return false;
            }

            if (count < MinValue)
            {
                ErrorMessage = $"The count must be greater than {MinValue - 1}";
                return false;
            }

            return true;
        }
    }
}
