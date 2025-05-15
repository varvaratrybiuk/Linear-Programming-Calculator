namespace Methods.MathObjects
{
    public class LinearProgrammingProblem
    {
        public bool IsMaximization { get; init; }
        public required List<string> ObjectiveFunctionCoefficients { get; init; }
        public List<string>? SlackVariableCoefficients { get; set; } = new List<string>();
        public List<string>? ArtificialVariableCoefficients { get; set; } = new List<string>();
        public required List<Constraint> Constraints { get; init; }
        public int VariablesCount => ObjectiveFunctionCoefficients.Count + SlackVariableCoefficients.Count + ArtificialVariableCoefficients.Count;
    }
}
