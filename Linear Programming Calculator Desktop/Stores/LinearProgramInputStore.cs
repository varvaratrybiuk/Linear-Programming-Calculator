using Linear_Programming_Calculator_Desktop.Models;


namespace Linear_Programming_Calculator_Desktop.Stores
{
    /// <summary>
    /// Stores the current input data for a LPP
    /// and notifies subscribers when the input changes.
    /// </summary>
    public class LinearProgramInputStore
    {
        /// <summary>
        /// Backing field for the current linear program input.
        /// </summary>
        private LinearProgramInput _currentLinearProgramInput;

        /// <summary>
        /// Current linear program input.
        /// </summary>
        /// <remarks>
        /// Invokes the <see cref="CurrentLinearProgramInputChanged"/> event when set.
        /// </remarks>
        public LinearProgramInput CurrentLinearProgramInput
        {
            get => _currentLinearProgramInput;
            set
            {
                _currentLinearProgramInput = value;
                CurrentLinearProgramInputChanged?.Invoke();
            }
        }
        /// <summary>
        /// Event that is raised when the current linear program input changes.
        /// </summary>
        public event Action CurrentLinearProgramInputChanged;
    }
}
