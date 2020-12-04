using System;
using System.Windows.Input;

namespace NoesisApp
{
    public class DelegateCommand : ICommand
    {
        public DelegateCommand(Action<object> execute)
        {
            _execute = execute ?? throw new ArgumentNullException("execute");
        }

        public DelegateCommand(Func<object, bool> canExecute, Action<object> execute)
        {
            _canExecute = canExecute ?? throw new ArgumentNullException("canExecute");
            _execute = execute ?? throw new ArgumentNullException("execute");
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, System.EventArgs.Empty);
        }

        #region Private members
        private Func<object, bool> _canExecute;
        private Action<object> _execute;
        #endregion
    }
}
