using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace GameGui
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

        /// <summary>
        /// AreaID
        /// </summary>
        private string _areaId;
        public string AreaID
        {
            get { return _areaId; }
            set { _areaId = value; NotifyPropertyChanged("AreaID"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}
