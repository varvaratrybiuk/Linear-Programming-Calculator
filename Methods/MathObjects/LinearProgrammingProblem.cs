namespace Methods.MathObjects
{
    public class LinearProgrammingProblem : ICloneable
    {
        public bool IsMaximization { get; init; }
        public required List<string> ObjectiveFunctionCoefficients { get; set; }
        public List<string>? SlackVariableCoefficients { get; set; } = [];
        public List<string>? ArtificialVariableCoefficients { get; set; } = [];
        public required List<Constraint> Constraints { get; set; }
        public int VariablesCount => ObjectiveFunctionCoefficients.Count + SlackVariableCoefficients.Count + ArtificialVariableCoefficients.Count;

        public object Clone()
        {
            var newProblem = new LinearProgrammingProblem()
            {
                IsMaximization = IsMaximization,
                ObjectiveFunctionCoefficients = new List<string>(ObjectiveFunctionCoefficients),
                SlackVariableCoefficients = new List<string>(SlackVariableCoefficients),
                ArtificialVariableCoefficients = new List<string>(ArtificialVariableCoefficients),
                Constraints = []
            };

            foreach (var constr in Constraints)
            {
                var newConstraint = (Constraint)constr.Clone();
                newProblem.Constraints.Add(newConstraint);
            }

            return newProblem;
        }
    }
}
