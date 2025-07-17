using CommunityToolkit.Mvvm.ComponentModel;
using Linear_Programming_Calculator_Desktop.Stores;

namespace Linear_Programming_Calculator_Desktop.Services
{
    public class NavigationService<TViewModel> : INavigator
        where TViewModel : ObservableObject
    {
        private readonly NavigationStore _navigationStore;
        private readonly Func<TViewModel> _createViewModel;

        public NavigationService(NavigationStore navigationStore, Func<TViewModel> createViewModel)
        {
            _navigationStore = navigationStore;
            _createViewModel = createViewModel;
        }

        public void Navigate()
        {
            _navigationStore.CurrentViewModel = _createViewModel();
        }
    }

    public class NavigationService<TParameter, TViewModel> : INavigator<TParameter>
        where TViewModel : ObservableObject
    {
        private readonly NavigationStore _navigationStore;
        private readonly Func<TParameter, TViewModel> _createViewModelWithParam;

        public NavigationService(NavigationStore navigationStore, Func<TParameter, TViewModel> createViewModelWithParam)
        {
            _navigationStore = navigationStore;
            _createViewModelWithParam = createViewModelWithParam;
        }

        public void Navigate(TParameter parameter)
        {
            _navigationStore.CurrentViewModel = _createViewModelWithParam(parameter);
        }
    }
}
