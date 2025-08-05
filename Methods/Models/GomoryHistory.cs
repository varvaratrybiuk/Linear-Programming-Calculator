using Fractions;

namespace Methods.Models
{
    public class GomoryHistory
    {
        public (int rowIndex, Fraction value) MaxFracValue { get; set; }
        public BranchCut Cut { get; set; } = new BranchCut();
        public List<SimplexStep> Steps { get; set; } = [];
    }
}
