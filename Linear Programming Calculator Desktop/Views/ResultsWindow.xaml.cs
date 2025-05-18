using Methods.Enums;
using Methods.MathObjects;
using Methods.Models;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Linear_Programming_Calculator_Desktop
{
    /// <summary>
    /// Interaction logic for ResultsWindow.xaml
    /// </summary>
    public partial class ResultsWindow : Window
    {
        private SimplexHistory _history;
        private LinearProgrammingProblem _problem;
        public ResultsWindow(SimplexHistory history, LinearProgrammingProblem problem)
        {
            InitializeComponent();
            _history = history;
            _problem = problem;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ShowSolution(_history);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ShowSolution(SimplexHistory history)
        {
            try
            {
                SolutionPanel.Children.Clear();

                AddSectionToSolutionPanel("Математична модель", RenderProblem(history.InitialLinearProgrammingProblem));
                AddSectionToSolutionPanel($"Вводимо {history.FreeVariableProblem.SlackVariableCoefficients.Count} вільні змінні", 
                    RenderProblem(history.FreeVariableProblem));

                if (history.ArtificialProblemTable != null)
                {
                    AddSectionToSolutionPanel($"Вводимо {history.ArtificialProblemTable.ArtificialVariableCoefficients.Count} штучні змінні",
                        RenderProblem(history.ArtificialProblemTable));
                }

                var basisText = string.Join(", ", history.InitialBasis.Select(kvp => $"{kvp.Key} = {kvp.Value}"));
                AddSectionToSolutionPanel("Отримуємо початковий допустимий базисний розв’язок задачі", basisText);

                for (int stepIndex = 0; stepIndex < history.Steps.Count; stepIndex++)
                {
                    SolutionPanel.Children.Add(CreateHeader("Будуємо відповідну симплекс-таблицю"));
                    if (stepIndex == history.Steps.Count - 1)
                    {
                        var LastTableGrid = CreateSimplexTable(history.Steps[stepIndex].Table, null, null);
                        SolutionPanel.Children.Add(LastTableGrid);
                        continue;
                    }
                    var tableGrid = CreateSimplexTable(history.Steps[stepIndex].Table, history.Steps[stepIndex].PivotRow, history.Steps[stepIndex].PivotColumn);
                    SolutionPanel.Children.Add(CreateMathBlock($"Напрямний стовпець: A{history.Steps[stepIndex].PivotColumn}." +
                        $"\nНапрямнтй рядок: {history.Steps[stepIndex].Table.RowVariables.ElementAt(history.Steps[stepIndex].PivotRow).Key}." +
                        $"\nНапрямний елемент: {history.Steps[stepIndex].Table.RowVariables.ElementAt(history.Steps[stepIndex].PivotRow).Key}{history.Steps[stepIndex].PivotColumn}."));
                    SolutionPanel.Children.Add(tableGrid);
                }

                SolutionPanel.Children.Add(CreateHeader("Відповідь"));
                string mathString = "";
                var solution = history.Steps.LastOrDefault();
                for (int i = 0; i < _problem.ObjectiveFunctionCoefficients.Count; i++)
                {
                    string key = $"x{i + 1}";
                    int rowIndex = solution.Table.RowVariables.Keys.ToList().IndexOf(key);
                    var value = rowIndex >= 0 ? solution.Table.Values[rowIndex, 0] : Fractions.Fraction.Zero;
                    mathString += $"x{i + 1} = {value.ToString()}, ";
                }
                mathString += $"F{(_problem.IsMaximization ? "max" : "min")} = {solution.Table.DeltaRow[0]}";
                SolutionPanel.Children.Add(CreateMathBlock(mathString));
            }
            catch (Exception ex)
            {
                SolutionPanel.Children.Add(new TextBlock
                {
                    Text = $"Сталася помилка: {ex.Message}",
                    Foreground = Brushes.Red,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(5)
                });
            }
        }
        private void AddSectionToSolutionPanel(string header, string latex)
        {
            SolutionPanel.Children.Add(CreateHeader(header));
            SolutionPanel.Children.Add(CreateMathBlock(latex));
        }

        private TextBlock CreateHeader(string text) => new TextBlock
        {
            Text = text,
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(10, 20, 10, 5)
        };

        private TextBlock CreateMathBlock(string text) => new TextBlock
        {
            Text = text,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(10)
        };

        private string RenderProblem(LinearProgrammingProblem? problem)
        {
            if (problem == null) return string.Empty;

            var sb = new StringBuilder();

            var allVariables = new List<string>();
            allVariables.AddRange(problem.ObjectiveFunctionCoefficients);
            allVariables.AddRange(problem.SlackVariableCoefficients);
            allVariables.AddRange(problem.ArtificialVariableCoefficients);

            var variables = Enumerable.Range(1, allVariables.Count).Select(i => $"x{i}").ToList();

            sb.Append("F(" + string.Join(", ", variables) + ") = ");
            sb.Append(string.Join(" + ", allVariables.Select((c, i) => $"{c}x{i + 1}")));
            sb.Append(problem.IsMaximization ? " → max" : " → min");
            sb.AppendLine();

            foreach (var c in problem.Constraints)
            {
                var line = string.Join(" + ", c.Coefficients.Select((coef, i) => $"{coef}x{i + 1}"));
                var sign = c.Type switch
                {
                    ConstraintType.LessThanOrEqual => "≤",
                    ConstraintType.Equal => "=",
                    ConstraintType.GreaterThanOrEqual => "≥",
                    _ => "?"
                };
                sb.AppendLine($"{line} {sign} {c.RightHandSide}");
            }

            sb.AppendLine(string.Join(", ", variables.Select(v => $"{v} ≥ 0")));
            return sb.ToString();
        }

        private Border CreateSimplexTable(SimplexTable table, int? pivotRow, int? pivotCol)
        {
            var border = new Border
            {
                BorderThickness = new Thickness(2),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0xD9, 0x66)),
                CornerRadius = new CornerRadius(5),
            };
            var grid = new Grid
            {
                Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xF0)),
            };
            border.Child = grid;

            int rows = table.Values.GetLength(0) + 3;
            int cols = table.Values.GetLength(1) + 2;

            for (int i = 0; i < cols; i++) grid.ColumnDefinitions.Add(new ColumnDefinition());
            for (int i = 0; i < rows; i++) grid.RowDefinitions.Add(new RowDefinition());

            AddCell(grid, "C", 0, 1);
            for (int j = 0; j < table.ColumnVariables.Count; j++)
            {
                AddCell(grid, table.ColumnVariables.Values.ElementAt(j), 0, j + 2);
            }

            AddCell(grid, "B", 1, 1);
            for (int j = 0; j < table.ColumnVariables.Count; j++)
            {
                AddCell(grid, table.ColumnVariables.Keys.ElementAt(j), 1, j + 2);
            }

            for (int i = 0; i < table.Values.GetLength(0); i++)
            {
                AddCell(grid, table.RowVariables.Values.ElementAt(i), i + 2, 0);

                AddCell(grid, table.RowVariables.Keys.ElementAt(i), i + 2, 1);

                for (int j = 0; j < table.Values.GetLength(1); j++)
                {
                    TextBlock cell = cell = new TextBlock
                    {
                        Text = table.Values[i, j].ToString(),
                        Padding = new Thickness(5),
                        TextAlignment = TextAlignment.Center,
                    };
                    if (pivotRow == i && pivotCol == j)
                        cell.Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xBF, 0x00));
                    else if (pivotRow == i || pivotCol == j)
                        cell.Background = new SolidColorBrush(Color.FromRgb(0xF5, 0xDE, 0xB3));
                    else
                        cell.Background = Background = Brushes.Transparent;

                    Grid.SetRow(cell, i + 2);
                    Grid.SetColumn(cell, j + 2);
                    grid.Children.Add(cell);
                }
            }

            AddCell(grid, "∆", rows - 1, 1);
            for (int i = 0; i < table.DeltaRow.Length; i++)
            {
                var value = table.DeltaRow[i].ToString();
                AddCell(grid, value, rows - 1, i + 2);
            }

            return border;
        }

        private void AddCell(Grid grid, string text, int row, int col)
        {
            var tb = new TextBlock
            {
                Text = text,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(5),
            };

            Grid.SetRow(tb, row);
            Grid.SetColumn(tb, col);
            grid.Children.Add(tb);
        }


    }
}
