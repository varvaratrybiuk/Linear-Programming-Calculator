using Methods.Enums;

namespace Methods.MathObjects
{
    /// <summary>
    /// Class that represent a constraint parts
    /// </summary>
    public class Constraint : ICloneable
    {
        /// <summary>
        /// List of coefficients.
        /// </summary>
        public List<string> Coefficients { get; set; } = [];
        /// <summary>
        /// Represents the type of constraint, such as greater than or equal, less than or equal, or equal.
        /// </summary>
        public ConstraintType Type { get; set; }
        /// <summary>
        /// Represents the expression on the right-hand side of the constraint.
        /// </summary>
        public string RightHandSide { get; set; } = string.Empty;

        /// <summary>
        /// Creates a clone of the current object.
        /// </summary>
        /// <returns>A new instance of the <see cref="Constraint"/> object.</returns>
        public object Clone()
        {
            var newConstraint = (Constraint)MemberwiseClone();
            newConstraint.Coefficients = new List<string>(Coefficients);
            return newConstraint;
        }
    }
}
