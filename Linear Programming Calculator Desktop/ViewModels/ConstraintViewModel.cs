using CommunityToolkit.Mvvm.ComponentModel;
using Linear_Programming_Calculator_Desktop.Attributes;
using Methods.Enums;
using System.Collections.ObjectModel;

namespace Linear_Programming_Calculator_Desktop.ViewModels
{
    public partial class ConstraintViewModel : ObservableValidator
    {
        [ObservableProperty]
        private ObservableCollection<FieldViewModel> _constraintValues = [];

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [NumericOnly]
        private string _rightSideValue = "0";

        [ObservableProperty]
        private ConstraintType _constraintType = ConstraintType.LessThanOrEqual;

        public ConstraintViewModel(int variables)
        {
            _constraintValues = new ObservableCollection<FieldViewModel>(
           Enumerable.Range(1, variables)
                     .Select(i => new FieldViewModel() { Label = $"x{i}" })
            );
        }
    }
}
