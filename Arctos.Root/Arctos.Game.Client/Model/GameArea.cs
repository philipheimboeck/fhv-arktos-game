using System;
using System.Collections.ObjectModel;
using System.Linq;
using Arctos.Game.Client.Model;

namespace Arctos.Game.Client
{
    [Serializable]
    public class GameArea : PropertyChangedBase
    {
        /// <summary>
        /// GameArea Constructor
        /// </summary>
        public GameArea()
        {
            this.AreaList = new ObservableCollection<Area>();
        }

        /// <summary>
        /// All available areas on this game field
        /// </summary>
        public ObservableCollection<Area> AreaList { get; set; }

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
        /// Height of each field
        /// </summary>
        public int AreaHeight { get; set; }

        /// <summary>
        /// Width of each field
        /// </summary>
        public int AreaWidth { get; set; }

        /// <summary>
        /// Game width
        /// </summary>
        public int GameWidth { get; set; }

        /// <summary>
        /// Game height
        /// </summary>
        public int GameHeight { get; set; }

        /// <summary>
        /// Game Map Name
        /// </summary>
        public string Name { get; set; }
    }
}
