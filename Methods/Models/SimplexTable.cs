using Fractions;

namespace Methods.Models
{
    public class SimplexTable : ICloneable
    {
        public Dictionary<string, string> RowVariables { get; set; }
        public Dictionary<string, string> ColumnVariables { get; set; }
        public Fraction[,] Values { get; set; }
        public Fraction[]? DeltaRow { get; set; }
        public List<string> TetaRow { get; set; }

        public object Clone()
        {
            return new SimplexTable
            {
                RowVariables = RowVariables != null ? new Dictionary<string, string>(RowVariables) : new Dictionary<string, string>(),
                ColumnVariables = ColumnVariables != null ? new Dictionary<string, string>(ColumnVariables) : new Dictionary<string, string>(),
                Values = (Fraction[,])Values.Clone(),
                DeltaRow = DeltaRow != null ? DeltaRow.Select(f => new Fraction(f.Numerator, f.Denominator)).ToArray() : null,
                TetaRow = TetaRow != null ? new List<string>(TetaRow) : new List<string>()
            };
        }

        private Fraction[,] CloneFractionArray(Fraction[,] source)
        {
            if (source == null)
                return null;

            int rows = source.GetLength(0);
            int cols = source.GetLength(1);
            var clone = new Fraction[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    clone[i, j] = new Fraction(source[i, j].Numerator, source[i, j].Denominator);
                }
            }

            return clone;
        }
    }
}

