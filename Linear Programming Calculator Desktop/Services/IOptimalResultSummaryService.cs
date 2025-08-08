using Fractions;
using Methods.Models;

namespace Linear_Programming_Calculator_Desktop.Services
{
    public interface IOptimalResultSummaryService
    {
        (Fraction, string) FormatVariableAssignment(SimplexTable table, int currentIndex);

        string FormatObjectiveFunctionValue(SimplexTable table);

    }
}
