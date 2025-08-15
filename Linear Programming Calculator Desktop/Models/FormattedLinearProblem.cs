namespace Linear_Programming_Calculator_Desktop.Models
{
    /// <summary>
    /// Represents a formatted text for display in the Result view.
    /// </summary>
    public record FormattedLinearProblem
    {
        /// <summary>
        /// Formatted text of the objective function.
        /// </summary>
        public required string FormattedObjectiveFunction { get; init; }

        /// <summary>
        /// Formatted text of the constraints.
        /// </summary>
        public required List<string> FormattedConstraints { get; init; }

        /// <summary>
        /// Formatted text of the domain constraint, such as x1, x2, ..., xn ≥ 0.
        /// </summary>
        public required string DomainText { get; init; }

        /// <summary>
        /// Formatted text of the integer constraint, such as x1, x2, ..., xn are integers.
        /// </summary>
        public string? IntegerNote { get; init; }
    }
}
