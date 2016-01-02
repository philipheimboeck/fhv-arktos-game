using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Arctos.Annotations;

namespace Arctos.Game.Client.Model
{
    /// <summary>
    /// The Area indicates where the robot has to drive
    /// and where it already was
    /// </summary>
    public class Area : INotifyPropertyChanged
    {
        public Area()
        {
            
        }

        public int Row { get; set; }
        public int Column { get; set; }

        private bool isActive;

        public bool IsActive
        {
            get
            {
                return isActive;
            }
            set
            {
                isActive = value;
                OnPropertyChanged();
            }
        }

        public SolidColorBrush Color
        {
            get {
                return new SolidColorBrush(IsActive ? Colors.Red : Colors.Gray);
            }
        }

        /// <summary>
        /// AreaID
        /// </summary>
        private string _areaId;
        public string AreaID
        {
            get { return _areaId; }
            set { _areaId = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
