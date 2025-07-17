using Fractions;
using Methods.Interfaces;
using Methods.MathObjects;
using Methods.Models;

namespace Methods.Solvers
{
    public class GomorySolver(SimplexTable table, LinearProgrammingProblem problem) : ILinearSolver
    {
        public SimplexTable Table { get => _table; set => _table = value; }
        public List<GomoryHistory> GomoryHistory { get; set; } = [];
        private int _historyStep = -1;

        private SimplexTable _table = table;
        private LinearProgrammingProblem _problem = problem;
        private Dictionary<Tuple<int, string>, Fraction> _result = [];

        public void Pivot()
        {
            // Напрямний рядок
            int pivotRow = -1;
            Fraction minRatio = Fraction.PositiveInfinity;

            for (int i = 0; i < _table.Values.GetLength(0); i++)
            {
                var element = _table.Values[i, 0];
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
            _table.ThetaRow = new List<string>();

            for (int j = 1; j < _table.Values.GetLength(1); j++)
            {
                var element = _table.Values[pivotRow, j];

                if (element < 0)
                {
                    var teta = Fraction.Abs(_table.DeltaRow[j] / element);
                    _table.ThetaRow.Add(teta.ToString());

                    if (teta < minTeta)
                    {
                        minTeta = teta;
                        pivotCol = j;
                    }
                }
                else
                {
                    _table.ThetaRow.Add("-");
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
                Table = (SimplexTable)_table.Clone(),
            });

            // Заміна напрямленого рядка
            var newKey = $"x{pivotCol}";
            var newValue = _table.ColumnVariables[$"A{pivotCol}"];

            var oldKey = _table.RowVariables.ElementAt(pivotRow).Key;
            _table.RowVariables.Remove(oldKey);
            _table.RowVariables[newKey] = newValue;

            // Перерахунок
            Fraction pivotElement = _table.Values[pivotRow, pivotCol];
            int totalColumns = _table.ColumnVariables.Count;
            for (int i = 0; i < _table.RowVariables.Count; i++)
            {
                if (i == pivotRow) continue;
                Fraction factor = _table.Values[i, pivotCol];
                for (int j = 0; j < totalColumns; j++)
                {
                    _table.Values[i, j] -= factor * _table.Values[pivotRow, j] / pivotElement;
                }
            }

            for (int j = 0; j < totalColumns; j++)
            {
                _table.Values[pivotRow, j] /= pivotElement;
            }
            CalculateReducedCosts();

        }

        private void CalculateReducedCosts()
        {
            int rowCount = _table.RowVariables.Count;
            int columnCount = _table.ColumnVariables.Count;

            _table.DeltaRow = new Fraction[columnCount];

            var columnKeys = _table.ColumnVariables.Keys.ToList();

            for (int j = 0; j < columnCount; j++)
            {
                Fraction delta = 0;
                string columnVar = columnKeys[j];

                for (int i = 0; i < rowCount; i++)
                {
                    string rowVar = _table.RowVariables.Keys.ElementAt(i);
                    Fraction cb = Fraction.FromString(_table.RowVariables[rowVar]);

                    Fraction aij = _table.Values[i, j];

                    delta += cb * aij;
                }
                Fraction cj = 0;
                Fraction.TryParse(_table.ColumnVariables[columnVar], out cj);
                _table.DeltaRow[j] = delta - cj;
            }
        }
        public void Solve()
        {
            while (!IsIntegerOptimalSolutionFound() || !isOptimal())
            {
                if (isOptimal())
                {
                    if (IsUnbounded()) throw new InvalidOperationException("Немає розв'язку! В рядку немає жодного дробовога значення!");
                    var fractionalRow = FindMostFractionalRow();
                    _historyStep++;

                    var variableName = _table.RowVariables.Keys.ToList()[fractionalRow];
                    var readableName = variableName.Replace("x", " ");
                    GomoryHistory.Add(new GomoryHistory()
                    {
                        MaxValue = new Tuple<int, Fraction>(int.Parse(readableName), _result.Where(k => k.Key.Item1 == fractionalRow).Select(k => k.Value).First())
                    });
                    var cutRow = BuildGomoryCutRow(fractionalRow);
                    AddCuttingPlaneRow(cutRow);
                }
                Pivot();
                GomoryHistory[_historyStep].Steps.Add(new SimplexStep()
                {
                    PivotColumn = -1,
                    PivotRow = -1,
                    Table = (SimplexTable)_table.Clone(),
                });
            }
        }
        private bool IsUnbounded()
        {
            int fractionalRow = FindMostFractionalRow();
            int columnCount = _table.Values.GetLength(1);

            for (int j = 1; j < columnCount; j++)
            {
                Fraction value = _table.Values[fractionalRow, j];
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
                    fractionalRowIndex = keys[i].Item1;
                }
            }

            return fractionalRowIndex;
        }
        private List<Fraction> BuildGomoryCutRow(int fractionalRowIndex)
        {
            var newBranchCut = new BranchCut();
            // Отримуємо рядок
            var rowValues = new List<Fraction>();
            for (int j = 0; j < _table.Values.GetLength(1); j++)
            {
                rowValues.Add(_table.Values[fractionalRowIndex, j]);
            }

            // Створюємо відсічення
            List<Fraction> cut = new();
            for (int j = 0; j < rowValues.Count; j++)
            {
                var coeff = rowValues[j];
                int wholePart = (int)(coeff.Numerator / coeff.Denominator);
                if (coeff.IsNegative && coeff.Denominator != 1)
                    wholePart = -1;
                Fraction fractionalPart = coeff - wholePart;
                var variableName = _table.RowVariables.Keys.ToList()[fractionalRowIndex];
                var readableName = variableName.Replace("x", " ");
                newBranchCut.Elements[$"y{readableName}{j}"] = new Tuple<Fraction, Fraction>(wholePart, coeff);
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
            int oldRowCount = _table.Values.GetLength(0);
            int oldColCount = _table.Values.GetLength(1);

            var newRowVarName = $"x{oldColCount}";
            var newColVarName = $"A{oldColCount}";

            _table.RowVariables.Add(newRowVarName, "0");
            _table.ColumnVariables.Add(newColVarName, "0");

            Fraction[,] newTable = new Fraction[oldRowCount + 1, oldColCount + 1];

            // Додаємо значення у симплекс таблицю
            for (int i = 0; i < oldRowCount; i++)
            {
                for (int j = 0; j < oldColCount; j++)
                {
                    newTable[i, j] = _table.Values[i, j];
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

            _table.Values = newTable;
            _table.DeltaRow = _table.DeltaRow.Append(Fraction.Zero).ToArray();
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
                if (table.RowVariables.ContainsKey(variableName))
                {
                    int index = _table.RowVariables.Keys.ToList().IndexOf(variableName);
                    _result[new Tuple<int, string>(index, variableName)] = _table.Values[index, 0];
                }
                else
                {
                    _result[new Tuple<int, string>(-1, variableName)] = new Fraction(0);
                }
            }
        }
        private bool isOptimal()
        {
            for (int i = 0; i < _table.Values.GetLength(0); i++)
            {
                if (_table.Values[i, 0] < 0)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
