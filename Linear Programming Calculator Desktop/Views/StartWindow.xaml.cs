using System.Windows;
using System.Windows.Controls;

namespace Linear_Programming_Calculator_Desktop
{
    /// <summary>
    /// Interaction logic for StartWindow.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        public StartWindow()
        {
            InitializeComponent();
        }

        private void ValidateInputs()
        {
            bool isVariablesValid = true;
            bool isConstraintsValid = true;

            variablesErrorLabel.Content = "";
            constraintsErrorLabel.Content = "";

            if (string.IsNullOrWhiteSpace(variables.Text))
            {
                variablesErrorLabel.Content = "Поле обов'язкове";
                isVariablesValid = false;
            }
            else if (!int.TryParse(variables.Text, out _))
            {
                variablesErrorLabel.Content = "Введіть ціле число";
                isVariablesValid = false;
            }

            if (string.IsNullOrWhiteSpace(constraints.Text))
            {
                constraintsErrorLabel.Content = "Поле обов'язкове";
                isConstraintsValid = false;
            }
            else if (!int.TryParse(constraints.Text, out _))
            {
                constraintsErrorLabel.Content = "Введіть ціле число";
                isConstraintsValid = false;
            }

            generate.IsEnabled = isVariablesValid && isConstraintsValid;
        }

        private void numericInputCheck_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateInputs();
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void generate_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(variables.Text, out int variableCount) && int.TryParse(constraints.Text, out int constraintCount))
            {
                var window = new EquationInputWindow(variableCount, constraintCount);
                window.Show();
                Hide();
            }
        }
    }
}
