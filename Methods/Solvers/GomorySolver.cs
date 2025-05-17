using Fractions;
using Methods.Interfaces;
using Methods.MathObjects;
using Methods.Models;

namespace Methods.Solvers
{
    public class GomorySolver(SimplexTable table, LinearProgrammingProblem problem) : ILinearSolver
    {
        private SimplexTable _table = table;
        private LinearProgrammingProblem _problem = problem;
        private Dictionary<string, Fraction> result = new Dictionary<string, Fraction>();
        SimplexTable ILinearSolver.Table { get => _table; set => _table = value; }

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
            _table.TetaRow = new List<string>();

            for (int j = 1; j < _table.Values.GetLength(1); j++)
            {
                var element = _table.Values[pivotRow, j];

                if (element < 0)
                {
                    var teta = Fraction.Abs(_table.DeltaRow[j] / element);
                    _table.TetaRow.Add(teta.ToString());

                    if (teta < minTeta)
                    {
                        minTeta = teta;
                        pivotCol = j;
                    }
                }
                else
                {
                    _table.TetaRow.Add("-");
                }
            }

            if (pivotCol == -1)
            {
                throw new InvalidOperationException("Немає розв'язку!");
            }

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
            // Перевірка на цілочисельність
            // Шукаємо найбільше ДРОБОВЕ значення, яке лежить в межах від 0 до 1
            // Створюємо відсічення
            // Додаємо у нашу симплекс таблицю нове відсічення (наш напрямний рядок)
            // Розраховуємо min {^/Aij} наш стовпець (значення повинні бути від'ємні тільки для них розраховуємо)
            // Якщо є від'ємні в A0, то треба перераховувати
            // Перевірка чи у вибраному стовпці є дробові значення, якщо немає (Немає розв'язку)
            // Перераховуємо все
            // Розрахунок індексного рядка
            // Перевірка A0, щоб не було від'єних
            // Знову
            while (!IsIntegerOptimalSolutionFound() || !isOptimal())
            {
                if (isOptimal())
                {
                    if (IsUnbounded()) throw new InvalidOperationException("Немає розв'язку!");
                    var fractionalRow = FindMostFractionalRow();
                    var cutRow = BuildGomoryCutRow(fractionalRow);
                    AddCuttingPlaneRow(cutRow);
                }
                Pivot();
            }
            Console.WriteLine("я ВИРІШИЛА");
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
            var values = result.Values.ToList();

            for (int i = 0; i < values.Count; i++)
            {
                Fraction value = values[i];
                int wholePart = (int)(value.Numerator / value.Denominator);
                Fraction fractionalPart = value - wholePart;

                if (fractionalPart > maxFraction)
                {
                    maxFraction = fractionalPart;
                    fractionalRowIndex = i;
                }
            }

            return fractionalRowIndex;
        }
        private List<Fraction> BuildGomoryCutRow(int fractionalRowIndex)
        {
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
                cut.Add(-fractionalPart);
            }
            cut.Add(1);
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
            // Винести в окремий методи
            for (int i = 0; i < _problem.ObjectiveFunctionCoefficients.Count; i++)
            {
                string variableName = $"x{i + 1}";
                if (_table.RowVariables.ContainsKey(variableName))
                {
                    var row = _table.RowVariables[variableName];
                    int index = _table.RowVariables.Keys.ToList().IndexOf(variableName);
                    var value = _table.Values[index, 0];
                    result[variableName] = value;
                    continue;
                }
                result[variableName] = 0;
            }
            foreach (var variableValue in result.Values)
            {
                decimal valueAsDecimal = (decimal)variableValue;
                if (Math.Abs(valueAsDecimal - Math.Round(valueAsDecimal)) > 1e-6m)
                    return false;
            }
            return true;

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
        SimplexTable ILinearSolver.GetSolution()
        {
            return _table;
        }
    }
}
