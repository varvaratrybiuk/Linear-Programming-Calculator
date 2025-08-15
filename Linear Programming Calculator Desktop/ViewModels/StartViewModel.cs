using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Linear_Programming_Calculator_Desktop.Models;
using Linear_Programming_Calculator_Desktop.Attributes;
using Linear_Programming_Calculator_Desktop.Services;
using Linear_Programming_Calculator_Desktop.Stores;


namespace Linear_Programming_Calculator_Desktop.ViewModels
{
    /// <summary>
    /// Represents a ViewModel for entering the number of variables and constraints.
    /// </summary>
    /// <param name="navigationService">Navigation service used to navigate to the <see cref="EquationInputViewModel"/>.</param>
    /// <param name="linearProgramInputStore">Store containing the current <see cref="LinearProgramInput"/>.</param>
    public partial class StartViewModel(INavigator<EquationInputViewModel, (int variables, int constraints)> navigationService, LinearProgramInputStore linearProgramInputStore) : ObservableValidator
    {
        /// <summary>
        /// Number of variables.
        /// </summary>
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [ValidVariableCount(2)]
        private string _variables = "2";

        /// <summary>
        /// Number of constraint.
        /// </summary>
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [ValidVariableCount(2)]
        private string _constraints = "2";

        /// <summary>
        /// Navigation service used to navigate to the <see cref="EquationInputViewModel"/>,
        /// passing a tuple containing the number of variables and constraints as a parameter.
        /// </summary>
        private readonly INavigator<EquationInputViewModel, (int variables, int constraints)> _navigationService = navigationService;
        /// <summary>
        /// Store containing the current <see cref="LinearProgramInput"/>, used to restore 
        /// the view model with previously entered data.
        /// </summary>
        private readonly LinearProgramInputStore _linearProgramInputStore = linearProgramInputStore;

        /// <summary>
        /// Clears the previous data in <see cref="LinearProgramInputStore"/> and navigates to the parameter entry view.
        /// </summary>
        /// <remarks>
        /// The command can only execute if <see cref="CanGenerateProblem"/> returns <c>true</c>.
        /// </remarks>
        [RelayCommand(CanExecute = nameof(CanGenerateProblem))]
        public void GenerateProblem()
        {
            _linearProgramInputStore.CurrentLinearProgramInput = new();

            _navigationService.Navigate((int.Parse(Variables), int.Parse(Constraints)));

        }

        /// <summary>
        /// Checks whether any validation errors exist in the current view model.
        /// </summary>
        /// <returns><c>true</c> if there aren't validation errors; otherwise, <c>false</c>.</returns>
        private bool CanGenerateProblem() => !HasErrors;
    }
}
