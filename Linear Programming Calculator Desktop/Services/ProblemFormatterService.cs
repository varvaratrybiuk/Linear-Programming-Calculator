using Methods.Enums;
using Methods.MathObjects;

namespace Linear_Programming_Calculator_Desktop.Services
{
    /// <summary>
    /// Service that provides methods to format different parts of a LPP for display.
    /// </summary>
    public class ProblemFormatterService : IProblemFormatterService
    {
        /// <summary>
        /// Builds a formatted string representation of the objective function.
        /// </summary>
        /// <param name="problem">The linear programming problem.</param>
        /// <returns>A formatted string of the objective function.</returns>
        /// <remarks>
        /// Builds a formatted string representation of the objective function by concatenating
        /// all coefficients (objective function, slack, and artificial variables) with variable names,
        /// properly formatting negative coefficients, and appending the optimization direction (max or min).
        /// </remarks>
        public string BuildObjectiveFunction(LinearProgrammingProblem problem)
        {
            var allCoeffs = new List<string>();
            allCoeffs.AddRange(problem.ObjectiveFunctionCoefficients);
            allCoeffs.AddRange(problem.SlackVariableCoefficients);
            allCoeffs.AddRange(problem.ArtificialVariableCoefficients);

            var variables = Enumerable.Range(1, allCoeffs.Count).Select(i => $"x{i}").ToList();

            var expr = string.Join(" + ", allCoeffs.Select((c, i) =>
            {
                var coeffStr = c.StartsWith("-") ? $"({c})" : c;
                return $"{coeffStr}{variables[i]}";
            }));

            var direction = problem.IsMaximization ? "→ max" : "→ min";
            return $"F({string.Join(", ", variables)}) = {expr} {direction}";
        }

        /// <summary>
        /// Builds a list of formatted strings representing the problem constraints.
        /// </summary>
        /// <param name="problem">The linear programming problem.</param>
        /// <param name="isEqual">Indicates whether constraints are equalities or inequalities.</param>
        /// <returns>A list of formatted constraint strings.</returns>
        /// <remarks>
        /// Builds a list of formatted constraint strings by processing each constraint's coefficients,
        /// formatting negative coefficients with parentheses, assigning appropriate inequality or equality signs,
        /// and combining these into readable constraint expressions.
        /// </remarks>
        public List<string> BuildConstraints(LinearProgrammingProblem problem, bool isEqual)
        {
            var result = new List<string>();
            foreach (var c in problem.Constraints)
            {
                var line = string.Join(" + ", c.Coefficients.Select((coef, i) =>
                {
                    var coeffStr = coef.StartsWith("-") ? $"({coef})" : coef;
                    return $"{coeffStr}x{i + 1}";
                }));

                var sign = isEqual ? "=" : c.Type switch
                {
                    ConstraintType.LessThanOrEqual => "≤",
                    ConstraintType.GreaterThanOrEqual => "≥",
                    ConstraintType.Equal => "=",
                    _ => "?"
                };

                result.Add($"{line} {sign} {c.RightHandSide}");
            }

            return result;
        }

        public string BuildDomain(LinearProgrammingProblem problem)
        {
            var allVars = GetAllVariables(problem);
            return string.Join(", ", allVars.Select(v => $"{v} ≥ 0"));
        }

        public string BuildIntegerNote(LinearProgrammingProblem problem, bool isIntegerProblem)
        {
            if (!isIntegerProblem)
                return string.Empty;
            var allVars = GetAllVariables(problem);
            return string.Join(", ", allVars) + " are integers";
        }
        /// <summary>
        /// Return a list of all variable names involved in the LPP,
        /// including variables from the objective function, slack variables, and artificial variables.
        /// </summary>
        /// <param name="problem">The LPP instance.</param>
        /// <returns>A list of all variable names as strings.</returns>
        private static List<string> GetAllVariables(LinearProgrammingProblem problem)
        {
            int count =
                problem.ObjectiveFunctionCoefficients.Count +
                problem.SlackVariableCoefficients.Count +
                problem.ArtificialVariableCoefficients.Count;

            return Enumerable.Range(1, count).Select(i => $"x{i}").ToList();
        }

    }
}
