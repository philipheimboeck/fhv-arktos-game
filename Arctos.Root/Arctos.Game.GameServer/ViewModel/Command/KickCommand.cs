using System;
using System.Windows.Input;

namespace ArctosGameServer.ViewModel.Command
{
    public class KickCommand : ICommand
    {
        public delegate void KickAction();
        public KickAction Action { get; set; }

        public bool CanExecute(object parameter)
        {
            return Action != null;
        }

        public void Execute(object parameter)
        {
            Action();
        }

        public event EventHandler CanExecuteChanged;
    }
}