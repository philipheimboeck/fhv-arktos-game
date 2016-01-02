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
    public class Area : PropertyChangedBase
    {
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
                OnPropertyChanged("Color");
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
    }
}
