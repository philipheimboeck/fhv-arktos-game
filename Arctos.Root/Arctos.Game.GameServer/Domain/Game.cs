using System;
using System.Collections.Generic;
using System.Linq;
using Arctos.Game.Model;

namespace ArctosGameServer.Domain
{
    internal class Game
    {
        public Game()
        {
            PlayableMaps = new List<GameArea>();
        }

        public bool Ready { get; set; }

        public bool Started { get; set; }

        public bool Finished { get; set; }

        public List<GameArea> PlayableMaps { get; set; }

        /// <summary>
        /// Creates a random path through the fields
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public List<Tuple<int, int>> CreatePath(int width, int height)
        {
            var path = new List<Tuple<int, int>>();
            var r = new Random();

            // Add start field
            var current = new Tuple<int, int>(r.Next(0, width - 1), height - 1);
            path.Add(current);

            var direction = 0; // 0 -> Top, 1 -> Left, 2 -> Right

            // Create new fields until the top is reached
            while (current.Item2 > 0)
            {
                // Check for possible directions
                bool[] possibleDirections = {true, true, true};

                // Left is not possible when there is no left field or when the player went right last time
                possibleDirections[1] = current.Item1 > 0 && direction != 2;

                // Right is not possible when there is no right field or when the player went left last time
                possibleDirections[2] = current.Item1 < width - 1 && direction != 1;

                if (possibleDirections.Count(x => x == true) == 0)
                {
                    // No way found! Aborting!
                    throw new Exception("No way found!");
                }

                // Retrieve one possible direction
                do
                {
                    direction = r.Next(0, 3);
                } while (!possibleDirections[direction]);

                // Add new tuple
                var posX = current.Item1;
                var posY = current.Item2;

                switch (direction)
                {
                    case 0:
                        posY--;
                        break;
                    case 1:
                        posX--;
                        break;
                    case 2:
                        posX++;
                        break;
                }
                current = new Tuple<int, int>(posX, posY);
                path.Add(current);
            }

            return path;
        }

        public GameArea GetAvailableMap()
        {
            if (PlayableMaps.Count > 0)
            {
                var map = PlayableMaps[0];
                PlayableMaps.Remove(map);
                return map;
            }
            return null;
        }
    }
}