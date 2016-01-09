using System;
using System.Windows.Media;
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

        public int Row { get; set; }
        public int Column { get; set; }

        private Area.AreaStatus status;
        public Area.AreaStatus Status
        {
            get { return status; }
            set
            {
                status = value;
                OnPropertyChanged();
                OnPropertyChanged("Color");
            }
        }

        public SolidColorBrush Color
        {
            get
            {
                Color color;

                switch (Status)
                {
                    case Area.AreaStatus.WronglyPassed:
                        color = Colors.Red;
                        break;
                    case Area.AreaStatus.CorrectlyPassed:
                        color = Colors.DarkGreen;
                        break;
                    case Area.AreaStatus.None:
                    default:
                        color = Colors.Gray;
                        break;
                }

                return new SolidColorBrush(color);
            }
        }

        public string AreaId
        {
            get { return _areaId; }
            set
            {
                _areaId = value;
                OnPropertyChanged();
            }
        }
    }
}