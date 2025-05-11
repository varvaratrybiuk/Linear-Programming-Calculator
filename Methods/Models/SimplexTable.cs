namespace Methods.Models
{
    public class SimplexTable
    {
        public Dictionary<string, string> RowVariables { get; set; }
        public Dictionary<string, string> ColumnVariables { get; set; }
        public double[,] Values { get; set; }
        public double[] DeltaRow { get; set; }

    }
}

