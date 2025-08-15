using Methods.MathObjects;

namespace Linear_Programming_Calculator_Desktop.Services
{
    /// <summary>
    /// Service interface that provides methods to format different parts of a LPP for display.
    /// </summary>
    public interface IProblemFormatterService
    {
        /// <summary>
        /// Builds a formatted string representation of the objective function.
        /// </summary>
        /// <param name="problem">The linear programming problem.</param>
        /// <returns>A formatted string of the objective function.</returns>
        string BuildObjectiveFunction(LinearProgrammingProblem problem);

        /// <summary>
        /// Builds a list of formatted strings representing the problem constraints.
        /// </summary>
        /// <param name="problem">The linear programming problem.</param>
        /// <param name="isEqual">Indicates whether constraints are equalities or inequalities.</param>
        /// <returns>A list of formatted constraint strings.</returns>
        List<string> BuildConstraints(LinearProgrammingProblem problem, bool isEqual);

        /// <summary>
        /// Builds a formatted string representing the domain constraint, such as x1, x2, ..., xn ≥ 0..
        /// </summary>
        /// <param name="problem">The linear programming problem.</param>
        /// <returns>A formatted string of the domain constraints.</returns>
        string BuildDomain(LinearProgrammingProblem problem);

        /// <summary>
        /// Builds a formatted note about integer constraint if the problem is integer-constrained.
        /// </summary>
        /// <param name="problem">The linear programming problem.</param>
        /// <param name="isIntegerProblem">Indicates whether the problem has integer constraint.</param>
        /// <returns>A formatted string note about integer constraint, or an empty string if not applicable.</returns>
        string BuildIntegerNote(LinearProgrammingProblem problem, bool isIntegerProblem);

    }
}
