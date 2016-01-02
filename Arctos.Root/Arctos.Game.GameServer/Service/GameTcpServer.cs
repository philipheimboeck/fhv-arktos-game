using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Build.Framework;
using ArctosGameServer.Communication;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ArctosGameServer.Service
{
    public class GameTcpServer : IObserver<GameEvent>
    {
        private TcpListener _tcpListener;
        private List<TcpClient> _clients = new List<TcpClient>();

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
            TcpClient clientSocket = default(TcpClient);
            clientSocket = _tcpListener.EndAcceptTcpClient(result);
            _clients.Add(clientSocket);

            // Start the client handler
            var handler = new ClientRequestHandler(clientSocket, this);
            handler.StartClient();

            // And wait for the next client
            WaitForClient();
        }

        public void Send(GameEvent gameEvent) 
        {
            // Remove disconnected clients
            RemoveDisconnected();

            // Send event to all clients
            foreach(TcpClient client in _clients)
            {
                // Serialize and send event
                var xmlSerializer = new XmlSerializer(typeof(GameEvent));
                var stream = client.GetStream();
                if (stream.CanWrite)
                {
                    xmlSerializer.Serialize(stream, gameEvent);
                }
            }
        }
        
        /// <summary>
        /// Will be called by the ClientRequest Handlers when they receive an event
        /// </summary>
        /// <param name="gameEvent"></param>
        public void OnReceived(GameEvent gameEvent)
        {
            // TODO Implement
        }

        private void RemoveDisconnected()
        {
            // If not connected, remove it from the list
            _clients.RemoveAll(x => x.Connected == false);
        }

        public void CloseConnections()
        {
            foreach (TcpClient client in _clients)
            {
                client.Close();
            }
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
    }
}