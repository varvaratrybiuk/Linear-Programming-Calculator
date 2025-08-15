namespace Linear_Programming_Calculator_Desktop.Services
{
    /// <summary>
    /// Interface for Navigator without parameters.
    /// </summary>
    /// <typeparam name="TViewModel">The type of ViewModel to navigate to.</typeparam>
    public interface INavigator<TViewModel>
    {
        /// <summary>
        /// Used to navigate to target View Model.
        /// </summary>
        void Navigate();
    }
    /// <summary>
    /// Interface for Navigator with parameters.
    /// </summary>
    /// <typeparam name="TViewModel">The type of ViewModel to navigate to.</typeparam>
    /// <typeparam name="TParameter">Parameters to be passed.</typeparam>
    public interface INavigator<TViewModel, TParameter>
    {
        /// <summary>
        /// Used to navigate to target View Model with parameters.
        /// </summary>
        void Navigate(TParameter parameter);
    }
}
