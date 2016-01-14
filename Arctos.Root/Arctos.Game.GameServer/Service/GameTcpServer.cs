using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Xml.Serialization;
using Arctos.Game.Middleware.Logic.Model.Model;

namespace ArctosGameServer.Service
{
    public class GameTcpServer : IObserver<GameEvent>, IObservable<Tuple<Guid, GameEvent>>
    {
        private Dictionary<Guid, TcpClient> _clients = new Dictionary<Guid, TcpClient>();
        private List<IObserver<Tuple<Guid, GameEvent>>> _observers = new List<IObserver<Tuple<Guid, GameEvent>>>();
        private TcpListener _tcpListener;

        public GameTcpServer()
        {
            this._tcpListener = new TcpListener(IPAddress.Any, 13000);
            this._tcpListener.Start();
        }

        public GameTcpServer(IPAddress ip, Int32 port)
        {
            this._tcpListener = new TcpListener(ip, port);
            this._tcpListener.Start();
        }

        public IDisposable Subscribe(IObserver<Tuple<Guid, GameEvent>> observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }
            return new Disposable(this, observer);
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(GameEvent value)
        {
            Send(value);
        }

        public void StartService()
        {
            WaitForClient();
        }

        private void WaitForClient()
        {
            _tcpListener.BeginAcceptTcpClient(new AsyncCallback(OnClientConnect), null);
        }

        private void OnClientConnect(IAsyncResult result)
        {
            // Create a new client
            var clientSocket = default(TcpClient);
            clientSocket = _tcpListener.EndAcceptTcpClient(result);

            // Create a new Unique ID
            var id = System.Guid.NewGuid();
            _clients.Add(id, clientSocket);

            // Start the client handler
            var handler = new ClientRequestHandler(id, clientSocket, this);
            handler.StartClient();

            // And wait for the next client
            WaitForClient();
        }

        public void Send(GameEvent gameEvent)
        {
            // Remove disconnected clients
            RemoveDisconnected();

            // Send event to all clients
            foreach (var client in _clients.Values)
            {
                SendData(client, gameEvent);
            }
        }

        public void Send(GameEvent gameEvent, Guid clientId)
        {
            // Remove disconnected clients
            RemoveDisconnected();

            if (!_clients.ContainsKey(clientId))
            {
                throw new Exception("Client is disconnected!");
            }

            SendData(_clients[clientId], gameEvent);
        }

        protected void SendData(TcpClient client, GameEvent gameEvent)
        {
            // Serialize and send event
            var xmlSerializer = new XmlSerializer(typeof (GameEvent));
            var stream = client.GetStream();
            if (stream.CanWrite)
            {
                xmlSerializer.Serialize(stream, gameEvent);
            }
        }

        /// <summary>
        /// Will be called by the ClientRequest Handlers when they receive an event
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="gameEvent"></param>
        public void OnReceived(Guid guid, GameEvent gameEvent)
        {
            NotifyObservers(new Tuple<Guid, GameEvent>(guid, gameEvent));
        }

        private void RemoveDisconnected()
        {
            // If not connected, remove it from the list
            _clients = _clients.Where(pair => pair.Value.Connected == true)
                .ToDictionary(pair => pair.Key,
                    pair => pair.Value);
        }

        public void CloseConnections()
        {
            foreach (var client in _clients.Values)
            {
                client.Close();
            }
        }

        public void Unsubscribe(IObserver<Tuple<Guid, GameEvent>> observer)
        {
            _observers.Remove(observer);
        }

        public void NotifyObservers(Tuple<Guid, GameEvent> e)
        {
            foreach (var observer in _observers)
            {
                observer.OnNext(e);
            }
        }

        public void OnClientClosed(Guid id)
        {
            _clients.Remove(id);

            NotifyObservers(new Tuple<Guid, GameEvent>(id, new GameEvent(GameEvent.Type.ConnectionLost, "Connection Closed")));
        }
    }

    public class Disposable : IDisposable
    {
        private IObserver<Tuple<Guid, GameEvent>> _observer;
        private GameTcpServer _server;

        public Disposable(GameTcpServer server, IObserver<Tuple<Guid, GameEvent>> observer)
        {
            _server = server;
            _observer = observer;
        }

        public void Dispose()
        {
            _server.Unsubscribe(_observer);
        }
    }
}