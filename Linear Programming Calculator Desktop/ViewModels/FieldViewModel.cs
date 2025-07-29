using CommunityToolkit.Mvvm.ComponentModel;
using Linear_Programming_Calculator_Desktop.Attributes;

namespace Linear_Programming_Calculator_Desktop.ViewModels
{
    public partial class FieldViewModel : ObservableValidator
    {
        public required string Label { get; set; }

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [NumericOnly]
        private string _value = "0";

    }
}
