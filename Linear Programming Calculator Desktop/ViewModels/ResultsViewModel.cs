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
    /// <summary>
    /// Represents a ViewModel that displays the results of solving a LPP.
    /// </summary>
    public partial class ResultsViewModel : ObservableObject
    {
        /// <summary>
        /// Formatted representation of the original mathematical model of the problem.
        /// </summary>
        public FormattedLinearProblem MathModelBlock { get; private set; }

        /// <summary>
        /// Formatted representation of the problem after introducing slack variables.
        /// </summary>
        public FormattedLinearProblem SlackVariableBlock { get; private set; }

        /// <summary>
        /// Formatted representation of the problem after introducing artificial variables, if applicable.
        /// </summary>
        public FormattedLinearProblem? ArtificialVariableBlock { get; private set; }

        /// <summary>
        /// Text description of the initial basis for the problem.
        /// </summary>
        public string InitialBasisText => string.Join(", ", _resultDto.SHistory.InitialBasis.Select(kvp => $"{kvp.Key} = {kvp.Value}"));
        /// <summary>
        /// Error message describing any issues encountered during solving.
        /// </summary>
        public string? ErrorMessage => _resultDto.ErrorMessage;
        /// <summary>
        /// List of formatted simplex tables for each iteration step.
        /// </summary>
        public List<SimplexViewModel> SimplexTables => _resultDto.SHistory.Steps.Select(s =>
        {
            return new SimplexViewModel(s).LoadFromTable();
        }).ToList();

        /// <summary>
        /// Final optimal simplex table after solving.
        /// </summary>
        public SimplexViewModel OptimalResult => new SimplexViewModel(new SimplexStep()
        {
            Table = _resultDto.SHistory.OptimalTable,
        }).LoadFromTable();

        /// <summary>
        /// Summary of the solution obtained by the simplex method.
        /// </summary>
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

        /// <summary>
        /// History of Gomory cut method steps, if needed.
        /// </summary>
        public List<GomoryViewModel>? GomoryHistory => _resultDto.GHistory?.Select(s =>
        {
            return new GomoryViewModel(s, _resultDto.SHistory.InitialLinearProgrammingProblem!.ObjectiveFunctionCoefficients, _resultSummaryService, _gomoryCutFormatterService);
        }).ToList();

        /// <summary>
        /// DTO containing the results of solving the LPP.
        /// </summary>
        private readonly LinearProgramResultDto _resultDto;

        /// <summary>
        /// Navigator for returning to the <see cref="StartViewModel"/>.
        /// </summary>
        private readonly INavigator<StartViewModel> _backNavigator;

        /// <summary>
        /// Navigator for returning to the equation <see cref="EquationInputViewModel"/>.
        /// </summary>
        private readonly INavigator<EquationInputViewModel> _editNavigator;

        /// <summary>
        /// Service for formatting the problem for display.
        /// </summary>
        private readonly IProblemFormatterService _problemFormatter;

        /// <summary>
        /// Service for generating a summary of the optimal result.
        /// </summary>
        private readonly IOptimalResultSummaryService _resultSummaryService;

        /// <summary>
        /// Service for formatting Gomory cut steps for display.
        /// </summary>
        private readonly IGomoryCutFormatterService _gomoryCutFormatterService;

        /// <summary>
        /// Initializes all services and formats all mathematical block steps.
        /// </summary>
        /// <param name="resultDto">Results of the solved LPP.</param>
        /// <param name="backNavigator">Navigator for returning to the <see cref="StartViewModel"/>.</param>
        /// <param name="editNavigator">Navigator for returning to the equation <see cref="EquationInputViewModel"/>.</param>
        /// <param name="problemFormatter">Service for formatting the problem for display.</param>
        /// <param name="resultSummaryService">Service for generating the optimal result summary.</param>
        /// <param name="gomoryCutFormatterService">Service for formatting Gomory cut steps for display.</param>
        public ResultsViewModel(LinearProgramResultDto resultDto,
                        INavigator<StartViewModel> backNavigator,
                        INavigator<EquationInputViewModel> editNavigator,
                        IProblemFormatterService problemFormatter,
                        IOptimalResultSummaryService resultSummaryService,
                        IGomoryCutFormatterService gomoryCutFormatterService)
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
            _gomoryCutFormatterService = gomoryCutFormatterService;
        }

        /// <summary>
        /// Navigates to the view for creating a new LPP.
        /// </summary>
        [RelayCommand]
        public void NewProblem() => _backNavigator.Navigate();

        /// <summary>
        /// Navigates to the view for editing the current LPP.
        /// </summary>
        [RelayCommand]
        public void EditProblem() => _editNavigator.Navigate();

        /// <summary>
        /// Builds a formatted text block.
        /// </summary>
        /// <param name="problem">The <see cref="LinearProgrammingProblem"/> containing the objective function and constraints to format.</param>
        /// <param name="isEqual">
        /// A boolean flag indicating whether the constraints should be formatted as equalities (<c>true</c>) 
        /// or inequalities (<c>false</c>).
        /// </param>
        /// <returns>A formatted text block of the mathematical model.</returns>
        /// <remarks>
        /// Representing each step in constructing the mathematical model
        /// from the given linear programming problem.
        /// </remarks>
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
