using Fractions;

namespace Methods.Models
{
    /// <summary>
    /// Represents a parts of Gomory cut.
    /// </summary>
    public class BranchCut
    {
        /// <summary>
        /// Dictionary containing fractional elements and their integer parts.
        /// </summary>
        /// <remarks>
        /// Each entry maps a fractional variable name (e.g., y20) to a tuple,
        /// where <c>intPart</c> is the integer part [x20] and <c>valueOfOriginalFraction</c> is the original fraction value x20.
        /// </remarks>
        public Dictionary<string, (Fraction intPart, Fraction valueOfOriginalFraction)> FractionalElements { get; set; } = [];
        /// <summary>
        /// List of Gomory cut values, calculated as x20 − [x20].
        /// </summary>
        public List<Fraction> CutExpression { get; set; } = [];

    }
}
