using Fractions;
using Methods.Models;

namespace Linear_Programming_Calculator_Desktop.Services
{
    /// <summary>
    /// Service interface for formatting the optimal result summary.
    /// </summary>
    public interface IOptimalResultSummaryService
    {
        /// <summary>
        /// Formats the output of a variable assignment.
        /// </summary>
        /// <param name="table">The table containing the results.</param>
        /// <param name="currentIndex">The current index of the variable in the objective function.</param>
        /// <returns>A tuple containing the variable value and its formatted output.</returns>
        (Fraction, string) FormatVariableAssignment(SimplexTable table, int currentIndex);
        /// <summary>
        /// Formats the output of the objective function's optimal value.
        /// </summary>
        /// <param name="table">The table containing the results.</param>
        /// <returns>The formatted optimal result of the objective function.</returns>
        string FormatObjectiveFunctionValue(SimplexTable table);

    }
}
