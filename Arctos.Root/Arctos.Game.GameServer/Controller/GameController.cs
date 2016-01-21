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

        private Game _game = new Game();
        private MapGenerator _mapGenerator = new MapGenerator();
        private Dictionary<string, Player> _players = new Dictionary<string, Player>();

        private GameTcpServer _server;

        /// <summary>
        /// Constructor of the Game Controller
        /// </summary>
        /// <param name="server"></param>
        public GameController(GameTcpServer server)
        {
            _server = server;

            _game.PlayableMaps.Add(_mapGenerator.GenerateMap());
        }

        /// <summary>
        /// When true the GameController will exit from the loop
        /// </summary>
        public bool ShutdownRequested { get; set; }

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
                if (_game.Started == false)
                {
                    // Check if game can be started
                    if (_game.Ready == false && PlayersReady())
                    {
                        GameReady();
                    }
                    else if (_game.Ready == true && !PlayersReady())
                    {
                        GameNotReady();
                    }
                }
                else if (_game.Finished == false)
                {
                    // Game is already running

                    // Check for finished players
                    foreach (var player in _players.Values)
                    {
                        if (player.HasRecentlyFinished())
                        {
                            FinishPlayer(player);
                        }
                    }

                    // Check if all player have finished
                    if (_players.Values.Count > 0 && _players.Values.Count(x => x.FinishedGame == false) == 0)
                    {
                        FinishGame();
                    }
                }
            }
        }

        private void GameNotReady()
        {
            LogLine("Game is not ready to start anymore");

            // Game changed from ready to not ready
            _game.Ready = false;

            // Notify CUs and GUIs
            _server.Send(new GameEvent(GameEvent.Type.GameReady, null));

            // Send Event
            OnGameReadyEvent(new GameReadeEventArgs() {Ready = false});
        }

        private void GameReady()
        {
            LogLine("Game is ready to start");

            // Game changed to ready
            _game.Ready = true;

            // Create the path
            var map = _players.FirstOrDefault().Value.Map;
            var path = _game.CreatePath(map.GameColumns, map.GameRows);
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

        private void FinishGame()
        {
            // All players are finished
            LogLine("All players finished the game");

            _game.Finished = true;

            // Get winner
            var winner = _players.Values.OrderBy(x => x.Duration).First();

            // Only tell the winner if there is more than just one player
            foreach (var player in _players.Values)
            {
                // Send game event
                var gameEvent = new GameEvent(GameEvent.Type.GameFinish, player.Equals(winner));

                if (!player.GuiId.Equals(Guid.Empty))
                {
                    _server.Send(gameEvent, player.GuiId);
                }

                // Send event
                OnGameFinishedEvent(new GameFinishEventArgs() {Finished = true});
            }
        }

        private void FinishPlayer(Player player)
        {
            player.FinishedGame = true;
            player.EndCounter();

            // Send log
            LogLine("Player " + player.Name + " finished with " + player.Duration);

            // Send GUI
            if (!player.GuiId.Equals(Guid.Empty))
            {
                // Send Counter
                _server.Send(new GameEvent(GameEvent.Type.PlayerFinish, player.Duration.TotalMilliseconds), player.GuiId);
            }

            // Send Event
            OnPlayerFinishedEvent(new PlayerFinishedEventArgs() {Player = player});
        }

        /// <summary>
        /// Kicks the player from the game
        /// </summary>
        /// <param name="player"></param>
        public void KickPlayer(Player player)
        {
            // Notify GUI and Player that they will be kicked
            if (!player.ControlUnitId.Equals(Guid.Empty))
            {
                _server.Send(new GameEvent(GameEvent.Type.PlayerKicked, null));
            }
            if (!player.GuiId.Equals(Guid.Empty))
            {
                _server.Send(new GameEvent(GameEvent.Type.PlayerKicked, null));
            }

            // Remove them from the game
            RemovePlayer(player);
            RemoveGUI(player);
        }

        /// <summary>
        /// Process All Events
        /// </summary>
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
            if (player.Map != null)
            {
                _game.PlayableMaps.Add(player.Map);
            }

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
        /// Pauses the player, meaning that the player won't be able to update its status. A paused player can continue when reconnected
        /// </summary>
        /// <param name="player"></param>
        private void PausePlayer(Player player)
        {
            player.Pause = true;

            // Notify GUI if existing
            if (!player.GuiId.Equals(Guid.Empty))
            {
                _server.Send(new GameEvent(GameEvent.Type.PlayerLost, true), player.GuiId);
            }

            // Send event
            OnPlayerLostEvent(new PlayerLostEventArgs() { Player = player, Lost = true });
        }

        /// <summary>
        /// Let the player resume his game
        /// </summary>
        /// <param name="player"></param>
        private void ResumePlayer(Player player)
        {
            player.Pause = false;

            // Notify GUI if existing
            if (!player.GuiId.Equals(Guid.Empty))
            {
                _server.Send(new GameEvent(GameEvent.Type.PlayerLost, false), player.GuiId);
            }

            // Send event
            OnPlayerLostEvent(new PlayerLostEventArgs() { Player = player, Lost = false });
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
                player.ControlUnitId = Guid.Empty;
                PausePlayer(player);
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

                // Is the player paused?
                if (!_players[playerName].Pause)
                {
                    LogLine("Player cannot connect because of duplicate usernames");
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

                // Resume the player
                LogLine("Player can now resume");
                ResumePlayer(_players[playerName]);
                return;
            }

            // Is a map available
            var map = _game.GetAvailableMap();
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
            }), guid);

            // Send Event
            OnPlayerJoinedEvent(new PlayerJoinedEventArgs(player));
        }

        private void UpdateArea(Guid controlUnitGuid, string areaId)
        {
            // Find GUI
            var player = FindPlayerByCu(controlUnitGuid);

            // Change player position
            if (player != null)
            {
                LogLine("Received area update from " + player.Name + " at field " + areaId);

                player.UpdatePosition(areaId);

                // Game started?
                if (player.Location != null && _game.Started)
                {
                    var position = player.ChangePositionStatus(areaId);

                    if (position.Status.Equals(Area.AreaStatus.CorrectlyPassed))
                    {
                        LogLine("Player has correctly passed the next field in the queue");
                    }
                    else
                    {
                        LogLine("Player has wrongly passed that field");

                        // Add a penalty second
                        player.Duration = player.Duration.Add(new TimeSpan(0, 0, 0, 1));
                    }

                    // Send Update to GUI when connected
                    if (!player.GuiId.Equals(Guid.Empty))
                    {
                        _server.Send(new GameEvent(GameEvent.Type.AreaUpdate, position), player.GuiId);
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
            _game.Started = true;
            _game.Ready = false;

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

        #region EventHandling

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

        #endregion

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

        protected virtual void OnPlayerLostEvent(PlayerLostEventArgs e)
        {
            if (PlayerLostEvent != null) PlayerLostEvent.Invoke(this, e);
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
        public event PlayerLostEventHandler PlayerLostEvent;

        #endregion

    }
}