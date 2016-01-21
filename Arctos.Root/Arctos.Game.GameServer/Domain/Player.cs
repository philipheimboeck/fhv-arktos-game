using System;
using System.Linq;
using Arctos.Game.Model;

namespace ArctosGameServer.Domain
{
    public class Player
    {
        public Player()
        {
            Duration = new TimeSpan(0, 0, 0, 0);
        }

        public string Name { get; set; }

        public Guid ControlUnitId { get; set; }

        public Guid GuiId { get; set; }

        public GameArea Map { get; set; }
        public Area Location { get; set; }
        public Area LastVisited { get; set; }
        public bool FinishedGame { get; set; }
        public DateTime StartTime { get; set; }
        public TimeSpan Duration { get; set; }

        private bool _pause;
        public bool Pause
        {
            get { return _pause; }
            set
            {
                _pause = value;

                if (_pause)
                {
                    // Start the pause by stopping the counter
                    EndCounter();
                }
                else
                {
                    // Stop the pause
                    StartTime = DateTime.Now;
                }
               
                

            }
        }

        public bool HasRecentlyFinished()
        {
            return Pause == false && FinishedGame == false && LastVisited != null && Map.Path[Map.Path.Count - 1].Equals(LastVisited);
        }

        public TimeSpan EndCounter()
        {
            Duration = Duration.Add(DateTime.Now - StartTime);

            return Duration;
        }

        /// <summary>
        /// Update the Position of the Player
        /// </summary>
        /// <param name="areaId"></param>
        /// <returns></returns>
        public Area UpdatePosition(string areaId)
        {
            // Return the same location when paused
            if (Pause)
            {
                return Location;
            }

            // Update location
            Location = Map.StartField.AreaId.Equals(areaId)
                ? Map.StartField
                : Map.AreaList.FirstOrDefault(x => x.AreaId.Equals(areaId));

            return Location;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="areaId"></param>
        /// <returns>The Area with its new status</returns>
        public Area ChangePositionStatus(string areaId)
        {
            // Return the same location when paused
            if (Pause)
            {
                return Location;
            }

            // Check if location was passed correctly
            var lastVisitedIndex = LastVisited != null
                ? Map.Path.IndexOf(Map.Path.FirstOrDefault(x => x.Equals(LastVisited)))
                : -1;

            // Is passed field the next one in the path?
            var pathArea = Map.Path.FirstOrDefault(x => x.AreaId.Equals(areaId));
            if (pathArea != null)
            {
                if (Map.Path.IndexOf(pathArea) == lastVisitedIndex + 1)
                {
                    // Field is correctly passed
                    // Set last visited
                    LastVisited = pathArea;
                    pathArea.Status = Area.AreaStatus.CorrectlyPassed;

                    return pathArea;
                }

                // Field is in path, but not in that order
                pathArea.Status = Area.AreaStatus.WronglyPassed;

                return pathArea;
            }

            // Field not in path and therefore wrongly passed!
            Location.Status = Area.AreaStatus.WronglyPassed;
            return Location;
        }
    }
}