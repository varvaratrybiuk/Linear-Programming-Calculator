using Fractions;

namespace Methods.Models
{
    public class SimplexTable
    {
        public Dictionary<string, string> RowVariables { get; set; }
        public Dictionary<string, string> ColumnVariables { get; set; }
        public Fraction[,] Values { get; set; }
        public Fraction[] DeltaRow { get; set; }

        public List<string> TetaRow { get; set; }

    }
}

