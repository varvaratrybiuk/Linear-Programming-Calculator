using Methods.Models;

namespace Linear_Programming_Calculator_Desktop.DTOs
{
    public record LinearProgramResultDto
    {
        public required SimplexHistory SHistory { get; init; }
        public required bool IsIntegerProblem { get; init; }
        public List<GomoryHistory>? GHistory { get; init; }
        public string? ErrorMessage { get; init; }
    }
}
