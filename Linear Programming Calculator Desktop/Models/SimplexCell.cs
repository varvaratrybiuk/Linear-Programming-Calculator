using System.Windows.Media;

namespace Linear_Programming_Calculator_Desktop.Models
{
    /// <summary>
    /// Represents a cell in the Simplex table.
    /// </summary>
    public class SimplexCell
    {
        /// <summary>
        /// The text displayed in the cell.
        /// </summary>
        public string Text { get; set; } = "";
        /// <summary>
        /// The background brush used to highlight the pivot row or column.
        /// </summary>
        public Brush Background { get; set; } = Brushes.Transparent;
    }
}
