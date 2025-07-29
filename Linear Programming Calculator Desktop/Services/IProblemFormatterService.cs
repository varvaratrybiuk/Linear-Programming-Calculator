using Methods.MathObjects;

namespace Linear_Programming_Calculator_Desktop.Services
{
    public interface IProblemFormatterService
    {
        string BuildObjectiveFunction(LinearProgrammingProblem problem);
        List<string> BuildConstraints(LinearProgrammingProblem problem, bool isEqual);
        string BuildDomain(LinearProgrammingProblem problem);
        string BuildIntegerNote(LinearProgrammingProblem problem, bool isIntegerProblem);

    }
}
