using Fractions;
using Methods.Models;

namespace Linear_Programming_Calculator_Desktop.Services
{
    /// <summary>
    /// Service for formatting the optimal result summary.
    /// </summary>
    public class OptimalResultSummaryService : IOptimalResultSummaryService
    {
        /// <summary>
        /// Formats the output of a variable assignment.
        /// </summary>
        /// <param name="table">The table containing the results.</param>
        /// <param name="currentIndex">The current index of the variable in the objective function.</param>
        /// <returns>A tuple containing the variable value and its formatted output.</returns>
        /// <remarks>
        /// Searches the row variables for the name and index of the variable that corresponds to the objective function's variable index, 
        /// then retrieves the value from the table if available, and returns the result.
        /// </remarks>
        public (Fraction, string) FormatVariableAssignment(SimplexTable table, int currentIndex)
        {
            (string elementName, int elementIndex) = table.RowVariables.Keys
                .Select((e, ind) => (e, ind))
                .FirstOrDefault(pair => pair.e == $"x{currentIndex + 1}");

            var value = elementIndex >= 0 && elementName is not null
                        ? table.Values[elementIndex, 0]
                        : Fraction.Zero;

            return (value, $"x{currentIndex + 1} = {value}, ");
        }
        /// <summary>
        /// Formats the output of the objective function's optimal value.
        /// </summary>
        /// <param name="table">The table containing the results.</param>
        /// <returns>The formatted optimal result of the objective function.</returns>
        /// <remarks>
        /// Retrieves the first value from the delta row.
        /// </remarks>
        public string FormatObjectiveFunctionValue(SimplexTable table) => $"F = {table.DeltaRow![0].Value}";
    }
}
