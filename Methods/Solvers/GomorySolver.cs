using Fractions;
using Methods.Interfaces;
using Methods.MathObjects;
using Methods.Models;

namespace Methods.Solvers
{
    public class GomorySolver(SimplexTable Table, LinearProgrammingProblem problem) : ILinearSolver
    {
        public List<GomoryHistory> GomoryHistory { get; set; } = [];

        private int _historyStep = -1;
        private readonly LinearProgrammingProblem _problem = problem;
        private readonly Dictionary<(int rowIndex, string variableName), Fraction> _result = [];

        public void Pivot()
        {
            int pivotRow = -1;
            Fraction minRatio = Fraction.PositiveInfinity;

            for (int i = 0; i < Table.Values.GetLength(0); i++)
            {
                var element = Table.Values[i, 0];
                if (element < 0)
                {
                    if (element < minRatio)
                    {
                        minRatio = element;
                        pivotRow = i;
                    }
                }
            }

            var pivotCol = -1;
            Fraction minTheta = Fraction.PositiveInfinity;
            Table.ThetaRow = [];

            for (int j = 1; j < Table.Values.GetLength(1); j++)
            {
                var element = Table.Values[pivotRow, j];

                if (element < 0)
                {
                    var theta = Fraction.Abs(Table.DeltaRow![j] / element);
                    Table.ThetaRow.Add(theta.ToString());

                    if (theta < minTheta)
                    {
                        minTheta = theta;
                        pivotCol = j;
                    }
                }
                else
                {
                    Table.ThetaRow.Add("-");
                }
            }

            if (pivotCol == -1)
            {
                throw new InvalidOperationException("No solution! It's impossible to determine the pivot column!");
            }

            GomoryHistory[_historyStep].Steps.Add(new SimplexStep()
            {
                PivotColumn = pivotCol,
                PivotRow = pivotRow,
                Table = (SimplexTable)Table.Clone(),
            });

            var newKey = $"x{pivotCol}";
            var newValue = Table.ColumnVariables[$"A{pivotCol}"];

            var oldKey = Table.RowVariables.ElementAt(pivotRow).Key;
            Table.RowVariables.Remove(oldKey);
            Table.RowVariables[newKey] = newValue;

            Fraction pivotElement = Table.Values[pivotRow, pivotCol];
            int totalColumns = Table.ColumnVariables.Count;
            for (int i = 0; i < Table.RowVariables.Count; i++)
            {
                if (i == pivotRow) continue;
                Fraction factor = Table.Values[i, pivotCol];
                for (int j = 0; j < totalColumns; j++)
                {
                    Table.Values[i, j] -= factor * Table.Values[pivotRow, j] / pivotElement;
                }
            }

            for (int j = 0; j < totalColumns; j++)
            {
                Table.Values[pivotRow, j] /= pivotElement;
            }
            CalculateReducedCosts();

        }

        private void CalculateReducedCosts()
        {
            int rowCount = Table.RowVariables.Count;
            int columnCount = Table.ColumnVariables.Count;

            Table.DeltaRow = new Fraction[columnCount];

            var columnKeys = Table.ColumnVariables.Keys.ToList();

            for (int j = 0; j < columnCount; j++)
            {
                Fraction delta = 0;
                string columnVar = columnKeys[j];

                for (int i = 0; i < rowCount; i++)
                {
                    string rowVar = Table.RowVariables.Keys.ElementAt(i);
                    Fraction cb = Fraction.FromString(Table.RowVariables[rowVar]);

                    Fraction aij = Table.Values[i, j];

                    delta += cb * aij;
                }
                Fraction.TryParse(Table.ColumnVariables[columnVar], out Fraction cj);
                Table.DeltaRow[j] = delta - cj;
            }
        }
        public void Solve()
        {
            while (!IsIntegerOptimalSolutionFound() || !IsOptimal())
            {
                if (IsOptimal())
                {
                    if (IsUnbounded()) throw new InvalidOperationException("No solution! There is no fractional value in the row!");
                    var fractionalRow = FindMostFractionalRow();
                    _historyStep++;

                    var variableIndex = GetVariableIndexName(fractionalRow);
                    GomoryHistory.Add(new GomoryHistory()
                    {
                        MaxFracValue = (
                                 int.Parse(variableIndex),
                                _result.First(k => k.Key.rowIndex == fractionalRow).Value
                            )
                    });
                    var cutRow = BuildGomoryCutRow(fractionalRow);
                    AddCuttingPlaneRow(cutRow);
                }
                Pivot();
                GomoryHistory[_historyStep].Steps.Add(new SimplexStep()
                {
                    PivotColumn = -1,
                    PivotRow = -1,
                    Table = (SimplexTable)Table.Clone(),
                });
            }

            if (GomoryHistory.Count == 0)
                throw new InvalidOperationException("The optimal solution to the problem is already integer!");
        }

        private string GetVariableIndexName(int fractionalRow)
        {
            var variableName = Table.RowVariables.Keys.ToList()[fractionalRow];
            return variableName.Replace("x", " ");
        }

        public bool IsUnbounded()
        {
            int fractionalRow = FindMostFractionalRow();
            int columnCount = Table.Values.GetLength(1);

            for (int j = 1; j < columnCount; j++)
            {
                Fraction value = Table.Values[fractionalRow, j];
                if (value.Numerator % value.Denominator != 0)
                {
                    return false;
                }
            }
            return true;
        }
        private int FindMostFractionalRow()
        {
            int fractionalRowIndex = -1;
            Fraction maxFraction = 0;
            var values = _result.Values.ToList();
            var keys = _result.Keys.ToList();

            for (int i = 0; i < values.Count; i++)
            {
                Fraction value = values[i];
                int wholePart = (int)(value.Numerator / value.Denominator);
                Fraction fractionalPart = value - wholePart;

                if (fractionalPart > maxFraction)
                {
                    maxFraction = fractionalPart;
                    fractionalRowIndex = keys[i].rowIndex;
                }
            }

            return fractionalRowIndex;
        }
        private List<Fraction> BuildGomoryCutRow(int fractionalRowIndex)
        {
            var newBranchCut = new BranchCut();

            var rowValues = new List<Fraction>();
            for (int j = 0; j < Table.Values.GetLength(1); j++)
            {
                rowValues.Add(Table.Values[fractionalRowIndex, j]);
            }


            List<Fraction> cut = [];
            for (int j = 0; j < rowValues.Count; j++)
            {
                var coeff = rowValues[j];
                int wholePart = (int)(coeff.Numerator / coeff.Denominator);
                if (coeff.IsNegative && coeff.Denominator != 1)
                    wholePart = -1;
                Fraction fractionalPart = coeff - wholePart;

                var variableIndex = GetVariableIndexName(fractionalRowIndex);
                newBranchCut.FractionalElements[$"y{variableIndex}{j}"] = (wholePart, coeff);

                cut.Add(-fractionalPart);
            }

            cut.Add(1);
            newBranchCut.CutExpression = new List<Fraction>(cut);
            GomoryHistory[_historyStep].Cut = newBranchCut;

            return cut;
        }
        private void AddCuttingPlaneRow(List<Fraction> cutRow)
        {
            int oldRowCount = Table.Values.GetLength(0);
            int oldColCount = Table.Values.GetLength(1);

            var newRowVarName = $"x{oldColCount}";
            var newColVarName = $"A{oldColCount}";

            Table.RowVariables.Add(newRowVarName, "0");
            Table.ColumnVariables.Add(newColVarName, "0");

            Fraction[,] newTable = new Fraction[oldRowCount + 1, oldColCount + 1];

            for (int i = 0; i < oldRowCount; i++)
            {
                for (int j = 0; j < oldColCount; j++)
                {
                    newTable[i, j] = Table.Values[i, j];
                }
            }

            for (int i = 0; i < oldRowCount; i++)
            {
                newTable[i, oldColCount] = Fraction.Zero;
            }

            for (int j = 0; j < cutRow.Count; j++)
            {
                newTable[oldRowCount, j] = cutRow[j];
            }

            newTable[oldRowCount, oldColCount] = Fraction.One;

            Table.Values = newTable;
            Table.DeltaRow = [.. Table.DeltaRow!, Fraction.Zero];
        }

        private bool IsIntegerOptimalSolutionFound()
        {
            ExtractBaseVariables();
            foreach (var variableValue in _result.Values)
            {
                if (!int.TryParse(variableValue.ToString(), out _))
                    return false;
            }
            return true;

        }

        private void ExtractBaseVariables()
        {
            for (int i = 0; i < _problem.ObjectiveFunctionCoefficients.Count; i++)
            {
                string variableName = $"x{i + 1}";
                if (Table.RowVariables.ContainsKey(variableName))
                {
                    int index = Table.RowVariables.Keys.ToList().IndexOf(variableName);
                    _result[(index, variableName)] = Table.Values[index, 0];
                }
                else
                {
                    _result[(-1, variableName)] = new Fraction(0);
                }
            }
        }

        public bool IsOptimal()
        {
            for (int i = 0; i < Table.Values.GetLength(0); i++)
            {
                if (Table.Values[i, 0] < 0)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
