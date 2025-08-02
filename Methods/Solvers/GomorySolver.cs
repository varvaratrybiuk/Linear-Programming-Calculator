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
        private readonly Dictionary<(int index, string variableName), Fraction> _result = [];

        public void Pivot()
        {
            // Напрямний рядок
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

            // Напрямний стовпець
            var pivotCol = -1;
            Fraction minTeta = Fraction.PositiveInfinity;
            Table.ThetaRow = [];

            for (int j = 1; j < Table.Values.GetLength(1); j++)
            {
                var element = Table.Values[pivotRow, j];

                if (element < 0)
                {
                    var teta = Fraction.Abs(Table.DeltaRow![j] / element);
                    Table.ThetaRow.Add(teta.ToString());

                    if (teta < minTeta)
                    {
                        minTeta = teta;
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
                throw new InvalidOperationException("Немає розв'язку! Неможливо визначити напрямний стовпець!");
            }

            GomoryHistory[_historyStep].Steps.Add(new SimplexStep()
            {
                PivotColumn = pivotCol,
                PivotRow = pivotRow,
                Table = (SimplexTable)Table.Clone(),
            });

            // Заміна напрямленого рядка
            var newKey = $"x{pivotCol}";
            var newValue = Table.ColumnVariables[$"A{pivotCol}"];

            var oldKey = Table.RowVariables.ElementAt(pivotRow).Key;
            Table.RowVariables.Remove(oldKey);
            Table.RowVariables[newKey] = newValue;

            // Перерахунок
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
                    if (IsUnbounded()) throw new InvalidOperationException("Немає розв'язку! В рядку немає жодного дробовога значення!");
                    var fractionalRow = FindMostFractionalRow();
                    _historyStep++;

                    var variableName = Table.RowVariables.Keys.ToList()[fractionalRow];
                    var readableName = variableName.Replace("x", " ");
                    GomoryHistory.Add(new GomoryHistory()
                    {
                        MaxFracValue = (
                                 int.Parse(readableName),
                                _result.First(k => k.Key.index == fractionalRow).Value
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
        }
        private bool IsUnbounded()
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
                    fractionalRowIndex = keys[i].index;
                }
            }

            return fractionalRowIndex;
        }
        private List<Fraction> BuildGomoryCutRow(int fractionalRowIndex)
        {
            var newBranchCut = new BranchCut();
            // Отримуємо рядок
            var rowValues = new List<Fraction>();
            for (int j = 0; j < Table.Values.GetLength(1); j++)
            {
                rowValues.Add(Table.Values[fractionalRowIndex, j]);
            }

            // Створюємо відсічення
            List<Fraction> cut = [];
            for (int j = 0; j < rowValues.Count; j++)
            {
                var coeff = rowValues[j];
                int wholePart = (int)(coeff.Numerator / coeff.Denominator);
                if (coeff.IsNegative && coeff.Denominator != 1)
                    wholePart = -1;
                Fraction fractionalPart = coeff - wholePart;
                var variableName = Table.RowVariables.Keys.ToList()[fractionalRowIndex];
                var readableName = variableName.Replace("x", " ");
                newBranchCut.Elements[$"y{readableName}{j}"] = (wholePart, coeff);
                cut.Add(-fractionalPart);
            }
            cut.Add(1);
            newBranchCut.CutExpression = new List<Fraction>(cut);
            GomoryHistory[_historyStep].Cut = newBranchCut;
            return cut;
        }
        private void AddCuttingPlaneRow(List<Fraction> cutRow)
        {
            //Додаємо відсічення у симплекс таблицю
            int oldRowCount = Table.Values.GetLength(0);
            int oldColCount = Table.Values.GetLength(1);

            var newRowVarName = $"x{oldColCount}";
            var newColVarName = $"A{oldColCount}";

            Table.RowVariables.Add(newRowVarName, "0");
            Table.ColumnVariables.Add(newColVarName, "0");

            Fraction[,] newTable = new Fraction[oldRowCount + 1, oldColCount + 1];

            // Додаємо значення у симплекс таблицю
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
                decimal valueAsDecimal = (decimal)variableValue;
                if (Math.Abs(valueAsDecimal - Math.Round(valueAsDecimal)) > 1e-6m)
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
        private bool IsOptimal()
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
