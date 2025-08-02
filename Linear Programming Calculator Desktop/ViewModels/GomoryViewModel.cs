using CommunityToolkit.Mvvm.ComponentModel;
using Fractions;
using Methods.Models;
using System.Text;

namespace Linear_Programming_Calculator_Desktop.ViewModels
{
    public partial class GomoryViewModel(GomoryHistory gomoryStep) : ObservableObject
    {
        public string MaxFractionDisplayText => $"Найбільше дробове значення серед змінних:, x{gomoryStep.MaxFracValue.index} = {gomoryStep.MaxFracValue.value}";
        public List<string> GomoryCutLines => BuildGomoryCutLines();
        public List<SimplexViewModel>? GomoryTables => gomoryStep.Steps.Select(stp =>
        {
            return new SimplexViewModel(stp).LoadFromTable();
        }).ToList();

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
            sb.AppendLine("Визначимо дробові частини:");

            foreach (var (varName, (intPart, fracPart)) in gomoryStep.Cut.Elements)
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

            return $"Запишемо правильне відсічення:\n{string.Join(" + ", lhsParts)} ≥ {constant}";
        }

        private string BuildEqualitySection()
        {
            var lhsParts = GetLhsParts();
            var constant = GetConstant();
            int artificialIndex = gomoryStep.Cut.Elements.Count;

            return $"Приводимо до рівності:\n{string.Join(" + ", lhsParts)} - x{artificialIndex} = {constant}";
        }

        private string BuildRightHandSideSection()
        {
            var rhsParts = new List<string>();
            int artificialIndex = gomoryStep.Cut.Elements.Count;
            rhsParts.Add($"x{artificialIndex}");

            for (int k = 1; k < gomoryStep.Cut.Elements.Count; k++)
            {
                var (intPart, fracPart) = gomoryStep.Cut.Elements.ElementAt(k).Value;
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

            for (int k = 1; k < gomoryStep.Cut.Elements.Count; k++)
            {
                var (intPart, fracPart) = gomoryStep.Cut.Elements.ElementAt(k).Value;
                if (fracPart.Denominator != 1)
                {
                    lhsParts.Add($"{fracPart - intPart}x{k}");
                }
            }

            return lhsParts;
        }

        private Fraction GetConstant()
        {
            var (intPart, fracPart) = gomoryStep.Cut.Elements.ElementAt(0).Value;
            return fracPart - intPart;
        }
    }
}
