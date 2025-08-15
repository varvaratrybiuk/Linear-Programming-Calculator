using Fractions;

namespace Methods.Models
{
    /// <summary>
    /// Represents a text expression for the table and its actual numeric value.
    /// </summary>
    public class ExpressionValue(string expressionText, Fraction value)
    {
        /// <summary>
        /// Formatted text for the table, used in the Big M method to represent expressions like "7M - 11".
        /// </summary>
        public string ExpressionText { get; set; } = expressionText;
        /// <summary>
        /// Actual numeric value of the expression.
        /// </summary>
        public Fraction Value { get; set; } = value;
    }
}
