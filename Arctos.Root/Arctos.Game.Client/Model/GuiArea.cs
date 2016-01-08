using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Arctos.Annotations;
using Arctos.Game.Model;

namespace Arctos.Game.GUIClient
{
    /// <summary>
    /// The GuiArea indicates where the robot has to drive
    /// and where it already was
    /// </summary>
    [Serializable]
    public class GuiArea : PropertyChangedBase
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

        public GuiArea()
        {
            
        }

        /// <summary>
        /// Create a new GuiArea instance based on an Area instance
        /// </summary>
        /// <param name="area"></param>
        public GuiArea(Area area)
        {
            AreaId = area.AreaId;
            Column = area.Column;
            Row = area.Row;
        }

        public string AreaId
        {
            get { return _areaId; }
            set { _areaId = value; OnPropertyChanged(); }
        }
    }
}
