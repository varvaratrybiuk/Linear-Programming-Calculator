namespace Methods.Models
{
    /// <summary>
    /// Represents the current state of the simplex table during pivot selection.
    /// </summary>
    public class SimplexStep
    {
        /// <summary>
        /// Index of the pivot row in the current simplex table.
        /// </summary>
        public int PivotRow { get; set; }
        /// <summary>
        /// Index of the pivot column in the current simplex table.
        /// </summary>
        public int PivotColumn { get; set; } = -1;
        /// <summary>
        /// Simplex tableau at the current step.
        /// </summary>
        public required SimplexTable Table { get; set; }
    }
}
