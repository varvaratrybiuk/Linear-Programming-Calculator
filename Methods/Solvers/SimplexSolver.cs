using Fractions;
using Methods.Enums;
using Methods.Interfaces;
using Methods.MathObjects;
using Methods.Models;
using System.Data;
using System.Globalization;

namespace Methods.Solvers
{
    public class SimplexSolver(LinearProgrammingProblem problem) : ILinearSolver
    {
        private SimplexTable? _table;
        private LinearProgrammingProblem _problem = problem;
        private const double M = int.MaxValue;
        public SimplexTable Table { get => _table; set => _table = value; }

        public SimplexTable GetSolution()
        {
            return _table;
        }

        public void Solve()
        {
            // Приведення до канонічного вигляду
            ConvertToCanonicalForm();
            // Додавання вільних змінних та штучних змінних
            ProcessAuxiliaryVariables(isSlack: true);
            ProcessAuxiliaryVariables(isSlack: false);
            // Заповнення першої таблиці
            InitializeTableau();
            // Пошук оптимального рішення, якщо це можливо
            while (true)
            {
                CalculateReducedCosts();
                if (IsUnbounded()) throw new InvalidOperationException("Немає розв'язку!");
                if (IsOptimal()) break;
                Pivot();
            }
            // Видалення штучних змінних
            RemoveArtificialVariables();

            Console.WriteLine("я ВИРІШИЛА");
        }

        // Видалення штучних змінних
        private void RemoveArtificialVariables()
        {
            int artificialVariablesCount = _problem.ArtificialVariableCoefficients.Count;

            if (artificialVariablesCount <= 0)
                return;

            var allKeys = _table.ColumnVariables.Keys.ToList();

            var keysToRemove = allKeys.Skip(allKeys.Count - artificialVariablesCount).ToList();
            var indexesToRemove = keysToRemove
                .Select(key => allKeys.IndexOf(key))
                .OrderByDescending(i => i)
                .ToList();

            foreach (var key in keysToRemove)
            {
                _table.ColumnVariables.Remove(key);
            }

            int rows = _table.Values.GetLength(0);
            int cols = _table.Values.GetLength(1) - artificialVariablesCount;
            var newValues = new Fraction[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                int newCol = 0;
                for (int j = 0; j < _table.Values.GetLength(1); j++)
                {
                    if (!indexesToRemove.Contains(j))
                    {
                        newValues[i, newCol++] = _table.Values[i, j];
                    }
                }
            }

            _table.Values = newValues;

            var newDelta = new Fraction[cols];
            int newDeltaIndex = 0;
            for (int i = 0; i < _table.DeltaRow.Length; i++)
            {
                if (!indexesToRemove.Contains(i))
                {
                    newDelta[newDeltaIndex++] = _table.DeltaRow[i];
                }
            }

            _table.DeltaRow = newDelta;
        }

        // Додавання вільних та штучних змін
        private void ProcessAuxiliaryVariables(bool isSlack)
        {
            int constCount = _problem.Constraints.Count;
            for (int i = 0; i < constCount; i++)
            {
                var constType = _problem.Constraints[i].Type;
                switch (isSlack)
                {
                    case true:
                        if (constType != ConstraintType.Equal)
                        {
                            AddAuxiliaryVariable(_problem.Constraints, constType, i, isSlack);
                        }
                        break;
                    case false:
                        if (constType == ConstraintType.GreaterThanOrEqual || constType == ConstraintType.Equal)
                        {
                            AddAuxiliaryVariable(_problem.Constraints, constType, i, isSlack);
                        }
                        break;
                }

            }
        }

        private void AddAuxiliaryVariable(List<MathObjects.Constraint> constraints, ConstraintType type, int index, bool isSlack)
        {
            for (int i = 0; i < constraints.Count; i++)
            {
                if (i == index)
                {
                    if (isSlack)
                    {
                        switch (type)
                        {
                            case ConstraintType.GreaterThanOrEqual:
                                constraints[i].Coefficients.Add("-1");
                                break;
                            case ConstraintType.LessThanOrEqual:
                                constraints[i].Coefficients.Add("1");
                                break;
                        }
                        continue;
                    }
                    else
                    {
                        if (type == ConstraintType.GreaterThanOrEqual || type == ConstraintType.Equal)
                            constraints[i].Coefficients.Add("1");
                    }
                }
                constraints[i].Coefficients.Add("0");
            }
            if (isSlack)
                _problem.SlackVariableCoefficients?.Add("0");
            else
                _problem.ArtificialVariableCoefficients?.Add(_problem.IsMaximization ? (-M).ToString() : M.ToString());
        }

        // Ініціалізація початкової симплекс таблиці
        private void InitializeTableau()
        {
            int totalColumns = _problem.VariablesCount;
            int baseVariableCount = _problem.ObjectiveFunctionCoefficients.Count;
            _table = new SimplexTable
            {
                RowVariables = [],
                ColumnVariables = [],
                Values = new Fraction[_problem.Constraints.Count, totalColumns + 1]
            };

            _table.ColumnVariables.Add("A0", "-");
            for (int i = 0; i < totalColumns; i++)
            {
                string variableName = $"A{i + 1}";
                string coefficient = GetObjectiveCoefficient(i).ToString();
                _table.ColumnVariables.Add(variableName, coefficient);
            }

            // Знаходимо початковий допустимий базисний розв’язок
            List<int> usedRows = new();
            TryAssignBasicVariableColumns(usedRows);

            // Заповнюємо таблицю
            for (int i = 0; i < _problem.Constraints.Count; i++)
            {
                if (usedRows.Contains(i)) continue;

                for (int j = 0; j < totalColumns; j++)
                {
                    if (_problem.Constraints[i].Coefficients[j] == "1" || _problem.Constraints[i].Coefficients[j] == "-1")
                    {
                        AddTableRow(i, j);
                        usedRows.Add(i);
                        break;
                    }
                }
            }

            CalculateReducedCosts();
        }
        private void TryAssignBasicVariableColumns(List<int> usedRows)
        {
            int totalColumns = _problem.VariablesCount;
            int baseVariableCount = _problem.ObjectiveFunctionCoefficients.Count;

            for (int j = baseVariableCount; j < totalColumns; j++)
            {
                for (int i = 0; i < _problem.Constraints.Count; i++)
                {
                    if (usedRows.Contains(i)) continue;

                    if (_problem.Constraints[i].Coefficients[j] == "1")
                    {
                        bool isUnitColumn = true;

                        for (int k = 0; k < _problem.Constraints.Count; k++)
                        {
                            if (k != i && _problem.Constraints[k].Coefficients[j] != "0")
                            {
                                isUnitColumn = false;
                                break;
                            }
                        }

                        if (isUnitColumn)
                        {
                            AddTableRow(i, j);
                            usedRows.Add(i);
                            break;
                        }
                    }
                }
            }
        }
        private void AddTableRow(int constraintIndex, int basicVarIndex)
        {
            int rowIndex = _table.RowVariables.Count;
            int totalColumns = _problem.VariablesCount;

            _table.RowVariables.Add($"x{basicVarIndex + 1}", GetObjectiveCoefficient(basicVarIndex).ToString());
            _table.Values[rowIndex, 0] = Fraction.FromString(_problem.Constraints[constraintIndex].RightHandSide);

            for (int j = 0; j < totalColumns; j++)
            {
                _table.Values[rowIndex, j + 1] = Fraction.FromString(_problem.Constraints[constraintIndex].Coefficients[j]);
            }
        }

        private string GetObjectiveCoefficient(int index)
        {
            if (index < _problem.ObjectiveFunctionCoefficients.Count)
                return _problem.ObjectiveFunctionCoefficients[index];
            else if (index < _problem.ObjectiveFunctionCoefficients.Count + (_problem.SlackVariableCoefficients?.Count ?? 0))
                return _problem.SlackVariableCoefficients![index - _problem.ObjectiveFunctionCoefficients.Count];
            else
                return _problem.ArtificialVariableCoefficients![index - _problem.ObjectiveFunctionCoefficients.Count - (_problem.SlackVariableCoefficients?.Count ?? 0)];
        }
        // Розрахунок індексного рядка
        private void CalculateReducedCosts()
        {
            int rowCount = _problem.Constraints.Count;
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
        private bool IsOptimal()
        {
            return _problem.IsMaximization ? !_table.DeltaRow.Skip(1).Any(n => n < 0) : !_table.DeltaRow.Skip(1).Any(n => n > 0);
        }
        private bool IsUnbounded()
        {
            int rowIndex = _problem.Constraints.Count;
            // Штучні змінні не вивелися
            if (_table.RowVariables
                    .Select(row => row.Value)
                    .Any(value => double.Parse(value) == M || double.Parse(value) == -M) &&
                (_problem.IsMaximization
                    ? !_table.DeltaRow.Skip(1).Any(n => n < 0)
                    : !_table.DeltaRow.Skip(1).Any(n => n > 0)))
            {
                return true;
            }
            return false;
        }

        // Пошук оптимального значення
        public void Pivot()
        {
            int basicVariablesCount = _problem.Constraints.Count; // к-сть умов обмежень
            // Визначення напрямного стовпця
            int offset = 1;
            Fraction[] deltaSlice = _table.DeltaRow.Skip(offset).ToArray();

            int pivotCol = _problem.IsMaximization
                ? Array.IndexOf(deltaSlice, deltaSlice.Min()) + offset
                : Array.IndexOf(deltaSlice, deltaSlice.Max()) + offset;

            // Визначення напрямного рядка та напрямного елемента
            int pivotRow = -1;
            Fraction minRatio = new Fraction(double.MaxValue).Reduce();
            for (int i = 0; i < basicVariablesCount; i++)
            {
                Fraction a = _table.Values[i, pivotCol];
                Fraction b = _table.Values[i, 0];
                if (a > 0)
                {
                    Fraction ratio = (b / a).Reduce();
                    if (ratio < minRatio)
                    {
                        minRatio = ratio;
                        pivotRow = i;
                    }
                }
            }
            if (pivotRow == -1) throw new InvalidOperationException("Немає розв'язку! Неможливо визначити напрямний рядок!");
            // Перерахунок симплекс таблиці

            // Заміна напрямленого рядка
            var newKey = $"x{pivotCol}";
            var newValue = _table.ColumnVariables[$"A{pivotCol}"];

            var oldKey = _table.RowVariables.ElementAt(pivotRow).Key;
            _table.RowVariables.Remove(oldKey);
            _table.RowVariables[newKey] = newValue;

            // Перерахунок
            Fraction pivotElement = _table.Values[pivotRow, pivotCol];
            int totalColumns = _table.ColumnVariables.Count;
            for (int i = 0; i < _problem.Constraints.Count; i++)
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
        }

        // Приведення до канонічного вигляду
        private void ConvertToCanonicalForm()
        {
            foreach (var constraint in _problem.Constraints)
            {
                double rhs = double.Parse(constraint.RightHandSide, CultureInfo.InvariantCulture);

                if (rhs < 0)
                {
                    rhs *= -1;
                    constraint.RightHandSide = rhs.ToString("0.####", CultureInfo.InvariantCulture);

                    for (int i = 0; i < constraint.Coefficients.Count; i++)
                    {
                        double coeff = double.Parse(constraint.Coefficients[i], CultureInfo.InvariantCulture);
                        coeff *= -1;
                        constraint.Coefficients[i] = coeff.ToString("0.####", CultureInfo.InvariantCulture);
                    }

                    constraint.Type = constraint.Type switch
                    {
                        ConstraintType.LessThanOrEqual => ConstraintType.GreaterThanOrEqual,
                        ConstraintType.GreaterThanOrEqual => ConstraintType.LessThanOrEqual,
                        _ => constraint.Type
                    };
                }
            }
        }
    }
}
