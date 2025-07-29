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
        public ObservableCollection<SimplexCell> Cells { get; } = new();

        [ObservableProperty]
        private int _totalRows;

        [ObservableProperty]
        private int _totalCols;


        public SimplexViewModel LoadFromTable()
        {
            int rows = Step.Table.Values.GetLength(0) + 4 + (Step.Table.ThetaRow.Count != 0 ? 2 : 0);
            int cols = Step.Table.Values.GetLength(1) + 2;

            TotalRows = rows;
            TotalCols = cols;

            Cells.Add(new SimplexCell { Text = "" });
            Cells.Add(new SimplexCell { Text = "C" });
            for (int j = 0; j < Step.Table.ColumnVariables.Count; j++)
                Cells.Add(new SimplexCell { Text = Step.Table.ColumnVariables.Values.ElementAt(j) });

            Cells.Add(new SimplexCell { Text = "" });
            Cells.Add(new SimplexCell { Text = "B" });
            for (int j = 0; j < Step.Table.ColumnVariables.Count; j++)
                Cells.Add(new SimplexCell { Text = Step.Table.ColumnVariables.Keys.ElementAt(j) });

            for (int i = 0; i < Step.Table.Values.GetLength(0); i++)
            {
                Cells.Add(new SimplexCell { Text = Step.Table.RowVariables.Values.ElementAt(i) });
                Cells.Add(new SimplexCell { Text = Step.Table.RowVariables.Keys.ElementAt(i) });

                for (int j = 0; j < Step.Table.Values.GetLength(1); j++)
                {
                    var text = Step.Table.Values[i, j].ToString();
                    var bg = Brushes.Transparent;

                    if (Step.PivotColumn != 0)
                    {
                        if (Step.PivotRow == i && Step.PivotColumn == j)
                            bg = new SolidColorBrush(Color.FromRgb(0xFF, 0xBF, 0x00));
                        else if (Step.PivotRow == i || Step.PivotColumn == j)
                            bg = new SolidColorBrush(Color.FromRgb(0xF5, 0xDE, 0xB3));
                    }

                    Cells.Add(new SimplexCell { Text = text, Background = bg });
                }
            }

            Cells.Add(new SimplexCell { Text = "" });
            Cells.Add(new SimplexCell { Text = "∆" });
            for (int j = 0; j < Step.Table.DeltaRow?.Length; j++)
                Cells.Add(new SimplexCell { Text = Step.Table.DeltaRow[j].ToString() });

            if (Step.Table.ThetaRow.Count != 0)
            {
                Cells.Add(new SimplexCell { Text = "" });
                Cells.Add(new SimplexCell { Text = "θ" });

                Cells.Add(new SimplexCell { Text = "" });

                for (int j = 0; j < Step.Table.ThetaRow.Count; j++)
                    Cells.Add(new SimplexCell { Text = Step.Table.ThetaRow[j] });
            }

            return this;
        }
    }
}
