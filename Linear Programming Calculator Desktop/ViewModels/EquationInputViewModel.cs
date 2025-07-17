using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Linear_Programming_Calculator_Desktop.Services;
using Methods.MathObjects;
using Methods.Models;
using Methods.Solvers;
using System.Collections.ObjectModel;

namespace Linear_Programming_Calculator_Desktop.ViewModels
{
    public partial class EquationInputViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _integerCheck;

        [ObservableProperty]
        private ObservableCollection<FieldViewModel> _objectiveFunctionValues = new();
        [ObservableProperty]
        private bool _isMaximization;

        [ObservableProperty]
        private ObservableCollection<ConstraintViewModel> _constraintValues = new();

        public string IntegerVariablesText =>
                 string.Join(", ", ObjectiveFunctionValues.Select(v => v.Label)) + " — цілі";

        public EquationInputViewModel((int variables, int constraints) parameters, INavigator<(SimplexHistory sHistory, List<GomoryHistory>? gHistory, string? errorMessage)> navigationService, INavigator backNavigator)
        {
            Parameters = parameters;
            _navigationService = navigationService;
            _backNavigator = backNavigator;

            ObjectiveFunctionValues = new ObservableCollection<FieldViewModel>(
                Enumerable.Range(1, parameters.variables)
                     .Select(i => new FieldViewModel() { Label = $"x{i}" })
            );

            ConstraintValues = new ObservableCollection<ConstraintViewModel>(
                Enumerable.Range(1, parameters.constraints)
                          .Select(_ => new ConstraintViewModel(parameters.variables))
            );
        }

        public (int variables, int constraints) Parameters { get; set; }
        private readonly INavigator<(SimplexHistory sHistory, List<GomoryHistory>? gHistory, string? errorMessage)> _navigationService;
        private readonly INavigator _backNavigator;

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
                if (IntegerCheck! == true)
                {
                    gomory = new GomorySolver(solver.Table, problem);
                    gomory.Solve();
                }

            }
            catch (InvalidOperationException ex)
            {
                errorMessage = ex.Message;
            }

            _navigationService.Navigate((solver.SimplexHistory, gomory?.GomoryHistory, errorMessage));
        }

        private LinearProgrammingProblem BuildLPProblem()
        {
            List<Constraint> constraints = new List<Constraint>();
            foreach (var constraint in ConstraintValues)
            {
                constraints.Add(new Constraint()
                {
                    Coefficients = constraint.ConstraintValues.Select(x => x.Value.ToString()).ToList(),
                    RightHandSide = constraint.RightSideValue.ToString(),
                    Type = constraint.ConstraintType
                });

            }
            return new LinearProgrammingProblem()
            {
                IsMaximization = IsMaximization,
                ObjectiveFunctionCoefficients = ObjectiveFunctionValues.Select(f => f.Value.ToString()).ToList(),
                Constraints = constraints
            };

        }

    }
}

