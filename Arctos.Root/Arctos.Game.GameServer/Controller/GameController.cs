using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Arctos.Game.Middleware.Logic.Model.Model;
using Arctos.Game.Model;
using ArctosGameServer.Controller.Events;
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

        private bool _gameReady;
        private int _height = 10;

        private List<GameArea> _playableMaps = new List<GameArea>();

        private Dictionary<string, Player> _players = new Dictionary<string, Player>();

        private GameTcpServer _server;

        private int _width = 10;

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
            // Generate path
            var path = createPath(_width, _height);

            // Generate Map
            var areas = new List<Area>();
            for (var i = 0; i < _width; i++)
            {
                for (var j = 0; j < _height; j++)
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
                AreaList = areas,
                StartField = new Area()
                {
                    AreaId = "420018DB3B"
                }
            };

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

        /// <summary>
        /// Game Loop
        /// </summary>
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

                // Check if game can be started
                if (_gameReady == false && PlayersReady())
                {
                    // Game changed to ready
                    _gameReady = true;

                    // Create the path
                    var path = createPath(_width, _height);
                    foreach (var player in _players.Values)
                    {
                        player.Map.setPath(path);
                    }

                    // Notify CUs and GUIs
                    _server.Send(new GameEvent(GameEvent.Type.GameReady, path));

                    // Send Event
                    OnGameReadyEvent(new GameReadeEventArgs() { Ready = true });
                }
                else if (_gameReady == true && !PlayersReady())
                {
                    // Game changed from ready to not ready
                    _gameReady = false;

                    // Notify CUs and GUIs
                    _server.Send(new GameEvent(GameEvent.Type.GameReady, null));

                    // Send Event
                    OnGameReadyEvent(new GameReadeEventArgs() { Ready = false });
                }
            }
        }

        private void GuiRequest(Guid guid, string playerName)
        {
            // Player not existing?
            if (!_players.ContainsKey(playerName))
            {
                // Send NOT OK
                _server.Send(new GameEvent(GameEvent.Type.GuiJoined, false), guid);
                return;
            }

            // Add GUI-Guid to player
            _players[playerName].GuiId = guid;

            // Send OK
            _server.Send(new GameEvent(GameEvent.Type.GuiJoined, true), guid);

            // Send Event
            OnGuiJoinedEvent(new GuidJoinedEventArgs(_players[playerName]));
        }

        private void PlayerRequest(Guid guid, string playerName)
        {
            // Player already existing?
            if (_players.ContainsKey(playerName))
            {
                // Send NOT OK
                _server.Send(
                    new GameEvent(GameEvent.Type.PlayerJoined, new Tuple<bool, string>(false, "Username already taken")),
                    guid);
                return;
            }

            // Is a map available
            var map = InstantiateMap();
            if (map == null)
            {
                _server.Send(
                    new GameEvent(GameEvent.Type.PlayerJoined, new Tuple<bool, string>(false, "No map available")), guid);
                return;
            }

            // Add players instance to map
            var player = new Player
            {
                ControlUnitId = guid,
                Name = playerName,
                Map = map
            };

            // Add player
            _players.Add(playerName, player);

            // Send OK
            _server.Send(new GameEvent(GameEvent.Type.PlayerJoined, new Tuple<bool, string>(true, "Player added")), guid);

            // Send Event
            OnPlayerJoinedEvent(new PlayerJoinedEventArgs(player));
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
            var player = FindPlayerByCu(controlUnitGuid);

            // Change player position
            if (player != null)
            {
                player.Location = player.Map.AreaList.FirstOrDefault(x => x.AreaId.Equals(areaId));

                // GUI connected?
                if (!player.GuiId.Equals(Guid.Empty))
                {
                    _server.Send(new GameEvent(GameEvent.Type.AreaUpdate, areaId), player.GuiId);
                }
            }
            
        }

        private Player FindPlayerByCu(Guid controlUnitGuid)
        {
            return _players.Values.FirstOrDefault(player => player.ControlUnitId.Equals(controlUnitGuid));
        }

        public void StartGame()
        {
            // Check if all players are ready
            if (PlayersReady())
            {
                return;
            }

            // Send Message to all CUs and GUIs
            _server.Send(new GameEvent(GameEvent.Type.GameStart, true));

            // Start timers
            // Todo
        }
    
        public bool PlayersReady()
        {
            return _players.Count > 0 && _players.Values.Count(x => x.Location.Equals(x.Map.StartField)) != _players.Count;
        }

        protected virtual void OnPlayerJoinedEvent(PlayerJoinedEventArgs e)
        {
            PlayerJoinedEvent?.Invoke(this, e);
        }

        protected virtual void OnGuiJoinedEvent(GuidJoinedEventArgs e)
        {
            GuiJoinedEvent?.Invoke(this, e);
        }

        protected virtual void OnGameReadyEvent(GameReadeEventArgs e)
        {
            GameReadyEvent?.Invoke(this, e);
        }

        #region Events

        public event PlayerJoinedEventHandler PlayerJoinedEvent;
        public event GuiJoinedEventHandler GuiJoinedEvent;
        public event GameReadyEventHandler GameReadyEvent;

        #endregion
    }
}