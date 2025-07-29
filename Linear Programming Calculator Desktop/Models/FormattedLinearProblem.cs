namespace Linear_Programming_Calculator_Desktop.Models
{
    public record FormattedLinearProblem
    {
        public required string FormattedObjectiveFunction { get; init; }
        public required List<string> FormattedConstraints { get; init; }
        public required string DomainText { get; init; }
        public string? IntegerNote { get; init; }
    }
}
