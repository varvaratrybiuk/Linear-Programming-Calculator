using Fractions;

namespace Methods.Models
{
    public class ExpressionValue(string expressionText, Fraction value)
    {
        public string ExpressionText { get; set; } = expressionText;

        public Fraction Value { get; set; } = value;
    }
}
