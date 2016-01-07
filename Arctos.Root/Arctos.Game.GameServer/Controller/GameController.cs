using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Arctos.Game.Middleware.Logic.Model.Model;
using ArctosGameServer.Domain;
using ArctosGameServer.Service;

namespace ArctosGameServer.Controller
{
    /// <summary>
    /// The GameController
    /// </summary>
    public class GameController : IObserver<Tuple<Guid, GameEvent>>
    {
        private readonly ConcurrentQueue<Tuple<Guid, GameEvent>> _receivedEvents = new ConcurrentQueue<Tuple<Guid, GameEvent>>();
        public bool ShutdownRequested { get; set; } = false;

        private Dictionary<string, Player> _players = new Dictionary<string, Player>();

        private GameTcpServer _server;

        public GameController(GameTcpServer server)
        {
            _server = server;
        }

        /// <summary>
        /// Generates a new map
        /// </summary>
        public void GenerateGame()
        {

            // GameConfiguration
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
                                var playerName = (string)e.Item2.Data;
                                PlayerRequest(e.Item1, playerName);
                            }
                            break;
                        case GameEvent.Type.PlayerJoined:
                            break;
                        case GameEvent.Type.PlayerLeft:
                            break;
                        case GameEvent.Type.GuiRequest:
                            {
                                var playerName = (string)e.Item2.Data;
                                GuiRequest(e.Item1, playerName);
                            }
                            break;
                        case GameEvent.Type.GuiJoined:
                            // Should never occur
                            break;
                        case GameEvent.Type.AreaUpdate:
                            {
                                var area = (string)e.Item2.Data;
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
                _server.Send(new GameEvent(GameEvent.Type.GuiJoined, false), guid);
                return;
            }

            // Add GUI-Guid to player
            _players[playerName].GuiId = guid;

            // Send OK
            _server.Send(new GameEvent(GameEvent.Type.GuiJoined, true), guid);
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

            // Add players instance to map
            var player = new Player
            {
                ControlUnitId = guid,
                Name = playerName
            };

            _players.Add(playerName, player);

            // Send OK
            _server.Send(new GameEvent(GameEvent.Type.PlayerJoined, true), guid);
        }

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

        private void UpdateArea(Guid controlUnitGuid, string areaID)
        {
            // Find GUI
            var player = findPlayerByCU(controlUnitGuid);

            if (player != null && !player.GuiId.Equals(Guid.Empty))
            {
                _server.Send(new GameEvent(GameEvent.Type.AreaUpdate, areaID), player.GuiId);
            }
        }

        private Player findPlayerByCU(Guid controlUnitGuid)
        {
            return _players.Values.FirstOrDefault(player => player.ControlUnitId.Equals(controlUnitGuid));
        }
    }
}