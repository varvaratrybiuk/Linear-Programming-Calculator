using Fractions;

namespace Methods.Models
{
    public class GomoryHistory
    {
        public Tuple<int, Fraction> MaxValue { get; set; }
        public BranchCut Cut { get; set; } = new BranchCut();
        public List<SimplexStep> Steps { get; set; } = new List<SimplexStep>();
    }
}
