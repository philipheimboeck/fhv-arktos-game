using System.Collections.Generic;
using System.Linq;
using Arctos.Game.Client.Model;

namespace Arctos.Game.Client
{
    public class GameArea
    {
        public List<Area> AreaList { get; set; }

        public int Rows
        {
            get
            {
                if (AreaList != null && AreaList.Count > 0)
                {
                    return AreaList.Max(x => x.Row);
                }
                return 0;
            }
        }

        public int Columns
        {
            get
            {
                if (AreaList != null && AreaList.Count > 0)
                {
                    return AreaList.Max(x => x.Column);
                }
                return 0;
            }
        }

        public int AreaHeight { get; set; }
        public int AreaWidth { get; set; }

        public int GameWidth { get; set; }
        public int GameHeight { get; set; }

        /// <summary>
        /// GameArea Constructor
        /// </summary>
        public GameArea()
        {
            this.AreaList = new List<Area>();
        }

    }
}
