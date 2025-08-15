using CommunityToolkit.Mvvm.ComponentModel;

namespace Linear_Programming_Calculator_Desktop.Stores
{
    /// <summary>
    /// Stores the current active ViewModel in the application
    /// and notifies subscribers when the current ViewModel changes.
    /// </summary>
    public class NavigationStore
    {
        /// <summary>
        /// Backing field for the current ViewModel.
        /// </summary>
        private ObservableObject _currentViewModel;
        /// <summary>
        /// Current ViewModel.
        /// </summary>
        /// <remarks>
        /// Raises the <see cref="CurrentViewModelChanged"/> event.
        /// </remarks>
        public ObservableObject CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;
                OnCurrentViewModelChanged();
            }
        }
        /// <summary>
        /// Event that is raised when the current ViewModel changes.
        /// </summary>
        public event Action CurrentViewModelChanged;

        /// <summary>
        /// Raises the <see cref="CurrentViewModelChanged"/> event.
        /// </summary>
        private void OnCurrentViewModelChanged()
        {
            CurrentViewModelChanged?.Invoke();
        }
    }
}
