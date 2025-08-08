using CommunityToolkit.Mvvm.ComponentModel;
using Linear_Programming_Calculator_Desktop.Stores;

namespace Linear_Programming_Calculator_Desktop.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly NavigationStore _navigationStore;
        private readonly LinearProgramInputStore _linearProgramInputStore;

        public ObservableObject CurrentViewModel => _navigationStore.CurrentViewModel;

        public MainViewModel(NavigationStore navigationStore, LinearProgramInputStore inputStore)
        {
            _navigationStore = navigationStore;
            _linearProgramInputStore = inputStore;

            _linearProgramInputStore.CurrentLinearProgramInput = new();
            _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
        }

        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }
    }
}
