namespace Linear_Programming_Calculator_Desktop.Services
{
    public interface INavigator
    {
        void Navigate();
    }

    public interface INavigator<T>
    {
        void Navigate(T parameter);
    }
}
