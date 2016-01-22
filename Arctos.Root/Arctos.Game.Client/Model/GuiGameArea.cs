using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;
using Arctos.Game.Model;

namespace Arctos.Game.GUIClient
{
    [Serializable]
    public class GuiGameArea : PropertyChangedBase
    {
        /// <summary>
        /// GuiGameArea Constructor
        /// </summary>
        public GuiGameArea()
        {
            this.AreaList = new ObservableCollection<GuiArea>();
            this.Path = new List<GuiArea>();
        }

        /// <summary>
        /// Create a new GuiGameArea instance based on a GameArea instance
        /// </summary>
        /// <param name="gameArea"></param>
        public GuiGameArea(GameArea gameArea)
        {
            this.AreaList = new ObservableCollection<GuiArea>();
            this.Path = new List<GuiArea>();

            if (gameArea != null) 
            {
                if (gameArea.AreaList != null)
                {
                    foreach (var area in gameArea.AreaList)
                    {
                        GuiArea guiArea = new GuiArea(area);
                        AreaList.Add(guiArea);
                    }
                }

                if (gameArea.Path != null)
                {
                    foreach (var pathArea in gameArea.Path)
                    {
                        Path.Add(AreaList.FirstOrDefault(x => x.AreaId == pathArea.AreaId));
                    }
                }
            }
        }

        /// <summary>
        /// Path to walk
        /// </summary>
        public List<GuiArea> Path { get; set; }

        /// <summary>
        /// Set Path
        /// </summary>
        /// <param name="path"></param>
        public void SetPath(List<Tuple<int, int>> path)
        {
            List<GuiArea> newPath = path.Select(tuple => AreaList.FirstOrDefault(x => x.Row == tuple.Item2 && x.Column == tuple.Item1)).ToList();
            Path = newPath;
        }

        /// <summary>
        /// All available areas on this game field
        /// </summary>
        [XmlElement]
        public ObservableCollection<GuiArea> AreaList { get; set; }

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
        /// GUIGameInstance width
        /// </summary>
        public int GameWidth { get; set; }

        /// <summary>
        /// GUIGameInstance height
        /// </summary>
        public int GameHeight { get; set; }

        /// <summary>
        /// GUIGameInstance Map Name
        /// </summary>
        public string Name { get; set; }
    }
}