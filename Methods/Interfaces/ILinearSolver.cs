namespace Methods.Interfaces
{
    internal interface ILinearSolver
    {
        void Solve();
        void Pivot();
        bool IsUnbounded();
        bool IsOptimal();
    }
}
