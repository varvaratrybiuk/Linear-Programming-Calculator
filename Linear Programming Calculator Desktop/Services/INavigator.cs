namespace Linear_Programming_Calculator_Desktop.Services
{
    public interface INavigator<TViewModel>
    {
        void Navigate();
    }

    public interface INavigator<TViewModel, TParameter>
    {
        void Navigate(TParameter parameter);
    }
}
