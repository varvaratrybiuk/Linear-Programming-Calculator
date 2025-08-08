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
        public SimplexHistory SimplexHistory { get; set; } = new SimplexHistory();
        public SimplexTable Table { get; set; } = new();

        private readonly LinearProgrammingProblem _problem = problem;
        private const double M = int.MaxValue;

        public void Solve()
        {
            SimplexHistory.InitialLinearProgrammingProblem = (LinearProgrammingProblem)_problem.Clone();

            ConvertToCanonicalForm();
            ProcessAuxiliaryVariables(isSlack: true);
            SimplexHistory.SlackVariableProblem = (LinearProgrammingProblem)_problem.Clone();
            ProcessAuxiliaryVariables(isSlack: false);
            if (_problem.ArtificialVariableCoefficients?.Count != 0)
            {
                SimplexHistory.ArtificialProblemProblem = (LinearProgrammingProblem)_problem.Clone();
            }

            InitializeTableau();
            while (true)
            {
                CalculateReducedCosts();
                if (IsUnbounded())
                {
                    SimplexHistory.Steps.Add(new SimplexStep()
                    {
                        PivotColumn = -1,
                        PivotRow = -1,
                        Table = (SimplexTable)Table.Clone(),
                    });
                    throw new InvalidOperationException("No solution! Artificial variables have not left the basis!");
                }
                if (IsOptimal())
                    break;
                Pivot();
            }
            RemoveArtificialVariables();
            SimplexHistory.OptimalTable = (SimplexTable)Table.Clone();
        }

        private void RemoveArtificialVariables()
        {
            int artificialVariablesCount = _problem.ArtificialVariableCoefficients?.Count ?? 0;

            if (artificialVariablesCount <= 0)
                return;

            var allKeys = Table.ColumnVariables.Keys.ToList();

            var keysToRemove = allKeys.Skip(allKeys.Count - artificialVariablesCount).ToList();
            var indexesToRemove = keysToRemove
                .Select(key => allKeys.IndexOf(key))
                .OrderByDescending(i => i)
                .ToList();

            foreach (var key in keysToRemove)
            {
                Table.ColumnVariables.Remove(key);
            }

            int rows = Table.Values.GetLength(0);
            int cols = Table.Values.GetLength(1) - artificialVariablesCount;
            var newValues = new Fraction[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                int newCol = 0;
                for (int j = 0; j < Table.Values.GetLength(1); j++)
                {
                    if (!indexesToRemove.Contains(j))
                    {
                        newValues[i, newCol++] = Table.Values[i, j];
                    }
                }
            }

            Table.Values = newValues;

            var newDelta = new List<ExpressionValue>();

            for (int i = 0; i < Table.DeltaRow!.Count; i++)
            {
                if (!indexesToRemove.Contains(i))
                {
                    newDelta.Add(Table.DeltaRow[i]);
                }
            }

            Table.DeltaRow = newDelta;
        }

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
                        {
                            constraints[i].Coefficients.Add("1");
                            continue;
                        }
                    }
                }
                constraints[i].Coefficients.Add("0");
            }
            if (isSlack)
                _problem.SlackVariableCoefficients?.Add("0");
            else
                _problem.ArtificialVariableCoefficients?.Add(_problem.IsMaximization ? "-" + nameof(M) : nameof(M));
        }

        private void InitializeTableau()
        {
            int totalColumns = _problem.VariablesCount;
            int baseVariableCount = _problem.ObjectiveFunctionCoefficients.Count;
            Table = new SimplexTable
            {
                RowVariables = [],
                ColumnVariables = [],
                Values = new Fraction[_problem.Constraints.Count, totalColumns + 1]
            };

            Table.ColumnVariables.Add("A0", "-");
            for (int i = 0; i < totalColumns; i++)
            {
                string variableName = $"A{i + 1}";
                string coefficient = GetObjectiveCoefficient(i).ToString();
                Table.ColumnVariables.Add(variableName, coefficient);
            }

            List<int> usedRows = [];
            TryAssignBasicVariableColumns(usedRows);

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
                            SimplexHistory.InitialBasis[$"x{j + 1}"] = _problem.Constraints[i].RightHandSide;
                            break;
                        }
                    }
                }
            }
        }
        private void AddTableRow(int constraintIndex, int basicVarIndex)
        {
            int rowIndex = Table.RowVariables.Count;
            int totalColumns = _problem.VariablesCount;

            Table.RowVariables.Add($"x{basicVarIndex + 1}", GetObjectiveCoefficient(basicVarIndex).ToString());
            Table.Values[rowIndex, 0] = Fraction.FromString(_problem.Constraints[constraintIndex].RightHandSide);

            for (int j = 0; j < totalColumns; j++)
            {
                Table.Values[rowIndex, j + 1] = Fraction.FromString(_problem.Constraints[constraintIndex].Coefficients[j]);
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
        private void CalculateReducedCosts()
        {
            int rowCount = _problem.Constraints.Count;
            int columnCount = Table.ColumnVariables.Count;
            var columnKeys = Table.ColumnVariables.Keys.ToList();

            Table.DeltaRow.Clear();

            for (int j = 0; j < columnCount; j++)
            {
                Fraction delta = 0;
                Fraction deltaM = 0;
                string columnVar = columnKeys[j];

                for (int i = 0; i < rowCount; i++)
                {
                    string rowVar = Table.RowVariables.Keys.ElementAt(i);
                    string cbStr = Table.RowVariables[rowVar];
                    Fraction aij = Table.Values[i, j];

                    if (TryGetMSign(cbStr, out int sign))
                    {
                        deltaM += sign * aij;
                        continue;
                    }

                    Fraction cb = Fraction.FromString(cbStr);
                    delta += cb * aij;
                }

                string cjStr = Table.ColumnVariables[columnVar];
                var (cj, cjSign) = ParseCj(cjStr);

                Fraction totalDeltaM = deltaM - cjSign;

                string deltaMStr = totalDeltaM == 0 ? "" : $"{(totalDeltaM < 0 ? "-" : "")}{Fraction.Abs(totalDeltaM)}M";
                Fraction numericPart = delta - (cjStr is "M" or "-M" ? Fraction.Zero : cj);

                string expressionStr = CombineParts(deltaMStr, numericPart);

                var value = (delta + (deltaM * new Fraction(M)) - cj).Reduce();
                Table.DeltaRow.Add(new ExpressionValue(expressionStr, value));
            }
        }

        private static bool TryGetMSign(string s, out int sign)
        {
            if (s == "M") { sign = 1; return true; }
            if (s == "-M") { sign = -1; return true; }
            sign = 0;
            return false;
        }

        private (Fraction cj, Fraction sign) ParseCj(string cjStr)
        {
            if (TryGetMSign(cjStr, out int sign))
                return (new Fraction(sign * M), sign);

            Fraction.TryParse(cjStr, out var cj);
                return (cj, 0);
        }

        private static string CombineParts(string mPart, Fraction numeric)
        {
            if (string.IsNullOrEmpty(mPart) && numeric == 0) return "0";
            if (string.IsNullOrEmpty(mPart)) return numeric.ToString();
            if (numeric == 0) return mPart;

            var op = numeric > 0 ? " + " : " - ";
            var abs = Fraction.Abs(numeric);
            return mPart + op + abs;
        }

        public bool IsOptimal()
        {
            return _problem.IsMaximization ? !Table.DeltaRow!.Skip(1).Any(n => n.Value < 0) : !Table.DeltaRow!.Skip(1).Any(n => n.Value > 0);
        }
        public bool IsUnbounded()
        {
            int rowIndex = _problem.Constraints.Count;
            if (Table.RowVariables
                    .Select(row => row.Value)
                    .Any(value => value == "M" || value == "-M") &&
                (_problem.IsMaximization
                    ? !Table.DeltaRow!.Skip(1).Any(n => n.Value < 0)
                    : !Table.DeltaRow!.Skip(1).Any(n => n.Value > 0)))
            {
                return true;
            }
            return false;
        }

        public void Pivot()
        {
            int basicVariablesCount = _problem.Constraints.Count;

            int offset = 1;
            Fraction[] deltaSlice = Table.DeltaRow!.Skip(offset).Select(f => f.Value).ToArray();

            int pivotCol = _problem.IsMaximization
                ? Array.IndexOf(deltaSlice, deltaSlice.Min()) + offset
                : Array.IndexOf(deltaSlice, deltaSlice.Max()) + offset;

            int pivotRow = -1;
            Fraction minRatio = new Fraction(double.MaxValue).Reduce();
            for (int i = 0; i < basicVariablesCount; i++)
            {
                Fraction a = Table.Values[i, pivotCol];
                Fraction b = Table.Values[i, 0];
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

            SimplexHistory.Steps.Add(new SimplexStep()
            {
                PivotColumn = pivotCol,
                PivotRow = pivotRow,
                Table = (SimplexTable)Table.Clone(),
            });
            if (pivotRow == -1) throw new InvalidOperationException("No solution! It's impossible to determine the pivot row!");

            var newKey = $"x{pivotCol}";
            var newValue = Table.ColumnVariables[$"A{pivotCol}"];

            var oldKey = Table.RowVariables.ElementAt(pivotRow).Key;
            Table.RowVariables.Remove(oldKey);
            Table.RowVariables[newKey] = newValue;

            Fraction pivotElement = Table.Values[pivotRow, pivotCol];
            int totalColumns = Table.ColumnVariables.Count;
            for (int i = 0; i < _problem.Constraints.Count; i++)
            {
                if (i == pivotRow) continue;
                Fraction factor = Table.Values[i, pivotCol];
                for (int j = 0; j < totalColumns; j++)
                {
                    Table.Values[i, j] -= factor * Table.Values[pivotRow, j] / pivotElement;
                    Table.Values[i, j] = Table.Values[i, j].Reduce();
                }
            }

            for (int j = 0; j < totalColumns; j++)
            {
                Table.Values[pivotRow, j] /= pivotElement;
                Table.Values[pivotRow, j] = Table.Values[pivotRow, j].Reduce();
            }
        }

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
