using System;
using System.Linq;
using Arctos.Game.Model;

namespace ArctosGameServer.Domain
{
    public class Player
    {
        private bool _pause;

        public string Name { get; set; }

        public Guid ControlUnitId { get; set; }

        public Guid GuiId { get; set; }

        public GameArea Map { get; set; }
        public Area Location { get; set; }
        public Area LastVisited { get; set; }
        public bool FinishedGame { get; set; }
        public DateTime StartTime { get; set; }
        public TimeSpan Duration { get; set; }

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

        public bool Kicked { get; set; }

        public bool HasRecentlyFinished()
        {
            return Pause == false && FinishedGame == false && LastVisited != null &&
                   Map.Path[Map.Path.Count - 1].Equals(LastVisited);
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
                return null;
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
                    // When correctly passing a field, reset all wrongly passed field
                    foreach (var area in Map.AreaList)
                    {
                        area.Status = Area.AreaStatus.None;
                    }
                    foreach (var area in Map.Path)
                    {
                        if (area.Equals(Location))
                        {
                            break;
                        }

                        area.Status = Area.AreaStatus.CorrectlyPassed;
                    }

                    // Field is correctly passed
                    // Set last visited
                    LastVisited = pathArea;
                    pathArea.Status = Area.AreaStatus.CorrectlyPassed;

                    // Return the field
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

        /// <summary>
        /// Start the counter
        /// </summary>
        /// <param name="startTime"></param>
        public void Start(DateTime startTime)
        {
            StartTime = startTime;
            Duration = new TimeSpan(0, 0, 0, 0);
        }

        public void Reset()
        {
            Duration = new TimeSpan(0, 0, 0, 0);
            FinishedGame = false;
            LastVisited = null;
            Location = null;
            Map = null;
        }
    }
}