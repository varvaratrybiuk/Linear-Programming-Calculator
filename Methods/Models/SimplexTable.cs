using Fractions;

namespace Methods.Models
{
    /// <summary>
    /// Represents a simplex tableau used during the simplex algorithm iterations. 
    /// </summary>
    public class SimplexTable : ICloneable
    {
        /// <summary>
        /// Dictionary of row variables, where the key is the variable name (e.g., "x1"),
        /// and the value is the corresponding coefficient from the objective function.
        /// </summary>
        public Dictionary<string, string> RowVariables { get; set; } = [];
        /// <summary>
        /// Dictionary of column variables, where the key is the variable name (e.g., "A0"),
        /// and the value is the corresponding coefficient from the objective function.
        /// </summary>
        public Dictionary<string, string> ColumnVariables { get; set; } = [];
        /// <summary>
        /// Numerical values of the simplex tableau.
        /// </summary>
        public Fraction[,] Values { get; set; }
        /// <summary>
        /// Delta row values representing the results after computing reduced costs.
        /// </summary>
        public List<ExpressionValue> DeltaRow { get; set; } = [];
        /// <summary>
        /// Theta row values used to identify pivot elements during the Gomory method.
        /// </summary>
        public List<string> ThetaRow { get; set; }

        /// <summary>
        /// Creates a clone of the current object.
        /// </summary>
        /// <returns>A new instance of the <see cref="SimplexTable"/> object.</returns>
        public object Clone()
        {
            return new SimplexTable
            {
                RowVariables = RowVariables != null ? new Dictionary<string, string>(RowVariables) : [],
                ColumnVariables = ColumnVariables != null ? new Dictionary<string, string>(ColumnVariables) : [],
                Values = (Fraction[,])Values.Clone(),
                DeltaRow = DeltaRow != null ? new List<ExpressionValue>(DeltaRow) : [],
                ThetaRow = ThetaRow != null ? new List<string>(ThetaRow) : []
            };
        }
    }
}

