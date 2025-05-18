using Methods.Enums;

namespace Methods.MathObjects
{
    public class Constraint : ICloneable
    {
        public List<string> Coefficients { get; set; }
        public ConstraintType Type { get; set; }
        public string RightHandSide { get; set; }

        public object Clone()
        {
            var newConstraint = (Constraint)MemberwiseClone();
            newConstraint.Coefficients = new List<string>(Coefficients);
            return newConstraint;
        }
    }
}
