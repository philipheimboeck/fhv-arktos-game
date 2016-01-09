using System;
using System.Collections.Generic;

namespace Arctos.Game.Middleware.Logic.Model.Model
{
    public class Path
    {
        public List<GameEventTuple<int, int>> Waypoints { get; set; }
    }
}