using CommunityToolkit.Mvvm.ComponentModel;
using Linear_Programming_Calculator_Desktop.Services;
using Methods.Models;
using System.Text;

namespace Linear_Programming_Calculator_Desktop.ViewModels
{
    /// <summary>
    /// Represents a ViewModel with the single step of the Gomory cutting-plane method.
    /// </summary>
    /// <param name="gomoryStep">The current step of Gomory history containing the cut and fractional values.</param>
    /// <param name="objFuncCoeff">List of objective function coefficients for reference in formatting.</param>
    /// <param name="summaryService">Service for generating summaries of optimal results.</param>
    /// <param name="cutFormatterService">Service for formatting Gomory cuts into readable strings.</param>
    public partial class GomoryViewModel(GomoryHistory gomoryStep, List<string> objFuncCoeff, IOptimalResultSummaryService summaryService, IGomoryCutFormatterService cutFormatterService) : ObservableObject
    {
        /// <summary>
        /// Text displaying the variable with the maximum fractional part in this Gomory step.
        /// </summary>
        public string MaxFractionDisplayText => $"Maximum fractional part among the variables:, x{gomoryStep.MaxFracValue.rowIndex} = {gomoryStep.MaxFracValue.value}";

        /// <summary>
        /// List of formatted strings representing the Gomory cut for this step.
        /// </summary>
        public List<string> GomoryCutLines => cutFormatterService.BuildGomoryCutLines(gomoryStep.Cut);

        /// <summary>
        /// List of <see cref="SimplexViewModel"/> representing simplex tables associated with this Gomory step.
        /// </summary>
        public List<SimplexViewModel>? GomoryTables => gomoryStep.Steps.Select(stp =>
        {
            return new SimplexViewModel(stp).LoadFromTable();
        }).ToList();

        /// <summary>
        /// A summary string describing this Gomory step.
        /// </summary>
        public string GomoryStepSummary
        {
            get
            {
                StringBuilder summary = new();
                bool hasIntegerAnswer = true;

                for (int i = 0; i < objFuncCoeff.Count; i++)
                {
                    var (value, resultStr) = summaryService.FormatVariableAssignment(gomoryStep.Steps.Last().Table, i);

                    summary.Append(resultStr);
                    if (hasIntegerAnswer)
                        hasIntegerAnswer = int.TryParse(value.ToString(), out _);

                }
                summary.Append(summaryService.FormatObjectiveFunctionValue(gomoryStep.Steps.Last().Table));

                summary.Append(hasIntegerAnswer ? " are integers." : " aren't integers.");

                return summary.ToString();
            }
        }
    }
}
