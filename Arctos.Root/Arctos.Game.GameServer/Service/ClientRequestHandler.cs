using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Arctos.Game.Middleware.Logic.Model.Client;
using Arctos.Game.Middleware.Logic.Model.Model;
using ArctosGameServer.Communication;
using ArctosGameServer.Communication.ServerProtocol;

namespace ArctosGameServer.Service
{
    internal class ClientRequestHandler
    {
        protected ProtocolLayer _protocol;
        protected TcpClient _clientSocket;
        protected Guid _id;
        protected GameTcpServer _tcpServer = null;
        private TcpCommunicator _communicator;
        protected bool Closed { get; set; }

        public ClientRequestHandler(Guid id, TcpClient clientConnected, GameTcpServer server)
        {
            this._id = id;
            this._clientSocket = clientConnected;
            this._tcpServer = server;
            this._communicator = new TcpCommunicator(_clientSocket);

            _protocol = new PresentationLayer(
                new SessionLayer(
                    new TransportLayer(_communicator)));
        }

        public void StartClient()
        {
            var thread = new Thread(WaitForRequest);
            thread.Start();
        }

        private void WaitForRequest()
        {
            while (!Closed)
            {
                try
                {
                    if (_communicator.Connected)
                    {
                        var data = _protocol.receive();
                        if (data != null)
                        {
                            var gameEvent = data.data as GameEvent;
                            _tcpServer.OnReceived(_id, gameEvent);
                        }
                        else
                        {
                            Close();
                        }
                    }
                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    Close();
                }
            }
        }
        
        private void Close()
        {
            Closed = true;
            _clientSocket.Close();

            // Notify about the lost connection
            _tcpServer.OnClientClosed(_id);
        }
    }
}