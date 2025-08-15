using Linear_Programming_Calculator_Desktop.ViewModels;

namespace Linear_Programming_Calculator_Desktop.Models
{
    /// <summary>
    /// Represents user input for a LPP collected from the view.
    /// </summary>
    public class LinearProgramInput
    {
        /// <summary>
        /// The values of the objective function coefficients entered by the user.
        /// </summary>
        public List<FieldViewModel> ObjectiveFunctionValues { get; set; } = [];

        /// <summary>
        /// The values of the constraints entered by the user.
        /// </summary>
        public List<ConstraintViewModel> ConstraintValues { get; set; } = [];

        /// <summary>
        /// Indicates whether the problem is a maximization problem.
        /// </summary>
        public bool IsMaximization { get; set; } = true;

        /// <summary>
        /// Indicates whether the problem includes integer constraint.
        /// </summary>
        public bool IntegerCheck { get; set; } = false;
    }
}
