using Methods.MathObjects;

namespace Methods.Models
{
    public class SimplexHistory
    {
        public LinearProgrammingProblem? InitialLinearProgrammingProblem { get; set; }
        public LinearProgrammingProblem? SlackVariableProblem { get; set; }
        public LinearProgrammingProblem? ArtificialProblemProblem { get; set; }
        public Dictionary<string, string> InitialBasis { get; set; } = [];
        public SimplexTable? OptimalTable { get; set; }
        public List<SimplexStep> Steps { get; set; } = [];

    }
}
