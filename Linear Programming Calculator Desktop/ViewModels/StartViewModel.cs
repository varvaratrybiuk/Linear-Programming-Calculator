using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Linear_Programming_Calculator_Desktop.Attributes;
using Linear_Programming_Calculator_Desktop.Services;

namespace Linear_Programming_Calculator_Desktop.ViewModels
{
    public partial class StartViewModel(INavigator<(int variables, int constraints)> navigationService) : ObservableValidator
    {
        [ObservableProperty]
        [ValidVariableCount(2)]
        private int _variables;

        [ObservableProperty]
        [ValidVariableCount(2)]
        private int _constraints;

        private readonly INavigator<(int variables, int constraints)> _navigationService = navigationService;

        [RelayCommand]
        public void GenerateExample() => _navigationService.Navigate((Variables, Constraints));

    }
}
