namespace Methods.Models
{
    public class SimplexStep
    {
        public int PivotRow { get; set; }
        public int PivotColumn { get; set; }
        public required SimplexTable Table { get; set; }
    }
}
