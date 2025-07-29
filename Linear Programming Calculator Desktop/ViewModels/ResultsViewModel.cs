using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        public List<SimplexViewModel> SimplexTables => _resultDto.SHistory.Steps.Select(s =>
        {
            return new SimplexViewModel(s).LoadFromTable();
        }).ToList();

        public SimplexViewModel OptimalResult => new SimplexViewModel(new SimplexStep()
        {
            Table = _resultDto.SHistory.OptimalTable,
        }).LoadFromTable();


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
