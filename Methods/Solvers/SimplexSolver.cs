using Methods.Contracts;
using Methods.Enums;
using Methods.Interfaces;
using Methods.Models;
using System.Data;

namespace Methods.Solvers
{
    public class SimplexSolver : ILinearSolver
    {
        private SimplexTable _table;
        private readonly LinearProgrammingProblem _problem;
        private const double M = int.MaxValue;
        public SimplexTable Table { get => _table; set => _table = value; }

        public SimplexSolver(LinearProgrammingProblem problem)
        {
            _problem = problem;
        }
        public SimplexTable GetSolution()
        {
            return _table;
        }

        public void Solve()
        {
            // Приведення до канонічного вигляду
            ConvertToCanonicalForm();
            // Додавання вільних змінних та штучних змін
            var constCount = _problem.Constraints.Count;
            foreach (var constType in _problem.Constraints)
            {
                var constraintIndex = _problem.Constraints.IndexOf(constType);
                if (constType.Type != ConstraintType.Equal)
                    AddSlackVariables(constType.Coefficients, constType.Type, constraintIndex, constCount);
                if (constType.Type == ConstraintType.GreaterThanOrEqual || constType.Type == ConstraintType.Equal)
                    AddArtificialVariables(constType.Coefficients, constType.Type);
                if ((_problem.Constraints.Any(c => c.Type == ConstraintType.GreaterThanOrEqual) ||
                    _problem.Constraints.Any(c => c.Type == ConstraintType.Equal)) && constType.Type == ConstraintType.LessThanOrEqual)
                {
                    constType.Coefficients.Add(0);
                }
            }
            // Заповнення першої таблиці
            InitializeTableau();
            // Пошук оптимального рішення, якщо це можливо
            
            while (true)
            {
                CalculateReducedCosts();
                if (IsOptimal()) break;
                if (IsUnbounded()) throw new InvalidOperationException("Немає розв'язку!");
                Pivot();
            }

            Console.WriteLine("я ВИРІШИЛА");
        }
        private void AddSlackVariables(List<double> Coefficients, ConstraintType type, int index, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (i == index)
                {
                    if (type == ConstraintType.GreaterThanOrEqual)
                        Coefficients.Add(-1);
                    else if (type == ConstraintType.LessThanOrEqual)
                        Coefficients.Add(1);
                    continue;
                }
                Coefficients.Add(0);
            }

            _problem.SlackVariableCoefficients?.Add(0);
        }
        private void AddArtificialVariables(List<double> Coefficients, ConstraintType type)
        {
            if (type == ConstraintType.GreaterThanOrEqual || type == ConstraintType.Equal)
            {
                Coefficients.Add(1);
                _problem.ArtificialVariableCoefficients?.Add(_problem.IsMaximization ? -M : M);
            }
        }
        private void InitializeTableau()
        {
            int totalColumns = _problem.VariablesCount;
            int baseVariableCount = _problem.ObjectiveFunctionCoefficients.Count;
            _table = new SimplexTable
            {
                RowVariables = [],
                ColumnVariables = [],
                Values = new double[_problem.Constraints.Count, totalColumns + 1]
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

            for (int j = baseVariableCount; j < totalColumns; j++)
            {
                for (int i = 0; i < _problem.Constraints.Count; i++)
                {
                    if (usedRows.Contains(i)) continue;

                    if (_problem.Constraints[i].Coefficients[j] == 1)
                    {
                        bool isUnitColumn = true;

                        for (int k = 0; k < _problem.Constraints.Count; k++)
                        {
                            if (k != i && _problem.Constraints[k].Coefficients[j] != 0)
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
            // Заповнюємо таблицю
            for (int i = 0; i < _problem.Constraints.Count; i++)
            {
                if (usedRows.Contains(i)) continue;

                for (int j = 0; j < totalColumns; j++)
                {
                    if (_problem.Constraints[i].Coefficients[j] == 1 || _problem.Constraints[i].Coefficients[j] == -1)
                    {
                        AddTableRow(i, j);
                        usedRows.Add(i);
                        break;
                    }
                }
            }

            CalculateReducedCosts();
        }
        private void AddTableRow(int constraintIndex, int basicVarIndex)
        {
            int rowIndex = _table.RowVariables.Count;
            int totalColumns = _problem.VariablesCount;

            _table.RowVariables.Add($"x{basicVarIndex + 1}", GetObjectiveCoefficient(basicVarIndex).ToString());
            _table.Values[rowIndex, 0] = _problem.Constraints[constraintIndex].RightHandSide;

            for (int j = 0; j < totalColumns; j++)
            {
                _table.Values[rowIndex, j + 1] = _problem.Constraints[constraintIndex].Coefficients[j];
            }
        }

        private double GetObjectiveCoefficient(int index)
        {
            if (index < _problem.ObjectiveFunctionCoefficients.Count)
                return _problem.ObjectiveFunctionCoefficients[index];
            else if (index < _problem.ObjectiveFunctionCoefficients.Count + (_problem.SlackVariableCoefficients?.Count ?? 0))
                return _problem.SlackVariableCoefficients![index - _problem.ObjectiveFunctionCoefficients.Count];
            else
                return _problem.ArtificialVariableCoefficients![index - _problem.ObjectiveFunctionCoefficients.Count - (_problem.SlackVariableCoefficients?.Count ?? 0)];
        }
        private void CalculateReducedCosts()
        {
            int rowCount = _problem.Constraints.Count;
            int columnCount = _table.ColumnVariables.Count;

            _table.DeltaRow = new double[columnCount];

            var columnKeys = _table.ColumnVariables.Keys.ToList();

            for (int j = 0; j < columnCount; j++)
            {
                double delta = 0;
                string columnVar = columnKeys[j];

                for (int i = 0; i < rowCount; i++)
                {
                    string rowVar = _table.RowVariables.Keys.ElementAt(i);
                    double cb = double.Parse(_table.RowVariables[rowVar]);

                    double aij = _table.Values[i, j];

                    delta += cb * aij;
                }
                double cj = 0;
                double.TryParse(_table.ColumnVariables[columnVar], out cj);
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
                    ? !_table.DeltaRow.Any(value => value > 0)
                    : !_table.DeltaRow.Any(value => value < 0)))
            {
                return true;
            }
            return false;
        }
        private void Pivot()
        {
            int lastRow = _problem.Constraints.Count; // к-сть умов обмежень
            // Визначення напрямного стовпця
            int offset = 1;
            double[] deltaSlice = _table.DeltaRow.Skip(offset).ToArray();

            int pivotCol = _problem.IsMaximization
                ? Array.IndexOf(deltaSlice, deltaSlice.Min()) + offset
                : Array.IndexOf(deltaSlice, deltaSlice.Max()) + offset;
            // Визначення напрямного рядка та напрямного елемента
            int pivotRow = -1;
            double minRatio = double.MaxValue;
            for (int i = 0; i < lastRow; i++)
            {
                double a = _table.Values[i, pivotCol];
                double b = _table.Values[i, 0];
                if (a > 0)
                {
                    double ratio = b / a;
                    if (ratio < minRatio)
                    {
                        minRatio = ratio;
                        pivotRow = i;
                    }
                }
            }
            if (pivotRow == -1) throw new InvalidOperationException("Немає розв'язку");
            // Перерахунок симплекс таблиці
            // Заміна напрямленого рядка
            var newKey = $"x{pivotCol}";
            var newValue = _table.ColumnVariables[$"A{pivotCol}"];

            var oldKey = _table.RowVariables.ElementAt(pivotRow).Key;
            _table.RowVariables.Remove(oldKey);
            _table.RowVariables[newKey] = newValue;

            // Перерахунок
            double pivotElement = _table.Values[pivotRow, pivotCol];
            int totalColumns = _table.ColumnVariables.Count;
            for (int i = 0; i < _problem.Constraints.Count; i++)
            {
                if (i == pivotRow) continue;
                double factor = _table.Values[i, pivotCol];
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

        private void ConvertToCanonicalForm()
        {
            foreach (var constraint in _problem.Constraints)
            {
                if (constraint.RightHandSide < 0)
                {
                    constraint.RightHandSide *= -1;

                    for (int i = 0; i < constraint.Coefficients.Count; i++)
                    {
                        constraint.Coefficients[i] *= -1;
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
