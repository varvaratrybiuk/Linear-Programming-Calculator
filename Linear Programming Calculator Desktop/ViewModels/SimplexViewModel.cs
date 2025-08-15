using CommunityToolkit.Mvvm.ComponentModel;
using Linear_Programming_Calculator_Desktop.Models;
using Methods.Models;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace Linear_Programming_Calculator_Desktop.ViewModels
{
    /// <summary>
    /// Represents a ViewModel holding the table content and related information displayed in the View.
    /// </summary>
    /// <param name="simplexStep">Current step of the solved problem.</param>
    public partial class SimplexViewModel(SimplexStep simplexStep) : ObservableObject
    {
        /// <summary>
        /// Current step of the solved problem.
        /// </summary>
        public SimplexStep Step { get; } = simplexStep;
        /// <summary>
        /// Gets the key of the pivot row in the current table.
        /// </summary>
        public string PivotRowKey => Step.Table.RowVariables.ElementAtOrDefault(Step.PivotRow).Key ?? string.Empty;

        /// <summary>
        /// Collection of cells representing the current table.
        /// </summary>
        public ObservableCollection<SimplexCell> Cells { get; } = [];
        /// <summary>
        /// Total number of rows in the table.
        /// </summary>
        [ObservableProperty]
        private int _totalRows;

        /// <summary>
        /// Total number of columns in the table.
        /// </summary>
        [ObservableProperty]
        private int _totalCols;

        /// <summary>
        /// Loads the table data into a new <see cref="SimplexViewModel"/> instance.
        /// </summary>
        /// <returns>A new <see cref="SimplexViewModel"/> populated with the current table data.</returns>
        public SimplexViewModel LoadFromTable()
        {
            if (Step?.Table is null)
                return this;

            int rows = Step.Table.Values.GetLength(0) + 3 + (Step.Table.ThetaRow.Count != 0 ? 1 : 0);
            int cols = Step.Table.Values.GetLength(1) + 2;

            TotalRows = rows;
            TotalCols = cols;

            AddRowToCells("C", Step.Table.ColumnVariables.Values);

            AddRowToCells("B", Step.Table.ColumnVariables.Keys);

            for (int i = 0; i < Step.Table.Values.GetLength(0); i++)
            {
                Cells.Add(new SimplexCell { Text = Step.Table.RowVariables.Values.ToList()[i] });
                Cells.Add(new SimplexCell { Text = Step.Table.RowVariables.Keys.ToList()[i] });

                for (int j = 0; j < Step.Table.Values.GetLength(1); j++)
                {
                    var text = Step.Table.Values[i, j].ToString();
                    var bg = Brushes.Transparent;

                    Cells.Add(new SimplexCell { Text = text, Background = GetCellBackground(i, j) });
                }
            }

            AddRowToCells("∆", Step.Table.DeltaRow!.Select(d => d.ExpressionText.ToString()));


            if (Step.Table.ThetaRow.Count != 0)
            {
                Cells.Add(new SimplexCell { Text = "" });
                AddRowToCells("θ", Step.Table.ThetaRow);
            }

            return this;
        }
        /// <summary>
        /// Determines the background brush for a specific cell in the table.
        /// </summary>
        /// <param name="i">The row index of the cell.</param>
        /// <param name="j">The column index of the cell.</param>
        /// <returns>A <see cref="Brush"/> indicating the background color of a cell, highlighting pivot rows, columns, and elements.</returns>
        private Brush GetCellBackground(int i, int j)
        {
            if (Step.PivotColumn == -1) return Brushes.Transparent;
            if (Step.PivotRow == i && Step.PivotColumn == j)
                return new SolidColorBrush(Color.FromRgb(0xFF, 0xBF, 0x00));
            if (Step.PivotRow == i || Step.PivotColumn == j)
                return new SolidColorBrush(Color.FromRgb(0xF5, 0xDE, 0xB3));
            return Brushes.Transparent;
        }

        /// <summary>
        /// Adds a row of cells to the current table representation with a label and a list of values.
        /// </summary>
        /// <param name="label1">The label for the row.</param>
        /// <param name="values">The collection of values to populate the row cells.</param>
        private void AddRowToCells(string label1, IEnumerable<string> values)
        {
            Cells.Add(new SimplexCell { Text = "" });
            Cells.Add(new SimplexCell { Text = label1 });

            foreach (var value in values)
            {
                Cells.Add(new SimplexCell { Text = value });
            }
        }
    }
}
