using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fractions;
using Linear_Programming_Calculator_Desktop.DTOs;
using Linear_Programming_Calculator_Desktop.Models;
using Linear_Programming_Calculator_Desktop.Services;
using Methods.MathObjects;
using Methods.Models;

namespace Linear_Programming_Calculator_Desktop.ViewModels
{
    public partial class ResultsViewModel : ObservableObject
    {
        public FormattedLinearProblem MathModelBlock { get; private set; }
        public FormattedLinearProblem FreeVariableBlock { get; private set; }
        public FormattedLinearProblem? ArtificialVariableBlock { get; private set; }
        public string InitialBasisText => string.Join(", ", _resultDto.SHistory.InitialBasis.Select(kvp => $"{kvp.Key} = {kvp.Value}"));
        public string? ErrorMessage => _resultDto.ErrorMessage;
        public List<SimplexViewModel> SimplexTables => _resultDto.SHistory.Steps.Select(s =>
        {
            return new SimplexViewModel(s).LoadFromTable();
        }).ToList();

        public SimplexViewModel OptimalResult => new SimplexViewModel(new SimplexStep()
        {
            Table = _resultDto.SHistory.OptimalTable,
        }).LoadFromTable();

        public string SimplexSolutionSummary
        {
            get
            {
                string summary = string.Empty;

                if (_resultDto.SHistory.OptimalTable is null)
                    return summary;

                for (int i = 0; i < _resultDto.SHistory.InitialLinearProgrammingProblem!.ObjectiveFunctionCoefficients.Count; i++)
                {
                    string key = $"x{i + 1}";

                    int rowIndex = OptimalResult.Step.Table.RowVariables.Keys.ToList().IndexOf(key);
                    var value = rowIndex >= 0 ? OptimalResult.Step.Table.Values[rowIndex, 0] : Fraction.Zero;

                    summary += $"{key} = {value}, ";
                }

                summary += $"F{(_resultDto.SHistory.InitialLinearProgrammingProblem!.IsMaximization ? "max" : "min")} = {OptimalResult.Step.Table.DeltaRow![0]}";

                return summary;
            }
        }

        public List<GomoryViewModel>? GomoryHistory => _resultDto.GHistory?.Select(s =>
        {
            return new GomoryViewModel(s);
        }).ToList();


        private readonly LinearProgramResultDto _resultDto;

        private readonly INavigator _backNavigator;
        private readonly INavigator _editNavigator;

        private readonly IProblemFormatterService _problemFormatter;

        public ResultsViewModel(LinearProgramResultDto resultDto,
                        INavigator backNavigator,
                        INavigator editNavigator,
                        IProblemFormatterService problemFormatter)
        {
            _resultDto = resultDto;
            _backNavigator = backNavigator;
            _editNavigator = editNavigator;
            _problemFormatter = problemFormatter;

            MathModelBlock = FormatBlock(resultDto.SHistory.InitialLinearProgrammingProblem!, isEqual: false);
            FreeVariableBlock = FormatBlock(resultDto.SHistory.FreeVariableProblem!, isEqual: true);
            ArtificialVariableBlock = resultDto.SHistory.ArtificialProblemProblem != null
                ? FormatBlock(resultDto.SHistory.ArtificialProblemProblem, isEqual: true)
                : null;

        }

        [RelayCommand]
        public void NewProblem() => _backNavigator.Navigate();

        [RelayCommand]
        public void EditProblem() => _editNavigator.Navigate();

        private FormattedLinearProblem FormatBlock(LinearProgrammingProblem problem, bool isEqual)
        {
            return new FormattedLinearProblem
            {
                FormattedConstraints = _problemFormatter.BuildConstraints(problem, isEqual),
                FormattedObjectiveFunction = _problemFormatter.BuildObjectiveFunction(problem),
                DomainText = _problemFormatter.BuildDomain(problem),
                IntegerNote = _problemFormatter.BuildIntegerNote(problem, _resultDto.IsIntegerProblem)
            };
        }
    }
}
