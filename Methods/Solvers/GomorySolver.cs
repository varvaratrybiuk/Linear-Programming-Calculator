using Fractions;
using Methods.Interfaces;
using Methods.MathObjects;
using Methods.Models;

namespace Methods.Solvers
{
    /// <summary>
    /// Represents a solver for the Gomory Cutting Plane method.
    /// </summary>
    /// <param name="Table">The simplex table obtained after solving by the Simplex method.</param>
    /// <param name="Problem">The initial linear programming problem.</param>
    public class GomorySolver(SimplexTable Table, LinearProgrammingProblem Problem) : ILinearSolver
    {
        /// <summary>
        /// History of each Gomory cut step.
        /// </summary>
        public List<GomoryHistory> GomoryHistory { get; set; } = [];

        /// <summary>
        /// Counter indicating how many steps have been performed.
        /// </summary>
        private int _historyStep = -1;
        /// <summary>
        /// The initial LPP.
        /// </summary>
        private readonly LinearProgrammingProblem _problem = Problem;
        /// <summary>
        /// The optimal solution from each step, or from the initial optimal Simplex solution, 
        /// used to check whether the solution is integer.
        /// </summary>
        private readonly Dictionary<(int rowIndex, string variableName), Fraction> _result = [];

        /// <summary>
        /// Finds the pivot row and column for the next solving step.
        /// </summary>
        /// <exception cref="InvalidOperationException"> Arises if the pivot column is not found.</exception>
        /// <remarks>
        /// The pivot row is determined by the minimum value in column A0. 
        /// Then, the theta row is calculated as |Δ * xij|, and the pivot column 
        /// is chosen by the minimum theta value. After that, the reduced costs are recalculated.
        /// </remarks>
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
                    var theta = Fraction.Abs(Table.DeltaRow![j].Value / element);
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
        public void CalculateReducedCosts()
        {
            int rowCount = Table.RowVariables.Count;
            int columnCount = Table.ColumnVariables.Count;

            var columnKeys = Table.ColumnVariables.Keys.ToList();

            Table.DeltaRow.Clear();

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
                Table.DeltaRow.Add(new ExpressionValue($"{delta - cj}", delta - cj));
            }
        }
        /// <summary>
        /// Performs the steps required to solve the LPP.
        /// </summary>
        /// <exception cref="InvalidOperationException">"Arises if no solution exists or the given initial values are optimal."</exception>
        /// <remarks>
        /// First, checks whether the current solution contains non-integer values or is not optimal.  
        /// If the solution is not optimal, calls the pivot method and repeats the check.  
        /// If it is optimal, verifies whether a feasible solution exists.  
        /// Then, finds the most fractional value in the solution, constructs the Gomory cut,  
        /// adds it to the simplex table, and determines the pivot element.
        /// </remarks>
        public void Solve()
        {
            while (!IsIntegerOptimalSolutionFound() || !IsOptimal())
            {
                if (IsOptimal())
                {
                    if (IsUnbounded()) throw new InvalidOperationException("No solution! There is no fractional value in the row!");
                    var fractionalValueIndex = FindMostFractionalValue();
                    _historyStep++;

                    var variableIndex = GetVariableIndexName(fractionalValueIndex);
                    GomoryHistory.Add(new GomoryHistory()
                    {
                        MaxFracValue = (
                                 int.Parse(variableIndex),
                                _result.First(k => k.Key.rowIndex == fractionalValueIndex).Value
                            )
                    });
                    var cutRow = BuildGomoryCutRow(fractionalValueIndex);
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

        /// <summary>
        /// Retrieves the variable name from the RowVariables collection by the specified index,
        /// replacing the 'x' prefix with a space.
        /// </summary>
        /// <param name="fractionalValueIndex">Index of the variable in the RowVariables collection.</param>
        /// <returns>Returns the variable's index as used in the equation.</returns>
        private string GetVariableIndexName(int fractionalValueIndex)
        {
            var variableName = Table.RowVariables.Keys.ToList()[fractionalValueIndex];
            return variableName.Replace("x", " ");
        }

        /// <summary>
        /// Determines whether the LPP has a solution.
        /// If the chosen row does not contain fractional values, no solution is found.
        /// </summary>
        /// <returns><c>true</c> if the problem has no solution; otherwise, <c>false</c>.</returns>
        public bool IsUnbounded()
        {
            int fractionalValueIndex = FindMostFractionalValue();
            int columnCount = Table.Values.GetLength(1);

            for (int j = 1; j < columnCount; j++)
            {
                Fraction value = Table.Values[fractionalValueIndex, j];
                if (value.Numerator % value.Denominator != 0)
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// Searches for the index of the most fractional value in the optimal solution.
        /// </summary>
        /// <returns>The index of the row containing the largest fractional value.</returns>
        private int FindMostFractionalValue()
        {
            int fractionalValueIndex = -1;
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
                    fractionalValueIndex = keys[i].rowIndex;
                }
            }

            return fractionalValueIndex;
        }
        /// <summary>
        /// Builds a Gomory cut row for the specified fractional value index.
        /// </summary>
        /// <param name="fractionalValueIndex">Index of the row containing the most fractional value.</param>
        /// <returns>A list of coefficients representing the Gomory cut row.</returns>
        private List<Fraction> BuildGomoryCutRow(int fractionalValueIndex)
        {
            var newBranchCut = new BranchCut();

            var rowValues = new List<Fraction>();
            for (int j = 0; j < Table.Values.GetLength(1); j++)
            {
                rowValues.Add(Table.Values[fractionalValueIndex, j]);
            }


            List<Fraction> cut = [];
            for (int j = 0; j < rowValues.Count; j++)
            {
                var coeff = rowValues[j];
                int wholePart = (int)(coeff.Numerator / coeff.Denominator);
                if (coeff.IsNegative && coeff.Denominator != 1)
                    wholePart = -1;
                Fraction fractionalPart = coeff - wholePart;

                var variableIndex = GetVariableIndexName(fractionalValueIndex);
                newBranchCut.FractionalElements[$"y{variableIndex}{j}"] = (wholePart, coeff);

                cut.Add(-fractionalPart);
            }

            cut.Add(1);
            newBranchCut.CutExpression = new List<Fraction>(cut);
            GomoryHistory[_historyStep].Cut = newBranchCut;

            return cut;
        }

        /// <summary>
        /// Adds a new cutting plane row to the simplex table based on the given Gomory cut row.
        /// </summary>
        /// <param name="cutRow">Coefficients of the Gomory cut row to add.</param>
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
            Table.DeltaRow = [.. Table.DeltaRow!, new ExpressionValue("0", Fraction.Zero)];
        }

        /// <summary>
        /// Checks whether the optimal solution contains only integer values.
        /// </summary>
        /// <returns><c>true</c> if all variables in the optimal solution are integers; otherwise, <c>false</c>.</returns>
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
        /// <summary>
        /// Extracts the base variables and their corresponding values from the simplex table.
        /// Updates the internal result dictionary with variable indexes and their values.
        /// </summary>
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
        /// <summary>
        /// Determines whether the current solution in the simplex table is optimal.
        /// </summary>
        /// <returns><c>true</c> if the solution is optimal; otherwise, <c>false</c>.</returns>
        /// <remarks>        
        /// A solution is optimal if all values in the first column are non-negative.
        /// </remarks>
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
