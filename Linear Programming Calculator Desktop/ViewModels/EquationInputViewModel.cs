using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Linear_Programming_Calculator_Desktop.DTOs;
using Linear_Programming_Calculator_Desktop.Services;
using Methods.MathObjects;
using Methods.Solvers;
using System.Collections.ObjectModel;

namespace Linear_Programming_Calculator_Desktop.ViewModels
{
    public partial class EquationInputViewModel : ObservableValidator
    {
        public ObservableCollection<FieldViewModel> ObjectiveFunctionValues =>
        new ObservableCollection<FieldViewModel>(
                Enumerable.Range(1, _parameters.variables)
                     .Select(i => new FieldViewModel() { Label = $"x{i}" })
        );

        public ObservableCollection<ConstraintViewModel> ConstraintValues => new ObservableCollection<ConstraintViewModel>(
                Enumerable.Range(1, _parameters.constraints)
                          .Select(_ => new ConstraintViewModel(_parameters.variables))
        );
        public string IntegerVariablesText =>
            string.Join(", ", ObjectiveFunctionValues.Select(v => v.Label)) + " — цілі";


        [ObservableProperty]
        private bool _isMaximization = true;

        [ObservableProperty]
        private bool _integerCheck;

        private readonly (int variables, int constraints) _parameters;

        private readonly INavigator<LinearProgramResultDto> _navigationService;
        private readonly INavigator _backNavigator;

        public EquationInputViewModel((int variables, int constraints) parameters, INavigator<LinearProgramResultDto> navigationService, INavigator backNavigator)
        {
            _parameters = parameters;
            _navigationService = navigationService;
            _backNavigator = backNavigator;
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

                _navigationService.Navigate(resultDto);
            }
        }

        private LinearProgrammingProblem BuildLPProblem()
        {
            List<Constraint> constraints = new List<Constraint>();
            foreach (var constraint in ConstraintValues)
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
                ObjectiveFunctionCoefficients = ObjectiveFunctionValues.Select(f => f.Value).ToList(),
                Constraints = constraints
            };

        }

    }
}

