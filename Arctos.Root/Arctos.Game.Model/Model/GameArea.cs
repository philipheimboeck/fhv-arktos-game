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
        public List<Area> AreaList { get; set; }

        public List<Area> Path { get; set; }

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

        public Area StartField { get; set; }

        public void setPath(List<Tuple<int, int>> path)
        {
            List<Area> newPath = new List<Area>();

            foreach(var tuple in path)
            {
                newPath.Add(AreaList.Find(x => x.Column == tuple.Item1 && x.Row == tuple.Item2));
            }

            Path = newPath;
        }
    }
}