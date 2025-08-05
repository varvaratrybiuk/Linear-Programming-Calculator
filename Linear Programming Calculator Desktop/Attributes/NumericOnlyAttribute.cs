using System.ComponentModel.DataAnnotations;

namespace Linear_Programming_Calculator_Desktop.Attributes
{
    public class NumericOnlyAttribute : ValidationAttribute
    {
        public NumericOnlyAttribute()
        {
            ErrorMessage = "Enter a valid number";
        }

        public override bool IsValid(object? value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return false;

            return double.TryParse(value.ToString(), out _);
        }
    }
}
