using System;
using System.Collections.Generic;

namespace Arctos.Game.Middleware.Logic.Model.Model
{
    public class Path
    {
        public List<GameEventTuple<int, int>> Waypoints { get; set; }

        public Path()
        {
            
        }

        public Path(List<Tuple<int, int>> path)
        {
            var tuples = new List<GameEventTuple<int, int>>();
            foreach (var t in path)
            {
                tuples.Add(new GameEventTuple<int, int>() { Item1 = t.Item1, Item2 = t.Item2 });
            }
            Waypoints = tuples;
        }
    }
}