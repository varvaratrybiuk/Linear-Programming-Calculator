using CommunityToolkit.Mvvm.ComponentModel;
using Linear_Programming_Calculator_Desktop.Attributes;
using Methods.Enums;
using System.Collections.ObjectModel;

namespace Linear_Programming_Calculator_Desktop.ViewModels
{
    /// <summary>
    /// Represents a ViewModel that is bound to a single constraint in the View.
    /// </summary>
    public partial class ConstraintViewModel : ObservableValidator
    {
        /// <summary>
        /// List of coefficients for the variables.
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<FieldViewModel> _constraintValues = [];

        /// <summary>
        /// The right-hand side (RHS) value.
        /// </summary>
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [NumericOnly]
        private string _rightSideValue = "0";

        /// <summary>
        /// The type of the constraint.
        /// </summary>
        [ObservableProperty]
        private ConstraintType _constraintType = ConstraintType.LessThanOrEqual;

        /// <summary>
        /// Constructor that initializes the constraint values.
        /// </summary>
        /// <param name="variables">Quantity of constraints variables.</param>
        public ConstraintViewModel(int variables)
        {
            _constraintValues = new ObservableCollection<FieldViewModel>(
           Enumerable.Range(1, variables)
                     .Select(i => new FieldViewModel() { Label = $"x{i}" })
            );
        }
    }
}
