using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Arctos.Game.Model
{
    [Serializable]
    public class GameArea
    {
        /// <summary>
        /// GameArea Constructor
        /// </summary>
        public GameArea()
        {
            this.AreaList = new List<Area>();
        }

        /// <summary>
        /// All available areas on this game field
        /// </summary>
        [XmlElement]
        public List<Area> AreaList { get; set; }

        /// <summary>
        /// Get the amount of rows for the game
        /// </summary>
        public int GameRows
        {
            get
            {
                if (AreaList != null && AreaList.Count > 0)
                {
                    return AreaList.Max(x => x.Row) + 1;
                }
                return 0;
            }
        }

        /// <summary>
        /// Get the amount of columns for the game
        /// </summary>
        public int GameColumns
        {
            get
            {
                if (AreaList != null && AreaList.Count > 0)
                {
                    return AreaList.Max(x => x.Column) + 1;
                }
                return 0;
            }
        }

        /// <summary>
        /// Game Map Name
        /// </summary>
        public string Name { get; set; }
    }
}