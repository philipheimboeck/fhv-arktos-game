using System;
using System.Linq;
using Arctos.Game.Model;

namespace ArctosGameServer.Domain
{
    public class Player
    {
        public string Name { get; set; }

        public Guid ControlUnitId { get; set; }

        public Guid GuiId { get; set; }

        public GameArea Map { get; set; }
        public Area Location { get; set; }
        public Area LastVisited { get; set; }
        public bool StopGame { get; set; }

        public bool HasFinished()
        {
            return StopGame != false && LastVisited != null && Map.Path[Map.Path.Count - 1].Equals(LastVisited);
        }
    }
}