namespace Methods.Interfaces
{
    /// <summary>
    /// Interface that represents the essential methods for solving Linear Programming Problems (LPPs).
    /// </summary>
    public interface ILinearSolver
    {
        /// <summary>
        /// Performs the steps required to solve the linear programming problem.
        /// </summary>
        void Solve();
        /// <summary>
        /// Finds the pivot row and column for the next step of solving.
        /// </summary>
        void Pivot();
        /// <summary>
        /// Calculates the reduced costs (delta values) for the current simplex tableau.
        /// </summary>
        void CalculateReducedCosts();
        /// <summary>
        /// Determines whether the LPP has a solution.
        /// </summary>
        /// <returns><c>true</c> if the problem has no solution; otherwise, <c>false</c>.</returns>
        bool IsUnbounded();
        /// <summary>
        /// Determines whether the current solution is optimal.
        /// </summary>
        /// <returns><c>true</c> if the problem has an optimal solution; otherwise, <c>false</c>.</returns>
        bool IsOptimal();
    }
}
