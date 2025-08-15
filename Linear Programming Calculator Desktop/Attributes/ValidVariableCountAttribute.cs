using System.ComponentModel.DataAnnotations;

namespace Linear_Programming_Calculator_Desktop.Attributes
{
    /// <summary>
    /// Validation attribute to ensure an integer input meets a minimum value requirement.
    /// </summary>
    /// <param name="minValue">The minimum acceptable integer value.</param>
    public class ValidVariableCountAttribute(int minValue) : ValidationAttribute
    {
        /// <summary>
        /// Gets the minimum allowed value for the input.
        /// </summary>
        public int MinValue { get; } = minValue;

        /// <summary>
        /// Validates whether the input value is a non-null, non-empty integer
        /// and meets the minimum value requirement.
        /// </summary>
        /// <param name="value">The input value to validate.</param>
        /// <returns>True if the input is valid; otherwise, false.</returns>
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
