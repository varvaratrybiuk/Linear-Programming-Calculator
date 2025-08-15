using Fractions;
using Methods.Models;
using System.Text;

namespace Linear_Programming_Calculator_Desktop.Services
{
    /// <summary>
    /// Service for formatting Gomory cuts into readable string.
    /// </summary>
    public class GomoryCutFormatterService : IGomoryCutFormatterService
    {
        public List<string> BuildGomoryCutLines(BranchCut branchCut)
        {

            var lines = new List<string>
            {
                BuildFractionPartsSection(branchCut),
                BuildInequalitySection(branchCut),
                BuildEqualitySection(branchCut),
                BuildRightHandSideSection(branchCut)
            };

            return lines;
        }

        public string BuildFractionPartsSection(BranchCut branchCut)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Determine the fractional parts:");

            foreach (var (varName, (intPart, fracPart)) in branchCut.FractionalElements)
            {
                if (fracPart.Denominator == 1) continue;

                string intStr = intPart.IsNegative ? $"({intPart})" : intPart.ToString();
                sb.AppendLine($"{varName} = {fracPart} - {intStr} = {fracPart - intPart}");
            }

            return sb.ToString().TrimEnd();
        }

        public string BuildInequalitySection(BranchCut branchCut)
        {
            var lhsParts = GetLhsParts(branchCut);
            var constant = GetConstant(branchCut);

            return $"Write the valid cut: \n{string.Join(" + ", lhsParts)} ≥ {constant}";
        }

        public string BuildEqualitySection(BranchCut branchCut)
        {
            var lhsParts = GetLhsParts(branchCut);
            var constant = GetConstant(branchCut);
            int artificialElementIndex = branchCut.FractionalElements.Count;

            return $"Convert to an equation: \n{string.Join(" + ", lhsParts)} - x{artificialElementIndex} = {constant}";
        }

        public string BuildRightHandSideSection(BranchCut branchCut)
        {
            var rhsParts = new List<string>();
            int artificialElementIndex = branchCut.FractionalElements.Count;
            rhsParts.Add($"x{artificialElementIndex}");

            for (int k = 1; k < branchCut.FractionalElements.Count; k++)
            {
                var (intPart, fracPart) = branchCut.FractionalElements.ElementAt(k).Value;
                if (fracPart.Denominator != 1)
                {
                    rhsParts.Add($"- {fracPart - intPart}x{k}");
                }
            }

            var constant = GetConstant(branchCut);
            return $"Convert to:\n{-constant} = {string.Join(" ", rhsParts)}";
        }

        /// <summary>
        /// Gets the left-hand side terms of the Gomory cut as a list of strings.
        /// </summary>
        /// <returns>A list of formatted LHS terms.</returns>
        private List<string> GetLhsParts(BranchCut branchCut)
        {
            var lhsParts = new List<string>();

            for (int k = 1; k < branchCut.FractionalElements.Count; k++)
            {
                var (intPart, fracPart) = branchCut.FractionalElements.ElementAt(k).Value;
                if (fracPart.Denominator != 1)
                {
                    lhsParts.Add($"{fracPart - intPart}x{k}");
                }
            }

            return lhsParts;
        }

        /// <summary>
        /// Gets the constant term of the Gomory cut based on the first fractional element.
        /// </summary>
        /// <returns>The constant as a <see cref="Fraction"/>.</returns>
        private Fraction GetConstant(BranchCut branchCut)
        {
            var (intPart, fracPart) = branchCut.FractionalElements.ElementAt(0).Value;
            return fracPart - intPart;
        }
    }
}
