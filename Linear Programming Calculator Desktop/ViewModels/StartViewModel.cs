using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Linear_Programming_Calculator_Desktop.Attributes;
using Linear_Programming_Calculator_Desktop.Services;

namespace Linear_Programming_Calculator_Desktop.ViewModels
{
    public partial class StartViewModel(INavigator<EquationInputViewModel, (int variables, int constraints)> navigationService) : ObservableValidator
    {
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [ValidVariableCount(2)]
        private string _variables = "2";

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [ValidVariableCount(2)]
        private string _constraints = "2";

        private readonly INavigator<EquationInputViewModel, (int variables, int constraints)> _navigationService = navigationService;

        [RelayCommand(CanExecute = nameof(CanGenerateProblem))]
        public void GenerateProblem() => _navigationService.Navigate((int.Parse(Variables), int.Parse(Constraints)));

        private bool CanGenerateProblem() => !HasErrors;
    }
}
