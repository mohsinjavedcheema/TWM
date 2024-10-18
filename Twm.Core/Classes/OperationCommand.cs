using System;
using System.Windows.Input;

namespace Twm.Core.Classes
{
    public class OperationCommand : ICommand
    {
        public bool CantExecute
        {
            get { return CanExecute(null); }
        }

        readonly Action<object> _executeAction;
        readonly Func<object, bool> _canExecute;


        public event EventHandler CanExecuteChanged;


        public OperationCommand(Action<object> executeAction)
            : this(executeAction, null)
        {
        }

        public OperationCommand(Action<object> executeAction, Func<object, bool> canExecute = null)
        {
            _executeAction = executeAction;
            _canExecute = canExecute;

        }

        public OperationCommand(ICommand optionsCommand)
        {
            throw new NotImplementedException();
        }


        public bool CanExecute(object parameter)
        {
            bool result = true;
            Func<object, bool> canExecuteHandler = this._canExecute;
            if (canExecuteHandler != null)
            {
                result = canExecuteHandler(parameter);
            }

            return result;
        }

        public void RaiseCanExecuteChanged()
        {
            EventHandler handler = this.CanExecuteChanged;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        public void Execute(object parameter)
        {
            
            _executeAction(parameter);
        }


    }
}