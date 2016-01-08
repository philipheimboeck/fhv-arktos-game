using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Arctos.Game.Middleware.Logic.Model.Model;
using Arctos.Game.Model;
using ArctosGameServer.Domain;
using ArctosGameServer.Service;

namespace ArctosGameServer.Controller
{
    /// <summary>
    /// The GameController
    /// </summary>
    public class GameController : IObserver<Tuple<Guid, GameEvent>>
    {
        private readonly ConcurrentQueue<Tuple<Guid, GameEvent>> _receivedEvents =
            new ConcurrentQueue<Tuple<Guid, GameEvent>>();

        private List<GameArea> _playableMaps = new List<GameArea>();

        private Dictionary<string, Player> _players = new Dictionary<string, Player>();

        private GameTcpServer _server;

        public GameController(GameTcpServer server)
        {
            _server = server;

            GenerateGame();
        }

        public bool ShutdownRequested { get; set; }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(Tuple<Guid, GameEvent> value)
        {
            // Receive GameEvent
            _receivedEvents.Enqueue(value);
        }

        /// <summary>
        /// Generates a new map
        /// </summary>
        public void GenerateGame()
        {
            // Todo: Make maps customable
            int width = 10;
            int height = 10;

            // Generate path
            var path = createPath(width, height);

            // Generate Map
            var areas = new List<Area>();
            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    areas.Add(new Area()
                    {
                        AreaId = i + ":" + j,
                        Column = i,
                        Row = j,
                        IsActive = false
                    });
                }
            }

            var map = new GameArea()
            {
                Name = "Map 1",
                AreaList = areas
            };
            map.setPath(path);

            _playableMaps.Add(map);
        }

        /// <summary>
        /// Creates a random path through the fields
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private List<Tuple<int, int>> createPath(int width, int height)
        {
            List<Tuple<int, int>> path = new List<Tuple<int, int>>();
            Random r = new Random();

            // Add start field
            var current = new Tuple<int, int>(r.Next(0, width - 1), height - 1);
            path.Add(current);

            int direction = 0; // 0 -> Top, 1 -> Left, 2 -> Right

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
                int posX = current.Item1;
                int posY = current.Item2;

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

        public void Loop()
        {
            while (!ShutdownRequested)
            {
                // Process all received events
                Tuple<Guid, GameEvent> e = null;
                while (_receivedEvents.TryDequeue(out e))
                {
                    switch (e.Item2.EventType)
                    {
                        case GameEvent.Type.PlayerRequest:
                        {
                            var playerName = (string) e.Item2.Data;
                            PlayerRequest(e.Item1, playerName);
                        }
                            break;
                        case GameEvent.Type.PlayerJoined:
                            break;
                        case GameEvent.Type.PlayerLeft:
                            break;
                        case GameEvent.Type.GuiRequest:
                        {
                            var playerName = (string) e.Item2.Data;
                            GuiRequest(e.Item1, playerName);
                        }
                            break;
                        case GameEvent.Type.GuiJoined:
                            // Should never occur
                            break;
                        case GameEvent.Type.AreaUpdate:
                        {
                            var area = (string) e.Item2.Data;
                            UpdateArea(e.Item1, area);
                        }
                            break;
                    }
                }
            }
        }

        private void GuiRequest(Guid guid, string playerName)
        {
            // Player not existing?
            if (!_players.ContainsKey(playerName))
            {
                // Send NOT OK
                _server.Send(new GameEvent(GameEvent.Type.GuiJoined, null), guid);
                return;
            }

            // Add GUI-Guid to player
            _players[playerName].GuiId = guid;

            // Send OK
            _server.Send(new GameEvent(GameEvent.Type.GuiJoined, _players[playerName].Map), guid);
        }

        private void PlayerRequest(Guid guid, string playerName)
        {
            // Player already existing?
            if (_players.ContainsKey(playerName))
            {
                // Send NOT OK
                _server.Send(new GameEvent(GameEvent.Type.PlayerJoined, false), guid);
                return;
            }

            // Is a map available
            var map = InstantiateMap();
            if (map == null)
            {
                _server.Send(new GameEvent(GameEvent.Type.PlayerJoined, false), guid);
                return;
            }

            // Add players instance to map
            var player = new Player
            {
                ControlUnitId = guid,
                Name = playerName,
                Map = map
            };

            _players.Add(playerName, player);

            // Send OK
            _server.Send(new GameEvent(GameEvent.Type.PlayerJoined, true), guid);
        }

        private GameArea InstantiateMap()
        {
            if (_playableMaps.Count > 0)
            {
                var map = _playableMaps[0];
                _playableMaps.Remove(map);
                return map;
            }
            return null;
        }

        private void UpdateArea(Guid controlUnitGuid, string areaId)
        {
            // Find GUI
            var player = findPlayerByCU(controlUnitGuid);

            if (player != null && !player.GuiId.Equals(Guid.Empty))
            {
                _server.Send(new GameEvent(GameEvent.Type.AreaUpdate, areaId), player.GuiId);
            }
        }

        private Player findPlayerByCU(Guid controlUnitGuid)
        {
            return _players.Values.FirstOrDefault(player => player.ControlUnitId.Equals(controlUnitGuid));
        }
    }
}