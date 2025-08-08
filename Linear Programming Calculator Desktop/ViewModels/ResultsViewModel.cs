using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Linear_Programming_Calculator_Desktop.DTOs;
using Linear_Programming_Calculator_Desktop.Models;
using Linear_Programming_Calculator_Desktop.Services;
using Methods.MathObjects;
using Methods.Models;
using System.Text;

namespace Linear_Programming_Calculator_Desktop.ViewModels
{
    public partial class ResultsViewModel : ObservableObject
    {
        public FormattedLinearProblem MathModelBlock { get; private set; }
        public FormattedLinearProblem SlackVariableBlock { get; private set; }
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
                StringBuilder summary = new();

                if (_resultDto.SHistory.OptimalTable is null)
                    return summary.ToString();

                for (int i = 0; i < _resultDto.SHistory.InitialLinearProgrammingProblem!.ObjectiveFunctionCoefficients.Count; i++)
                {
                    var (_, resultStr) = _resultSummaryService.FormatVariableAssignment(OptimalResult.Step.Table, i);
                    summary.Append(resultStr);
                }

                summary.Append(_resultSummaryService.FormatObjectiveFunctionValue(OptimalResult.Step.Table));

                return summary.ToString();
            }
        }

        public List<GomoryViewModel>? GomoryHistory => _resultDto.GHistory?.Select(s =>
        {
            return new GomoryViewModel(s, _resultDto.SHistory.InitialLinearProgrammingProblem!.ObjectiveFunctionCoefficients, _resultSummaryService);
        }).ToList();


        private readonly LinearProgramResultDto _resultDto;
        private (int variables, int constraints) InputParameters => (_resultDto.SHistory.InitialLinearProgrammingProblem!.VariablesCount, _resultDto.SHistory.InitialLinearProgrammingProblem.Constraints.Count);

        private readonly INavigator<StartViewModel> _backNavigator;
        private readonly INavigator<EquationInputViewModel, (int variables, int constraints)> _editNavigator;

        private readonly IProblemFormatterService _problemFormatter;
        private readonly IOptimalResultSummaryService _resultSummaryService;

        public ResultsViewModel(LinearProgramResultDto resultDto,
                        INavigator<StartViewModel> backNavigator,
                        INavigator<EquationInputViewModel, (int variables, int constraints)> editNavigator,
                        IProblemFormatterService problemFormatter,
                        IOptimalResultSummaryService resultSummaryService)
        {
            _resultDto = resultDto;
            _backNavigator = backNavigator;
            _editNavigator = editNavigator;
            _problemFormatter = problemFormatter;
            _resultSummaryService = resultSummaryService;

            MathModelBlock = FormatBlock(resultDto.SHistory.InitialLinearProgrammingProblem!, isEqual: false);
            SlackVariableBlock = FormatBlock(resultDto.SHistory.SlackVariableProblem!, isEqual: true);
            ArtificialVariableBlock = resultDto.SHistory.ArtificialProblemProblem != null
                ? FormatBlock(resultDto.SHistory.ArtificialProblemProblem, isEqual: true)
                : null;

        }

        [RelayCommand]
        public void NewProblem() => _backNavigator.Navigate();

        [RelayCommand]
        public void EditProblem() => _editNavigator.Navigate(InputParameters);

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
