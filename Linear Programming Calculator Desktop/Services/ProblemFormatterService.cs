using Methods.Enums;
using Methods.MathObjects;

namespace Linear_Programming_Calculator_Desktop.Services
{
    public class ProblemFormatterService : IProblemFormatterService
    {
        public string BuildObjectiveFunction(LinearProgrammingProblem problem)
        {
            var allCoeffs = new List<string>();
            allCoeffs.AddRange(problem.ObjectiveFunctionCoefficients);
            allCoeffs.AddRange(problem.SlackVariableCoefficients ?? Enumerable.Empty<string>());
            allCoeffs.AddRange(problem.ArtificialVariableCoefficients ?? Enumerable.Empty<string>());

            var variables = Enumerable.Range(1, allCoeffs.Count).Select(i => $"x{i}").ToList();

            var expr = string.Join(" + ", allCoeffs.Select((c, i) =>
            {
                var coeffStr = c.StartsWith("-") ? $"({c})" : c;
                return $"{coeffStr}{variables[i]}";
            }));

            var direction = problem.IsMaximization ? "→ max" : "→ min";
            return $"F({string.Join(", ", variables)}) = {expr} {direction}";
        }

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

        private List<string> GetAllVariables(LinearProgrammingProblem problem)
        {
            int count =
                problem.ObjectiveFunctionCoefficients.Count +
                problem.SlackVariableCoefficients?.Count ?? 0 +
                problem.ArtificialVariableCoefficients?.Count ?? 0;

            return Enumerable.Range(1, count).Select(i => $"x{i}").ToList();
        }

    }
}
