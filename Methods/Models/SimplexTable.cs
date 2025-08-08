using Fractions;

namespace Methods.Models
{
    public class SimplexTable : ICloneable
    {
        public Dictionary<string, string> RowVariables { get; set; } = [];
        public Dictionary<string, string> ColumnVariables { get; set; } = [];
        public Fraction[,] Values { get; set; }
        public List<ExpressionValue> DeltaRow { get; set; } = [];
        public List<string> ThetaRow { get; set; }

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

