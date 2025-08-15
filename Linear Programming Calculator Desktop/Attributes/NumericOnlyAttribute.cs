using System.ComponentModel.DataAnnotations;

namespace Linear_Programming_Calculator_Desktop.Attributes
{
    /// <summary>
    /// Validation attribute that ensures a value represents a valid numeric (double) input.
    /// </summary>
    public class NumericOnlyAttribute : ValidationAttribute
    {
        public NumericOnlyAttribute()
        {
            ErrorMessage = "Enter a valid number";
        }
        /// <summary>
        /// Determines whether the specified value is a valid number.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <returns>
        /// True if <paramref name="value"/> is non-null, not whitespace, and can be parsed as a double; otherwise, false.
        /// </returns>
        public override bool IsValid(object? value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return false;

            return double.TryParse(value.ToString(), out _);
        }
    }
}
