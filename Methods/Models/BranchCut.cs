using Fractions;

namespace Methods.Models
{
    public class BranchCut
    {
        public Dictionary<string, Tuple<Fraction, Fraction>> Elements { get; set; } = [];

        public List<Fraction> CutExpression { get; set; } = [];

    }
}
