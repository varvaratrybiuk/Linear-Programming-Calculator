using Methods.Models;

namespace Methods.Interfaces
{
    internal interface ILinearSolver
    {
        SimplexTable Table { get; set; }
        void Solve();
        void Pivot();
        SimplexTable GetSolution();
    }
}
