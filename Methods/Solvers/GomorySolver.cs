using Methods.Interfaces;
using Methods.Models;

namespace Methods.Solvers
{
    public class GomorySolver : ILinearSolver
    {
        private SimplexTable _table;
        SimplexTable ILinearSolver.Table { get => _table; set => _table = value; }

        public void Solve()
        {
            throw new NotImplementedException();
        }

        SimplexTable ILinearSolver.GetSolution()
        {
            throw new NotImplementedException();
        }
    }
}
