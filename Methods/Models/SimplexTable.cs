using Fractions;

namespace Methods.Models
{
    public class SimplexTable : ICloneable
    {
        public Dictionary<string, string> RowVariables { get; set; }
        public Dictionary<string, string> ColumnVariables { get; set; }
        public Fraction[,] Values { get; set; }
        public Fraction[]? DeltaRow { get; set; }
        public List<string> ThetaRow { get; set; }

        public object Clone()
        {
            return new SimplexTable
            {
                RowVariables = RowVariables != null ? new Dictionary<string, string>(RowVariables) : new Dictionary<string, string>(),
                ColumnVariables = ColumnVariables != null ? new Dictionary<string, string>(ColumnVariables) : new Dictionary<string, string>(),
                Values = (Fraction[,])Values.Clone(),
                DeltaRow = DeltaRow != null ? DeltaRow.Select(f => new Fraction(f.Numerator, f.Denominator)).ToArray() : null,
                ThetaRow = ThetaRow != null ? new List<string>(ThetaRow) : new List<string>()
            };
        }
    }
}

