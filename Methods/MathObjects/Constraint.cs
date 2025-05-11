using Methods.Enums;

namespace Methods.Contracts
{
    public class Constraint
    {
        public List<double> Coefficients { get; set; }
        public ConstraintType Type { get; set; }
        public double RightHandSide { get; set; }
    }
}
