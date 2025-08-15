using Methods.Models;

namespace Linear_Programming_Calculator_Desktop.Services
{
    /// <summary>
    /// Service interface for formatting Gomory cuts into readable string representations.
    /// </summary>
    public interface IGomoryCutFormatterService
    {
        /// <summary>
        /// Builds the formatted lines representing the given <see cref="BranchCut"/> instance.
        /// </summary>
        /// <param name="branchCut">The branch cut.</param>
        /// <returns>A list of strings, each representing a line of the Gomory cut.</returns>
        public List<string> BuildGomoryCutLines(BranchCut branchCut);

        /// <summary>
        /// Builds the section of the Gomory cut showing fractional parts of the variables.
        /// </summary>
        /// <param name="branchCut">The branch cut.</param>
        /// <returns>A formatted string representing the fractional parts section.</returns>
        public string BuildFractionPartsSection(BranchCut branchCut);

        /// <summary>
        /// Builds the section of the Gomory cut corresponding to inequality constraints.
        /// </summary>
        /// <returns>A formatted string representing the inequality section.</returns>
        public string BuildInequalitySection(BranchCut branchCut);

        /// <summary>
        /// Builds the section of the Gomory cut corresponding to equality constraints.
        /// </summary>
        /// <param name="branchCut">The branch cut.</param>
        /// <returns>A formatted string representing the equality section.</returns>
        public string BuildEqualitySection(BranchCut branchCut);

        /// <summary>
        /// Builds the section showing the right-hand side (RHS) values of the Gomory cut.
        /// </summary>
        /// <param name="branchCut">The branch cut.</param>
        /// <returns>A formatted string representing the RHS section.</returns>
        public string BuildRightHandSideSection(BranchCut branchCut);
    }
}
