using Fractions;
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
        private string _errorMessage;
        private List<GomoryHistory>? _gomoryHistory;
        private EquationInputWindow _quationInputWindow;


        public ResultsWindow(EquationInputWindow quationInputWindow, SimplexHistory simplexHistory, List<GomoryHistory>? gomoryHistory, string errorMessage = "")
        {
            InitializeComponent();
            _quationInputWindow = quationInputWindow;
            _history = simplexHistory;
            _errorMessage = errorMessage;
            _gomoryHistory = gomoryHistory;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var buttons = InitializeButtons();
                ButtonPanel.Children.Add(buttons.editProblem);
                ButtonPanel.Children.Add(buttons.newProblem);

                BuildSolution();
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

        private (Button editProblem, Button newProblem) InitializeButtons()
        {
            var editProblem = new Button()
            {
                Content = "Редагувати задачу",
                Name = "edit",
                Width = 120,
                Height = 30
            };

            editProblem.Click += (s, e) =>
            {
                _quationInputWindow.Show();
                Hide();
            };

            var newProblem = new Button()
            {
                Content = "Нова задача",
                Name = "newProblem",
                Width = 100,
                Height = 30
            };

            newProblem.Click += (s, e) =>
            {
                var startWindow = new StartWindow();
                startWindow.Show();
                Hide();
            };
            return (editProblem, newProblem);
        }
        private void BuildSolution()
        {
            SolutionPanel.Children.Clear();
            var isIntegerProblem = _gomoryHistory != null ? true : false;
            AddSectionToSolutionPanel("Математична модель", RenderProblem(_history.InitialLinearProgrammingProblem, isIntegerProblem));
            ShowSimplexSolution();
            if (_gomoryHistory != null)
            {
                SolutionPanel.Children.Add(CreateHeader("Пошук цілочисельного розв’язку (метод Гоморі)"));
                if (_gomoryHistory.Count > 0)
                {
                    SolutionPanel.Children.Add(CreateMathBlock(GenerateSolutionSummaryString(_history.OptimalTable) + " - не є цілочисельним"));
                    ShowGomorySolution();
                }
                else
                {
                    SolutionPanel.Children.Add(CreateMathBlock(GenerateSolutionSummaryString(_history.OptimalTable) + " - є цілочисельним"));
                }
            }
        }
        private void ShowGomorySolution()
        {
            for (int i = 0; i < _gomoryHistory.Count; i++)
            {
                var history = _gomoryHistory[i];
                AddSectionToSolutionPanel($"Крок {i + 1} методу Гоморі", string.Empty);
                var max = history.MaxValue;
                AddSectionToSolutionPanel("Найбільше дробове значення серед змінних",
                    $"x{max.Item1} = {max.Item2}");

                AddGomoryCut(history);

                for (int j = 0; j < history.Steps.Count; j++)
                {
                    var step = history.Steps[j];
                    SolutionPanel.Children.Add(CreateHeader("Будуємо відповідну симплекс-таблицю"));

                    bool isLastStep = j == history.Steps.Count - 1;

                    var table = CreateSimplexTable(
                        step.Table,
                        isLastStep ? null : step.PivotRow,
                        isLastStep ? null : step.PivotColumn,
                        showTheta: !isLastStep
                    );

                    if (step.PivotRow != -1 && step.PivotColumn != -1)
                    {
                        var pivotRowName = step.Table.RowVariables.Keys.ElementAt(step.PivotRow);
                        AddSectionToSolutionPanel("Визначаємо",
                            $"Напрямний рядок: {pivotRowName}.\n" +
                            $"Напрямний стовпець: A{step.PivotColumn}.\n" +
                            $"Напрямний елемент: {pivotRowName}{step.PivotColumn}.");
                    }

                    SolutionPanel.Children.Add(table);
                }
                SolutionPanel.Children.Add(CreateHeader("Отримали оптимальний розв'язок задачі"));

                if (i == _gomoryHistory.Count - 1)
                {
                    SolutionPanel.Children.Add(CreateMathBlock(GenerateSolutionSummaryString(history.Steps[history.Steps.Count - 1].Table)));
                    break;
                }

                var result = GenerateSolutionSummaryString(history.Steps[history.Steps.Count - 1].Table) + "- не є цілочисельним";
                SolutionPanel.Children.Add(CreateMathBlock(result));

            }
        }

        private void AddGomoryCut(GomoryHistory history)
        {
            var cut = history.Cut;
            string cutElements = string.Empty;
            foreach (var c in cut.Elements)
            {
                if (c.Value.Item2.Denominator != 1)
                    cutElements += $"\n{c.Key} = {c.Value.Item2} - {(c.Value.Item1.IsNegative ? $"({c.Value.Item1})" : $"{c.Value.Item1}")} = {c.Value.Item2 - c.Value.Item1}";
            }

            AddSectionToSolutionPanel("Визначимо дробові частини:", cutElements);
            var lhsParts = new List<string>();
            var rhsParts = new List<string>();
            Fraction constantTerm = Fraction.Zero;

            for (int k = 1; k < cut.Elements.Count; k++)
            {
                var element = cut.Elements.ElementAt(k);
                var variableIndex = k;
                var coeff = element.Value.Item2;
                if (coeff.Denominator != 1)
                    lhsParts.Add($"{coeff - element.Value.Item1}x{variableIndex}");
            }

            var constant = cut.Elements.ElementAt(0).Value.Item2 - cut.Elements.ElementAt(0).Value.Item1;

            var inequality = $"{string.Join(" + ", lhsParts)} ≥ {constant}";
            AddSectionToSolutionPanel("Запишемо правильне відсічення:", inequality);
            int artificialIndex = cut.Elements.Count;
            var equality = $"{string.Join(" + ", lhsParts)} - x{artificialIndex} = {constant}";
            AddSectionToSolutionPanel("Приводимо до рівності:", equality);

            rhsParts.Add($"x{artificialIndex}");

            for (int k = 1; k < cut.Elements.Count; k++)
            {
                var element = cut.Elements.ElementAt(k);
                var coeff = element.Value.Item2;
                if (coeff.Denominator != 1)
                    rhsParts.Add($"- {coeff - element.Value.Item1}x{k}");
            }

            var rhsExpression = $"{constant * -1} = {string.Join(" ", rhsParts)}";
            AddSectionToSolutionPanel("Перетворимо:", rhsExpression);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ShowSimplexSolution()
        {

            AddSectionToSolutionPanel($"Вводимо {_history.FreeVariableProblem?.SlackVariableCoefficients?.Count} вільн. змінні", 
                RenderProblem(_history.FreeVariableProblem, isEqual: true));

            if (_history.ArtificialProblemTable != null)
                AddSectionToSolutionPanel($"Вводимо {_history.ArtificialProblemTable?.ArtificialVariableCoefficients?.Count} штучн. змінні", 
                    RenderProblem(_history.ArtificialProblemTable, isEqual: true));

            var basisText = string.Join(", ", _history.InitialBasis.Select(kvp => $"{kvp.Key} = {kvp.Value}"));
            AddSectionToSolutionPanel("Отримуємо початковий допустимий базисний розв’язок задачі", basisText);

            for (int stepIndex = 0; stepIndex < _history.Steps.Count; stepIndex++)
            {
                SolutionPanel.Children.Add(CreateHeader("Будуємо відповідну симплекс-таблицю"));

                var tableGrid = CreateSimplexTable(_history.Steps[stepIndex].Table, _history.Steps[stepIndex].PivotRow, _history.Steps[stepIndex].PivotColumn);
                if (_history.Steps[stepIndex].PivotColumn != -1 && _history.Steps[stepIndex].PivotRow != -1)
                    SolutionPanel.Children.Add(CreateMathBlock($"Напрямний стовпець: A{_history.Steps[stepIndex].PivotColumn}." +
                    $"\nНапрямнтй рядок: {_history.Steps[stepIndex].Table.RowVariables.ElementAt(_history.Steps[stepIndex].PivotRow).Key}." +
                    $"\nНапрямний елемент: {_history.Steps[stepIndex].Table.RowVariables.ElementAt(_history.Steps[stepIndex].PivotRow).Key}{_history.Steps[stepIndex].PivotColumn}."));
                SolutionPanel.Children.Add(tableGrid);
            }
            if (_history.OptimalTable != null)
            {
                SolutionPanel.Children.Add(CreateHeader("Будуємо відповідну симплекс-таблицю"));
                var LastTableGrid = CreateSimplexTable(_history.OptimalTable, null, null);
                SolutionPanel.Children.Add(LastTableGrid);

                SolutionPanel.Children.Add(CreateHeader("Отримали оптимальний розв'язок задачі"));
                SolutionPanel.Children.Add(CreateMathBlock(GenerateSolutionSummaryString(_history.OptimalTable)));
            }
            SolutionPanel.Children.Add(new TextBlock
            {
                Text = $"{_errorMessage}",
                Foreground = Brushes.Red,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(5)
            });

        }
        private string GenerateSolutionSummaryString(SimplexTable solution)
        {
            string mathString = "";

            for (int i = 0; i < _history.InitialLinearProgrammingProblem.ObjectiveFunctionCoefficients.Count; i++)
            {
                string key = $"x{i + 1}";
                int rowIndex = solution.RowVariables.Keys.ToList().IndexOf(key);
                var value = rowIndex >= 0 ? solution.Values[rowIndex, 0] : Fraction.Zero;
                mathString += $"x{i + 1} = {value}, ";
            }

            mathString += $"F{(_history.InitialLinearProgrammingProblem.IsMaximization ? "max" : "min")} = {solution.DeltaRow[0]}";
            return mathString;
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
            Margin = new Thickness(5)
        };

        private TextBlock CreateMathBlock(string text) => new TextBlock
        {
            Text = text,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(5)
        };

        private string RenderProblem(LinearProgrammingProblem? problem, bool isIntegerProblem = false, bool isEqual = false)
        {
            if (problem == null) return string.Empty;

            var sb = new StringBuilder();

            var allVariables = new List<string>();
            allVariables.AddRange(problem.ObjectiveFunctionCoefficients);
            allVariables.AddRange(problem.SlackVariableCoefficients);
            allVariables.AddRange(problem.ArtificialVariableCoefficients);

            var variables = Enumerable.Range(1, allVariables.Count).Select(i => $"x{i}").ToList();

            sb.Append("F(" + string.Join(", ", variables) + ") = ");
            sb.Append(string.Join(" + ", allVariables.Select((c, i) =>
            {
                var coeffStr = c.StartsWith("-") ? $"({c})" : c;
                return $"{coeffStr}x{i + 1}";
            })));

            sb.Append(problem.IsMaximization ? " → max" : " → min");
            sb.AppendLine();
            sb.AppendLine();

            foreach (var c in problem.Constraints)
            {
                var line = string.Join(" + ", c.Coefficients.Select((coef, i) => {
                    var coeffStr = coef.StartsWith("-") ? $"({coef})" : coef;
                    return $"{coeffStr}x{i + 1}";
                }));
                var sign = "=";
                if (!isEqual)
                {
                    sign = c.Type switch
                    {
                        ConstraintType.LessThanOrEqual => "≤",
                        ConstraintType.Equal => "=",
                        ConstraintType.GreaterThanOrEqual => "≥",
                        _ => "?"
                    };
                }
                
                sb.AppendLine($"{line} {sign} {c.RightHandSide}");
            }

            sb.AppendLine(string.Join(", ", variables.Select(v => $"{v} ≥ 0")));
            if (isIntegerProblem)
                sb.AppendLine(string.Join(", ", variables) + " — цілі");
            return sb.ToString();
        }

        private Border CreateSimplexTable(SimplexTable table, int? pivotRow, int? pivotCol, bool showTheta = false)
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

            int rows = table.Values.GetLength(0) + 4 + (showTheta ? 2 : 0);
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
                        cell.Background = Brushes.Transparent;

                    Grid.SetRow(cell, i + 2);
                    Grid.SetColumn(cell, j + 2);
                    grid.Children.Add(cell);
                }
            }

            AddCell(grid, "∆", rows - 2, 1);
            for (int i = 0; i < table.DeltaRow.Length; i++)
            {
                var value = table.DeltaRow[i].ToString();
                AddCell(grid, value, rows - 2, i + 2);
            }
            if (showTheta)
            {
                AddCell(grid, "θ", rows - 1, 1);
                for (int i = 0; i < table.ThetaRow.Count; i++)
                {
                    string thetaValue = table.ThetaRow[i];
                    AddCell(grid, thetaValue, rows - 1, i + 3);
                }
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
