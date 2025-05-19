using Methods.MathObjects;

namespace Methods.Models
{
    public class SimplexHistory
    {
        public LinearProgrammingProblem? InitialLinearProgrammingProblem { get; set; }
        public LinearProgrammingProblem? FreeVariableProblem { get; set; }
        public LinearProgrammingProblem? ArtificialProblemTable { get; set; }
        public Dictionary<string, string> InitialBasis { get; set; } = [];
        public SimplexTable OptimalTable { get; set; }
        public List<SimplexStep> Steps { get; set; } = [];

    }
}
