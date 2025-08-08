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
    public partial class EquationInputViewModel : ObservableValidator
    {
        public string IntegerVariablesText =>
                 string.Join(", ", ObjectiveFunctionValues!.Select(v => v.Label)) + " are integers";

        public ObservableCollection<FieldViewModel>? ObjectiveFunctionValues { get; private set; }

        public ObservableCollection<ConstraintViewModel>? ConstraintValues { get; private set; }

        [ObservableProperty]
        private bool _isMaximization;

        [ObservableProperty]
        private bool _integerCheck;

        private readonly INavigator<ResultsViewModel, LinearProgramResultDto> _navigationService;
        private readonly INavigator<StartViewModel> _backNavigator;
        private readonly LinearProgramInputStore _lpStore;

        public EquationInputViewModel((int variables, int constraints) parameters, LinearProgramInputStore lpStore, INavigator<ResultsViewModel, LinearProgramResultDto> navigationService, INavigator<StartViewModel> backNavigator)
        {
            _navigationService = navigationService;
            _backNavigator = backNavigator;
            _lpStore = lpStore;

            _lpStore.CurrentLinearProgramInputChanged += OnCurrentLinearProgramInputStoreChanged;
            OnCurrentLinearProgramInputStoreChanged();

            var input = lpStore.CurrentLinearProgramInput;

            if (input.ObjectiveFunctionValues.Count == 0)
            {
                ObjectiveFunctionValues = new ObservableCollection<FieldViewModel>(
                Enumerable.Range(1, parameters.variables)
                     .Select(i => new FieldViewModel() { Label = $"x{i}" })
                );

                ConstraintValues = new ObservableCollection<ConstraintViewModel>(
                    Enumerable.Range(1, parameters.constraints)
                              .Select(_ => new ConstraintViewModel(parameters.variables))
                );
            }

        }

        private void OnCurrentLinearProgramInputStoreChanged()
        {
            IsMaximization = _lpStore.CurrentLinearProgramInput.IsMaximization;
            IntegerCheck = _lpStore.CurrentLinearProgramInput.IntegerCheck;

            ObjectiveFunctionValues = new ObservableCollection<FieldViewModel>(_lpStore.CurrentLinearProgramInput.ObjectiveFunctionValues);
            ConstraintValues = new ObservableCollection<ConstraintViewModel>(_lpStore.CurrentLinearProgramInput.ConstraintValues);
        }

        [RelayCommand]
        public void NewProblem() => _backNavigator.Navigate();

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

                _lpStore.CurrentLinearProgramInput = SaveToStore();

                _navigationService.Navigate(resultDto);
            }
        }

        private LinearProgramInput SaveToStore()
        {
            return new LinearProgramInput()
            {
                ObjectiveFunctionValues = new List<FieldViewModel>(ObjectiveFunctionValues!),
                ConstraintValues = new List<ConstraintViewModel>(ConstraintValues!),
                IsMaximization = IsMaximization,
                IntegerCheck = IntegerCheck
            };
        }

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

