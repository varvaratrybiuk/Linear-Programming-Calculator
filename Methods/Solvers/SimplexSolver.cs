using Fractions;
using Methods.Enums;
using Methods.Interfaces;
using Methods.MathObjects;
using Methods.Models;
using System.Data;
using System.Globalization;

namespace Methods.Solvers
{
    /// <summary>
    /// Solver for Linear Programming Problems using the Simplex method.
    /// </summary>
    /// <param name="problem">The LPP to be solved.</param>
    public class SimplexSolver(LinearProgrammingProblem problem) : ILinearSolver
    {
        /// <summary>
        /// Stores the history of the Simplex solving process.
        /// </summary>
        public SimplexHistory SimplexHistory { get; set; } = new SimplexHistory();
        /// <summary>
        /// The current Simplex table used during the solution process.
        /// </summary>
        public SimplexTable Table { get; set; } = new();

        /// <summary>
        /// The LPP instance to solve.
        /// </summary>
        private readonly LinearProgrammingProblem _problem = problem;
        /// <summary>
        /// A large constant used in the Big M method is a notation for artificial variables.
        /// </summary>
        private const double M = int.MaxValue;

        /// <summary>
        /// Performs the steps required to solve the linear programming problem.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if artificial variables remain in the basis after solving.</exception>
        /// <remarks>
        /// First, if the right-hand side (RHS) of any constraint is less than 0, the constraints are converted into canonical form.
        /// Then, slack and artificial variables (if needed) are added.
        /// After that, the initial simplex table is initialized.
        /// The solver proceeds by calculating reduced costs, checking for solution existence, determining if the solution is optimal, and searching for a pivot element.
        /// Finally, artificial variables are removed if they were added.
        /// </remarks>
        public void Solve()
        {
            SimplexHistory.InitialLinearProgrammingProblem = (LinearProgrammingProblem)_problem.Clone();

            ConvertToCanonicalForm();
            ProcessAuxiliaryVariables(isSlack: true);
            SimplexHistory.SlackVariableProblem = (LinearProgrammingProblem)_problem.Clone();
            ProcessAuxiliaryVariables(isSlack: false);

            if (_problem.ArtificialVariableCoefficients?.Count != 0)
                SimplexHistory.ArtificialProblemProblem = (LinearProgrammingProblem)_problem.Clone();

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
        /// <summary>
        /// Removes artificial variables from the simplex table.
        /// </summary>
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
                Table.ColumnVariables.Remove(key);

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

            Table.DeltaRow = Table.DeltaRow!
                    .Where((_, index) => !indexesToRemove.Contains(index))
                    .ToList();
        }
        /// <summary>
        /// Processes auxiliary variables (slack or artificial) based on constraint types.
        /// </summary>
        /// <param name="isSlack">If true, processes slack variables; otherwise, artificial variables.</param>
        /// <remarks>
        /// Adds slack variables for less-than or greater-than constraints and artificial variables for equalities and greater-than constraints.
        /// </remarks>
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
        /// <summary>
        /// Adds an auxiliary variable (slack or artificial) to constraints coefficients.
        /// </summary>
        /// <param name="constraints">List of constraints to modify.</param>
        /// <param name="type">Type of constraint (LessThanOrEqual, Equal, GreaterThanOrEqual).</param>
        /// <param name="index">Index of the constraint to which the auxiliary variable is added.</param>
        /// <param name="isSlack">If true, adds slack variable; otherwise, adds artificial variable.</param>
        /// <remarks>
        /// For slack variables, adds 1 or -1 based on constraint type.
        /// For artificial variables, adds 1 for greater-than or equality constraints.
        /// </remarks>
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

        /// <summary>
        /// Initializes the simplex tableau after adding all variables and constraints.
        /// </summary>
        private void InitializeTableau()
        {
            int totalColumns = _problem.VariablesCount;
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
            AssignBasicVariables(usedRows);

            CalculateReducedCosts();
        }
        /// <summary>
        /// Checks columns corresponding to slack and artificial variables to identify unit columns.
        /// </summary>
        /// <param name="usedRows">List of constraint row that already assigned to basic variables.</param>
        /// <remarks>
        /// If a unit column is found, marks it as a basic variable and adds the corresponding row to the basis.
        /// </remarks>
        private void AssignBasicVariables(List<int> usedRows)
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
        /// <summary>
        /// Adds a row to the simplex table corresponding to the specified constraint and basic variable.
        /// </summary>
        /// <param name="constraintIndex">The index of the constraint.</param>
        /// <param name="basicVarIndex">The index of the basic variable in the objective function.</param>
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
        /// <summary>
        /// Returns the coefficient of a variable from the objective function based on the index.
        /// </summary>
        /// <param name="index">The index of the variable.</param>
        /// <returns>The coefficient of the variable in the objective function.</returns>
        private string GetObjectiveCoefficient(int index)
        {
            if (index < _problem.ObjectiveFunctionCoefficients.Count)
                return _problem.ObjectiveFunctionCoefficients[index];
            else if (index < _problem.ObjectiveFunctionCoefficients.Count + (_problem.SlackVariableCoefficients?.Count ?? 0))
                return _problem.SlackVariableCoefficients![index - _problem.ObjectiveFunctionCoefficients.Count];
            else
                return _problem.ArtificialVariableCoefficients![index - _problem.ObjectiveFunctionCoefficients.Count - (_problem.SlackVariableCoefficients?.Count ?? 0)];
        }
        public void CalculateReducedCosts()
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

        /// <summary>
        /// Attempts to identify if the input string represents "M" or "-M" and returns its sign.
        /// </summary>
        /// <param name="s">Input string to check.</param>
        /// <param name="sign">Output sign value: 1 for "M", -1 for "-M", 0 otherwise.</param>
        /// <returns>True if the input is "M" or "-M"; otherwise false.</returns>
        private static bool TryGetMSign(string s, out int sign)
        {
            if (s == "M") { sign = 1; return true; }
            if (s == "-M") { sign = -1; return true; }
            sign = 0;
            return false;
        }

        /// <summary>
        /// Parses a coefficient string that may contain "M" or numeric value.
        /// </summary>
        /// <param name="cjStr">Coefficient string to parse.</param>
        /// <returns>
        /// A tuple containing:
        /// <b>cj</b> - the parsed Fraction value (scaled by M if applicable).
        /// <b>sign</b> - the sign of M (1, -1) if applicable, otherwise 0.
        /// </returns>
        private static (Fraction cj, Fraction sign) ParseCj(string cjStr)
        {
            if (TryGetMSign(cjStr, out int sign))
                return (new Fraction(sign * M), sign);

            Fraction.TryParse(cjStr, out var cj);
            return (cj, 0);
        }

        /// <summary>
        /// Combines the M-part and numeric part of an expression into a single formatted string.
        /// </summary>
        /// <param name="mPart">String representing the M-part (e.g., "3M" or "-M").</param>
        /// <param name="numeric">Numeric fraction part.</param>
        /// <returns>Formatted string combining both parts with proper signs.</returns>
        private static string CombineParts(string mPart, Fraction numeric)
        {
            if (string.IsNullOrEmpty(mPart) && numeric == 0) return "0";
            if (string.IsNullOrEmpty(mPart)) return numeric.ToString();
            if (numeric == 0) return mPart;

            var op = numeric > 0 ? " + " : " - ";
            var abs = Fraction.Abs(numeric);
            return mPart + op + abs;
        }

        ///<summary>
        /// Determines whether the current solution in the simplex table is optimal.
        /// </summary>
        /// <returns><c>true</c> if the solution is optimal; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// The solution is optimal if all values in the delta row are non-negative when maximizing the objective function,
        /// or all values are non-positive when minimizing.
        /// </remarks>
        public bool IsOptimal()
        {
            return _problem.IsMaximization ? !Table.DeltaRow!.Skip(1).Any(n => n.Value < 0) : !Table.DeltaRow!.Skip(1).Any(n => n.Value > 0);
        }
        /// <summary>
        /// Determines whether the LPP has a solution.
        /// </summary>
        /// <returns><c>true</c> if the problem has no solution; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// Returns true if the basis still contains artificial variables even though the delta row indicates an optimal solution.
        /// </remarks>
        public bool IsUnbounded()
        {
            int rowIndex = _problem.Constraints.Count;
            if (Table.RowVariables
                    .Select(row => row.Value)
                    .Any(value => value == "M" || value == "-M") && IsOptimal())
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Finds the pivot row and column for the next solving step.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the pivot row cannot be determined (all values in the pivot column are less than or equal to 0).</exception>
        /// <remarks>
        /// The pivot column is determined by the minimum or maximum value, depending on the objective function.
        /// Then, in the pivot column, the minimum positive row value is found and selected as the pivot row.
        /// Finally, recalculates all table values accordingly.
        /// </remarks>
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
        /// <summary>
        /// Converts all constraints with negative right-hand side values
        /// into canonical form by multiplying the entire constraint by -1.
        /// </summary>
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
