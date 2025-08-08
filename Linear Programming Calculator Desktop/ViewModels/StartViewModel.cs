using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Linear_Programming_Calculator_Desktop.Attributes;
using Linear_Programming_Calculator_Desktop.Services;
using Linear_Programming_Calculator_Desktop.Stores;


namespace Linear_Programming_Calculator_Desktop.ViewModels
{
    public partial class StartViewModel(INavigator<EquationInputViewModel, (int variables, int constraints)> navigationService, LinearProgramInputStore linearProgramInputStore) : ObservableValidator
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
        private readonly LinearProgramInputStore _linearProgramInputStore = linearProgramInputStore;

        [RelayCommand(CanExecute = nameof(CanGenerateProblem))]
        public void GenerateProblem()
        {
            _linearProgramInputStore.CurrentLinearProgramInput = new();

            _navigationService.Navigate((int.Parse(Variables), int.Parse(Constraints)));

        }

        private bool CanGenerateProblem() => !HasErrors;
    }
}
