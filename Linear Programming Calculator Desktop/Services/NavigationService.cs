using CommunityToolkit.Mvvm.ComponentModel;
using Linear_Programming_Calculator_Desktop.Stores;

namespace Linear_Programming_Calculator_Desktop.Services
{
    /// <summary>
    /// Represents a navigation service for navigating between ViewModels without parameters.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the ViewModel to navigate to. Must inherit from ObservableObject.</typeparam>
    /// <param name="navigationStore">The store that contains the current ViewModel instance.</param>
    /// <param name="createViewModel">Factory method to create a new instance of the target ViewModel.</param>
    public class NavigationService<TViewModel>(NavigationStore navigationStore, Func<TViewModel> createViewModel) : INavigator<TViewModel>
        where TViewModel : ObservableObject
    {
        /// <summary>
        /// Stores the current ViewModel instance.
        /// </summary>
        private readonly NavigationStore _navigationStore = navigationStore;
        /// <summary>
        /// Factory method to create a new instance of the target ViewModel.
        /// </summary>
        private readonly Func<TViewModel> _createViewModel = createViewModel;

        /// <summary>
        /// Navigates to the target ViewModel.
        /// </summary>
        public void Navigate()
        {
            _navigationStore.CurrentViewModel = _createViewModel();
        }
    }

    /// <summary>
    /// Represents a navigation service for navigating between ViewModels with parameters.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the ViewModel to navigate to. Must inherit from ObservableObject.</typeparam>
    /// <typeparam name="TParameter">The type of parameter to be passed during navigation.</typeparam>
    /// <param name="navigationStore">The store that contains the current ViewModel instance.</param>
    /// <param name="createViewModelWithParam">Factory method to create a new instance of the target ViewModel with parameters.</param>
    public class NavigationService<TViewModel, TParameter>(NavigationStore navigationStore, Func<TParameter, TViewModel> createViewModelWithParam) : INavigator<TViewModel, TParameter>
        where TViewModel : ObservableObject
    {
        /// <summary>
        /// Stores the current ViewModel instance.
        /// </summary>
        private readonly NavigationStore _navigationStore = navigationStore;
        /// <summary>
        /// Factory method to create a new instance of the target ViewModel with parameters.
        /// </summary>
        private readonly Func<TParameter, TViewModel> _createViewModelWithParam = createViewModelWithParam;
        /// <summary>
        /// Navigates to the target ViewModel with parameters.
        /// </summary>
        /// <param name="parameter">The parameter to pass to the target ViewModel.</param>
        public void Navigate(TParameter parameter)
        {
            _navigationStore.CurrentViewModel = _createViewModelWithParam(parameter);
        }
    }
}
