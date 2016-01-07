using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Arctos.Game.Model
{
    public class PropertyChangedBase : INotifyPropertyChanged, ICommand
    {
        private ICommand mUpdater;
        public ICommand UpdateCommand
        {
            get
            {
                if (mUpdater == null) mUpdater = this;
                return this;
            }
            set { mUpdater = value; }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public virtual void Execute(object parameter)
        {
            throw new NotImplementedException();
        }

        public event EventHandler CanExecuteChanged;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}