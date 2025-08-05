using Fractions;

namespace Methods.Models
{
    public class BranchCut
    {
        public Dictionary<string, (Fraction intPart, Fraction fracPart)> FractionalElements { get; set; } = [];

        public List<Fraction> CutExpression { get; set; } = [];

    }
}
