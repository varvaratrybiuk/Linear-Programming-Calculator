using Linear_Programming_Calculator_Desktop.Models;


namespace Linear_Programming_Calculator_Desktop.Stores
{
    public class LinearProgramInputStore
    {
        private LinearProgramInput _currentLinearProgramInput;
     
        public LinearProgramInput CurrentLinearProgramInput
        {
            get => _currentLinearProgramInput;
            set
            {
                _currentLinearProgramInput = value;
                CurrentLinearProgramInputChanged?.Invoke();
            }
        }

        public Action CurrentLinearProgramInputChanged;
    }
}
