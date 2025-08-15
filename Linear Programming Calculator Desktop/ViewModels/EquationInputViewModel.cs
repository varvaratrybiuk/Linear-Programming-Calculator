using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Linear_Programming_Calculator_Desktop.DTOs;
using Linear_Programming_Calculator_Desktop.Models;
using Linear_Programming_Calculator_Desktop.Services;
using Linear_Programming_Calculator_Desktop.Stores;
using Methods.MathObjects;
using Methods.Solvers;
using System.Collections.ObjectModel;

namespace Linear_Programming_Calculator_Desktop.ViewModels
{
    /// <summary>
    /// Represent a ViewModel that provides blocks for problem input, shown in the View.
    /// </summary>
    public partial class EquationInputViewModel : ObservableValidator
    {
        /// <summary>
        /// Formatted text for displaying an integer constraint.
        /// </summary>
        public string IntegerVariablesText =>
                 string.Join(", ", ObjectiveFunctionValues!.Select(v => v.Label)) + " are integers";

        /// <summary>
        /// List of coefficients for the variables in the objective function.
        /// </summary>
        public ObservableCollection<FieldViewModel>? ObjectiveFunctionValues { get; private set; }
        /// <summary>
        /// A collection of constraints.
        /// </summary>
        public ObservableCollection<ConstraintViewModel>? ConstraintValues { get; private set; }
        /// <summary>
        /// Indicates whether the problem is a maximization.
        /// </summary>
        [ObservableProperty]
        private bool _isMaximization = true;
        /// <summary>
        /// Indicates whether the problem has integer constraint.
        /// </summary>
        [ObservableProperty]
        private bool _integerCheck;

        /// <summary>
        /// Navigation service used to navigate to the <see cref="ResultsViewModel"/>,
        /// passing a <see cref="LinearProgramResultDto"/> as parameter.
        /// </summary>
        private readonly INavigator<ResultsViewModel, LinearProgramResultDto> _navigationService;

        /// <summary>
        /// Navigation service used to navigate back to the <see cref="StartViewModel"/> without parameters.
        /// </summary>
        private readonly INavigator<StartViewModel> _backNavigationService;
        /// <summary>
        /// Store containing the current <see cref="LinearProgramInput"/>, used to restore 
        /// the view model with previously entered.
        /// </summary>
        private readonly LinearProgramInputStore _lpStore;

        /// <summary>
        /// Initializes a new instance of <see cref="EquationInputViewModel"/> using data from an existing store.
        /// Configures navigation services and restores previous data from storage.
        /// </summary>
        /// <param name="lpStore">The store containing the current <see cref="LinearProgramInput"/> data.</param>
        /// <param name="navigationService">Service for navigating to the <see cref="ResultsViewModel"/>.</param>
        /// <param name="backNavigationService">Service for navigating back to the <see cref="StartViewModel"/>.</param>
        public EquationInputViewModel(
            LinearProgramInputStore lpStore,
            INavigator<ResultsViewModel, LinearProgramResultDto> navigationService,
            INavigator<StartViewModel> backNavigationService)
        {
            _lpStore = lpStore;
            _navigationService = navigationService;
            _backNavigationService = backNavigationService;

            _lpStore.CurrentLinearProgramInputChanged += OnCurrentLinearProgramInputStoreChanged;
            OnCurrentLinearProgramInputStoreChanged();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="EquationInputViewModel"/> with a specified number of variables and constraints.
        /// </summary>
        /// <param name="parameters">Tuple containing the number of variables and constraints.</param>
        /// <param name="lpStore">The store containing the current <see cref="LinearProgramInput"/> data.</param>
        /// <param name="navigationService">Service for navigating to the <see cref="ResultsViewModel"/>.</param>
        /// <param name="backNavigationService">Service for navigating back to the <see cref="StartViewModel"/>.</param>
        /// <remarks>
        /// Configures navigation services and initializes <see cref="ObjectiveFunctionValues"/> and <see cref="ConstraintValues"/>
        /// based on the provided values.
        /// </remarks>
        public EquationInputViewModel(
            (int variables, int constraints) parameters,
            LinearProgramInputStore lpStore,
            INavigator<ResultsViewModel, LinearProgramResultDto> navigationService,
            INavigator<StartViewModel> backNavigationService)
        {
            _navigationService = navigationService;
            _backNavigationService = backNavigationService;
            _lpStore = lpStore;

            ObjectiveFunctionValues = new ObservableCollection<FieldViewModel>(
                Enumerable.Range(1, parameters.variables)
                          .Select(i => new FieldViewModel() { Label = $"x{i}" })
            );

            ConstraintValues = new ObservableCollection<ConstraintViewModel>(
                Enumerable.Range(1, parameters.constraints)
                          .Select(_ => new ConstraintViewModel(parameters.variables))
            );
        }

        /// <summary>
        /// Updates the current view model properties based on the data from the <see cref="LinearProgramInputStore"/>. 
        /// </summary>
        private void OnCurrentLinearProgramInputStoreChanged()
        {
            IsMaximization = _lpStore.CurrentLinearProgramInput.IsMaximization;
            IntegerCheck = _lpStore.CurrentLinearProgramInput.IntegerCheck;

            ObjectiveFunctionValues = new ObservableCollection<FieldViewModel>(_lpStore.CurrentLinearProgramInput.ObjectiveFunctionValues);
            ConstraintValues = new ObservableCollection<ConstraintViewModel>(_lpStore.CurrentLinearProgramInput.ConstraintValues);
        }

        /// <summary>
        /// Command for navigating to the <see cref="StartViewModel"/> for entering the number of variables and constraints.
        /// </summary>
        [RelayCommand]
        public void NewProblem() => _backNavigationService.Navigate();

        /// <summary>
        /// Command that solves the problem using the entered data and navigates to the <see cref="ResultsViewModel"/>.
        /// </summary>
        [RelayCommand]
        public void Solve()
        {
            var problem = BuildLPProblem();
            var solver = new SimplexSolver(problem);
            GomorySolver? gomory = null;
            string? errorMessage = string.Empty;

            try
            {
                solver.Solve();
                if (IntegerCheck == true)
                {
                    gomory = new GomorySolver(solver.Table, problem);
                    gomory.Solve();
                }

            }
            catch (InvalidOperationException ex)
            {
                errorMessage = ex.Message;
            }
            finally
            {
                var resultDto = new LinearProgramResultDto
                {
                    SHistory = solver.SimplexHistory,
                    GHistory = gomory?.GomoryHistory,
                    ErrorMessage = errorMessage,
                    IsIntegerProblem = IntegerCheck
                };

                _lpStore.CurrentLinearProgramInput = BuildLinearProgramInput();

                _navigationService.Navigate(resultDto);
            }
        }

        /// <summary>
        /// Creates a <see cref="LinearProgramInput"/> instance from the current view model data, 
        /// so it can be stored or restored when returning to this view.
        /// </summary>
        /// <returns>A new <see cref="LinearProgramInput"/> containing the current objective function, constraints, and settings.</returns>
        private LinearProgramInput BuildLinearProgramInput()
        {
            return new LinearProgramInput()
            {
                ObjectiveFunctionValues = new List<FieldViewModel>(ObjectiveFunctionValues!),
                ConstraintValues = new List<ConstraintViewModel>(ConstraintValues!),
                IsMaximization = IsMaximization,
                IntegerCheck = IntegerCheck
            };
        }

        /// <summary>
        /// Creates a <see cref="LinearProgrammingProblem"/> instance from the current view model data,
        /// ready to be used for solving the LPP.
        /// </summary>
        /// <returns>A new <see cref="LinearProgrammingProblem"/> containing the current LPP data.</returns>
        private LinearProgrammingProblem BuildLPProblem()
        {
            List<Constraint> constraints = [];
            foreach (var constraint in ConstraintValues!)
            {
                constraints.Add(new Constraint()
                {
                    Coefficients = constraint.ConstraintValues.Select(x => x.Value).ToList(),
                    RightHandSide = constraint.RightSideValue.ToString(),
                    Type = constraint.ConstraintType
                });

            }
            return new LinearProgrammingProblem()
            {
                IsMaximization = IsMaximization,
                ObjectiveFunctionCoefficients = ObjectiveFunctionValues!.Select(f => f.Value).ToList(),
                Constraints = constraints
            };

        }

    }
}

