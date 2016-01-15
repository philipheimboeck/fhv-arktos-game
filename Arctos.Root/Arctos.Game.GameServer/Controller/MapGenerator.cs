using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arctos.Game.Model;

namespace ArctosGameServer.Controller
{
    class MapGenerator
    {
        public GameArea GenerateMap()
        {
            // Todo: Make maps customable

            // Generate Map
            var areas = new List<Area>
            {
                new Area()
                {
                    AreaId = "420018D63E",
                    Column = 0,
                    Row = 0,
                    Status = Area.AreaStatus.None
                },
                new Area()
                {
                    AreaId = "420018DB3B",
                    Column = 1,
                    Row = 0,
                    Status = Area.AreaStatus.None
                },
                new Area()
                {
                    AreaId = "420018DB50",
                    Column = 2,
                    Row = 0,
                    Status = Area.AreaStatus.None
                },
                new Area()
                {
                    AreaId = "420013E5BA",
                    Column = 0,
                    Row = 1,
                    Status = Area.AreaStatus.None
                },
                new Area()
                {
                    AreaId = "420018DB45",
                    Column = 1,
                    Row = 1,
                    Status = Area.AreaStatus.None
                },
                new Area()
                {
                    AreaId = "420018D64D",
                    Column = 2,
                    Row = 1,
                    Status = Area.AreaStatus.None
                },
                new Area()
                {
                    AreaId = "420018D773",
                    Column = 0,
                    Row = 2,
                    Status = Area.AreaStatus.None
                },
                new Area()
                {
                    AreaId = "420014AA86",
                    Column = 1,
                    Row = 2,
                    Status = Area.AreaStatus.None
                },
                new Area()
                {
                    AreaId = "3D00997CB7",
                    Column = 2,
                    Row = 2,
                    Status = Area.AreaStatus.None
                },
            };

            var map = new GameArea()
            {
                Name = "Map 1",
                AreaList = areas,
                StartField = new Area()
                {
                    AreaId = "3D00997D02"
                }
            };

            return map;
        }
    }
}
