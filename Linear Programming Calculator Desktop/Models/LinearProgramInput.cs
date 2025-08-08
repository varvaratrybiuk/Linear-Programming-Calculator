using Linear_Programming_Calculator_Desktop.ViewModels;

namespace Linear_Programming_Calculator_Desktop.Models
{
    public class LinearProgramInput
    {
        public List<FieldViewModel> ObjectiveFunctionValues { get; set; } = [];
        public List<ConstraintViewModel> ConstraintValues { get; set; } = [];
        public bool IsMaximization { get; set; } = true;
        public bool IntegerCheck { get; set; } = false;
    }
}
