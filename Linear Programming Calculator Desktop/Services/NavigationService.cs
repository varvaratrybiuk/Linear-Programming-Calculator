using CommunityToolkit.Mvvm.ComponentModel;
using Linear_Programming_Calculator_Desktop.Stores;

namespace Linear_Programming_Calculator_Desktop.Services
{
    public class NavigationService<TViewModel>(NavigationStore navigationStore, Func<TViewModel> createViewModel) : INavigator<TViewModel>
        where TViewModel : ObservableObject
    {
        private readonly NavigationStore _navigationStore = navigationStore;
        private readonly Func<TViewModel> _createViewModel = createViewModel;

        public void Navigate()
        {
            _navigationStore.CurrentViewModel = _createViewModel();
        }
    }

    public class NavigationService<TViewModel, TParameter>(NavigationStore navigationStore, Func<TParameter, TViewModel> createViewModelWithParam) : INavigator<TViewModel, TParameter>
        where TViewModel : ObservableObject
    {
        private readonly NavigationStore _navigationStore = navigationStore;
        private readonly Func<TParameter, TViewModel> _createViewModelWithParam = createViewModelWithParam;

        public void Navigate(TParameter parameter)
        {
            _navigationStore.CurrentViewModel = _createViewModelWithParam(parameter);
        }
    }
}
