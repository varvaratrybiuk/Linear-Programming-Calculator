using Methods.Enums;

namespace Methods.MathObjects
{
    public class Constraint
    {
        public List<string> Coefficients { get; set; }
        public ConstraintType Type { get; set; }
        public string RightHandSide { get; set; }
    }
}
