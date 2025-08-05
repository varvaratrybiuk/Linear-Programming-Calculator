using CommunityToolkit.Mvvm.ComponentModel;
using Fractions;
using Methods.Models;
using System.Text;

namespace Linear_Programming_Calculator_Desktop.ViewModels
{
    public partial class GomoryViewModel(GomoryHistory gomoryStep, List<string> objFuncCoeff) : ObservableObject
    {
        public string MaxFractionDisplayText => $"Maximum fractional part among the variables:, x{gomoryStep.MaxFracValue.rowIndex} = {gomoryStep.MaxFracValue.value}";
        public List<string> GomoryCutLines => BuildGomoryCutLines();
        public List<SimplexViewModel>? GomoryTables => gomoryStep.Steps.Select(stp =>
        {
            return new SimplexViewModel(stp).LoadFromTable();
        }).ToList();

        public string GomoryStepSummary
        {
            get
            {
                StringBuilder summary = new();
                bool hasIntegerAnswer = true;

                for (int i = 0; i < objFuncCoeff.Count; i++)
                {
                    (string elementName, int elementIndex) = gomoryStep.Steps.Last().Table.RowVariables.Keys
                             .Select((e, ind) => (e, ind))
                             .FirstOrDefault(pair => pair.e == $"x{i + 1}");
                    var value = elementIndex >= 0 && elementName is not null ? gomoryStep.Steps.Last().Table.Values[elementIndex, 0] : Fraction.Zero;

                    summary.Append($"x{i + 1} = {value}, ");

                    if (hasIntegerAnswer)
                        hasIntegerAnswer = int.TryParse(value.ToString(), out _);

                }

                summary.Append($"F = {gomoryStep.Steps.Last().Table.DeltaRow![0]} ");

                summary.Append(hasIntegerAnswer ? "are integers." : "aren't integers.");

                return summary.ToString();
            }
        }


        private List<string> BuildGomoryCutLines()
        {
            var lines = new List<string>
            {
                BuildFractionPartsSection(),
                BuildInequalitySection(),
                BuildEqualitySection(),
                BuildRightHandSideSection()
            };

            return lines;
        }

        private string BuildFractionPartsSection()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Determine the fractional parts:");

            foreach (var (varName, (intPart, fracPart)) in gomoryStep.Cut.FractionalElements)
            {
                if (fracPart.Denominator == 1) continue;

                string intStr = intPart.IsNegative ? $"({intPart})" : intPart.ToString();
                sb.AppendLine($"{varName} = {fracPart} - {intStr} = {fracPart - intPart}");
            }

            return sb.ToString().TrimEnd();
        }

        private string BuildInequalitySection()
        {
            var lhsParts = GetLhsParts();
            var constant = GetConstant();

            return $"Write the valid cut: \n{string.Join(" + ", lhsParts)} ≥ {constant}";
        }

        private string BuildEqualitySection()
        {
            var lhsParts = GetLhsParts();
            var constant = GetConstant();
            int artificialElementIndex = gomoryStep.Cut.FractionalElements.Count;

            return $"Convert to an equation: \n{string.Join(" + ", lhsParts)} - x{artificialElementIndex} = {constant}";
        }

        private string BuildRightHandSideSection()
        {
            var rhsParts = new List<string>();
            int artificialElementIndex = gomoryStep.Cut.FractionalElements.Count;
            rhsParts.Add($"x{artificialElementIndex}");

            for (int k = 1; k < gomoryStep.Cut.FractionalElements.Count; k++)
            {
                var (intPart, fracPart) = gomoryStep.Cut.FractionalElements.ElementAt(k).Value;
                if (fracPart.Denominator != 1)
                {
                    rhsParts.Add($"- {fracPart - intPart}x{k}");
                }
            }

            var constant = GetConstant();
            return $"Перетворимо:\n{-constant} = {string.Join(" ", rhsParts)}";
        }

        private List<string> GetLhsParts()
        {
            var lhsParts = new List<string>();

            for (int k = 1; k < gomoryStep.Cut.FractionalElements.Count; k++)
            {
                var (intPart, fracPart) = gomoryStep.Cut.FractionalElements.ElementAt(k).Value;
                if (fracPart.Denominator != 1)
                {
                    lhsParts.Add($"{fracPart - intPart}x{k}");
                }
            }

            return lhsParts;
        }

        private Fraction GetConstant()
        {
            var (intPart, fracPart) = gomoryStep.Cut.FractionalElements.ElementAt(0).Value;
            return fracPart - intPart;
        }
    }
}
