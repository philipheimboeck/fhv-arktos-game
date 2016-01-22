using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Arctos.Game.Middleware.Logic.Model.Model;
using Arctos.Game.Model;

namespace ArctosGameServer.Domain
{
    internal class Game
    {
        private readonly GameConfiguration _configuration;

        public Game(GameConfiguration configuration)
        {
            _configuration = configuration;

            Reset();
        }

        public int GameWidth
        {
            get { return _configuration.Columns; }
        }

        public int GameHeight { get { return _configuration.Rows; } }

        public GameState State { get; set; }

        private List<GameArea> PlayableMaps { get; set; }

        public List<Tuple<int, int>> Path { get; private set; }

        /// <summary>
        /// Creates a random path through the fields
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private void CreatePath()
        {
            var path = new List<Tuple<int, int>>();
            var r = new Random();

            // Add start field
            var current = new Tuple<int, int>(0, r.Next(0, GameHeight - 1));
            path.Add(current);

            var direction = 0; // 0 -> Right, 1 -> Top, 2 -> Bottom

            // Create new fields until the right side is reached
            while (current.Item1 < GameHeight - 1)
            {
                // Check for possible directions
                bool[] possibleDirections = { true, true, true };

                // Top is not possible when there is no top field or when the player went bottom last time
                possibleDirections[1] = current.Item2 > 0 && direction != 2;

                // Bottom is not possible when there is no bottom field or when the player went top last time
                possibleDirections[2] = current.Item2 < GameHeight - 1 && direction != 1;

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
                        posX++;
                        break;
                    case 1:
                        posY--;
                        break;
                    case 2:
                        posY++;
                        break;
                }
                current = new Tuple<int, int>(posX, posY);
                path.Add(current);
            }

            // Set path property
            Path = path;
        }

        /// <summary>
        /// Returns the next map
        /// </summary>
        /// <returns></returns>
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

        public void AddMap(GameArea map)
        {
            PlayableMaps.Add(map);
        }

        /// <summary>
        /// Resets the game state to its initial state
        /// </summary>
        public void Reset()
        {
            State = GameState.Waiting;

            // Add all maps
            PlayableMaps = new List<GameArea>();
            foreach (var gameArea in _configuration.GameAreas)
            {
                PlayableMaps.Add(gameArea);
            }

            // Create a new path
            CreatePath();
        }
    }
}