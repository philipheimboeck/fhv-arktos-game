using System;
using System.Linq;
using System.Windows;
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
        public bool FinishedGame { get; set; }
        public DateTime StartTime { get; set; }
        public TimeSpan Duration { get; set; }

        public Player()
        {
            Duration = new TimeSpan(0, 0, 0, 0);
        }

        public bool HasRecentlyFinished()
        {
            return FinishedGame != false && LastVisited != null && Map.Path[Map.Path.Count - 1].Equals(LastVisited);
        }

        public TimeSpan EndCounter()
        {
            Duration = Duration.Add(DateTime.Now - StartTime);

            return Duration;
        }
    }
}