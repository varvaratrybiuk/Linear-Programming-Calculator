using Fractions;

namespace Methods.Models
{
    /// <summary>
    /// Represents the current state of finding an integer solution using the Gomory cut method.
    /// </summary>
    public class GomoryHistory
    {
        /// <summary>
        /// Maximum fractional part found from the optimal (non-integer) solution.
        /// The tuple contains the row index and the fractional value.
        /// </summary>
        public (int rowIndex, Fraction value) MaxFracValue { get; set; }
        /// <summary>
        /// Current branch cut applied in the Gomory method.
        /// </summary>
        public BranchCut Cut { get; set; } = new BranchCut();
        /// <summary>
        /// List of simplex steps taken during the Gomory cut process.
        /// </summary>
        public List<SimplexStep> Steps { get; set; } = [];
    }
}
