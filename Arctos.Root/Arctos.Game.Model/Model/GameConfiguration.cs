using System;
using System.Collections.Generic;

namespace Arctos.Game.Model
{
    [Serializable]
    public class GameConfiguration
    {

        public GameConfiguration()
        {

        }

        public GameConfiguration(int columns, int rows)
        {
            Columns = columns;
            Rows = rows;
            GameAreas = new List<GameArea>();
        }

        public int Columns { get; set; }

        public int Rows { get; set; }

        public List<GameArea> GameAreas { get; set; }
    }
}
