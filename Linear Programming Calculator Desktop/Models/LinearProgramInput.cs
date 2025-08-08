using Linear_Programming_Calculator_Desktop.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
