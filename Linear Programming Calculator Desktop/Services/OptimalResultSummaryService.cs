using Fractions;
using Methods.Models;

namespace Linear_Programming_Calculator_Desktop.Services
{
    public class OptimalResultSummaryService : IOptimalResultSummaryService
    {

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

        public string FormatObjectiveFunctionValue(SimplexTable table) => $"F = {table.DeltaRow![0].Value}";
    }
}
