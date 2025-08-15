using Methods.MathObjects;

namespace Methods.Models
{
    /// <summary>
    /// Represents the history of the simplex algorithm solving process.
    /// </summary>
    public class SimplexHistory
    {
        /// <summary>
        /// Initial LPP before any transformations.
        /// </summary>
        public LinearProgrammingProblem? InitialLinearProgrammingProblem { get; set; }
        /// <summary>
        /// LLP that including slack variables.
        /// </summary>
        public LinearProgrammingProblem? SlackVariableProblem { get; set; }
        /// <summary>
        /// LLP that including artificial variables.
        /// </summary>
        public LinearProgrammingProblem? ArtificialProblemProblem { get; set; }
        /// <summary>
        /// Initial basis variables with their values. 
        /// Key — variable name; Value — RHS taken from the corresponding constraint.
        /// </summary>
        public Dictionary<string, string> InitialBasis { get; set; } = [];
        /// <summary>
        /// Optimal simplex table.
        /// </summary>
        public SimplexTable? OptimalTable { get; set; }
        /// <summary>
        /// List of simplex steps performed during the solving process.
        /// </summary>
        public List<SimplexStep> Steps { get; set; } = [];

    }
}
