using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
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
        protected NetworkStream _networkStream = null;
        protected GameTcpServer _tcpServer = null;

        public ClientRequestHandler(Guid id, TcpClient clientConnected, GameTcpServer server)
        {
            this._id = id;
            this._clientSocket = clientConnected;
            this._tcpServer = server;

            _protocol = new PresentationLayer(
                new SessionLayer(
                    new TransportLayer(new TcpCommunicator(_clientSocket))));
        }

        public void StartClient()
        {
            _networkStream = _clientSocket.GetStream();

            WaitForRequest();
        }

        private void WaitForRequest()
        {
            var buffer = new byte[_clientSocket.ReceiveBufferSize];

            _networkStream.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);
        }

        private void ReadCallback(IAsyncResult result)
        {
            try
            {
                var received = _protocol.receive();
                if (received != null)
                {
                    var data = received.data as GameEvent;
                    _tcpServer.OnReceived(_id, data);
                }
                else
                {
                    Close();
                }
            }
            catch (IOException ex)
            {
                Close();
                return;
            }
           

            this.WaitForRequest();
        }

        private void Close()
        {
            _networkStream.Close();
            _clientSocket.Close();

            // Notify about the lost connection
            _tcpServer.OnClientClosed(_id);
        }
    }
}