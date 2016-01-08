using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Xml.Serialization;
using Arctos.Game.Middleware.Logic.Model.Model;

namespace ArctosGameServer.Service
{
    public class GameTcpServer : IObserver<GameEvent<object>>, IObservable<Tuple<Guid, GameEvent<object>>>
    {
        private Dictionary<Guid, TcpClient> _clients = new Dictionary<Guid, TcpClient>();
        private List<IObserver<Tuple<Guid, GameEvent<dynamic>>>> _observers = new List<IObserver<Tuple<Guid, GameEvent<dynamic>>>>();
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

        public IDisposable Subscribe(IObserver<Tuple<Guid, GameEvent<object>>> observer)
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

        public void OnNext(GameEvent<object> value)
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

        public void Send<T>(GameEvent<T> gameEvent)
        {
            // Remove disconnected clients
            RemoveDisconnected();

            // Send event to all clients
            foreach (var client in _clients.Values)
            {
                SendData(client, gameEvent);
            }
        }

        public void Send<T>(GameEvent<T> gameEvent, Guid clientId)
        {
            // Remove disconnected clients
            RemoveDisconnected();

            if (!_clients.ContainsKey(clientId))
            {
                throw new Exception("Client is disconnected!");
            }

            SendData(_clients[clientId], gameEvent);
        }

        protected void SendData<T>(TcpClient client, GameEvent<T> gameEvent)
        {
            // Serialize and send event
            var xmlSerializer = new XmlSerializer(typeof (GameEvent<T>));
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
        public void OnReceived(Guid guid, GameEvent<dynamic> gameEvent)
        {
            NotifyObservers(new Tuple<Guid, GameEvent<dynamic>>(guid, gameEvent));
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

        public void Unsubscribe(IObserver<Tuple<Guid, GameEvent<object>>> observer)
        {
            _observers.Remove(observer);
        }

        public void NotifyObservers(Tuple<Guid, GameEvent<dynamic>> e)
        {
            foreach (var observer in _observers)
            {
                observer.OnNext(e);
            }
        }

        /// <summary>
        /// Will be called by the ClientRequestHandler when it closes
        /// </summary>
        /// <param name="clientRequestHandler"></param>
        public void ClientClosed(Guid clientId)
        {
            // Todo: Throw Connection Lost GameEvent
            
        }
    }

    public class Disposable : IDisposable
    {
        private IObserver<Tuple<Guid, GameEvent<object>>> _observer;
        private GameTcpServer _server;

        public Disposable(GameTcpServer server, IObserver<Tuple<Guid, GameEvent<object>>> observer)
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