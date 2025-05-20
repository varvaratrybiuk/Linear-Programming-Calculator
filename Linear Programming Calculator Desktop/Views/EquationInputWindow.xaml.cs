using Methods.Enums;
using Methods.MathObjects;
using Methods.Solvers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Linear_Programming_Calculator_Desktop
{
    /// <summary>
    /// Interaction logic for EquationInputWindow.xaml
    /// </summary>
    public partial class EquationInputWindow : Window
    {
        private int _variables;
        private int _constraints;
        private bool _isMaximization = true;
        private Button _solve;
        private CheckBox _integerCheck;
        private Dictionary<TextBox, string> _objectiveFunctionValues = new Dictionary<TextBox, string>();
        private Dictionary<TextBox, string> _constraintValues = new Dictionary<TextBox, string>();
        private Dictionary<TextBox, string> _bValues = new Dictionary<TextBox, string>();
        private Dictionary<ComboBox, ConstraintType> _constraintSigns = new Dictionary<ComboBox, ConstraintType>();
        private List<Dictionary<TextBox, string>> _allDictionaries;

        public EquationInputWindow(int Variables, int Constraints)
        {
            InitializeComponent();
            InitializeDictionaries();
            _variables = Variables;
            _constraints = Constraints;
        }
        private void InitializeDictionaries()
        {
            _allDictionaries = new List<Dictionary<TextBox, string>>()
            {
                _bValues,
                _constraintValues,
                _objectiveFunctionValues
            };
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GenerateObjectiveSection();
            GenerateConstraintsSection();
        }
        private void NumberInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox textBox) return;

            if (!decimal.TryParse(textBox.Text, out _))
            {
                textBox.Foreground = Brushes.Crimson;
                _solve.IsEnabled = false;
                return;
            }

            textBox.ClearValue(ForegroundProperty);
            _solve.IsEnabled = true;

            foreach (var dict in _allDictionaries)
            {
                if (dict.ContainsKey(textBox))
                {
                    dict[textBox] = textBox.Text;
                    break;
                }
            }
        }
        private void GenerateObjectiveSection()
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(10) };

            for (int i = 1; i <= _variables; i++)
            {
                var label = new Label
                {
                    Content = $"x{i}",
                    VerticalAlignment = VerticalAlignment.Center
                };

                var textBox = new TextBox
                {
                    Width = 50,
                    Name = $"x{i}_objectFunc",
                };
                textBox.TextChanged += NumberInput_TextChanged;
                _objectiveFunctionValues[textBox] = string.Empty;
                panel.Children.Add(textBox);
                panel.Children.Add(label);


                if (i != _variables)
                {
                    var symbolLabel = new Label
                    {
                        Content = $" + ",
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    panel.Children.Add(symbolLabel);

                }
            }

            var arrowLabel = new Label
            {
                Content = "→",
                FontWeight = FontWeights.Bold,
                FontSize = 16,
                Margin = new Thickness(10, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            var goalSelector = new ComboBox
            {
                Width = 80,
                Height = 30,
                ItemsSource = new[] { "max", "min" },
                SelectedIndex = 0,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            goalSelector.SelectionChanged += (sender, args) =>
            {
                if (sender is not ComboBox comboBox) return;

                switch (comboBox.SelectedIndex)
                {
                    case 0:
                        _isMaximization = true;
                        break;
                    case 1:
                        _isMaximization = false;
                        break;
                }
            };

            panel.Children.Add(arrowLabel);
            panel.Children.Add(goalSelector);

            TopPanel.Children.Add(panel);
        }

        private void GenerateConstraintsSection()
        {
            var variablesName = string.Empty;
            for (int row = 0; row < _constraints; row++)
            {
                var constraintPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(10, 5, 10, 5),
                };

                for (int i = 1; i <= _variables; i++)
                {
                    var label = new Label
                    {
                        Content = $"x{i}",
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    var textBox = new TextBox
                    {
                        Name = $"x{i}_constraint",
                        Width = 50,
                    };
                    textBox.TextChanged += NumberInput_TextChanged;
                    _constraintValues[textBox] = string.Empty;
                    constraintPanel.Children.Add(textBox);
                    constraintPanel.Children.Add(label);

                    if (i != _variables)
                    {
                        var symbolLabel = new Label
                        {
                            Content = $" + ",
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        constraintPanel.Children.Add(symbolLabel);

                    }
                }

                var signCombo = new ComboBox
                {
                    Width = 60,
                    Height = 30,
                    ItemsSource = new[] { "≤", "≥", "=" },
                    SelectedIndex = 0,
                    Margin = new Thickness(10, 0, 5, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                };
                signCombo.SelectionChanged += signCombo_SelectionChanged;
                _constraintSigns[signCombo] = ConstraintType.LessThanOrEqual;

                var rhsTextBox = new TextBox
                {
                    Width = 60,
                    Height = 30,
                    Margin = new Thickness(5, 0, 0, 0)
                };
                rhsTextBox.TextChanged += NumberInput_TextChanged;
                _bValues[rhsTextBox] = string.Empty;
                constraintPanel.Children.Add(signCombo);
                constraintPanel.Children.Add(rhsTextBox);

                ConstraintPanel.Children.Add(constraintPanel);
            }

            for (int i = 0; i < _variables; i++)
            {
                variablesName += $"x{i + 1}";
                if (i < _variables - 1)
                    variablesName += ", ";
            }

            var integerCheck = new CheckBox
            {
                Content = variablesName + " - цілі",
            };
            _integerCheck = integerCheck;

            variablesName += "≥ 0";

            var exampleText = new TextBlock
            {
                Text = variablesName,
                Margin = new Thickness(5),
                FontStyle = FontStyles.Italic,
                FontWeight = FontWeights.SemiBold,
            };

            var textBlock = new TextBlock
            {
                Text = "Умови-обмеження",
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(5),
                TextAlignment = TextAlignment.Center,
            };
            var editButton = new Button
            {
                Content = "Змінити задачу",
                Name = "edit",
                Width = 100,
                Height = 30
            };
            editButton.Click += edit_Click;

            var button = new Button
            {
                Content = "Розв'язати",
                Name = "solve",
                Width = 100,
                Height = 30
            };
            _solve = button;
            button.Click += solve_Click;

            ConstraintPanel.Children.Add(exampleText);
            ConstraintPanel.Children.Add(integerCheck);
            TopPanel.Children.Add(button);
            TopPanel.Children.Add(editButton);
            TopPanel.Children.Add(textBlock);
        }

        private void edit_Click(object sender, RoutedEventArgs e)
        {
            var startWindow = new StartWindow();
            startWindow.Show();
            Hide();
        }

        private void signCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ComboBox comboBox) return;

            switch (comboBox.SelectedIndex)
            {
                case 0:
                    _constraintSigns[comboBox] = ConstraintType.LessThanOrEqual;
                    break;
                case 1:
                    _constraintSigns[comboBox] = ConstraintType.GreaterThanOrEqual;
                    break;
                case 2:
                    _constraintSigns[comboBox] = ConstraintType.Equal;
                    break;
            }
        }

        private async void solve_Click(object sender, RoutedEventArgs e)
        {
            EnsureTextBoxesHaveValues(_objectiveFunctionValues);
            EnsureTextBoxesHaveValues(_constraintValues);
            EnsureTextBoxesHaveValues(_bValues);

            var constraints = new List<Constraint>();
            GetVariablesFromConstraints(constraints);

            var problem = new LinearProgrammingProblem
            {
                IsMaximization = _isMaximization,
                ObjectiveFunctionCoefficients = _objectiveFunctionValues.Values.ToList(),
                Constraints = constraints
            };

            var solver = new SimplexSolver(problem);
            GomorySolver? gomory = null;
            Window? newResultWindow;

            try
            {
                solver.Solve();
                if (_integerCheck!.IsChecked == true)
                {
                    gomory = new GomorySolver(solver.GetSolution(), problem);
                    gomory.Solve();
                }
                newResultWindow = new ResultsWindow(this, solver.SimplexHistory, gomory?.GomoryHistory);
            }
            catch (InvalidOperationException ex)
            {
                var errorMessage = ex.Message;
                newResultWindow = new ResultsWindow(this, solver.SimplexHistory, gomory?.GomoryHistory, errorMessage: errorMessage);
            }

            newResultWindow.Show();
            Hide();

        }
        private void GetVariablesFromConstraints(List<Constraint> constr)
        {
            for (int constrIndex = 0; constrIndex < _constraints; constrIndex++)
            {
                var constraint = new Constraint();
                constraint.Coefficients = new List<string>();

                for (int varIndex = 0; varIndex < _variables; varIndex++)
                {
                    int keyIndex = constrIndex * _variables + varIndex;
                    var tb = _constraintValues.Values.ElementAt(keyIndex);
                    constraint.Coefficients.Add(tb);
                }
                var bTb = _bValues.Values.ElementAt(constrIndex);
                constraint.RightHandSide = bTb;

                constraint.Type = _constraintSigns.Values.ElementAt(constrIndex);
                constr.Add(constraint);
            }
        }
        private void EnsureTextBoxesHaveValues(Dictionary<TextBox, string> dictionary)
        {
            foreach (var kvp in dictionary.Keys.ToList())
            {
                if (string.IsNullOrWhiteSpace(kvp.Text))
                {
                    kvp.Text = "0";
                    dictionary[kvp] = "0";
                }
            }
        }
    }
}
