using CommunityToolkit.Mvvm.ComponentModel;
using Linear_Programming_Calculator_Desktop.Attributes;

namespace Linear_Programming_Calculator_Desktop.ViewModels
{
    /// <summary>
    /// Represents a ViewModel that contains a single field for user input.
    /// </summary>
    public partial class FieldViewModel : ObservableValidator
    {
        /// <summary>
        /// The label displayed for the field.
        /// </summary>
        public required string Label { get; set; }

        /// <summary>
        /// The numeric value of the field as a string.
        /// </summary>
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [NumericOnly]
        private string _value = "0";

    }
}
