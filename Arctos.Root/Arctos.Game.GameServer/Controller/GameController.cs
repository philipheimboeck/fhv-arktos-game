using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Arctos.Game.Middleware.Logic.Model.Model;
using Arctos.Game.Model;
using ArctosGameServer.Domain;
using ArctosGameServer.Service;

namespace ArctosGameServer.Controller
{
    /// <summary>
    /// The GameController
    /// </summary>
    public class GameController : IObserver<Tuple<Guid, GameEvent<Object>>>
    {
        private readonly ConcurrentQueue<Tuple<Guid, GameEvent<dynamic>>> _receivedEvents =
            new ConcurrentQueue<Tuple<Guid, GameEvent<dynamic>>>();

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

        public void OnNext(Tuple<Guid, GameEvent<dynamic>> value)
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

            var areas = new List<Area>();
            for (var i = 0; i < 10; i++)
            {
                for (var j = 0; j < 10; j++)
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

            _playableMaps.Add(map);
        }

        public void Loop()
        {
            while (!ShutdownRequested)
            {
                // Process all received events
                Tuple<Guid, GameEvent<dynamic>> e = null;
                while (_receivedEvents.TryDequeue(out e))
                {
                    switch (e.Item2.EventGameEventType)
                    {
                        case GameEventType.PlayerRequest:
                        {
                            var playerName = (string) e.Item2.Data;
                            PlayerRequest(e.Item1, playerName);
                        }
                            break;
                        case GameEventType.PlayerJoined:
                            break;
                        case GameEventType.PlayerLeft:
                            break;
                        case GameEventType.GuiRequest:
                        {
                            var playerName = (string) e.Item2.Data;
                            GuiRequest(e.Item1, playerName);
                        }
                            break;
                        case GameEventType.GuiJoined:
                            // Should never occur
                            break;
                        case GameEventType.AreaUpdate:
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
                _server.Send(new GameEvent<GameArea>(GameEventType.GuiJoined, null), guid);
                return;
            }

            // Add GUI-Guid to player
            _players[playerName].GuiId = guid;

            // Send OK
            _server.Send(new GameEvent<GameArea>(GameEventType.GuiJoined, _players[playerName].Map), guid);
        }

        private void PlayerRequest(Guid guid, string playerName)
        {
            // Player already existing?
            if (_players.ContainsKey(playerName))
            {
                // Send NOT OK
                _server.Send(new GameEvent<Boolean>(GameEventType.PlayerJoined, false), guid);
                return;
            }

            // Is a map available
            var map = InstantiateMap();
            if (map == null)
            {
                _server.Send(new GameEvent<Boolean>(GameEventType.PlayerJoined, false), guid);
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
            _server.Send(new GameEvent<Boolean>(GameEventType.PlayerJoined, true), guid);
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
                _server.Send(new GameEvent<String>(GameEventType.AreaUpdate, areaId), player.GuiId);
            }
        }

        private Player findPlayerByCU(Guid controlUnitGuid)
        {
            return _players.Values.FirstOrDefault(player => player.ControlUnitId.Equals(controlUnitGuid));
        }
    }
}