namespace Methods.Contracts
{
    public class LinearProgrammingProblem
    {
        public bool IsMaximization { get; init; }
        public required List<double> ObjectiveFunctionCoefficients { get; init; }
        public List<double>? SlackVariableCoefficients { get; set; } = new List<double>();
        public List<double>? ArtificialVariableCoefficients { get; set; } = new List<double>();
        public required List<Constraint> Constraints { get; init; }
        public int VariablesCount => ObjectiveFunctionCoefficients.Count + SlackVariableCoefficients.Count + ArtificialVariableCoefficients.Count;
    }
}
