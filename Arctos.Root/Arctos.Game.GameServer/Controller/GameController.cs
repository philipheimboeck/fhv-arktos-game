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
        private bool _gameStart;

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

            // Generate Map

            #region Hardcoded Map

            var areas = new List<Area>
            {
                new Area()
                {
                    AreaId = "420018D63E",
                    Column = 0,
                    Row = 0,
                    Status = Area.AreaStatus.None
                },
                new Area()
                {
                    AreaId = "420018DB3B",
                    Column = 1,
                    Row = 0,
                    Status = Area.AreaStatus.None
                },
                new Area()
                {
                    AreaId = "420018DB50",
                    Column = 2,
                    Row = 0,
                    Status = Area.AreaStatus.None
                },
                new Area()
                {
                    AreaId = "420013E5BA",
                    Column = 0,
                    Row = 1,
                    Status = Area.AreaStatus.None
                },
                new Area()
                {
                    AreaId = "420018DB45",
                    Column = 1,
                    Row = 1,
                    Status = Area.AreaStatus.None
                },
                new Area()
                {
                    AreaId = "420018D64D",
                    Column = 2,
                    Row = 1,
                    Status = Area.AreaStatus.None
                },
                new Area()
                {
                    AreaId = "420018D773",
                    Column = 0,
                    Row = 2,
                    Status = Area.AreaStatus.None
                },
                new Area()
                {
                    AreaId = "420014AA86",
                    Column = 1,
                    Row = 2,
                    Status = Area.AreaStatus.None
                },
                new Area()
                {
                    AreaId = "3D00997CB7",
                    Column = 2,
                    Row = 2,
                    Status = Area.AreaStatus.None
                },
            };

            var map = new GameArea()
            {
                Name = "Map 1",
                AreaList = areas,
                StartField = new Area()
                {
                    AreaId = "3D00997D02"
                }
            };

            #endregion

            _playableMaps.Add(map);
        }

        /// <summary>
        /// Creates a random path through the fields
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private List<Tuple<int, int>> CreatePath(int width, int height)
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
            LogLine("Starting server");

            while (!ShutdownRequested)
            {
                // Process all received events
                ProcessEvents();

                // Check for Game State
                if (_gameStart == false)
                {
                    // Check if game can be started
                    if (_gameReady == false && PlayersReady())
                    {
                        LogLine("Game is ready to start");

                        // Game changed to ready
                        _gameReady = true;

                        // Create the path
                        var map = _players.FirstOrDefault().Value.Map;
                        var path = CreatePath(map.GameColumns, map.GameRows);
                        foreach (var player in _players.Values)
                        {
                            player.Map.setPath(path);
                        }

                        // Notify CUs and GUIs
                        var tuples = new List<GameEventTuple<int, int>>();
                        foreach (var t in path)
                        {
                            tuples.Add(new GameEventTuple<int, int>() {Item1 = t.Item1, Item2 = t.Item2});
                        }
                        var sendPath = new Path() {Waypoints = tuples};
                        _server.Send(new GameEvent(GameEvent.Type.GameReady, sendPath));

                        // Send Event
                        OnGameReadyEvent(new GameReadeEventArgs() {Ready = true});
                    }
                    else if (_gameReady == true && !PlayersReady())
                    {
                        LogLine("Game is not ready to start anymore");

                        // Game changed from ready to not ready
                        _gameReady = false;

                        // Notify CUs and GUIs
                        _server.Send(new GameEvent(GameEvent.Type.GameReady, null));

                        // Send Event
                        OnGameReadyEvent(new GameReadeEventArgs() {Ready = false});
                    }
                }
                else
                {
                    // Game is already running

                    // Check for finished players
                    foreach (var player in _players.Values)
                    {
                        if (player.HasRecentlyFinished())
                        {
                            player.FinishedGame = true;
                            player.EndCounter();

                            // Send log
                            LogLine("Player " + player.Name + " finished with " + player.Duration);

                            // Send GUI
                            if (!player.GuiId.Equals(Guid.Empty))
                            {
                                // Send Counter
                                _server.Send(new GameEvent(GameEvent.Type.PlayerFinish, player.Duration), player.GuiId);
                            }

                            // Send Event
                            OnPlayerFinishedEvent(new PlayerFinishedEventArgs() {Player = player});
                        }
                    }

                    // Check if all player have finished
                    if (_players.Values.Count > 0 && _players.Values.Count(x => x.FinishedGame == false) == 0)
                    {
                        // All players are finished
                        LogLine("All players finished the game");

                        // Get winner
                        var winner = _players.Values.OrderBy(x => x.Duration).First();

                        // Only tell the winner if there is more than just one player
                        foreach (var player in _players.Values)
                        {
                            // Send game event
                            var gameEvent = new GameEvent(GameEvent.Type.GameFinish,
                                _players.Count > 0 && player.Equals(winner));

                            if (!player.GuiId.Equals(Guid.Empty))
                            {
                                _server.Send(gameEvent, player.GuiId);
                            }

                            // Send event
                            OnGameFinishedEvent(new GameFinishEventArgs() { Finished = true });
                        }
                    }
                }
            }
        }

        private void ProcessEvents()
        {
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
                    {
                        PlayerLeft(e.Item1);
                    }
                        break;
                    case GameEvent.Type.ConnectionLost:
                    {
                        ConnectionLost(e.Item1, (string) e.Item2.Data);
                    }
                        break;
                    case GameEvent.Type.GuiLeft:
                    {
                        GuiLeft(e.Item1);
                    }
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

        /// <summary>
        /// Will be called when the CU closes the unit
        /// </summary>
        /// <param name="id"></param>
        private void PlayerLeft(Guid id)
        {
            var player = _players.Values.FirstOrDefault(x => x.ControlUnitId.Equals(id));
            if (player != null)
            {
                LogLine("Player " + player.Name + " left");
                RemovePlayer(player);
            }
            else
            {
                LogLine("Unknown player left");
            }
        }

        /// <summary>
        /// Will be called when the GUI closes
        /// </summary>
        /// <param name="id"></param>
        private void GuiLeft(Guid id)
        {
            var player = _players.Values.FirstOrDefault(x => x.GuiId.Equals(id));
            if (player != null)
            {
                LogLine("GUI for player " + player.Name + " left");
                RemoveGUI(player);
            }
            else
            {
                LogLine("GUI for unknown player left");
            }
        }

        private void RemovePlayer(Player player)
        {
            _players.Remove(player.Name);

            // Add map
            _playableMaps.Add(player.Map);

            // Notify GUI if existing
            if (!player.GuiId.Equals(Guid.Empty))
            {
                _server.Send(new GameEvent(GameEvent.Type.PlayerLeft, null), player.GuiId);
            }

            // Send event
            OnPlayerLeftEvent(new PlayerLeftEventArgs() {Player = player});
        }

        private void RemoveGUI(Player player)
        {
            player.GuiId = Guid.Empty;

            // Notify CU
            if (!player.ControlUnitId.Equals(Guid.Empty))
            {
                _server.Send(new GameEvent(GameEvent.Type.GuiLeft, null), player.ControlUnitId);
            }

            // Send event
            OnGuiChangedEvent(new GuiChangedEventArgs(player));
        }

        /// <summary>
        /// Will be called if a connection to either a CU or GUI is closed
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        private void ConnectionLost(Guid id, string data)
        {
            // Check if it was a CU
            var player = _players.Values.FirstOrDefault(x => x.ControlUnitId.Equals(id));
            if (player != null)
            {
                LogLine("Connetion lost of CU for player " + player.Name);
                RemovePlayer(player);
            }
            else if ((player = _players.Values.FirstOrDefault(x => x.GuiId.Equals(id))) != null)
            {
                LogLine("Connetion lost of GUI for player " + player.Name);
                RemoveGUI(player);
            }
            else
            {
                LogLine("Connection lost of unknown connection");
            }
        }

        private void GuiRequest(Guid guid, string playerName)
        {
            LogLine("Received Gui Request for player " + playerName);

            // Player not existing?
            if (!_players.ContainsKey(playerName))
            {
                LogLine("Player is not existing");

                // Send NOT OK
                _server.Send(new GameEvent(GameEvent.Type.GuiJoined, null), guid);
                return;
            }

            LogLine("Accepted Request");

            // Add GUI-Guid to player
            _players[playerName].GuiId = guid;

            // Send OK
            _server.Send(new GameEvent(GameEvent.Type.GuiJoined, _players[playerName].Map), guid);

            // Send Event
            OnGuiChangedEvent(new GuiChangedEventArgs(_players[playerName]));
        }

        private void PlayerRequest(Guid guid, string playerName)
        {
            LogLine("Received Player Join Request from " + playerName);

            // Player already existing?
            if (_players.ContainsKey(playerName))
            {
                LogLine("Player is already registered");

                // Send NOT OK
                _server.Send(
                    new GameEvent(GameEvent.Type.PlayerJoined, new GameEventTuple<bool, string>()
                    {
                        Item1 = false,
                        Item2 = "Username already taken"
                    }),
                    guid);
                return;
            }

            // Is a map available
            var map = GetAvailableMap();
            if (map == null)
            {
                LogLine("No map available");

                _server.Send(
                    new GameEvent(GameEvent.Type.PlayerJoined, new GameEventTuple<bool, string>()
                    {
                        Item1 = false,
                        Item2 = "No map available"
                    }),
                    guid);
                return;
            }

            LogLine("Accepted Join Request");

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
            _server.Send(new GameEvent(GameEvent.Type.PlayerJoined, new GameEventTuple<bool, string>()
            {
                Item1 = true,
                Item2 = "Player added"
            }),
                guid);

            // Send Event
            OnPlayerJoinedEvent(new PlayerJoinedEventArgs(player));
        }

        private GameArea GetAvailableMap()
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
                LogLine("Received area update from " + player.Name + " at field " + areaId);

                player.Location = player.Map.StartField.AreaId.Equals(areaId)
                    ? player.Map.StartField
                    : player.Map.AreaList.FirstOrDefault(x => x.AreaId.Equals(areaId));
                if (player.Location == null)
                {
                    return;
                }

                // Game started?
                if (_gameStart)
                {
                    var lastVisitedIndex = player.LastVisited != null
                        ? player.Map.Path.IndexOf(player.Map.Path.FirstOrDefault(x => x.Equals(player.LastVisited)))
                        : -1;

                    // Is passed field the next one in the path?
                    var pathArea = player.Map.Path.FirstOrDefault(x => x.AreaId.Equals(areaId));
                    if (pathArea != null)
                    {
                        if (player.Map.Path.IndexOf(pathArea) == lastVisitedIndex + 1)
                        {
                            LogLine("Player has correctly passed the next field in the queue");

                            // Field is correctly passed
                            // Set last visited
                            player.LastVisited = pathArea;
                            pathArea.Status = Area.AreaStatus.CorrectlyPassed;
                        }
                        else
                        {
                            LogLine("Player has correctly wrongly passed that field");

                            // Field is in path, but not in that order
                            pathArea.Status = Area.AreaStatus.WronglyPassed;
                        }

                        // Send Update to GUI
                        // GUI connected?
                        if (!player.GuiId.Equals(Guid.Empty))
                        {
                            _server.Send(new GameEvent(GameEvent.Type.AreaUpdate, pathArea), player.GuiId);
                        }
                    }
                    else if (!player.GuiId.Equals(Guid.Empty))
                    {
                        LogLine("Player has correctly wrongly passed that field");

                        // Field not in path and therefore wrongly passed!
                        player.Location.Status = Area.AreaStatus.WronglyPassed;
                        _server.Send(new GameEvent(GameEvent.Type.AreaUpdate, player.Location), player.GuiId);
                    }
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
            if (!PlayersReady())
            {
                LogLine("Not all players are ready");
                return;
            }

            LogLine("Starting the game as all players are ready");

            // Send Message to all CUs and GUIs
            _server.Send(new GameEvent(GameEvent.Type.GameStart, true));

            // Start timers
            var startTime = DateTime.Now;
            foreach (var player in _players.Values)
            {
                player.StartTime = startTime;
            }

            // Start game
            _gameStart = true;
            _gameReady = false;

            // Send event
            OnGameStartEvent(new GameStartEventArgs() {Started = true});
        }

        /// <summary>
        /// Checks if all players are on their start field!
        /// </summary>
        /// <returns></returns>
        public bool PlayersReady()
        {
            return _players.Count > 0 &&
                   _players.Values.Count(x => x.Location != null && x.Location.Equals(x.Map.StartField)) ==
                   _players.Count;
        }

        protected void LogLine(string log)
        {
            OnLogEventHandler(new LogEventArgs() {Log = log});
        }

        #region EventHandlers

        protected virtual void OnPlayerJoinedEvent(PlayerJoinedEventArgs e)
        {
            if (PlayerJoinedEvent != null) PlayerJoinedEvent.Invoke(this, e);
        }

        protected virtual void OnGuiChangedEvent(GuiChangedEventArgs e)
        {
            if (GuiChangedEvent != null) GuiChangedEvent.Invoke(this, e);
        }

        protected virtual void OnGameReadyEvent(GameReadeEventArgs e)
        {
            if (GameReadyEvent != null) GameReadyEvent.Invoke(this, e);
        }

        protected virtual void OnGameStartEvent(GameStartEventArgs e)
        {
            if (GameStartEvent != null) GameStartEvent.Invoke(this, e);
        }

        protected virtual void OnLogEventHandler(LogEventArgs e)
        {
            if (LogEvent != null) LogEvent.Invoke(this, e);
        }

        protected virtual void OnPlayerLeftEvent(PlayerLeftEventArgs e)
        {
            if (PlayerLeftEvent != null) PlayerLeftEvent.Invoke(this, e);
        }

        protected virtual void OnGameFinishedEvent(GameFinishEventArgs e)
        {
            if (GameFinishedEvent != null) GameFinishedEvent.Invoke(this, e);
        }

        protected virtual void OnPlayerFinishedEvent(PlayerFinishedEventArgs e)
        {
            if (PlayerFinishedEvent != null) PlayerFinishedEvent.Invoke(this, e);
        }

        #endregion

        #region Events

        public event PlayerJoinedEventHandler PlayerJoinedEvent;
        public event PlayerLeftEventHandler PlayerLeftEvent;
        public event GuiChangedEventHandler GuiChangedEvent;
        public event GameReadyEventHandler GameReadyEvent;
        public event GameStartEventHandler GameStartEvent;
        public event LogEventHandler LogEvent;
        public event GameFinishedEventHandler GameFinishedEvent;
        public event PlayerFinishedEventHandler PlayerFinishedEvent;

        #endregion
    }
}