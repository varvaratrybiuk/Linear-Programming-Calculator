using CommunityToolkit.Mvvm.ComponentModel;
using Linear_Programming_Calculator_Desktop.Models;
using Methods.Models;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace Linear_Programming_Calculator_Desktop.ViewModels
{
    public partial class SimplexViewModel(SimplexStep simplexStep) : ObservableObject
    {
        public SimplexStep Step { get; } = simplexStep;
        public string PivotRowKey => Step.Table.RowVariables.ElementAtOrDefault(Step.PivotRow).Key ?? string.Empty;
        public ObservableCollection<SimplexCell> Cells { get; } = [];

        [ObservableProperty]
        private int _totalRows;

        [ObservableProperty]
        private int _totalCols;

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

            AddRowToCells("∆", Step.Table.DeltaRow!.Select(d => d.ToString()));


            if (Step.Table.ThetaRow.Count != 0)
            {
                Cells.Add(new SimplexCell { Text = "" });
                AddRowToCells("θ", Step.Table.ThetaRow);
            }

            return this;
        }

        private Brush GetCellBackground(int i, int j)
        {
            if (Step.PivotColumn == -1) return Brushes.Transparent;
            if (Step.PivotRow == i && Step.PivotColumn == j)
                return new SolidColorBrush(Color.FromRgb(0xFF, 0xBF, 0x00));
            if (Step.PivotRow == i || Step.PivotColumn == j)
                return new SolidColorBrush(Color.FromRgb(0xF5, 0xDE, 0xB3));
            return Brushes.Transparent;
        }


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
