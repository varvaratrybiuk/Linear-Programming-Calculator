using CommunityToolkit.Mvvm.ComponentModel;
using Linear_Programming_Calculator_Desktop.Attributes;
using Methods.Enums;
using System.Collections.ObjectModel;

namespace Linear_Programming_Calculator_Desktop.ViewModels
{
    public partial class ConstraintViewModel : ObservableValidator
    {
        [ObservableProperty]
        private ObservableCollection<FieldViewModel> _constraintValues = new();

        [ObservableProperty]
        [NumericOnly]
        private double _rightSideValue;

        [ObservableProperty]
        private ConstraintType _constraintType;

        public ConstraintViewModel(int variables)
        {
            _constraintValues = new ObservableCollection<FieldViewModel>(
           Enumerable.Range(1, variables)
                     .Select(i => new FieldViewModel() { Label = $"x{i}" })
            );

            _constraintType = ConstraintType.LessThanOrEqual;
            _rightSideValue = 0;
        }
    }
}
