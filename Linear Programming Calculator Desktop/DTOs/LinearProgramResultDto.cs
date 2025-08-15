using Methods.Models;

namespace Linear_Programming_Calculator_Desktop.DTOs
{
    /// <summary>
    /// Data transfer object for passing data between views.
    /// </summary>
    public record LinearProgramResultDto
    {
        /// <summary>
        /// Instance of the Simplex history steps.
        /// </summary>
        public required SimplexHistory SHistory { get; init; }
        /// <summary>
        /// Indicates whether the problem is an integer programming problem.
        /// </summary>
        public required bool IsIntegerProblem { get; init; }
        /// <summary>
        /// List of Gomory history steps.
        /// </summary>
        public List<GomoryHistory>? GHistory { get; init; }
        /// <summary>
        /// Error message text, if any.
        /// </summary>
        public string? ErrorMessage { get; init; }
    }
}
