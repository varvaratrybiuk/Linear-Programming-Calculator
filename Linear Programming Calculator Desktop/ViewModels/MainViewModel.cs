using CommunityToolkit.Mvvm.ComponentModel;
using Linear_Programming_Calculator_Desktop.Stores;

namespace Linear_Programming_Calculator_Desktop.ViewModels
{
    /// <summary>
    /// Represents a base ViewModel for storing and handling navigation between ViewModels.
    /// </summary>
    public class MainViewModel : ObservableObject
    {
        /// <summary>
        /// Stores the current ViewModel instance.
        /// </summary>
        private readonly NavigationStore _navigationStore;

        /// <summary>
        /// Current ViewModel.
        /// </summary>
        public ObservableObject CurrentViewModel => _navigationStore.CurrentViewModel;

        /// <summary>
        /// Subscribes to event to update the current view model.
        /// </summary>
        /// <param name="navigationStore">Navigation store containing the current view model.</param>
        public MainViewModel(NavigationStore navigationStore)
        {
            _navigationStore = navigationStore;

            _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
        }

        /// <summary>
        /// Handles the event by notifying the UI that the view model has changed.
        /// </summary>
        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }
    }
}
