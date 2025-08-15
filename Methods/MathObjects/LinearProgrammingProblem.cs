namespace Methods.MathObjects
{
    /// <summary>
    /// Represents a LPP.
    /// </summary>
    public class LinearProgrammingProblem : ICloneable
    {
        /// <summary>
        /// Indicates whether the problem is a maximization (<c>true</c>) or minimization (<c>false</c>).
        /// </summary>
        public bool IsMaximization { get; init; }
        /// <summary>
        /// Coefficients of the objective function.
        /// </summary>
        public required List<string> ObjectiveFunctionCoefficients { get; set; }
        /// <summary>
        /// Coefficients for slack variables.
        /// </summary>
        public List<string> SlackVariableCoefficients { get; set; } = [];
        /// <summary>
        /// Coefficients for artificial variables.
        /// </summary>
        public List<string> ArtificialVariableCoefficients { get; set; } = [];
        /// <summary>
        /// List of constraints defining the problem.
        /// </summary>
        public required List<Constraint> Constraints { get; set; }
        /// <summary>
        /// Gets the total number of variables, including primary, slack, and artificial variables.
        /// </summary>
        public int VariablesCount => ObjectiveFunctionCoefficients.Count + SlackVariableCoefficients.Count + ArtificialVariableCoefficients.Count;
        /// <summary>
        /// Creates a copy of the linear programming problem.
        /// </summary>
        /// <returns>A clone of the current <see cref="LinearProgrammingProblem"/> object.</returns>
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
